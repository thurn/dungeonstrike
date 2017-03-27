(ns dungeonstrike.main.log-processor
  (:require [clojure.string :as string]
            [com.stuartsierra.component :as component]
            [dungeonstrike.main.app :as app]))
(def Tail (.-Tail (js/require "tail")))

(defn- log-file-path []
  (string/replace js/__dirname
                  #"driver.*"
                  "DungeonStrike/Assets/Logs/logs.txt"))

(defn parse-log-header [line]
  (let [regex #"^DSLOG\[(\d+)\]\[(\w+)\]$"
        [_ timestamp type] (re-matches regex line)]
    {:type type
     :timestamp timestamp
     :message []
     :error? (= type "UnityException")
     :source "unity"
     :trace []}))

(defn handle-log-line [{:keys [app current-log-entry in-trace?]} line]
  (cond
    (string/starts-with? line "DSLOG")
    (do
      (reset! current-log-entry (parse-log-header line)))

    (= line "DSTRACE")
    (reset! in-trace? true)

    (= line "ENDLOG")
    (do
      (app/send-to-frontend app "log" @current-log-entry)
      (reset! in-trace? false))

    @in-trace?
    (swap! current-log-entry update :trace conj line)

    :else
    (swap! current-log-entry update :message conj line)))


(defn- handle-log-error [log-processor error]
  (println "error" error))

(defrecord LogProcessor [app log-file current-log-entry in-trace?]
  component/Lifecycle

  (start [this]
    (println "Starting LogProcessor")
    (let [log-file (Tail. (log-file-path))
          component (assoc this
                           :log-file log-file
                           :current-log-entry (atom {})
                           :in-trace? (atom false))]
      (.on log-file "line" #(handle-log-line component %))
      (.on log-file "error" #(handle-log-error component %))
      component))

  (stop [this]
    (println "Stopping LogProcessor")
    (.removeAllListeners (:log-file this))
    this))

(defn new-log-processor []
  (map->LogProcessor {}))
