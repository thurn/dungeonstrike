(ns dungeonstrike.log-tailer
  "A component which monitors changes to log files. Broadcasts messages on the
   debug log channel when new log entries are appended to the end of the file."
  (:require [clojure.core.async :as async :refer [<!]]
            [clojure.java.io :as io]
            [clojure.spec :as s]
            [clojure.string :as string]
            [dungeonstrike.channels :as channels]
            [dungeonstrike.logger :as logger :refer [log]]
            [dungeonstrike.messages :as messages]
            [dungeonstrike.nodes :as nodes]
            [com.stuartsierra.component :as component]
            [dungeonstrike.dev :as dev])
  (:import (org.apache.commons.io.input Tailer TailerListener
                                        TailerListenerAdapter)))
(dev/require-dev-helpers)

(nodes/defnode :m/client-connected
  [:m/client-log-file-path]
  (nodes/new-effect :log-tailer client-log-file-path))

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

  (start [{:keys [::log-file-path ::debug-log-channel ::logger] :as component}]
    (let [tailers (atom {log-file-path (new-tailer log-file-path
                                                   debug-log-channel)})]
      (assoc component
             ::tailers tailers
             ::log-context (logger/component-log-context logger "LogTailer"))))

  (stop [{:keys [::tailers] :as component}]
    (when tailers
      (doseq [[path tailer] @tailers]
        (when tailer (.stop tailer))))
    (dissoc component ::tailers))

  nodes/EffectHandler
  (execute-effect! [{:keys [::tailers ::debug-log-channel ::log-context]}
                    log-path]
    (when-not (@tailers log-path)
      (swap! tailers assoc log-path (new-tailer log-path debug-log-channel)))
    (log log-context "Client connected")))
