(ns dungeonstrike.main
  "Main entry point when invoked from the command line."
  (:gen-class)
  (:require [clojure.core.async :as async :refer [<!]]
            [clojure.spec.test :as spec-test]
            [clojure.tools.cli :as cli]
            [dungeonstrike.exception-handler :as exception-handler]
            [dungeonstrike.log-tailer :as log-tailer]
            [dungeonstrike.logger :as logger]
            [dungeonstrike.nodes :as nodes]
            [dungeonstrike.requests :as requests]
            [dungeonstrike.test-runner]
            [dungeonstrike.websocket]
            [mount.core :as mount]
            [dungeonstrike.dev :as dev])
  (:import (dungeonstrike.log_tailer LogEffector)
           (dungeonstrike.logger LoggerEffector)))
(dev/require-dev-helpers)

(defn- start-nodes
  "Starts a go loop which monitors the system `requests-channel` for new
   requests intended for execution by `nodes/execute!`."
  [query-handlers effect-handlers]
  (async/go-loop []
    (when-let [request (<! requests/requests-channel)]
      (nodes/execute! (requests/node-for-request request)
                      request
                      query-handlers
                      effect-handlers)
      (recur))))

(defn start!
  "Starts the system via the mount framework."
  [arguments query-handlers effect-handlers]
  (spec-test/instrument)
  (exception-handler/set-default-exception-handler!)
  (let [result (mount/start-with-args arguments)]
    (start-nodes query-handlers effect-handlers)
    result))

(def ^:private cli-options
  [[nil "--help" "Print this help message and quit"]
   [nil "--driver-path PATH" "Specifies the path to the driver."]
   [nil "--client-path PATH" "Specifies the path to the client."]
   [nil "--tests-path PATH" "Specifies the path to the tests directory."]
   [nil "--port PORT" "Specifies the port for the websocket server."]
   [nil "--verbose" "Requests verbose test output."]
   [nil "--crash-on-exceptions" "Causes the driver to exit on exceptions."]
   [nil "--test TEST" "Runs integration test TEST (or 'all')."]])

(defn exit [status msg]
  (println msg)
  (System/exit status))

(defn -main [& args]
  (let [{:keys [options arguments summary]} (cli/parse-opts args cli-options)]
    (cond
      (:help options)
      (exit 0 summary)

      (:test options)
      (start! options {} {:log-tailer (log-tailer/LogEffector.)
                          :logger (logger/LoggerEffector.)})

      :otherwise
      (exit 1 (str "Unrecognized flag(s).\n"
                   options "\n" arguments "\n" summary)))))
