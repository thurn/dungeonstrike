(ns dungeonstrike.nodes-test
  (:require [dungeonstrike.nodes :as nodes]
            [clojure.test :as test :refer [deftest is]]))

(nodes/defnode ::four [] 4)

(deftest call-no-arguments
  (is (= 4 (four))))

(deftest evaluation-step-no-arguments
  (is (=
       {::four 4}
       (nodes/evaluation-step {} ::four))))

(nodes/defnode ::plus-one [::four] (+ 1 four))

(deftest call-one-argument
  (is (= 5 (plus-one 4))))

(deftest evaluation-step-one-argument
  (is (=
       {::four 4, ::plus-one 5}
       (nodes/evaluation-step {} ::plus-one))))

(nodes/defnode ::plus-input [::input] (+ 2 input))

(deftest evaluation-step-with-input
  (is (=
       {::input 1, ::plus-input 3}
       (nodes/evaluation-step {::input 1} ::plus-input))))

(nodes/defnode ::plus-two
  [::plus-one]
  (+ 2 plus-one))

(deftest chain-two
  (is (=
       {::four 4, ::plus-one 5, ::plus-two 7}
       (nodes/evaluation-step {} ::plus-two))))

(nodes/defnode ::sum
  [::plus-two ::plus-input]
  (+ plus-two plus-input))

(deftest two-args
  (is (=
       {::input 1, ::four 4, ::plus-one 5, ::plus-two 7, ::plus-input 3,
        ::sum 10}
       (nodes/evaluation-step {::input 1} ::sum))))

(nodes/defnode ::returns-nil [::sum] (do nil))

(nodes/defnode ::depends-on-nil [::returns-nil] (str returns-nil))

(deftest returns-nil-exception
  (is (thrown? RuntimeException (nodes/evaluation-step {} ::returns-nil)))
  (is (thrown? RuntimeException (nodes/evaluation-step {} ::depends-on-nil))))

(nodes/defnode ::missing-input [::sum ::missing] (+ 1 sum))

(nodes/defnode ::depends-on-missing-input [::missing-input] (+ 1 missing-input))

(deftest missing-input
  (is (thrown? RuntimeException
               (nodes/evaluation-step {} ::missing-input)))
  (is (thrown? RuntimeException
               (nodes/evaluation-step {} ::depends-on-missing-input))))

(def test-query (nodes/new-query :test [:arg]))

(nodes/defnode ::return-query
  []
  test-query)

(deftest step-query
  (is (=
       {::return-query test-query}
       (nodes/evaluation-step {} ::return-query))))

(nodes/defnode ::other
  []
  "other")

(nodes/defnode ::depends-on-query
  [::return-query ::other]
  (+ 1 return-query))

(deftest step-query-dependency
  (is (=
       {::return-query test-query, ::other "other"}
       (nodes/evaluation-step {} ::depends-on-query))))

(nodes/defnode ::second-level
  [::depends-on-query ::return-query])

(deftest step-query-dependency
  (is (=
       {::return-query test-query, ::other "other"}
       (nodes/evaluation-step {} ::second-level))))

(nodes/defnode ::a [] [:a])

(nodes/defnode ::b [] [:b])

(nodes/defnode ::c [::a] [:c a])

(nodes/defnode ::d [::a ::b] [:d a b])

(nodes/defnode ::e [::c ::d] [:e c d])

(deftest evaluate-no-queries
  (is (=
       [:e [:c [:a]] [:d [:a] [:b]]]
       (nodes/evaluate ::e {} {}))))

(deftest missing-top-level-node
  (is (= #{}
         (nodes/evaluate ::some-node-which-does-not-exist))))

(def query1 (nodes/new-query :test-query [1]))

(def query2 (nodes/new-query :test-query [2]))

(defrecord TestQueryHandler []
  nodes/QueryHandler
  (query [this arguments]
    (* 10 (first arguments))))

(nodes/defnode ::f [] query1)

(deftest execute-simple-query
  (is (= 10
         (nodes/evaluate ::f {} {:test-query (TestQueryHandler.)}))))

(nodes/defnode ::g [::f ::e] (+ f 1))

(deftest execute-depends-on-query
  (is (= 11
         (nodes/evaluate ::g {} {:test-query (TestQueryHandler.)}))))

