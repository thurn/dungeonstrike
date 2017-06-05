(ns dungeonstrike.log-tailer
  "A component which monitors changes to log files. Broadcasts messages on the
   debug log channel when new log entries are appended to the end of the file."
  (:require [clojure.core.async :as async :refer [<!]]
            [clojure.java.io :as io]
            [clojure.spec :as s]
            [clojure.string :as string]
            [dungeonstrike.channels :as channels]
            [dungeonstrike.messages :as messages]
            [com.stuartsierra.component :as component]
            [dungeonstrike.dev :as dev])
  (:import (org.apache.commons.io.input Tailer TailerListener
                                        TailerListenerAdapter)))
(dev/require-dev-helpers)

(defn- new-tailer
  "Creates a new Tailer object which will automatically monitor additions to the
   log file at `log-file-path` and publish the new lines on
   `debug-log-channel`."
  [log-file-path debug-log-channel]
  (let [listener (proxy [TailerListenerAdapter] []
                   (handle [line]
                     (channels/put! debug-log-channel line)))
        file (io/file log-file-path)]
    (Tailer/create file listener 1000 true true)))

(defrecord LogTailer [options]
  component/Lifecycle

  (start [{:keys [::log-file-path ::debug-log-channel
                  ::client-connected-channel] :as component}]
    (let [tailers (atom {log-file-path (new-tailer log-file-path
                                                   debug-log-channel)})]
      (assoc component ::tailers tailers)))
  (stop [{:keys [::tailers] :as component}]
    (when tailers
      (doseq [[path tailer] @tailers]
        (when tailer (.stop tailer))))
    (dissoc component ::tailers)))
