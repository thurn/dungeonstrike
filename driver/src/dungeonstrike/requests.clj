(ns dungeonstrike.requests
  "Centralized definitions for request messages sent through the Reconciler
   system."
  (:require [clojure.core.async :as async]
            [clojure.spec :as s]
            [dungeonstrike.messages :as messages]
            [mount.core :as mount]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(mount/defstate requests-channel
  "Channel for requests to the Nodes system."
  :start (async/chan)
  :stop (async/close! requests-channel))

(s/def :r/request-type keyword?)

(defmulti request :r/request-type)

(s/def :r/message :m/message)

(defmethod request :r/client-message [_]
  :m/message)

(defmethod request :r/client-disconnected [_]
  some?)

(defmethod request :r/message-selected [_]
  (s/keys :req [:m/message-type]))

(s/def :r/request (s/multi-spec request :r/request-type))

(s/fdef request-type :args (s/cat :request :r/request) :ret keyword?)
(defn request-type
  "Returns the request type keyword to use to process the provided request."
  [{:keys [:r/request-type] :as request}]
  (if (= :r/client-message request-type)
    (request :m/message-type)
    request-type))

(s/fdef send-request! :args (s/cat :request :r/request))
(defn send-request!
  "Helper method to send a request."
  [request]
  (async/put! requests-channel request))