(nodes/defnode ::h [::g] query2)

(deftest execute-second-query
  (is (= 20
         (nodes/evaluate ::h {} {:test-query (TestQueryHandler.)}))))

(nodes/defnode ::i [::g ::h] (+ g h))

(deftest two-queries
  (is (= 31
         (nodes/evaluate ::i {} {:test-query (TestQueryHandler.)}))))

(nodes/defnode ::with-input [::i ::input] (* i input))

(deftest query-with-request-input
  (is (= 62
         (nodes/evaluate ::with-input
                         {::input 2}
                         {:test-query (TestQueryHandler.)}))))

(deftest no-handler
  (is (thrown? RuntimeException (nodes/evaluate ::i {} {}))))

(defrecord TestNilQueryHandler []
  nodes/QueryHandler
  (query [this arguments] nil))

(deftest nil-query-result
  (is (thrown? RuntimeException
               (nodes/evaluate ::i
                               {}
                               {:test-query (TestNilQueryHandler.)}))))

(def effect1 (nodes/new-effect :test-effect [5]))

(def effect2 (nodes/new-effect :test-effect [6]))

(defrecord TestEffectHandler [state]
  nodes/EffectHandler
  (execute-effect! [this arguments]
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

(deftest execute-missing-top-level-node
  (is (= #{}
         (nodes/execute! ::some-node-which-does-not-exist))))

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

(nodes/defnode :test/multi [:test/multi-input] (inc multi-input))

(nodes/defnode :test/multi2 [:test/multi2-input-1] (inc multi2-input-1))

(ns dungeonstrike.other-test-namespace
  (:require [dungeonstrike.nodes]))

(dungeonstrike.nodes/defnode :test/multi [:test/multi-input]
  (dec multi-input))

(dungeonstrike.nodes/defnode :test/multi2 [:test/multi2-input-2]
  (dec multi2-input-2))

(dungeonstrike.nodes/defnode :test/two-effects []
  (dungeonstrike.nodes/new-effect :test-effect [5]))

(dungeonstrike.nodes/defnode :test/two-queries []
  (dungeonstrike.nodes/new-query :test-query [2]))

(ns dungeonstrike.nodes-test)

(nodes/defnode ::sum-multi-nodes [:test/multi]
  (reduce + 0 multi))

(nodes/defnode ::sum-multi2 [:test/multi2]
  (reduce + 0 multi2))

(nodes/defnode :test/two-queries []
  (nodes/new-query :test-query [1]))

(nodes/defnode ::use-two-queries [:test/two-queries]
  (reduce + 0 two-queries))

(nodes/defnode ::use-use-two-queries [::use-two-queries]
  (+ 5 use-two-queries))

(nodes/defnode :test/two-effects []
  (nodes/new-effect :test-effect [10]))

(deftest set-binding
  (is (= #{1 3}
         (nodes/evaluate :test/multi {:test/multi-input 2})))
  (is (= 4
         (nodes/evaluate ::sum-multi-nodes {:test/multi-input 2}))))

(deftest different-multi-inputs
  (is (= #{1 10}
         (nodes/evaluate :test/multi2 {:test/multi2-input-1 0
                                       :test/multi2-input-2 11})))
  (is (= 11
         (nodes/evaluate ::sum-multi2 {:test/multi2-input-1 0
                                       :test/multi2-input-2 11}))))

(deftest missing-input
  (is (thrown? RuntimeException (nodes/evaluate :test/multi)))
  (is (thrown? RuntimeException (nodes/evaluate :test/multi2
                                                {:test/multi2-input-1 0})))
  (is (thrown? RuntimeException (nodes/evaluate ::sum-multi2
                                                {:test/multi2-input-2 0}))))

(deftest multi-queries
  (is (= #{10 20}
         (nodes/evaluate :test/two-queries {}
                         {:test-query (TestQueryHandler.)})))
  (is (= 30
         (nodes/evaluate ::use-two-queries {}
                         {:test-query (TestQueryHandler.)})))
  (is (= 35
         (nodes/evaluate ::use-use-two-queries {}
                         {:test-query (TestQueryHandler.)}))))

(deftest multi-effects
  (let [state (atom 0)]
    (nodes/execute! :test/two-effects {} {}
                    {:test-effect (TestEffectHandler. state)})
    (is (= 15 @state))))
