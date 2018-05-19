(ns dungeonstrike.request-handlers
  "Contains evaluation functions for client messages and UI requests."
  (:require [clojure.spec.alpha :as s]
            [effects.core :as effects]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(defmethod effects/evaluate :a/client-connected
  [{:keys [:a/client-log-file-path :a/client-id]}]
  [(effects/effect :dungeonstrike.logger/set-client-id :client-id client-id)
   (effects/optional-effect :dungeonstrike.log-tailer/add-tailer
                            :path client-log-file-path)])
