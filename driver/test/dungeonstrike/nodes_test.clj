(ns dungeonstrike.nodes-test
  (:require [dungeonstrike.nodes :as nodes]
            [clojure.test :as test :refer [deftest is]]))

(nodes/defnode ::four [] 4)

(deftest call-no-arguments
  (is (= 4 (four))))

(deftest execution-step-no-arguments
  (is (=
       {::four 4}
       (nodes/execution-step {} ::four))))

(nodes/defnode ::plus-one [::four] (+ 1 four))

(deftest call-one-argument
  (is (= 5 (plus-one 4))))

(deftest execution-step-one-argument
  (is (=
       {::four 4, ::plus-one 5}
       (nodes/execution-step {} ::plus-one))))

(nodes/defnode ::plus-input [::input] (+ 2 input))

(deftest execution-step-with-input
  (is (=
       {::input 1, ::plus-input 3}
       (nodes/execution-step {::input 1} ::plus-input))))

(nodes/defnode ::plus-two
  [::plus-one]
  (+ 2 plus-one))

(deftest chain-two
  (is (=
       {::four 4, ::plus-one 5, ::plus-two 7}
       (nodes/execution-step {} ::plus-two))))

(nodes/defnode ::sum
  [::plus-two ::plus-input]
  (+ plus-two plus-input))

(deftest two-args
  (is (=
       {::input 1, ::four 4, ::plus-one 5, ::plus-two 7, ::plus-input 3,
        ::sum 10}
       (nodes/execution-step {::input 1} ::sum))))

(nodes/defnode ::returns-nil [::sum] (do nil))

(nodes/defnode ::depends-on-nil [::returns-nil] (str returns-nil))

(deftest returns-nil-exception
  (is (thrown? RuntimeException (nodes/execution-step {} ::returns-nil)))
  (is (thrown? RuntimeException (nodes/execution-step {} ::depends-on-nil))))

(nodes/defnode ::missing-input [::sum ::missing] (+ 1 sum))

(nodes/defnode ::depends-on-missing-input [::missing-input] (+ 1 missing-input))

(deftest missing-input
  (is (thrown? RuntimeException
               (nodes/execution-step {} ::missing-input)))
  (is (thrown? RuntimeException
               (nodes/execution-step {} ::depends-on-missing-input))))

(def test-query (nodes/new-query :test [:arg]))

(nodes/defnode ::return-query
  []
  test-query)

(deftest step-query
  (is (=
       {::return-query test-query}
       (nodes/execution-step {} ::return-query))))

(nodes/defnode ::other
  []
  "other")

(nodes/defnode ::depends-on-query
  [::return-query ::other]
  (+ 1 return-query))

(deftest step-query-dependency
  (is (=
       {::return-query test-query, ::other "other"}
       (nodes/execution-step {} ::depends-on-query))))

(nodes/defnode ::second-level
  [::depends-on-query ::return-query])

(deftest step-query-dependency
  (is (=
       {::return-query test-query, ::other "other"}
       (nodes/execution-step {} ::second-level))))

(nodes/defnode ::a [] [:a])

(nodes/defnode ::b [] [:b])

(nodes/defnode ::c [::a] [:c a])

(nodes/defnode ::d [::a ::b] [:d a b])

(nodes/defnode ::e [::c ::d] [:e c d])

(deftest execute-queries-no-queries
  (is (=
       [:e [:c [:a]] [:d [:a] [:b]]]
       (nodes/execute-queries ::e {} {}))))

(def query1 (nodes/new-query :test-query [1]))

(def query2 (nodes/new-query :test-query [2]))

(defrecord TestQueryHandler []
  nodes/QueryHandler
  (query [this arguments]
    (* 10 (first arguments))))

(nodes/defnode ::f [] query1)

(deftest execute-simple-query
  (is (= 10
         (nodes/execute-queries ::f {} {:test-query (TestQueryHandler.)}))))

(nodes/defnode ::g [::f ::e] (+ f 1))

(deftest execute-depends-on-query
  (is (= 11
         (nodes/execute-queries ::g {} {:test-query (TestQueryHandler.)}))))

(nodes/defnode ::h [::g] query2)

(deftest execute-second-query
  (is (= 20
         (nodes/execute-queries ::h {} {:test-query (TestQueryHandler.)}))))

(nodes/defnode ::i [::g ::h] (+ g h))

(deftest two-queries
  (is (= 31
         (nodes/execute-queries ::i {} {:test-query (TestQueryHandler.)}))))

(nodes/defnode ::with-input [::i ::input] (* i input))

(deftest query-with-request-input
  (is (= 62
         (nodes/execute-queries ::with-input {::input 2}
                                {:test-query (TestQueryHandler.)}))))

(deftest no-handler
  (is (thrown? RuntimeException (nodes/execute-queries ::i {} {}))))

(defrecord TestNilQueryHandler []
  nodes/QueryHandler
  (query [this arguments] nil))

(deftest nil-query-result
  (is (thrown? RuntimeException
               (nodes/execute-queries ::i {}
                                      {:test-query (TestNilQueryHandler.)}))))

(def effect1 (nodes/new-effect :test-effect [5]))

(def effect2 (nodes/new-effect :test-effect [6]))

(defrecord TestEffectHandler [state]
  nodes/EffectHandler
  (apply! [this arguments]
    (swap! state + (first arguments))))

(nodes/defnode ::j [] effect1)

(deftest simple-effect
  (let [state (atom 0)]
    (nodes/execute! ::j {} {} {:test-effect (TestEffectHandler. state)})
    (is (= 5 @state))))

(nodes/defnode ::k [] [effect1 effect2])

(deftest two-simple-effects
  (let [state (atom 0)]
    (nodes/execute! ::k {} {} {:test-effect (TestEffectHandler. state)})
    (is (= 11 @state))))

(nodes/defnode ::l [::g ::h ::j] j)

(deftest query-produce-effect
  (let [state (atom 0)]
    (nodes/execute! ::l
                    {}
                    {:test-query (TestQueryHandler.)}
                    {:test-effect (TestEffectHandler. state)})
    (is (= 5 @state))))

(deftest no-effect-handler
  (is (thrown? RuntimeException (nodes/execute! ::l {} {} {}))))
