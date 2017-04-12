(ns dungeonstrike.websockets
  (:require [com.stuartsierra.component :as component]
            [dungeonstrike.logger :refer [dbg! log]]
            [org.httpkit.server :as http-kit]
            [taoensso.sente :as sente]
            [taoensso.sente.interfaces :as sente-interfaces]
            [taoensso.sente.server-adapters.http-kit :refer [get-sch-adapter]]
            [compojure.core :as compojure :refer [GET POST]]
            [compojure.route :as route]
            [ring.middleware.defaults :as ring-defaults]
            [dev]))
(dev/require-dev-helpers!)

(reset! sente/debug-mode?_ true)

(defrecord MyPacker []
  sente-interfaces/IPacker

  (pack [_ input]
    (println "packing" input)
    input)

  (unpack [_ input]
    (println "unpacking" input)
    input))

(defn new-ring-routes [{:keys [ajax-post-fn ajax-get-or-ws-handshake-fn]}]
  (compojure/routes
   (GET  "/" ring-req (ajax-get-or-ws-handshake-fn ring-req))
   (POST "/" ring-req (ajax-post-fn ring-req))))

(defn new-ring-handler [chsk-server]
  (ring-defaults/wrap-defaults (new-ring-routes chsk-server)
                               ring-defaults/site-defaults))

(defn new-channel-server []
  (sente/make-channel-socket-server!
   (get-sch-adapter)
   {:packer (map->MyPacker {})}))

(defn message-handler [{:keys [id ?data event] :as message}]
  (println "got message" message))

(defn start-router! [{:keys [ch-recv]}]
  (println "starting router")
  (sente/start-server-chsk-router! ch-recv message-handler))

(defn start-server! [ring-handler port]
  (println "starting server")
  (http-kit/run-server ring-handler {:port port}))

(defrecord Websocket [logger port stop-server! stop-router!]
  component/Lifecycle

  (start [{:keys [logger port] :as component}]
    (let [chsk-server (new-channel-server)
          ring-handler (new-ring-handler chsk-server)
          stop-router! (start-router! chsk-server)
          stop-server! (start-server! ring-handler port)]
      (log (:system-log-context logger) "Started Websocket" port)
      (assoc component :stop-server! stop-server! :stop-router! stop-router!)))

  (stop [{:keys [logger port stop-server! stop-router!] :as component}]
    (when stop-server! (stop-server!))
    (when stop-router! (stop-router!))
    (log (:system-log-context logger) "Stopped Websocket" port)
    (dissoc component :stop-server! :stop-router)))

(defn new-websocket [port]
  (map->Websocket {:port port}))
