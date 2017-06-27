(ns dungeonstrike.request-handlers-test
  (:require [dungeonstrike.request-handlers :as request-handlers]
            [dungeonstrike.reconciler :as reconciler]
            [clojure.test :as test :refer [deftest is]]))

(deftest client-connected
  (is (= #:d{:client-log-files #{"foo/client_logs.txt"}
             :current-client-id "C:0z+v2RXWOUiP7er+KAZsrw"
             :gui-configuration {:#send-button {:enabled? true}}}
         (reconciler/evaluate :m/client-connected
                              #:m{:client-log-file-path "foo/client_logs.txt"
                                  :client-id "C:0z+v2RXWOUiP7er+KAZsrw"
                                  :message-id "M:/ydkmbevzEmE4yE/XjfbJA"
                                  :message-type :m/client-connected}
                              {}))))

(deftest client-disconnected
  (is (= #:d{:gui-configuration {:#send-button {:enabled? false}}}
         (reconciler/evaluate :r/client-disconnected {}
                              #:d{:gui-configuration {:#send-button
                                                      {:enabled? true}}}))))
