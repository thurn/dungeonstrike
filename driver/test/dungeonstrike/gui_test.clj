(ns dungeonstrike.gui-test
  (:require [dungeonstrike.gui :as gui]
            [dungeonstrike.nodes :as nodes]
            [clojure.test :as test :refer [deftest is]]))

(deftest client-connected
  (is (some #{(nodes/new-effect :debug-gui [:gui-state :#send-button
                                            {:enabled? true}])}
            (nodes/evaluate :m/client-connected
                            {:m/client-log-file-path "foo/bar"
                             :m/client-id "1"}))))

(deftest client-disconnected
  (is (= (nodes/new-effect :debug-gui [:gui-state :#send-button
                                       {:enabled? false}])
         (nodes/evaluate :r/client-disconnected))))

(deftest message-selected
  (is (= (nodes/new-effect :debug-gui [:persistent-state :#message-form
                                       {:selected :m/quit-game}])
         (nodes/evaluate :r/message-selected {:m/message-type :m/quit-game}))))
