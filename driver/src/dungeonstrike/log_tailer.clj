(ns dungeonstrike.log-tailer
  "A component which monitors changes to a given log file. Broadcasts messages
   on the debug log channel when new log entries are appended to the end of the
   file."
  (:require [clojure.core.async :as async]
            [clojure.java.io :as io]
            [clojure.spec :as s]
            [clojure.string :as string]
            [com.stuartsierra.component :as component]
            [dev])
  (:import (org.apache.commons.io.input Tailer TailerListener
                                        TailerListenerAdapter)))
(dev/require-dev-helpers)

(defrecord LogTailer []
  component/Lifecycle

  (start [{:keys [::log-file-path ::debug-log-channel] :as component}]
    (let [listener (proxy [TailerListenerAdapter] []
                     (handle [line]
                       (async/put! (:channel debug-log-channel) line)))
          file (io/file log-file-path)
          tailer (Tailer/create file listener 1000 true true)]
      (assoc component ::tailer tailer)))
  (stop [{:keys [::tailer] :as component}]
    (when tailer (.stop tailer))
    (dissoc component ::tailer)))
