(ns dungeonstrike.test-runner
  "Component for running integration tests based on log recordings."
  (:require [clojure.core.async :as async :refer [<! >!]]
            [clojure.edn :as edn]
            [clojure.java.io :as io]
            [clojure.spec :as s]
            [dungeonstrike.channels :as channels]
            [dungeonstrike.messages :as messages]
            [com.stuartsierra.component :as component]
            [dev]))
(dev/require-dev-helpers)

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
   `message-sender` and then listens for logs on `log-channel` and
    verifies that they match the logs in the provided log entry. Returns a
    channel which will receive a value describing the success or failure of the
    test."
  [{:keys [::message-sender ::log-channel]}
   {:keys [:name :entries :message->client]}]
  (messages/send-message! message-sender message->client)
  (let [timeout (async/timeout 10000)]
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
  [test-runner recordings]
  (async/go-loop [[recording & remaining] recordings]
    (when-let [{:keys [:status] :as result}
               (<! (run-recording test-runner recording))]
      (if (or (empty? remaining) (= status :failed))
        result
        (recur remaining)))))

(defn- recording-sequence-for-test
  "Returns the sequence of recording objects consisting of all of the recursive
   prerequisites of `test-name` followed by the recording object for `test-name`
   itself."
  [{:keys [::test-recordings-path] :as test-runner} test-name]
  (let [recording (edn/read-string (slurp (io/file test-recordings-path
                                                   test-name)))]
    (when (empty? recording)
      (throw (RuntimeException. (str "Recording not found" test-name))))
    (if-let [prerequisite (:prerequisite recording)]
      (conj (recording-sequence-for-test test-runner prerequisite) recording)
      [recording])))

(s/fdef run-integration-test :args (s/cat :test-runner ::test-runner
                                          :test-name string?))
(defn run-integration-test
  "Runs a recording-based integration test. Each test recording consists of a
   message to send and a set of recorded logs which are expected in response to
   the message. Recordings can also have a 'prerequisite' recording, which means
   that they expect the named recording to be played first in order to set up
   their test state.

   At a high level, this function connects to the rest of the system, waits for
   startup to complete, recursively runs tests for the chain of prerequisites
   for the recording named in `test-name`, and then sends the message specified
   in this recording and verifies that the subsequent system logs match the
   expected log entries. Each entry is tagged with a log `:source` -- logs with
   the same `:source` must happen in the listed order. Logs with different log
   sources can occur in any order.

   Returns a channel which will receive the result of running the test on
   completion."
  [{:keys [::test-channel] :as test-runner} test-name]
  (let [recordings (recording-sequence-for-test test-runner test-name)
        result-channel (async/chan)]
    (async/go
      (>! test-channel {:recordings recordings
                        :result-channel result-channel})
      (<! result-channel))))

(defn dthurn [tr tn]
  (async/go
    (let [result (<! (run-integration-test tr tn))]
      (println "test result:")
      (pprint result))))

(defn- start-test-runner
  "Starts a go loop which awaits new recordings on `test-channel` and then runs
   and verifies them via `run-recording`."
  [{:keys [::test-channel ::message-sender ::log-channel]
    :as test-runner}]
  (async/go-loop []
    (let [[value port] (async/alts! [test-channel log-channel]
                                    :priority true)]
      (if (= port log-channel)
        ;; If no tests are available, ignore all log entries
        (recur)
        (when-let [{:keys [recordings result-channel]} value]
          (>! result-channel (<! (run-recording-list test-runner recordings)))
          (recur))))))

(def ^:private startup-recording
  {:name "startup"
   :prerequisite nil
   :entries
   [{:message ">> driver connected <<"
     :source "dungeonstrike.websocket"
     :log-type :driver}
    {:message ">> client connected <<"
     :source "DungeonStrike.Source.Messaging.WebsocketManager"
     :log-type :client}]})

(defrecord TestRunner [options]
  component/Lifecycle

  (start [{:keys [::debug-log-channel] :as component}]
    (let [test-channel (async/chan (async/dropping-buffer 1024))
          log-channel (channels/get-tap debug-log-channel ::debug-log-channel)
          result (assoc component
                        ::log-channel log-channel
                        ::test-channel test-channel)]
      (start-test-runner result)
      result))

  (stop [{:keys [::test-channel] :as component}]
    (when test-channel (async/close! test-channel))
    component))

(s/def ::test-runner #(instance? TestRunner %))
