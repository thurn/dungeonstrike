(ns dungeonstrike.core
  "Contain the system definition for the application. Utilizies the 'component'
   microframework to manage the lifecycle for each independent piece of the
   application. Should not be imported by any code expect in
   `dungeonstrike.main`."
  (:require [clojure.core.async :as async :refer [<!]]
            [clojure.spec.test :as spec-test]
            [clojure.tools.cli :as cli]
            [com.stuartsierra.component :as component]
            [dungeonstrike.channels :as channels]
            [dungeonstrike.code-generator :as code-generator]
            [dungeonstrike.gui :as gui]
            [dungeonstrike.log-tailer :as log-tailer]
            [dungeonstrike.logger :as logger]
            [dungeonstrike.test-runner :as test-runner]
            [dungeonstrike.websocket :as websocket]
            [dev]))
(dev/require-dev-helpers)

(defn- get-path
  "Returns the absolute path to a location at the relative `path`, based on the
   command-line flags in `options`."
  [options path]
  (str (get options :root-path (System/getProperty "user.dir")) "/../" path))

(defn- create-system
  "Creates a new component framework 'System Map' which instantiates all
   components used by the application and binds their dependencies. `options`
   should contain any command-line options passed on start."
  [options]
  (component/system-map
   ::driver-log-path
   (get-path options "driver/logs/driver_logs.txt")

   ::client-log-path
   (get-path options "DungeonStrike/Logs/client_logs.txt")

   ::code-generator-output-path
   (get-path options "DungeonStrike/Assets/Source/Messaging/Generated.cs")

   ::test-recordings-path
   (get-path options "tests/recordings")

   ::on-start-channel
   (channels/new-channel
    [(async/dropping-buffer 1024)])

   ::debug-log-channel
   (channels/new-channel
    [(async/sliding-buffer 1024) logger/debug-log-transducer]
    [:dungeonstrike.gui/debug-log-channel
     :dungeonstrike.test-runner/debug-log-channel])

   ::connection-status-channel
   (channels/new-channel
    [(async/sliding-buffer 1024)]
    [:dungeonstrike.gui/connection-status-channel])

   ::driver-log-tailer
   (component/using
    (log-tailer/LogTailer.)
    {:dungeonstrike.log-tailer/debug-log-channel
     ::debug-log-channel
     :dungeonstrike.log-tailer/log-file-path
     ::driver-log-path})

   ::client-log-tailer
   (component/using
    (log-tailer/LogTailer.)
    {:dungeonstrike.log-tailer/debug-log-channel
     ::debug-log-channel
     :dungeonstrike.log-tailer/log-file-path
     ::client-log-path})

   ::logger
   (component/using
    (logger/Logger.)
    {:dungeonstrike.logger/log-file-path
     ::driver-log-path
     :dungeonstrike.logger/driver-log-tailer
     ::driver-log-tailer
     :dungeonstrike.logger/client-log-tailer
     ::client-log-tailer})

   ::code-generator
   (component/using
    (code-generator/CodeGenerator.)
    {:dungeonstrike.code-generator/code-generator-output-path
     ::code-generator-output-path})

   ::websocket
   (component/using
    (websocket/Websocket. options)
    {:dungeonstrike.websocket/logger
     ::logger
     :dungeonstrike.websocket/connection-status-channel
     ::connection-status-channel
     :dungeonstrike.websocket/on-start-channel
     ::on-start-channel})

   ::test-runner
   (component/using
    (test-runner/TestRunner. options)
    {:dungeonstrike.test-runner/message-sender
     ::websocket
     :dungeonstrike.test-runner/test-recordings-path
     ::test-recordings-path
     :dungeonstrike.test-runner/debug-log-channel
     ::debug-log-channel})

   ::debug-gui
   (component/using
    (gui/DebugGui.)
    {:dungeonstrike.gui/logger
     ::logger
     :dungeonstrike.gui/test-runner
     ::test-runner
     :dungeonstrike.gui/message-sender
     ::websocket
     :dungeonstrike.gui/test-recordings-path
     ::test-recordings-path
     :dungeonstrike.gui/debug-log-channel
     ::debug-log-channel
     :dungeonstrike.gui/connection-status-channel
     ::connection-status-channel})))

; Atom containing the system -- for development use only
(defonce system (atom nil))

(defn create-and-start-system
  "Creates and starts the system. The `options` map should be populated with
   command-line options passed to the application. Returns the started system
   map."
  [options]
  (let [started (component/start (create-system options))
        on-start-channel (::on-start-channel started)]
    (async/go-loop []
      (when-some [function (<! on-start-channel)]
        (function)
        (recur)))
    started))

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
  (reset! system (create-and-start-system {})))

(def ^:private cli-options
  [[nil "--help" "Print this help message and quit"]
   [nil "--root-path PATH" "Specifies the path to the project filesystem root."]
   [nil "--port PORT" "Specifies the port for the websocket server."]
   [nil "--run-test TEST"
    "Connect to the client and run the integration test TEST"]
   [nil "--run-all-tests"
    "Connect to the client and run all integration tests."]])

(defn exit [status msg]
  (println msg)
  (System/exit status))

(defn -main [& args]
  (let [{:keys [options arguments summary]} (cli/parse-opts args cli-options)]
    (cond
      (:help options)
      (exit 0 summary)

      :otherwise
      (create-and-start-system options))))
