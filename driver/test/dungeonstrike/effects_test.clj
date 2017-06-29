(ns dungeonstrike.effects-test
  (:require [dungeonstrike.effects :as effects]
            [clojure.core.async :as async]
            [clojure.test :as test :refer [deftest is]]
            [orchestra.spec.test :as orchestra]))

(orchestra/instrument)

;; Disable fine-grained spec validation for simplicity
(defmethod effects/effect-spec :default [_] map?)
(defmethod effects/request-spec :default [_] map?)

(def numbers (atom #{}))

(def logs (atom []))

(test/use-fixtures :each
  (fn [f]
    (reset! numbers #{})
    (reset! logs [])
    (f)))

(defn- run-execute!
  ([request-type] (run-execute! request-type {} {}))
  ([request-type request] (run-execute! request-type request {}))
  ([request-type request options]
   (let [handler {:exception-handler identity}
         channel (effects/execute! request-type request (merge handler options))
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
  (effects/effect ::number ::number 4))

(defmethod effects/evaluate ::five [_]
  (effects/effect ::number ::number 5))

(deftest simple-execution
  (run-execute! ::four {})
  (is (= #{4} @numbers)))

(defmethod effects/evaluate ::four-five [_]
  [(effects/effect ::number ::number 4)
   (effects/effect ::number ::number 5)])

(deftest two-effects
  (run-execute! ::four-five {})
  (is (= #{4 5} @numbers)))

(defmethod effects/evaluate ::plus-one
  [{:keys [::input]}]
  (effects/effect ::number ::number (inc input)))

(deftest request-execution
  (run-execute! ::plus-one {::input 2})
  (is (= #{3} @numbers)))

(defn- logger [log-type & args]
  (swap! logs conj [log-type (into [] args)]))

(deftest test-log-fn
  (run-execute! ::plus-one {::input 2} {:log-fn logger})
  (is (=
       [[:request-received
         [{:request-type :dungeonstrike.effects-test/plus-one,
           :request #:dungeonstrike.effects-test{:input 2},
           :queries []}]]
        [:queries-completed
         [:request-type :dungeonstrike.effects-test/plus-one :results [{}]]]
        [:evaluated
         [{:request-type :dungeonstrike.effects-test/plus-one,
           :effects [::number]}]]
        [:done
         [{:request-type :dungeonstrike.effects-test/plus-one,
           :results :done}]]]
       @logs)))

(defmethod effects/query ::user-id
  [_ user-id]
  (async/go
    (Thread/sleep 10)
    {::user-age (* user-id 5)}))

(defmethod effects/evaluate ::double-age
  [{:keys [::user-age]}]
  (effects/effect ::number ::number (* 2 user-age)))

(deftest test-query
  (run-execute! ::double-age {::user-id 10})
  (is (= #{100} @numbers)))

(deftest test-query-logs
  (run-execute! ::double-age {::user-id 10} {:log-fn logger})
  (is (= [[:request-received
           [{:request-type :dungeonstrike.effects-test/double-age,
             :request #:dungeonstrike.effects-test{:user-id 10},
             :queries [:dungeonstrike.effects-test/user-id]}]]
          [:queries-completed
           [:request-type
            :dungeonstrike.effects-test/double-age
            :results
            [#:dungeonstrike.effects-test{:user-age 50}]]]
          [:evaluated
           [{:request-type :dungeonstrike.effects-test/double-age,
             :effects [:dungeonstrike.effects-test/number]}]]
          [:done
           [{:request-type :dungeonstrike.effects-test/double-age,
             :results :done}]]]
         @logs)))

(defmethod effects/query ::car-id
  [_ car-id]
  (async/go
    (Thread/sleep 5)
    {::car-number car-id}))

(defmethod effects/evaluate ::age-and-car-number
  [{:keys [::user-age ::car-number]}]
  (effects/effect ::number ::number (+ user-age car-number)))

(deftest test-two-queries
  (run-execute! ::age-and-car-number {::user-id 10 ::car-id -2})
  (is (= #{48} @numbers)))

(deftest unused-query
  (run-execute! ::double-age {::user-id 10 ::car-id -2})
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
  (run-execute! ::async-double {::input 3})
  (is (= #{6} @numbers)))

(deftest test-log-async-effect
  (run-execute! ::async-double {::input 3} {:log-fn logger})
  (is (= [[:request-received
           [{:request-type :dungeonstrike.effects-test/async-double,
             :request #:dungeonstrike.effects-test{:input 3},
             :queries []}]]
          [:queries-completed
           [:request-type
            :dungeonstrike.effects-test/async-double
            :results
            [{}]]]
          [:evaluated
           [{:request-type :dungeonstrike.effects-test/async-double,
             :effects [:dungeonstrike.effects-test/async-number]}]]
          [:done
           [{:request-type :dungeonstrike.effects-test/async-double,
             :results [[:finished 6]]}]]]
         @logs)))

(defmethod effects/evaluate ::optional
  [{:keys [::input]}]
  [(effects/optional-effect ::number ::number (+ 1 input))
   (effects/optional-effect ::missing-effect ::missing (+ 3 input))])

(deftest optional-effects
  (run-execute! ::optional {::input 2})
  (is (= #{3} @numbers)))

(deftest log-optional-effects
  (run-execute! ::optional {::input 2} {:log-fn logger})
  (is (= [[:request-received
           [{:request-type :dungeonstrike.effects-test/optional,
             :request #:dungeonstrike.effects-test{:input 2},
             :queries []}]]
          [:queries-completed
           [:request-type :dungeonstrike.effects-test/optional :results [{}]]]
          [:evaluated
           [{:request-type :dungeonstrike.effects-test/optional,
             :effects
             [:dungeonstrike.effects-test/number
              :dungeonstrike.effects-test/missing-effect]}]]
          [:optional-effect-ignored
           [{:effect-type :dungeonstrike.effects-test/missing-effect}]]
          [:done
           [{:request-type :dungeonstrike.effects-test/optional,
             :results :done}]]]
         @logs)))

(defmethod effects/query ::invalid-result
  [_ _]
  nil)

(deftest invalid-query-result
  (is (thrown? RuntimeException
               (run-execute! ::four {::invalid-result 2}))))

(defmethod effects/query ::invalid-channel-result
  [_ _]
  (async/go
    (Thread/sleep 10)
    17))

(deftest invalid-channel-result
  (is (thrown? RuntimeException
               (run-execute! ::four
                             {::invalid-channel-result 2}))))

(defmethod effects/evaluate ::invalid-evaluation
  [_]
  17)

(deftest invalid-evaluation
  (is (thrown? RuntimeException
               (run-execute! ::invalid-evaluation))))

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
  nil)

(deftest query-throws-exception
  (is (thrown? NegativeArraySizeException
               (run-execute! ::nil
                             {::normal-query 1 ::query-throws-exception 2}))))

(defmethod effects/apply! ::effect-throws-exception
  [_]
  (async/go
    (Thread/sleep 10)
    (NegativeArraySizeException. "fake effect failure")))

(defmethod effects/evaluate ::create-exception
  [_]
  (effects/effect ::effect-throws-exception))

(deftest effect-throws-exception
  (is (thrown? NegativeArraySizeException
               (run-execute! ::create-exception))))
