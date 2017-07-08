(ns dungeonstrike.request-handlers-test
  (:require [dungeonstrike.logger]
            [dungeonstrike.log-tailer]
            [dungeonstrike.gui]
            [dungeonstrike.request-handlers :as request-handlers]
            [clojure.test :as test :refer [deftest is]]
            [effects.core :as effects]
            [orchestra.spec.test :as orchestra]
            [dungeonstrike.dev :as dev]))
(orchestra/instrument)
(dev/require-dev-helpers)

(deftest client-connected
  (is (= [(effects/effect :dungeonstrike.logger/set-client-id
                          :client-id "C:0z+v2RXWOUiP7er+KAZsrw")
          (effects/optional-effect :dungeonstrike.log-tailer/add-tailer
                                   :path "foo/client_logs.txt")
          (effects/optional-effect :dungeonstrike.gui/config
                                   :selector :#send-button
                                   :key :enabled?
                                   :value true)]
         (effects/evaluate'
          (effects/request :m/client-connected
                           :m/client-log-file-path "foo/client_logs.txt"
                           :m/client-id "C:0z+v2RXWOUiP7er+KAZsrw"
                           :m/message-id "M:/ydkmbevzEmE4yE/XjfbJA"
                           :m/message-type :m/client-connected)))))

(deftest client-disconnected
  (is (= [(effects/optional-effect :dungeonstrike.gui/config
                                   :selector :#send-button
                                   :key :enabled?
                                   :value false)]
         (effects/evaluate' (effects/request :r/client-disconnected)))))
