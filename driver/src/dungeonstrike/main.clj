(ns dungeonstrike.main
  "Main entry point when invoked from the command line."
  (:gen-class)
  (:require [clojure.core.async :as async :refer [<!]]
            [clojure.tools.cli :as cli]
            [com.stuartsierra.component :as component]
            [dungeonstrike.channels :as channelz]
            [dungeonstrike.log-tailer]
            [dungeonstrike.logger :as logger :refer [die!]]
            [dungeonstrike.nodes :as nodes]
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

(extend-type (class (async/chan))
  component/Lifecycle
  (start [channel]
    channel)
  (stop [channel]
    (when channel (async/close! channel))))

(defn channels
  "Returns a map containing all shared communication channels in the system, as
   well as required `pub` objects for those channels. Pubs can be extracted by
   key and passed to the `sub` function to create a new subscriber channel."
  []
  (let [requests-channel (async/chan (async/dropping-buffer 1024))]
    {:requests-channel requests-channel}))

(defn core-system
  "Returns a new component framework 'System Map' which instantiates all
   components used by the core application and binds their dependencies.
   `options` should contain any command-line options passed on start.
   `channels` should contain system-wide channels and pubs."
  [options channels]
  (merge channels
         {:driver-log-path
          (get-path options :driver "logs/driver_logs.txt")

          :client-log-path
          (get-path options :client "Logs/client_logs.txt")

          :connection-status-channel
          (channelz/new-channel
           [(async/sliding-buffer 1024)]
           [:dungeonstrike.gui/connection-status-channel])

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
            :dungeonstrike.websocket/incoming-messages-channel
            :requests-channel
            :dungeonstrike.websocket/connection-status-channel
            :connection-status-channel})}))

(defn test-system
  "Returns a new system map which combines the elements of `core-system` with
   additional components required for running integration tests."
  [options channels]
  (merge (core-system options channels)
         {:test-recordings-path
          (get-path options :tests "recordings")

          :debug-log-channel
          (channelz/new-channel
           [(async/sliding-buffer 1024) logger/debug-log-transducer]
           [:dungeonstrike.test-runner/debug-log-channel])

          :log-tailer
          (component/using
           (LogTailer. options)
           {:dungeonstrike.log-tailer/logger
            :logger
            :dungeonstrike.log-tailer/debug-log-channel
            :debug-log-channel
            :dungeonstrike.log-tailer/log-file-path
            :driver-log-path})

          :test-runner
          (component/using
           (TestRunner. options)
           {:dungeonstrike.test-runner/message-sender
            :websocket
            :dungeonstrike.test-runner/test-recordings-path
            :test-recordings-path
            :dungeonstrike.test-runner/debug-log-channel
            :debug-log-channel})}))

(defn- request-type
  "Returns the appropriate node keyword to run for a given request."
  [request]
  (cond
    (:m/message-type request) (:m/message-type request)
    (:request-type request) (:request-type request)
    :otherwise (die! nil "Request is missing a :request-type" request)))

(defn- start-nodes
  "Starts a go loop which monitors the system `requests-channel` for new
   requests intended for excecution by `nodes/execute!`."
  [system]
  (let [query-handlers (into {} (filter
                                 #(satisfies? nodes/QueryHandler (val %))
                                 system))
        effect-handlers (into {} (filter
                                  #(satisfies? nodes/EffectHandler (val %))
                                  system))
        requests-channel (:requests-channel system)]
    (async/go-loop []
      (when-let [request (<! requests-channel)]
        (nodes/execute! (request-type request)
                        request
                        query-handlers
                        effect-handlers)
        (recur)))))

(defn create-and-start-system
  "Creates and starts the system specified by `system-fn`. The `options` map
   should be populated with command-line options passed to the application.
   Returns the started system map."
  [system-fn options]
  (let [system-args (flatten (into [] (system-fn options (channels))))
        system (component/start (apply component/system-map system-args))]
    (start-nodes system)
    system))

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
