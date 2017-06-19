(ns dungeonstrike.log-tailer
  "A component which monitors changes to log files. Broadcasts messages on the
   debug log channel when new log entries are appended to the end of the file."
  (:require [clojure.core.async :as async :refer [<!]]
            [clojure.java.io :as io]
            [clojure.spec :as s]
            [clojure.string :as string]
            [dungeonstrike.logger :as logger :refer [log]]
            [dungeonstrike.messages :as messages]
            [dungeonstrike.nodes :as nodes]
            [dungeonstrike.paths :as paths]
            [dungeonstrike.requests :as requests]
            [mount.core :as mount]
            [dungeonstrike.dev :as dev])
  (:import (org.apache.commons.io.input Tailer TailerListener
                                        TailerListenerAdapter)))
(dev/require-dev-helpers)

(mount/defstate ^:private log-context
  :start (logger/component-log-context "LogTailer"))

(defn- new-tailer
  "Creates a new Tailer object which will automatically monitor additions to the
   log file at `log-file-path` and publish the new lines on
   `debug-log-channel`."
  [log-file-path]
  (let [listener (proxy [TailerListenerAdapter] []
                   (handle [line]
                     (async/put! logger/debug-log-channel line)))
        file (io/file log-file-path)]
    ;; Arguments:
    ;; 1000: Delay between checks of the file for new content (in milliseconds)
    ;; true: Whether to tail from the end of the file
    ;; true: Whether to close and re-open the file between chunks
    (Tailer/create file listener 1000 false true)))

(mount/defstate ^:private tailers
  :start (atom {paths/driver-log-path (new-tailer paths/driver-log-path)})
  :stop (doseq [[path tailer] @tailers] (when tailer (.stop tailer))))

(nodes/defnode :m/client-connected
  [:m/client-log-file-path]
  (nodes/new-effect :log-tailer client-log-file-path))

(defrecord LogEffector []
  nodes/EffectHandler
  (execute-effect! [_ log-path]
    (when-not (@tailers log-path)
      (swap! tailers assoc log-path (new-tailer log-path)))
    (log log-context "Client connected")))
