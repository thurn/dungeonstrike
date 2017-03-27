(ns dungeonstrike.main.app
  (:require [com.stuartsierra.component :as component]))

(def electron (js/require "electron"))
(defonce main-window (atom nil))

(defn- create-window []
  (let [BrowserWindow (.-BrowserWindow electron)
        window (BrowserWindow. (clj->js {:width 1440 :height 900}))]
    (.loadURL window (str "file://" js/__dirname "/../../../../index.html"))
    (.openDevTools (.-webContents window))
    (.once window "closed" #(reset! main-window nil))
    window))

(defn send-to-frontend [app message-type message]
  (let [window (:window app)
        web-contents (.-webContents @window)]
    (.send web-contents message-type (clj->js message))))

(defrecord App [app window]
  component/Lifecycle

  (start [this]
    (println "Starting App")
    (let [app (.-app electron)
          result (assoc this :app app :window main-window)]
      (cond
        ; Persist state across restarts:
        @main-window
        result

        (.isReady app)
        (do
          (reset! main-window (create-window))
          result)

        :else
        (do
          (.once app "ready" #(reset! main-window (create-window)))
          result))))

  (stop [this]
    (println "Stopping App")
    this))

(defn new-app []
  (map->App {}))
