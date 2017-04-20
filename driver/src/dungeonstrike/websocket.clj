(ns dungeonstrike.websocket
  "A component to create a Websocket server for communication between the
   driver and the client."
  (:require [clojure.core.async :as async :refer [<!]]
            [clojure.data.json :as json]
            [clojure.spec :as s]
            [dungeonstrike.logger :as logger :refer [log error]]
            [dungeonstrike.messages :as messages]
            [camel-snake-kebab.core :as case]
            [com.stuartsierra.component :as component]
            [org.httpkit.server :as http-kit]
            [dev]))
(dev/require-dev-helpers)

(defn- channel-closed-handler
  "Handler for websocket channel close events."
  [log-context]
  (fn [status]
    (log log-context "Websocket closed" status)))

(defn- channel-receive-handler
  "Handler invoked when the websocket server receives a new message."
  [log-context]
  (fn [data]
    (let [parsed (json/read-str data :key-fn #(keyword (case/->kebab-case %)))]
      (log log-context "Websocket got message" parsed))))

(defn- create-handler
  "Returns a new handler function for websocket connections suitable for being
   passed to http-kit's run-server function. When a connection is established,
   starts a go loop which consumes messages from `outbound-channel` and sends
   them to the client."
  [log-context outbound-channel]
  (fn [request]
    (http-kit/with-channel request channel
      (log log-context "Driver got connection")

      (async/go-loop []
        (if (http-kit/open? channel)
          (when-some [message (<! outbound-channel)]
            (log log-context "Websocket sending message" message)
            (let [json (json/write-str message
                                       :key-fn #(case/->PascalCase (name %)))
                  result (http-kit/send! channel json)]
              (when-not result
                (error log-context "Unable to send message" message)))
            (recur))))

      (http-kit/on-close channel (channel-closed-handler log-context))
      (http-kit/on-receive channel (channel-receive-handler log-context)))))

(defprotocol MessageSender
  (send-message! [component message]
    "Sends a `message` to the client via `component`."))

(defonce ^:private stop-server-fn (atom nil))

(defrecord Websocket []
  component/Lifecycle

  (start [{:keys [::logger ::port] :as component}]
    (let [log-context (logger/component-log-context logger "Websocket")
          outbound-channel (async/chan (async/buffer 1024))
          stop-server! (http-kit/run-server
                        (create-handler log-context outbound-channel)
                        {:port port})]
      (log log-context "Started Websocket" port)
      (reset! stop-server-fn stop-server!)
      (assoc component
             ::log-context log-context
             ::stop-server! stop-server!
             ::outbound-channel outbound-channel)))

  (stop [{:keys [::log-context ::port ::stop-server! ::outbound-channel]
          :as component}]
    (stop-server!)
    (async/close! outbound-channel)
    (log log-context "Stopped Websocket" port)
    (dissoc component ::stop-server! ::port ::log-context ::outbound-channel))

  messages/MessageSender
  (send-message! [websocket message]
    (async/put! (::outbound-channel websocket) message)))

(s/def ::websocket #(instance? Websocket %))

(s/fdef new-websocket :args (s/cat :port integer?))
(defn new-websocket
  "Creates a new Websocket instance which will listen for connections on the
   indicated `port`."
  [port]
  (map->Websocket {::port port}))
