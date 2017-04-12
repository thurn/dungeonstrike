(ns dungeonstrike.log-processor
  (:require [clojure.java.io :as io]
            [com.stuartsierra.component :as component]
            [clojure.core.async :as async]
            [dungeonstrike.logger :refer [log]]
            [dev])
  (:import (org.apache.commons.io.input
            Tailer TailerListener TailerListenerAdapter)))
(dev/require-dev-helpers!)

(def split-on-endlog
  "A transducer which partitions inputs on the token 'ENDLOG'"
  (partition-by #{"ENDLOG" ""}))

(defn is-log?
  "Returns true if the first element of the provided sequence is a string
   prefixed with 'DSLOG'"
  [[head & rest]] (clojure.string/starts-with? head "DSLOG"))

(defn handle-log-line [line-channel line]
  (async/put! line-channel line))

(defn parse-log-header
  "Parses a log header string of the form DSLOG[time][type]"
  [line]
  (let [regex #"^DSLOG\[(\d+)\]\[(\w+)\]$"
        [_ timestamp type] (re-matches regex line)]
    {:type type
     :timestamp timestamp}))

(defn process-log-entry
  "Creates a new log entry map from a sequence representing lines of log input."
  [[header & entry]]
  (let [{timestamp :timestamp type :type} (parse-log-header header)
        [message _ trace] (partition-by #{"DSCONTEXT"} entry)]
    {:type type
     :timestamp timestamp
     :error? (= type "UnityException")
     :source "unity"
     :message message
     :trace trace}))

(def logs-transducer
  "Transducer to convert the input line stream into log entries."
  (comp
   ; Split input on ENDLOG tokens:
   split-on-endlog
   ; Select only entries prefixed with DSLOG:
   (filter is-log?)
   ; Transport the output into log entries:
   (map process-log-entry)))

(defn log-file-object
  "Returns a file object representing the Unity logfile."
  []
  (io/file (System/getProperty "user.dir"
            "../DungeonStrike/Assets/Logs/logs.txt")))

(defrecord LogProcessor [log-channel system-log-context tailer]
  component/Lifecycle

  (start [{:keys [log-channel system-log-context] :as component}]
    (log system-log-context "Started LogProcessor")
    (let [log-file (log-file-object)
          line-channel (async/chan 1 logs-transducer)
          listener (proxy [TailerListenerAdapter] []
                     (handle [line] (handle-log-line line-channel line)))
          tailer (Tailer/create log-file listener)]
      (async/pipe line-channel log-channel)
      (assoc component :tailer tailer)))

  (stop [{:keys [tailer system-log-context] :as component}]
    (log system-log-context "Stopped LogProcessor")
    (.stop tailer)
    (assoc component :tailer nil)))

(defn new-log-processor [log-channel]
  (map->LogProcessor {:log-channel log-channel}))
