(ns dungeonstrike.websocket
  "A component to create a Websocket server for communication between the
   driver and the client."
  (:require [clojure.core.async :as async :refer [<!]]
            [clojure.data.json :as json]
            [clojure.edn :as edn]
            [clojure.spec :as s]
            [clojure.string :as string]
            [dungeonstrike.channels :as channels]
            [dungeonstrike.logger :as logger :refer [log error]]
            [dungeonstrike.messages :as messages]
            [camel-snake-kebab.core :as case]
            [com.stuartsierra.component :as component]
            [org.httpkit.server :as http-kit]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(defn- channel-closed-handler
  "Handler for websocket channel close events."
  [{:keys [::log-context ::requests-channel]}]
  (fn [status]
    (log log-context "Driver connection closed" status)
    (async/put! requests-channel {:r/request-type :r/client-disconnected})))

(defn- to-json
  "Converts a message map into a JSON string."
  [message]
  (let [key-fn (fn [key] (case/->PascalCase (name key)))
        value-fn (fn [_ value] (if (keyword? value)
                                 (case/->PascalCase (name value))
                                 value))]
    (json/write-str message
                    :key-fn key-fn
                    :value-fn value-fn
                    :escape-slash false)))

(defn- parse-json
  "Converts a JSON string into a message map."
  [string]
  (let [key-fn (fn [key] (keyword "m" (case/->kebab-case key)))
        value-fn (fn [key value]
                   (cond
                     (= :m/message-type key)
                     (keyword "m" (case/->kebab-case value))

                     (messages/set-for-enum-name (case/->PascalCase (name key)))
                     (keyword (case/->kebab-case value))

                     :otherwise
                     value))]
    (json/read-str string :key-fn key-fn :value-fn value-fn)))

(defn- channel-receive-handler
  "Handler invoked when the websocket server receives a new message."
  [{:keys [::log-context ::requests-channel]}]
  (fn [data]
    (let [message (parse-json data)]
      (log log-context "Websocket got message" message)
      (if (s/valid? :m/message message)
        (async/put! requests-channel
                    (assoc message :r/request-type :r/client-message))
        (error "Invalid message received" (s/explain :m/message message))))))

(defn- create-handler
  "Returns a new handler function for websocket connections suitable for being
   passed to http-kit's run-server function.  When a connection is established,
   updates `socket-atom` with a websocket channel reference. When messages are
   received or connection status changes, publishes events to
   the `requests-channel`."
  [{:keys [::log-context ::socket-atom]
    :as component}]
  (fn [request]
    (http-kit/with-channel request channel
      (reset! socket-atom channel)
      (http-kit/on-close channel (channel-closed-handler component))
      (http-kit/on-receive channel (channel-receive-handler component)))))

(defonce stop-server-fn (atom nil))

(defrecord Websocket [options]
  component/Lifecycle

  (start [{:keys [:options ::logger]
           :as component}]
    (when @stop-server-fn (@stop-server-fn))
    (let [log-context (logger/component-log-context logger "Websocket")
          socket-atom (atom nil)
          outbound-channel (async/chan 1024)
          updated (assoc component
                         ::log-context log-context
                         ::outbound-channel outbound-channel
                         ::socket-atom socket-atom)
          port (Integer/parseInt (get options :port "59008"))
          stop-server! (http-kit/run-server (create-handler updated)
                                            {:port port})]
      (reset! stop-server-fn stop-server!)
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
              (error log-context "Unable to send: Channel is closed" message))
            (recur))))

      (log log-context "Started Websocket" port)
      (assoc updated ::stop-server! stop-server!)))

  (stop [{:keys [::log-context ::port ::stop-server! ::outbound-channel
                 ::socket-atom]
          :as component}]
    (when stop-server! (stop-server!))
    (when outbound-channel (async/close! outbound-channel))
    (when socket-atom (reset! socket-atom nil))
    (dissoc component ::stop-server! ::port ::log-context ::socket-atom
            ::outbound-channel))

  messages/MessageSender
  (send-message! [websocket message]
    (if (s/valid? :m/message message)
      (async/put! (::outbound-channel websocket) message)
      (throw (RuntimeException. (s/explain :m/message message))))))

(s/def ::websocket #(instance? Websocket %))
