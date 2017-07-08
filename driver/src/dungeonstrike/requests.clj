(ns dungeonstrike.requests
  "Centralized definitions for request messages sent through the Reconciler
   system."
  (:require [clojure.core.async :as async]
            [clojure.spec.alpha :as s]
            [dungeonstrike.messages :as messages]
            [effects.core :as effects]
            [mount.core :as mount]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(mount/defstate requests-channel
  "Channel for requests to the Nodes system."
  :start (async/chan)
  :stop (async/close! requests-channel))

(defmethod effects/request-spec :r/client-disconnected [_]
  map?)

(defmethod effects/request-spec :r/message-selected [_]
  (s/keys :req [:m/message-type]))

(s/fdef send-request! :args (s/cat :request :effects.core/request))
(defn send-request!
  "Helper method to send a request."
  [request]
  (async/put! requests-channel request))
