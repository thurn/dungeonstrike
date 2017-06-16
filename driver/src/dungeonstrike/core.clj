(ns dungeonstrike.core
  "Main entry point for the application during interactive development"
  (:require [clojure.core.async :as async]
            [clojure.spec.test :as spec-test]
            [dungeonstrike.logger :as logger]
            [dungeonstrike.code-generator]
            [dungeonstrike.gui]
            [dungeonstrike.main :as main]
            [dungeonstrike.nodes :as nodes]
            [dungeonstrike.dev :as dev]
            [com.stuartsierra.component :as component])
  (:import (dungeonstrike.gui DebugGui)
           (dungeonstrike.code_generator CodeGenerator)))
(dev/require-dev-helpers)

(defn- development-system
  "Returns a new system map which combines the elements of `main/test-system`
   with additional components suitable for interactive development."
  [options channels]
  (merge (main/test-system options channels)
         {:code-generator-output-path
          (main/get-path options :client "Assets/Source/Messaging/Generated.cs")

          :code-generator
          (component/using
           (CodeGenerator.)
           {:dungeonstrike.code-generator/logger
            :logger
            :dungeonstrike.code-generator/code-generator-output-path
            :code-generator-output-path})

          :debug-gui
          (component/using
           (DebugGui.)
           {:dungeonstrike.gui/logger
            :logger
            :dungeonstrike.gui/test-runner
            :test-runner
            :dungeonstrike.gui/message-sender
            :websocket
            :dungeonstrike.gui/test-recordings-path
            :test-recordings-path
            :dungeonstrike.gui/debug-log-mult
            :debug-log-mult})}))

; Atom containing the system -- for development use only
(defonce system (atom nil))

(defn stop!
  "Stops all components in the current system. For development use only."
  []
  (when @system (component/stop @system))
  (reset! system nil))

(defn start!
  "Creates a new system and starts all components in it. For development use
   only."
  []
  (when @system (stop!))
  (spec-test/instrument)
  (reset! system (main/create-and-start-system development-system {})))
