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
