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
  [log-context inbound-channel]
  (fn [status]
    (log log-context "Driver connection closed" status)
    (async/put! inbound-channel {:event-type :status
                                 :data :connection-closed})))

(defn- channel-receive-handler
  "Handler invoked when the websocket server receives a new message."
  [log-context inbound-channel]
  (fn [data]
    (let [parsed (json/read-str data :key-fn #(keyword (case/->kebab-case %)))]
      (log log-context "Websocket got message" parsed)
      (async/put! inbound-channel {:event-type :message
                                   :data parsed}))))

(defn- to-json
  "Converts a message map into a JSON string."
  [message]
  (let [key-fn (fn [key] (case/->PascalCase (name key)))
        value-fn (fn [_ value] (if (keyword? value)
                                 (case/->PascalCase (name value))
                                 value))]
    (json/write-str message :key-fn key-fn :value-fn value-fn)))

(defn- create-handler
  "Returns a new handler function for websocket connections suitable for being
   passed to http-kit's run-server function.  When a connection is established,
   updates `socket-atom` with a websocket channel reference. When messages are
   received or connection status changes, publishes events to
   `inbound-channel`."
  [log-context socket-atom inbound-channel]
  (fn [request]
    (http-kit/with-channel request channel
      (log log-context "Driver got connection")
      (async/put! inbound-channel {:event-type :status
                                   :data :connection-opened})
      (reset! socket-atom channel)

      (http-kit/on-close channel (channel-closed-handler log-context
                                                         inbound-channel))
      (http-kit/on-receive channel (channel-receive-handler log-context
                                                            inbound-channel)))))

(defonce stop-server-fn (atom nil))

(defrecord Websocket []
  component/Lifecycle

  (start [{:keys [::logger ::port ::inbound-channel] :as component}]
    (let [log-context (logger/component-log-context logger "Websocket")
          outbound-channel (async/chan 1024)
          socket-atom (atom nil)
          stop-server! (http-kit/run-server
                        (create-handler log-context
                                        socket-atom
                                        inbound-channel)
                        {:port port})]
      (async/go-loop []
        (when-some [message (<! outbound-channel)]
          (let [socket @socket-atom]
            (if (and (some? socket) (http-kit/open? socket))
              (do
                (log log-context "Websocket sending message" message)
                (let [json (to-json message)
                      result (http-kit/send! socket json)]
                  (when-not result
                    (error log-context "Unable to send message" message))))
              (log log-context "Unable to send: Channel is closed" message))
            (recur))))

      (reset! stop-server-fn stop-server!)
      (log log-context "Started Websocket" port)
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
    (if (s/valid? :m/message message)
      (async/put! (::outbound-channel websocket) message)
      (throw (RuntimeException. (s/explain :m/message message))))))

(s/def ::websocket #(instance? Websocket %))

(s/fdef new-websocket :args (s/cat :port integer? :inbound-channel some?))
(defn new-websocket
  "Creates a new Websocket instance which will listen for connections on the
   indicated `port` and publish the resulting messages on `inbound-channel`."
  [port inbound-channel]
  (map->Websocket {::port port ::inbound-channel inbound-channel}))
