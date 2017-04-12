(ns dungeonstrike.websocket
  (:require [com.stuartsierra.component :as component]
            [dungeonstrike.logger :refer [dbg! log]]
            [org.httpkit.server :as http-kit]
            [dev]))
(dev/require-dev-helpers!)

(defn channel-closed-handler [logger]
  (fn [status]
    (log (:system-log-context logger) "Websocket closed" status)))

(defn channel-receive-handler [logger]
  (fn [data]
    (log (:system-log-context logger) "Websocket got data" data)))

(defn create-handler [logger]
  (fn [request]
    (http-kit/with-channel request channel
      (http-kit/on-close channel (channel-closed-handler logger))
      (http-kit/on-receive channel (channel-receive-handler logger)))))

(defrecord Websocket [logger port stop-server!]
  component/Lifecycle

  (start [{:keys [logger port] :as component}]
    (let [stop-server! (http-kit/run-server (create-handler logger)
                                            {:port port})]
      (log (:system-log-context logger) "Started Websocket" port)
      (assoc component :stop-server! stop-server!)))

  (stop [{:keys [logger port stop-server!] :as component}]
    (stop-server!)
    (log (:system-log-context logger) "Stopped Websocket" port)
    (dissoc component :stop-server! :port)))

(defn new-websocket [port]
  (map->Websocket {:port port}))
