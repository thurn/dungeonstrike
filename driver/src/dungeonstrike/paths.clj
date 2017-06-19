(ns dungeonstrike.paths
  "Provides utilities for constructing useful filesystem paths."
  (:require [mount.core :as mount]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(defn- get-path
  "Returns the absolute path to a location at the relative `path` of type
   `type`, based on the command-line flags in `options`."
  [type path]
  (case type
    :driver
    (str (get (mount/args) :driver-path
              (System/getProperty "user.dir"))
         "/" path)
    :client
    (str (get (mount/args) :client-path
              (str (System/getProperty "user.dir") "/../DungeonStrike"))
         "/" path)
    :tests
    (str (get (mount/args) :tests-path
              (str (System/getProperty "user.dir") "/../tests"))
         "/" path)))

(mount/defstate driver-log-path
  "The path to the driver log file."
  :start (get-path :driver "logs/driver_logs.txt"))

(mount/defstate test-recordings-path
  "The path to the test recordings directory."
  :start (get-path :tests "recordings"))

(mount/defstate code-generator-output-path
  "The path at which the messages code generator should produce output."
  :start (get-path :client "Assets/Source/Messaging/Generated.cs"))
