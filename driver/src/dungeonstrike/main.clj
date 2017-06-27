(ns dungeonstrike.main
  "Main entry point when invoked from the command line."
  (:gen-class)
  (:require [clojure.core.async :as async]
            [clojure.spec :as s]
            [clojure.spec.test :as spec-test]
            [clojure.tools.cli :as cli]
            [dungeonstrike.exception-handler :as exception-handler]
            [dungeonstrike.log-tailer :as log-tailer]
            [dungeonstrike.logger :as logger]
            [dungeonstrike.reconciler :as reconciler]
            [dungeonstrike.request-handlers]
            [dungeonstrike.requests :as requests]
            [dungeonstrike.test-runner]
            [dungeonstrike.websocket]
            [mount.core :as mount]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(defmethod reconciler/valid-request? :default [_ request]
  (s/valid? :r/request request))

(defmethod reconciler/valid-domain? :default [_ _] true)

(defmethod reconciler/log :default [& args]
  (apply println args))

(defn- start-reconciler
  "Starts a go loop which monitors the system `requests-channel` for new
   requests intended for execution by `reconciler/execute!`."
  []
  (async/go-loop []
    (when-let [request (async/<! requests/requests-channel)]
      (reconciler/execute! (requests/request-type request) request)
      (recur))))

(mount/defstate reconciler
  :start (start-reconciler)
  :stop (async/close! requests/requests-channel))

(defn start!
  "Starts the system via the mount framework."
  [arguments]
  (spec-test/instrument)
  (exception-handler/set-default-exception-handler!)
  (mount/start-with-args arguments))

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
      (start! options)

      :otherwise
      (exit 1 (str "Unrecognized flag(s).\n"
                   options "\n" arguments "\n" summary)))))
