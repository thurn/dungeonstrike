(ns dungeonstrike.request-handlers
  "Contains evaluation functions for client messages and UI requests."
  (:require [clojure.spec.alpha :as s]
            [effects.core :as effects]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(defmethod effects/evaluate :m/client-connected
  [{:keys [:m/client-log-file-path :m/client-id]}]
  [(effects/effect :dungeonstrike.logger/set-client-id :client-id client-id)
   (effects/optional-effect :dungeonstrike.log-tailer/add-tailer
                            :path client-log-file-path)
   (effects/optional-effect :dungeonstrike.gui/config
                            :selector :#send-button
                            :key :enabled?
                            :value true)])

(defmethod effects/evaluate :r/client-disconnected
  [_]
  [(effects/optional-effect :dungeonstrike.gui/config
                            :selector :#send-button
                            :key :enabled?
                            :value false)])
