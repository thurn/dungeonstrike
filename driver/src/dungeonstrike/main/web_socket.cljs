(ns dungeonstrike.main.web-socket
  (:require [com.stuartsierra.component :as component]
            [dungeonstrike.main.logger :as logger]))

(def ws (js/require "ws"))
(def Server (.-Server ws))
(defonce websocket (atom nil))

(defn- handle-message [logger message]
  (println "message" message))

(defn- handle-error [logger error])

(defn- handle-connection [{:keys [logger]} socket]
  (logger/log logger "websockets" "Driver got connection")
  (.on socket "message" #(handle-message logger %))
  (.on socket "error" #(logger/error logger "websockets" %))
  (.on socket "close" #(logger/log logger
                                   "websockets"
                                   "Driver connection closed"))
  (reset! websocket socket))

(defrecord WebSocket [logger socket]
  component/Lifecycle
  (start [this]
    (println "Starting WebSocket")
    ; Persist connection across restarts:
    (if @websocket
      this
      (let [server (Server. (clj->js {:port 59005}))
            result (assoc this :socket websocket :server server)]
        (.on server "connection" #(handle-connection result %)))))

  (stop [this]))

(defn new-web-socket []
  (map->WebSocket {}))
