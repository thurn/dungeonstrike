(ns dungeonstrike.messages
  "Contains specifications for all 'messages' sent to the client and all
  'actions' sent to the driver."
  (:require [clojure.spec.alpha :as s]
            [clojure.set :as set]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(def integer-value
  "An integer-typed field"
  #::{:type :integer})

(def string-value
  "A string-typed field"
  #::{:type :string})

(defn enum-value
  "An enumeration of literal values. Argument should be a set of possible
   values."
  [spec-set]
  #::{:type :enum ::values spec-set})

(defn object-value
  "A field containing a map with keyword keys and typed values. Argument should
   be a vector of map key names."
  [spec-map]
  #::{:type :object ::values spec-map})

(defn seq-value
  "A field containing a sequence of fields of a single type. Argument should be
   the field type of the values."
  [value-type]
  #::{:type :seq ::value-type value-type})

(def fields
  {:m/x integer-value
   :m/y integer-value
   :a/entity-id string-value
   :a/client-log-file-path string-value
   :a/client-id string-value
   :m/entity-id string-value
   :m/scene-name (enum-value #{:empty :flat})
   :m/new-entity-id string-value
   :m/entity-type (enum-value #{:soldier})
   :m/position (object-value [:m/x :m/y])
   :m/positions (seq-value :m/position)})

(def actions
  {:a/client-connected [:a/client-log-file-path :a/client-id]})

(def messages
  {:m/test [:m/entity-id :m/scene-name]
   :m/load-scene [:m/scene-name]
   :m/quit-game []
   :m/create-entity [:m/new-entity-id :m/entity-type :m/position]
   :m/destroy-entity [:m/entity-id]
   :m/move-to-position [:m/entity-id :m/position]
   :m/show-move-selector [:m/entity-id :m/positions]})

(defn field-type
  "Returns the type keyword for the named field."
  [field-name]
  (::type (fields field-name)))

(defn values
  "Returns the 'values' component of a message field specification."
  [field-name]
  (::values (fields field-name)))

(defn seq-type
  "Returns the sequence type of a seq-value specification"
  [field-name]
  (::value-type (fields field-name)))
