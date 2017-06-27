(ns dungeonstrike.test-runner
  "Component for running integration tests based on log recordings."
  (:require [clojure.core.async :as async :refer [<! >!]]
            [clojure.edn :as edn]
            [clojure.java.io :as io]
            [clojure.spec.alpha :as s]
            [dungeonstrike.logger :as logger]
            [dungeonstrike.messages :as messages]
            [dungeonstrike.paths :as paths]
            [dungeonstrike.websocket :as websocket]
            [mount.core :as mount]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(mount/defstate ^:private test-channel
  :start (async/chan)
  :stop (async/close! test-channel))

(mount/defstate ^:private log-channel
  :start (async/tap logger/debug-log-mult (async/chan)))

(defn- equivalent-log-entries?
  "Returns `true` if two log entries should be considered the same for the
   purposes of test verification."
  [{source1 :source log-type1 :log-type message1 :message}
   {source2 :source log-type2 :log-type message2 :message}]
  (and
   (= source1 source2)
   (= log-type1 log-type2)
   (= message1 message2)))

(defn- run-recording
  "Runs a single recording. Sends the appropriate message for the recording via
  `websocket` and then listens for logs on `log-channel` and verifies that they
  match the logs in the provided log entry. Returns a channel which will receive
  a value describing the success or failure of the test."
  [{:keys [:name :entries :message->client]}]
  (when message->client
    (websocket/send-message! message->client))
  (let [timeout (async/timeout 30000)]
    (async/go-loop [missing-entries (group-by :source entries)]
      (let [[value port] (async/alts! [timeout log-channel])]
        (if (= port timeout)
          ;; Timeout exceeded, return failure value.
          {:status :timeout
           :test-name name
           :message "Test timed out waiting for logs!"
           :detail (flatten (vals (remove #(empty? (val %)) missing-entries)))}

          (let [{:keys [source] :as message} value
                [next & remaining] (missing-entries source)]
            (cond
              ;; If this log entry is expected, remove it from the expected set
              ;; and recur, signalling completion if the expected set is empty.
              (equivalent-log-entries? next message)
              (let [updated (update missing-entries source rest)]
                (if (every? #(empty? (val %)) updated)
                  {:status :success :test-name name}
                  (recur updated)))

              ;; If this log entry is in the expected set, but has arrived too
              ;; early, signal an error.
              (some #(equivalent-log-entries? message %) remaining)
              {:status :out-of-order
               :test-name name
               :message "Test message arrived out of order!"
               :detail message}

              ;; Otherwise, ignore this log entry.
              :otherwise
              (recur missing-entries))))))))

(defn- run-recording-list
  "Runs a sequence of recordings via the `run-recording` function. Returns a
   channel which will receieve either the *final* success value returned or the
   *first* failure value returned."
  [args recordings]
  (async/go-loop [[recording & remaining] recordings]
    (when (:verbose args)
      (println (str "RUNNING: '" (:name recording) "'")))
    (when-let [{:keys [:status] :as result}
               (<! (run-recording recording))]
      (when (and (:verbose args) (= status :success))
        (println (str "SUCCESS: '" (:name recording) "'")))
      (if (or (empty? remaining) (not= status :success))
        result
        (recur remaining)))))

(defn- all-tests
  "Returns a sequence containing the names of all tests in the recordings
   directory which are *not* found as prerequisites of any other test."
  []
  (let [files (filter #(.isFile %)
                      (file-seq (io/file paths/test-recordings-path)))
        recordings (map #(edn/read-string (slurp %)) files)
        prerequisites (into #{} (remove nil? (map :prerequisite recordings)))]
    (remove prerequisites (map :name recordings))))

(declare recording-sequence-for-all-tests)

(defn- recording-sequence-for-test
  "Returns the sequence of recording objects consisting of all of the recursive
   prerequisites of `test-name` followed by the recording object for `test-name`
   itself. Alternativly, if `test-name` is `:all-tests`, runs the recording
   sequence for all tests."
  [test-name]
  (if (= test-name :all-tests)
    (recording-sequence-for-all-tests)
    (let [recording (edn/read-string (slurp (io/file paths/test-recordings-path
                                                     test-name)))]
      (when (empty? recording)
        (throw (RuntimeException. (str "Recording not found" test-name))))
      (if-let [prerequisite (:prerequisite recording)]
        (conj (recording-sequence-for-test prerequisite) recording)
        [recording]))))

(defn- recording-sequence-for-all-tests
  "Returns the sequence of recording objects required to run all recording-based
   tests along with their respective prerequisites."
  []
  (flatten (map #(recording-sequence-for-test (str % ".edn"))
                (all-tests))))

(s/fdef run-integration-test :args
        (s/cat :test-name (s/or :test-name string? :test-keyword keyword?)))
(defn run-integration-test
  "Runs recording-based integration tests. Each test recording consists of a
  message to send and a set of recorded logs which are expected in response to
  the message. Recordings can also have a 'prerequisite' recording, which means
  that they expect the named recording to be played first in order to set up
  their test state. If the argument is a string, the named test is run. if the
  argument is `:all-tests`, then all tests will be run.

  At a high level, this function connects to the rest of the system,
  recursively runs tests for the chain of prerequisites for the recording named
  in `test-name`, and then sends the message specified in this recording and
  verifies that the subsequent system logs match the expected log entries. Each
  entry is tagged with a log `:source` -- logs with the same `:source` must
  happen in the listed order. Logs with different log sources can occur in any
  order.

  Returns a channel which will receive the result of running the test on
  completion."
  [test-name]
  (let [recordings (recording-sequence-for-test test-name)
        result-channel (async/chan)]
    (async/go
      (>! test-channel {:recordings recordings
                        :result-channel result-channel})
      (<! result-channel))))

(def ^:private startup-recording
  {:name "startup"
   :prerequisite nil
   :entries
   [{:message "Client connected"
     :source "dungeonstrike.log-tailer"
     :log-type :driver}]})

(def ^:private shutdown-recording
  {:name "shutdown"
   :prerequisite nil
   :message->client {:m/message-type :m/quit-game
                     :m/message-id "M:QUIT"}
   :entries
   [{:message "Quitting Client"
     :source "DungeonStrike.Source.Services.QuitGame"
     :log-type :client}]})

(defn- recordings-for-args
  "Returns the integration test recording sequence required for the current
  command-line arguments."
  [args]
  (let [test (:test args)]
    (cond
      (= "all" test)
      (concat [startup-recording]
              (recording-sequence-for-all-tests)
              [shutdown-recording])

      :otherwise
      (concat [startup-recording]
              (recording-sequence-for-test test)
              [shutdown-recording]))))

(defn- fail-test
  "Prints a test failure message for the provided test result and then
   terminates the program."
  [args {:keys [:status :message :test-name :detail]}]
  (println (str "FAILURE in '" test-name "' " status))
  (println message)
  (if (:verbose args)
    (pprint detail)
    (println "Pass --verbose to display full failure details."))
  (System/exit 42))

(defn- run-tests-from-args
  "Runs integration tests based on the current command-line arguments."
  [args]
  (async/go
    (let [test (:test args)
          done (async/chan)]
      (println (str "Starting test runner for '" test "'"))
      (>! test-channel {:recordings (recordings-for-args args)
                        :result-channel done})
      (let [{:keys [:status] :as result} (<! done)]
        (if (= status :success)
          (do
            (println (str "Test run '" test "' passed."))
            (System/exit 0))
          (fail-test args result))))))

(defn- start-test-runner
  "Starts a go loop which awaits new recordings on `test-channel` and then runs
   and verifies them via `run-recording`. Runs any tests specified via
  command-line flags."
  [args]
  (async/go-loop []
    (let [[value port] (async/alts! [test-channel log-channel]
                                    :priority true)]
      (if (= port log-channel)
        ;; If no tests are available, ignore all log entries
        (recur)
        (when-let [{:keys [recordings result-channel]} value]
          (>! result-channel (<! (run-recording-list args recordings)))
          (recur)))))
  (when (:test args)
    (run-tests-from-args args)))

(mount/defstate ^:private test-runner :start (start-test-runner (mount/args)))
