(ns dungeonstrike.main.core
  (:require [cljs.nodejs :as nodejs]
            [clojure.spec :as spec]
            [clojure.string :as string]))

(def electron (js/require "electron"))
(def ipc-main (.-ipcMain electron))
(def app (.-app electron))
(def BrowserWindow (.-BrowserWindow electron))
(def ws (js/require "ws"))
(def Tail (.-Tail (js/require "tail")))
(def Server (.-Server ws))
(def main-window (atom nil))
(def server (atom nil))
(def websocket (atom nil))
(def unity-log-file-path (str (.-HOME (.-env js/process))
                              "/Library/Logs/Unity/Editor.log"))
(def unity-log-file (Tail. unity-log-file-path))

(nodejs/enable-util-print!)

(defn -main [& args]
  (println "Starting Main Process..."))

(set! *main-cli-fn* -main)

(defn log
  ([type message] (log type message nil nil))
  ([type message timestamp source]
   (let [web-contents (.-webContents @main-window)]
     (.send web-contents
            "log"
            (clj->js {:type type
                      :message message
                      :source (or source "driver")
                      :timestamp (or timestamp (.getTime (js/Date.)))})))))

(defn create-window []
  (reset! main-window (BrowserWindow. (clj->js {:width 1440 :height 900})))
  (.loadURL @main-window
            (str "file://" js/__dirname "/../../../../index.html"))
  (.openDevTools (.-webContents @main-window))
  (.on @main-window "closed" #(reset! main-window nil)))

(defn handle-message [message-string]
  (println "Got message")
  (let [message (js->clj (.parse js/JSON message-string))]
    (println (message "hello"))))

(defn send-error [error-message]
  (let [web-contents (.-webContents @main-window)]
    (.send web-contents "error" error-message)))

(defn send-ready-state-error [socket]
  (case (.-readyState socket)
    0 (send-error "Socket connecting.")
    2 (send-error "Socket closing.")
    3 (send-error "Socket closed.")
    (send-error "Unknown error.")))

(defn send-message [event message]
  (cond
    (nil? @websocket) (send-error "Not connected.")
    (not= 1 (.-readyState @websocket)) (send-ready-state-error @websocket)
    :else (.send @websocket (.stringify js/JSON message))))

(.on ipc-main "send-message" send-message)

(defn handle-connection [socket]
  (log "websockets" "Got connection")
  (.on socket "message" handle-message)
  (.on socket "error" send-error)
  (.on socket "close" #(log "websockets" "Connection closed"))
  (reset! websocket socket))

(defn create-server []
  (reset! server (Server. (clj->js {:port 59005})))
  (.on @server "connection" handle-connection))

(.on app "ready" create-window)
(.on app "ready" create-server)

(.on app "window-all-closed"
     #(when-not (= js/process.platform "darwin")
        (.quit app)))

(.on app "activate"
     #(when (nil? @main-window)
        (create-window)))

(def enable-logs-prefixes
  ["DSAWAKE" "DSENABLE" "DSLOG"])

(def disable-logs-prefixes
  ["Hashing assets" "Mono: successfully reloaded assembly" "DSCLOSE"])

(def omit-from-logs-prefixes
  ["Refresh: detecting if any assets"])

(def flush-logs-prefixes
  ["(Filename:"])

(def logs-enabled (atom false))
(def current-logs-entry (atom {}))

(defn parse-dslog-header [line]
  (let [regex #"^DSLOG\[(\d+)\]\[(\w+)\]\s+(.*)$"
        [_ timestamp type message] (re-find regex line)]
    {:type type :timestamp timestamp :message message}))

(defn handle-log-line [line]
  (when-not (string/blank? line)
    (when (some #(string/starts-with? line %) enable-logs-prefixes)
      (reset! logs-enabled true)
      (if (string/starts-with? line "DSLOG")
        (reset! current-logs-entry
                (parse-dslog-header line))
        (reset! current-logs-entry {:type (string/lower-case line)})))

    (when (some #(string/starts-with? line %) disable-logs-prefixes)
      (reset! logs-enabled false))

    (when (and @logs-enabled
               (some #(string/starts-with? line %)
                     (concat disable-logs-prefixes flush-logs-prefixes)))
      (let [{:keys [type message timestamp details]} @current-logs-entry]
        (log type message timestamp "unity"))
      (reset! current-logs-entry {:type "unity"}))

    (when (and @logs-enabled
               (not (some #(string/starts-with? line %)
                          (concat flush-logs-prefixes
                                  omit-from-logs-prefixes))))
      (when (nil? (:message @current-logs-entry))
        (swap! current-logs-entry assoc :message line))
      (swap! current-logs-entry update :details conj line))))

(.on unity-log-file "line" handle-log-line)
(.on unity-log-file "error" #(send-error (str "Error reading unity logs: " %1)))

(defonce my-state (atom "dthurn"))

(def three-twenty 320)

(def load-scene
  {::message-type :load-scene,
   ::message-id (uuid "12345"),
   ::game-version "0.1.0",
   ::scene-name "flat"})

(defn what [] (str "foo" three-twenty @my-state))
