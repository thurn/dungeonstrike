(ns dungeonstrike.requests
  "Centralized definitions for request messages sent through the Nodes system."
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

(mount/defstate requests-mult
  "Mult over requests-channel."
  :start (async/mult requests-channel))

(s/def :r/request-type keyword?)

(defmulti request-type :r/request-type)

(s/def :r/message :m/message)

(defmethod request-type :r/client-message [_]
  :m/message)

(defmethod request-type :r/client-disconnected [_]
  some?)

(s/def :r/request (s/multi-spec request-type :r/request-type))

(s/fdef node-for-request :args (s/cat :request :r/request) :ret keyword?)
(defn node-for-request
  "Returns the node keyword to use to process the provided request."
  [{:keys [:r/request-type] :as request}]
  (if (= :r/client-message request-type)
    (request :m/message-type)
    request-type))

(s/def :r/test (s/and :m/message :r/request))
