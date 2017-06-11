(ns dungeonstrike.log-tailer-test
  (:require [dungeonstrike.log-tailer :as log-tailer]
            [dungeonstrike.nodes :as nodes]
            [clojure.test :as test :refer [deftest is]]))

(deftest client-connected
  (is (= (nodes/new-effect :log-tailer "foo/bar")
         (nodes/evaluate :m/client-connected
                         {:m/client-log-file-path "foo/bar"}))))
