(ns dungeonstrike.test-message-values
  "Stores useful test values for message fields."
  (:require [dungeonstrike.uuid :as uuid]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(def entity-ids
  ["E:DRLS2GWvR8GAZaZixfBamw"])

(def ^:private test-entity-id "E:DRLS2GWvR8GAZaZixfBamw")

(defn test-values-for-field-name
  "Returns a vector of test values that can be assigned to the named field."
  [field-name]
  (case field-name
    :m/entity-id
    [test-entity-id]
    :m/new-entity-id
    [test-entity-id, (uuid/new-entity-id)]
    :m/position
    [{:m/x 0, :m/y 0}, {:m/x 1, :m/y 1}]
    :m/positions
    []
    :m/client-id
    ["C:CLIENT_ID"]
    :m/material-updates
    [:test-values/soldier-green :test-values/soldier-black]))

(defn lookup-test-value-key
  "Test values can be replaced with a keyword namespaced with 'test-values' for
  legibility, in which case their actual values can be looked up via this
  function."
  [test-value-key]
  (case test-value-key
    :test-values/soldier-green
    [{:m/entity-child-path "Body" :m/material-name :soldier-forest}
     {:m/entity-child-path "Helmet" :m/material-name :soldier-helmet-green}
     {:m/entity-child-path "Bags" :m/material-name :soldier-bags-green}
     {:m/entity-child-path "Vest" :m/material-name :soldier-vest-green}]
    :test-values/soldier-black
    [{:m/entity-child-path "Body" :m/material-name :soldier-black}
     {:m/entity-child-path "Helmet" :m/material-name :soldier-helmet-black}
     {:m/entity-child-path "Bags" :m/material-name :soldier-bags-black}
     {:m/entity-child-path "Vest" :m/material-name :soldier-vest-black}]
    (throw (RuntimeException. (str "Unknown test-value-key " test-value-key)))))
