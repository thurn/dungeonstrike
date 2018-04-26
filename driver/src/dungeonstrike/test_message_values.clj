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
    ["C:CLIENT_ID"]))
