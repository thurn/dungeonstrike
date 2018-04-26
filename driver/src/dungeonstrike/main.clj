(ns dungeonstrike.main
  "Main entry point when invoked from the command line."
  (:gen-class)
  (:require [clojure.core.async :as async]
            [clojure.spec.alpha :as s]
            [clojure.tools.cli :as cli]
            [dungeonstrike.exception-handler :as exception-handler]
            [dungeonstrike.log-tailer :as log-tailer]
            [dungeonstrike.logger :as logger]
            [dungeonstrike.request-handlers]
            [dungeonstrike.requests :as requests]
            [dungeonstrike.test-runner]
            [dungeonstrike.websocket]
            [dungeonstrike.messages-old]
            [effects.core :as effects]
            [mount.core :as mount]
            [orchestra.spec.test :as orchestra]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(defn- effects-log-fn
  "Log function for the actions of the 'effects system."
  [log-type {:keys [:request-type]}]
  (case log-type
    :request-received
    (logger/log "Received request" request-type)
    :queries-completed
    (logger/log "Completed query execution" request-type)
    :evaluated
    (logger/log "Evaluated request" request-type)
    :optional-effect-ignored
    (logger/log "Ignored optional effect")
    :done
    (logger/log "Finished processing request" request-type)))

(defn- start-effects
  "Starts a go loop which monitors the system `requests-channel` for new
   requests intended for execution by `effects/execute!`. Returns a channel
   which will receive a Throwable object on error."
  []
  (async/go-loop []
    (when-let [request (async/<! requests/requests-channel)]
      (async/<! (effects/execute! request {:log-fn effects-log-fn}))
      (recur))))

(mount/defstate effects
  :start (start-effects)
  :stop (async/close! requests/requests-channel))

(defn start!
  "Starts the system via the mount framework."
  [arguments]
  (orchestra/instrument)
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
