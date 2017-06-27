(ns dungeonstrike.request-handlers
  "Contains evaluation functions for client messages."
  (:require [clojure.spec :as s]
            [dungeonstrike.reconciler :as reconciler]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(defmethod reconciler/evaluate :m/client-connected
  [_ {:keys [:m/client-log-file-path :m/client-id]}
   {:keys [:d/client-log-files :d/gui-configuration] :as universe}]
  (assoc universe
         :d/client-log-files (conj (or client-log-files #{})
                                   client-log-file-path)
         :d/current-client-id client-id
         :d/gui-configuration (assoc gui-configuration
                                     :#send-button {:enabled? true})))

(defmethod reconciler/evaluate :r/client-disconnected
  [_ _ {:keys [:d/gui-configuration] :as universe}]
  (assoc universe
         :d/gui-configuration (assoc gui-configuration
                                     :#send-button {:enabled? false})))
