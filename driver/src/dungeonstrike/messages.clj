(ns dungeonstrike.messages
  "Contains specifications for all 'messages' sent to the client and all
  'actions' sent to the driver."
  (:require [clojure.spec.alpha :as s]
            [clojure.set :as set]
            [dungeonstrike.generated.assets :as assets]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(def ^:private integer-value
  "An integer-typed field"
  #::{:type :integer})

(def ^:private string-value
  "A string-typed field"
  #::{:type :string})

(defn- enum-value
  "An enumeration of literal values. Argument should be a set of possible
   values."
  [spec-set]
  #::{:type :enum :values spec-set})

(defn- object-value
  "A field containing a map with keyword keys and typed values. Argument should
   be a vector of map key names."
  [field-list]
  #::{:type :object :values field-list})

(defn- seq-value
  "A field containing a sequence of fields of a single type. Argument should be
   the field type of the values."
  [value-type]
  #::{:type :seq :value-type value-type})

(defn- union-type
  "Defines a union type, which can consist of one of a fixed number of
  union-value typed fields. Argument should be a keyword identifying the type
  of value contained within."
  [type-keyword]
  #::{:type :union-type :union-type-keyword type-keyword})

(defn- union-value
  "Defines a single possible entry in a union. Union values are a special type
  of object, and all object-value semantics apply to them. First argument is a
  type-keyword to link to a given union-type. Second argument is a list of
  object fields."
  [type-keyword field-list]
  #::{:type :union-value :union-value-keyword type-keyword :values field-list})

(def fields
  {
   ;; Object Fields:
   :m/x integer-value
   :m/y integer-value
   :m/entity-child-path string-value
   :m/material-name (enum-value assets/material)
   :m/sprite-name (enum-value assets/sprite)

   ;; GUI fields
   :gui/size (object-value [:m/x :m/y])
   :gui/transform integer-value
   :gui/name string-value
   :gui/canvas-string string-value
   :gui/canvas (union-value :gui/component-type [:gui/canvas-string])
   :gui/event-system (union-value :gui/component-type [])
   :gui/component (union-type :gui/component-type)
   :gui/components (seq-value :gui/component)
   :m/node (object-value [:gui/name :gui/transform :gui/components])

   ;; Action Fields
   :a/entity-id string-value
   :a/client-log-file-path string-value
   :a/client-id string-value

   ;; Message Fields
   :m/entity-id string-value
   :m/scene-name (enum-value #{:empty :flat})
   :m/new-entity-id string-value
   :m/prefab-name (enum-value assets/prefab)
   :m/position (object-value [:m/x :m/y])
   :m/material-update (object-value [:m/entity-child-path :m/material-name])
   :m/material-updates (seq-value :m/material-update)})

(def actions
  {:a/client-connected [:a/client-log-file-path :a/client-id]})

(def messages
  {:m/test [:m/entity-id :m/scene-name]
   :m/load-scene [:m/scene-name]
   :m/quit-game []
   :m/create-entity [:m/new-entity-id :m/prefab-name :m/position
                     :m/material-updates]
   :m/update-gui [:m/node]})

(defn field-type
  "Returns the type keyword for the named field."
  [field-name]
  (::type (fields field-name)))

(defn union-value-keyword
  "Returns the type keyword for a union type or union value."
  [field-name]
  (::union-value-keyword (fields field-name)))

(defn union-type-keyword
  "Returns the type keyword for a union type or union value."
  [field-name]
  (::union-type-keyword (fields field-name)))

(defn values
  "Returns the 'values' component of a message field specification."
  [field-name]
  (::values (fields field-name)))

(defn seq-type
  "Returns the sequence type of a seq-value specification"
  [field-name]
  (::value-type (fields field-name)))
