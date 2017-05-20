(ns dungeonstrike.main
  "Main entry point when invoked from the command line."
  (:gen-class)
  (:require [clojure.core.async :as async :refer [<!]]
            [clojure.tools.cli :as cli]
            [com.stuartsierra.component :as component]
            [dungeonstrike.channels :as channels]
            [dungeonstrike.log-tailer]
            [dungeonstrike.logger :as logger]
            [dungeonstrike.test-runner]
            [dungeonstrike.websocket]
            [dungeonstrike.dev :as dev])
  (:import (dungeonstrike.log_tailer LogTailer)
           (dungeonstrike.logger Logger)
           (dungeonstrike.test_runner TestRunner)
           (dungeonstrike.websocket Websocket)))
(dev/require-dev-helpers)

(defn get-path
  "Returns the absolute path to a location at the relative `path` of type
   `type`, based on the command-line flags in `options`."
  [options type path]
  (case type
    :driver
    (str (get options :driver-path
              (System/getProperty "user.dir"))
         "/" path)
    :client
    (str (get options :client-path
              (str (System/getProperty "user.dir") "/../DungeonStrike"))
         "/" path)
    :tests
    (str (get options :tests-path
              (str (System/getProperty "user.dir") "/../tests"))
         "/" path)))

(defn core-system
  "Returns a new component framework 'System Map' which instantiates all
   components used by the core application and binds their dependencies.
   `options` should contain any command-line options passed on start."
  [options]
  {:driver-log-path
   (get-path options :driver "logs/driver_logs.txt")

   :connection-status-channel
   (channels/new-channel
    [(async/sliding-buffer 1024)]
    [:dungeonstrike.gui/connection-status-channel])

   :on-start-channel
   (channels/new-channel
    [(async/dropping-buffer 1024)]
    [])

   :logger
   (component/using
    (Logger. options)
    {:dungeonstrike.logger/log-file-path
     :driver-log-path})

   :websocket
   (component/using
    (Websocket. options)
    {:dungeonstrike.websocket/logger
     :logger
     :dungeonstrike.websocket/connection-status-channel
     :connection-status-channel
     :dungeonstrike.websocket/on-start-channel
     :on-start-channel})})

(defn test-system
  "Returns a new system map which combines the elements of `core-system` with
   additional components required for running integration tests."
  [options]
  (merge (core-system options)
         {:test-recordings-path
          (get-path options :tests "recordings")

          :client-log-path
          (get-path options :client "Logs/client_logs.txt")

          :debug-log-channel
          (channels/new-channel
           [(async/sliding-buffer 1024) logger/debug-log-transducer]
           [:dungeonstrike.test-runner/debug-log-channel])

          :driver-log-tailer
          (component/using
           (LogTailer. options)
           {:dungeonstrike.log-tailer/debug-log-channel
            :debug-log-channel
            :dungeonstrike.log-tailer/log-file-path
            :driver-log-path})

          :client-log-tailer
          (component/using
           (LogTailer. options)
           {:dungeonstrike.log-tailer/debug-log-channel
            :debug-log-channel
            :dungeonstrike.log-tailer/log-file-path
            :client-log-path})

          :test-runner
          (component/using
           (TestRunner. options)
           {:dungeonstrike.test-runner/message-sender
            :websocket
            :dungeonstrike.test-runner/test-recordings-path
            :test-recordings-path
            :dungeonstrike.test-runner/debug-log-channel
            :debug-log-channel})}))

(defn create-and-start-system
  "Creates and starts the system specified by `system-fn`. The `options` map
   should be populated with command-line options passed to the application.
   Returns the started system map."
  [system-fn options]
  (let [system-args (flatten (into [] (system-fn options)))
        started (component/start (apply component/system-map system-args))
        on-start-channel (channels/unwrap (:on-start-channel started))]
    (async/go-loop []
      (when-let [function (<! on-start-channel)]
        (function)
        (recur)))
    started))

(def ^:private cli-options
  [[nil "--help" (str "Print this help message and quit")]
   [nil "--driver-path PATH" "Specifies the path to the driver."]
   [nil "--client-path PATH" "Specifies the path to the client."]
   [nil "--tests-path PATH" "Specifies the path to the tests directory."]
   [nil "--port PORT" "Specifies the port for the websocket server."]
   [nil "--verbose" "Requests verbose test output."]
   [nil "--crash-on-exceptions" "Causes the driver to exit on exceptions."]
   [nil "--test TEST" "Runs integration test TEST (or 'all', or 'changed')."]])

(defn exit [status msg]
  (println msg)
  (System/exit status))

(defn -main [& args]
  (let [{:keys [options arguments summary]} (cli/parse-opts args cli-options)]
    (cond
      (:help options)
      (exit 0 summary)

      (:test options)
      (create-and-start-system test-system options)

      :otherwise
      (exit 1 (str "Unrecognized flag(s).\n"
                   options "\n" arguments "\n" summary)))))
