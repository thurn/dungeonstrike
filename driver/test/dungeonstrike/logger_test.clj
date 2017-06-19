(ns dungeonstrike.logger-test
  (:require [dungeonstrike.logger :as logger]
            [dungeonstrike.nodes :as nodes]
            [clojure.test :as test :refer [deftest is]]))

(deftest client-connected
  (is (some #{(nodes/new-effect :logger "123")}
            (nodes/evaluate :m/client-connected
                            {:m/client-log-file-path "foo/bar"
                             :m/client-id "123"}))))
