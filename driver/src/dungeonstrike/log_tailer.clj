(ns dungeonstrike.log-tailer
  (:require [clojure.string :as string]
            [clojure.java.io :as io]
            [clojure.core.async :as async]
            [com.stuartsierra.component :as component]
            [dungeonstrike.logger :as logger :refer [dbg! log]]
            [dev])
  (:import (org.apache.commons.io.input
            Tailer TailerListener TailerListenerAdapter)))
(dev/require-dev-helpers!)

(defn handle-log-line [log-channel line]
  (async/put! log-channel line))

(def listen-count (atom 0))

(defrecord LogTailer [log-file logger tailer log-channel]
  component/Lifecycle

  (start [{:keys [log-file logger] :as component}]
    (reset! listen-count 0)
    (let [listener (proxy [TailerListenerAdapter] []
                     (handle [line]
                       (swap! listen-count inc)
                       (handle-log-line (:debug-log-channel logger) line)))
          file (io/file log-file)
          tailer (Tailer/create file listener 1000 true true)]
      (log (:system-log-context logger) "Started LogTailer" (.getName file))
      (assoc component :tailer tailer)))
  (stop [{:keys [tailer log-file logger foo] :as component}]
    (reset! listen-count -1000)
    (when tailer (.stop tailer))
    (log (:system-log-context logger) "Stopped LogTailer")
    (dissoc component :tailer)))

(defn new-log-tailer [log-file]
  (map->LogTailer {:log-file log-file}))
