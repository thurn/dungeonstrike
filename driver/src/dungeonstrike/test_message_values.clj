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
    :m/create-objects
    [:test-values/create-soldier]
    :m/update-objects
    [:test-values/green-soldier]
    :m/delete-objects
    [[]]
    :m/material-updates
    [:test-values/soldier-green :test-values/soldier-black]))

(def create-soldier
  [{:m/object-name "Soldier"
    :m/transform {:m/position {:m/x 0, :m/y 0}}
    :m/prefab-name :assets/soldier
    :m/components []}])

(def green-soldier
  [{:m/object-path "Soldier/Body"
    :m/components [{:m/component-type :m/renderer
                    :m/material-name :assets/soldier-forest}]}
   {:m/object-path "Soldier/Helmet"
    :m/components [{:m/component-type :m/renderer
                    :m/material-name :assets/soldier-helmet-green}]}
   {:m/object-path "Soldier/Bags"
    :m/components [{:m/component-type :m/renderer
                    :m/material-name :assets/soldier-bags-green}]}
   {:m/object-path "Soldier/Vest"
    :m/components [{:m/component-type :m/renderer
                    :m/material-name :assets/soldier-vest-green}]}])

(defn lookup-test-value-key
  "Test values can be replaced with a keyword namespaced with 'test-values' for
  legibility, in which case their actual values can be looked up via this
  function."
  [test-value-key]
  (case test-value-key
    :test-values/create-soldier create-soldier
    :test-values/green-soldier green-soldier
    (throw (RuntimeException. (str "Unknown test-value-key " test-value-key)))))
