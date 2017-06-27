(ns dungeonstrike.core
  "Main entry point for the application during interactive development"
  (:require [clojure.core.async :as async]
            [clojure.spec.test :as spec-test]
            [dungeonstrike.logger :as logger]
            [dungeonstrike.log-tailer :as log-tailer]
            [dungeonstrike.code-generator]
            [dungeonstrike.gui :as gui]
            [dungeonstrike.main :as main]
            [mount.core :as mount]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(defn stop!
  "Stops all components in the current system. For development use only."
  []
  (mount/stop))

(defn start!
  "Creates a new system and starts all components in it. For development use
   only."
  []
  (main/start! {}))
