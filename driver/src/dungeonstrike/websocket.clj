(ns dungeonstrike.websocket
  "A component to create a Websocket server for communication between the
   driver and the client."
  (:require [clojure.core.async :as async :refer [<!]]
            [clojure.data.json :as json]
            [clojure.edn :as edn]
            [clojure.spec :as s]
            [clojure.string :as string]
            [dungeonstrike.logger :as logger]
            [dungeonstrike.messages :as messages]
            [dungeonstrike.requests :as requests]
            [camel-snake-kebab.core :as case]
            [mount.core :as mount]
            [org.httpkit.server :as http-kit]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

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
  [data]
  (let [message (parse-json data)]
    (logger/log "Websocket got message" message)
    (if (s/valid? :m/message message)
      (async/put! requests/requests-channel
                  (assoc message :r/request-type :r/client-message))
      (logger/error "Invalid message received"
                    (s/explain :m/message message)))))

(defn- channel-closed-handler
  "Handler for websocket channel close events."
  [status]
  (logger/log "Driver connection closed" status)
  (when-not (= status :server-close)
    (async/put! requests/requests-channel
                {:r/request-type :r/client-disconnected})))

(mount/defstate ^:private socket-atom
  :start (atom nil)
  :stop (when @socket-atom (http-kit/close @socket-atom)))

(defn- connection-handler
  "Handler function for websocket connections suitable for being passed to
  http-kit's run-server function.  When a connection is established, updates
  `socket-atom` with a websocket channel reference. When messages are received
  or connection status changes, publishes events to the `requests-channel`."
  [request]
  (http-kit/with-channel request channel
    (reset! socket-atom channel)
    (http-kit/on-close channel channel-closed-handler)
    (http-kit/on-receive channel channel-receive-handler)))

(mount/defstate ^:private server
  :start (http-kit/run-server connection-handler
                              {:port (Integer/parseInt
                                      (get (mount/args) :port "59008"))})
  :stop (server))

(s/fdef send-message! :args (s/cat :message :m/message))
(defn send-message!
  [message]
  (let [socket @socket-atom]
    (if (and (some? socket) (http-kit/open? socket))
      (do
        (logger/log "Websocket sending message" message)
        (let [json (to-json message)
              result (http-kit/send! socket json)]
          (when-not result
            (logger/error "Unable to send message" message))))
      (logger/error "Unable to send: Channel is closed" message))))
