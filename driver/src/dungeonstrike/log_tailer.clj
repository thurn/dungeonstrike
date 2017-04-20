(ns dungeonstrike.log-tailer
  "A component which monitors changes to a given log file. Broadcasts messages
   on the debug log channel when new log entries are appended to the end of the
   file."
  (:require [clojure.core.async :as async]
            [clojure.java.io :as io]
            [clojure.spec :as s]
            [clojure.string :as string]
            [com.stuartsierra.component :as component]
            [dungeonstrike.logger :as logger :refer [log error]]
            [dev])
  (:import (org.apache.commons.io.input Tailer TailerListener
                                        TailerListenerAdapter)))
(dev/require-dev-helpers)

(defrecord LogTailer []
  component/Lifecycle

  (start [{:keys [::log-file ::logger] :as component}]
    (let [log-context (logger/component-log-context logger "LogTailer")
          listener (proxy [TailerListenerAdapter] []
                     (handle [line]
                       (async/put! (logger/debug-log-channel logger) line)))
          file (io/file log-file)
          tailer (Tailer/create file listener 1000 true true)]
      (log log-context "Started LogTailer" (.getName file))
      (assoc component ::tailer tailer ::log-context log-context)))
  (stop [{:keys [::tailer ::log-context] :as component}]
    (when tailer (.stop tailer))
    (log log-context "Stopped LogTailer")
    (dissoc component ::tailer ::log-context)))

(s/fdef new-log-tailer :args (s/cat :log-file string?))
(defn new-log-tailer
  "Returns a new LogTailer instance which will monitor changes to the log file
   at the path indicated by the `log-file` path string."
  [log-file]
  (map->LogTailer {::log-file log-file}))
