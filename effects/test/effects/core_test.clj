(ns effects.core-test
  (:require [effects.core :as effects]
            [clojure.core.async :as async]
            [clojure.test :as test :refer [deftest is]]
            [orchestra.spec.test :as orchestra]))
(orchestra/instrument)

(def numbers (atom #{}))

(def logs (atom []))

(test/use-fixtures :each
  (fn [f]
    (reset! numbers #{})
    (reset! logs [])
    (f)))

(defn- run-execute!
  ([request] (run-execute! request {}))
  ([request options]
   (let [handler {:exception-handler identity}
         channel (effects/execute! request (merge handler options))
         timeout (async/timeout 1000)
         [val port] (async/alts!! [channel timeout])]
     (when (= port timeout)
       (throw (RuntimeException. "Test timed out!")))
     (when (instance? Throwable val)
       (throw val))
     (is (= :done val)))))

(defmethod effects/apply! ::number [{:keys [::number]}]
  (swap! numbers conj number))

(defmethod effects/evaluate ::four [_]
  [(effects/effect ::number ::number 4)])

(defmethod effects/evaluate ::five [_]
  [(effects/effect ::number ::number 5)])

(deftest simple-execution
  (run-execute! (effects/request ::four))
  (is (= #{4} @numbers)))

(defmethod effects/evaluate ::four-five [_]
  [(effects/effect ::number ::number 4)
   (effects/effect ::number ::number 5)])

(deftest two-effects
  (run-execute! (effects/request ::four-five))
  (is (= #{4 5} @numbers)))

(defmethod effects/evaluate ::plus-one
  [{:keys [::input]}]
  [(effects/effect ::number ::number (inc input))])

(deftest request-execution
  (run-execute! (effects/request ::plus-one ::input 2))
  (is (= #{3} @numbers)))

(defn- logger [log-type & args]
  (swap! logs conj [log-type (into [] args)]))

(deftest test-log-fn
  (run-execute! (effects/request ::plus-one ::input 2) {:log-fn logger})
  (is (=
       [[:request-received
         [{:request-type ::plus-one,
           :request (effects/request ::plus-one ::input 2)
           :queries []}]]
        [:evaluated
         [{:request-type ::plus-one,
           :effects [::number]}]]
        [:done
         [{:request-type ::plus-one,
           :results :done}]]]
       @logs)))

(defmethod effects/query ::user-id
  [_ user-id]
  (async/go
    (Thread/sleep 10)
    {::user-age (* user-id 5)}))

(defmethod effects/evaluate ::double-age
  [{:keys [::user-age]}]
  [(effects/effect ::number ::number (* 2 user-age))])

(deftest test-query
  (run-execute! (effects/request ::double-age ::user-id 10))
  (is (= #{100} @numbers)))

(deftest test-query-logs
  (run-execute! (effects/request ::double-age ::user-id 10) {:log-fn logger})
  (is (= [[:request-received
           [{:request-type ::double-age,
             :request (effects/request ::double-age ::user-id 10)
             :queries [::user-id]}]]
          [:queries-completed
           [{:request-type
             ::double-age
             :results
             [#::{:user-age 50}]}]]
          [:evaluated
           [{:request-type ::double-age,
             :effects [::number]}]]
          [:done
           [{:request-type ::double-age,
             :results :done}]]]
         @logs)))

(defmethod effects/query ::car-id
  [_ car-id]
  (async/go
    (Thread/sleep 5)
    {::car-number car-id}))

(defmethod effects/evaluate ::age-and-car-number
  [{:keys [::user-age ::car-number]}]
  [(effects/effect ::number ::number (+ user-age car-number))])

(deftest test-two-queries
  (run-execute! (effects/request ::age-and-car-number
                                 ::user-id 10 ::car-id -2))
  (is (= #{48} @numbers)))

(deftest unused-query
  (run-execute! (effects/request ::double-age ::user-id 10 ::car-id -2))
  (is (= #{100} @numbers)))

(defmethod effects/apply! ::async-number
  [{:keys [::number]}]
  (async/go
    (Thread/sleep 10)
    (swap! numbers conj number)
    [:finished number]))

(defmethod effects/evaluate ::async-double
  [{:keys [::input]}]
  [(effects/effect ::async-number ::number (* 2 input))])

(deftest test-async-effect
  (run-execute! (effects/request ::async-double ::input 3))
  (is (= #{6} @numbers)))

(deftest test-log-async-effect
  (run-execute! (effects/request ::async-double ::input 3) {:log-fn logger})
  (is (= [[:request-received
           [{:request-type ::async-double,
             :request (effects/request ::async-double ::input 3)
             :queries []}]]
          [:evaluated
           [{:request-type ::async-double,
             :effects [::async-number]}]]
          [:done
           [{:request-type ::async-double,
             :results [[:finished 6]]}]]]
         @logs)))

(defmethod effects/evaluate ::optional
  [{:keys [::input]}]
  [(effects/optional-effect ::number ::number (+ 1 input))
   (effects/optional-effect ::missing-effect ::missing (+ 3 input))])

(deftest optional-effects
  (run-execute! (effects/request ::optional ::input 2))
  (is (= #{3} @numbers)))

(deftest log-optional-effects
  (run-execute! (effects/request ::optional ::input 2) {:log-fn logger})
  (is (= [[:request-received
           [{:request-type ::optional,
             :request (effects/request ::optional ::input 2)
             :queries []}]]
          [:evaluated
           [{:request-type ::optional,
             :effects
             [::number
              ::missing-effect]}]]
          [:optional-effect-ignored
           [{:effect-type ::missing-effect}]]
          [:done
           [{:request-type ::optional,
             :results :done}]]]
         @logs)))

(defmethod effects/query ::invalid-result
  [_ _]
  nil)

(deftest invalid-query-result
  (is (thrown? RuntimeException
               (run-execute! (effects/request ::four ::invalid-result 2)))))

(defmethod effects/query ::invalid-channel-result
  [_ _]
  (async/go
    (Thread/sleep 10)
    17))

(deftest invalid-channel-result
  (is (thrown? RuntimeException
               (run-execute! (effects/request ::four
                                              ::invalid-channel-result 2)))))

(defmethod effects/evaluate ::invalid-evaluation
  [_]
  [17])

(deftest invalid-evaluation
  (is (thrown? RuntimeException
               (run-execute! (effects/request ::invalid-evaluation)))))

(defmethod effects/query ::normal-query
  [_ _]
  (async/go
    (Thread/sleep 5)
    {::output 15}))

(defmethod effects/query ::query-throws-exception
  [_ _]
  (async/go
    (Thread/sleep 10)
    (NegativeArraySizeException. "fake query failure")))

(defmethod effects/evaluate ::nil
  [_]
  [nil])

(deftest query-throws-exception
  (is (thrown? NegativeArraySizeException
               (run-execute! (effects/request ::nil
                                              ::normal-query 1
                                              ::query-throws-exception 2)))))

(defmethod effects/apply! ::effect-throws-exception
  [_]
  (async/go
    (Thread/sleep 10)
    (NegativeArraySizeException. "fake effect failure")))

(defmethod effects/evaluate ::create-exception
  [_]
  [(effects/effect ::effect-throws-exception)])

(deftest effect-throws-exception
  (is (thrown? NegativeArraySizeException
               (run-execute! (effects/request ::create-exception)))))
