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

(def ^:private float-value
  "A float-typed field"
  #::{:type :float})

(def ^:private boolean-value
  "A boolean-typed field"
  #::{:type :boolean})

(def ^:private string-value
  "A string-typed field"
  #::{:type :string})

(defn- enum-value
  "An enumeration of literal values. Argument should be a set of possible
   values."
  [spec-set]
  #::{:type :enum :values spec-set})

(defn- object-value
  "A field containing a map with keyword keys and typed values. Arguments should
   be vectors of required map key names and optional key names."
  [required-field-list optional-field-list]
  #::{:type :object
      :required-fields required-field-list
      :optional-fields optional-field-list})

(defn- alias-value
  "A field containing a map described by another field key. Argument should be
  the corresponding field key to look up."
  [field-key]
  #::{:type :alias :field-key field-key})

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
  [type-keyword required-field-list optional-field-list]
  #::{:type :union-value
      :union-value-keyword type-keyword
      :required-fields required-field-list
      :optional-fields optional-field-list})

;; TODO: Implement messages and actions using union-type mechanism

(def fields
  {
   ;; Primitives:
   :m/x integer-value
   :m/y integer-value
   :m/z integer-value
   :m/vector2 (object-value [:m/x :m/y] [])
   :m/vector3 (object-value [:m/x :m/y :m/z] [])
   :m/r float-value
   :m/g float-value
   :m/b float-value
   :m/a float-value
   :m/color (object-value [:m/r :m/g :m/b :m/a] [])
   :m/entity-child-path string-value
   :m/material-name (enum-value assets/material)
   :m/sprite-name (enum-value assets/sprite)

   ;; Updates
   :m/create-object (object-value [:m/object-name]
                                  [:m/parent-path :m/prefab-name
                                   :m/transform :m/components])
   :m/create-objects (seq-value :m/create-object)
   :m/update-object (object-value [:m/object-path]
                                  [:m/transform :m/components])
   :m/update-objects (seq-value :m/update-object)
   :m/delete-object (object-value [:m/object-path] [])
   :m/delete-objects (seq-value :m/delete-object)
   :m/parent-path string-value
   :m/object-path string-value
   :m/object-name string-value

   ;; Transforms
   :m/transform (union-type :m/transform-type)
   :m/cube-transform (union-value :m/transform-type [:m/position3d] [])
   :m/rect-transform (union-value :m/transform-type [] [:m/size
                                                        :m/pivot
                                                        :m/position2d
                                                        :m/vertical-anchor
                                                        :m/horizontal-anchor])
   :m/position3d (alias-value :m/vector3)
   :m/pivot (enum-value #{:upper-left :upper-center :upper-right :middle-left
                          :middle-center :middle-right :lower-left :lower-center
                          :lower-right})
   :m/size (alias-value :m/vector2)
   :m/position2d (alias-value :m/vector2)
   :m/vertical-anchor (enum-value #{:top :middle :bottom :stretch})
   :m/horizontal-anchor (enum-value #{:left :center :right :stretch})

   ;; Components
   :m/components (seq-value :m/component)
   :m/component (union-type :m/component-type)
   :m/canvas (union-value :m/component-type [:m/render-mode] [])
   :m/render-mode (enum-value #{:screen-space-overlay :screen-space-camera
                                :world-space})
   :m/canvas-scaler (union-value :m/component-type
                                 [:m/scale-mode]
                                 [:m/reference-resolution])
   :m/scale-mode (enum-value #{:constant-pixel-size :scale-with-screen-size
                               :constant-physical-size})
   :m/reference-resolution (object-value [:m/x :m/y] [])
   :m/graphic-raycaster (union-value :m/component-type [] [])
   :m/renderer (union-value :m/component-type [] [:m/material-name])
   :m/image (union-value :m/component-type [] [:m/sprite-name
                                               :m/color
                                               :m/material-name
                                               :m/is-raycast-target
                                               :m/image-type])
   :m/is-raycast-target boolean-value
   :m/image-type (union-type :m/image-subtype)
   :m/simple-image-type (union-value :m/image-subtype []
                                     [:m/preserve-aspect-ratio])
   :m/preserve-aspect-ratio boolean-value
   :m/content-size-fitter (union-value :m/component-type [:m/horizontal-fit-mode
                                                          :m/vertical-fit-mode]
                                       [])
   :m/horizontal-fit-mode (enum-value #{:unconstrained :min-size
                                        :preferred-size})
   :m/vertical-fit-mode (enum-value #{:unconstrained :min-size :preferred-size})

   ;; Actions
   :a/client-log-file-path string-value
   :a/client-id string-value

   ;; Messages
   :m/scene-name (enum-value #{:empty :flat})
   :m/prefab-name (enum-value assets/prefab)
   :m/material-update (object-value [:m/entity-child-path :m/material-name] [])
   :m/material-updates (seq-value :m/material-update)})

(def actions
  {:a/client-connected [:a/client-log-file-path :a/client-id]})

;; TODO: Replace messages with updates. Updates have five fields:
;; 1) load-scene 2) create-objects 3) update-objects 4) delete-objects
;; 5) quit-game
;; Unify recordings with test values
(def messages
  {:m/test [:m/scene-name]
   :m/load-scene [:m/scene-name]
   :m/quit-game []
   :m/update [:m/create-objects :m/update-objects :m/delete-objects]})

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

(defn required-fields
  "Returns the required fields of an object or union value."
  [field-name]
  (::required-fields (fields field-name)))

(defn optional-fields
  "Returns the optional fields of an object or union value."
  [field-name]
  (::optional-fields (fields field-name)))

(defn all-fields
  "Returns all fields for an object or union value."
  [field-name]
  (concat (required-fields field-name) (optional-fields field-name)))

(defn seq-type
  "Returns the sequence type of a seq-value specification"
  [field-name]
  (::value-type (fields field-name)))

(defn alias-field-key
  "Returns the field key for an alias-value type"
  [field-name]
  (::field-key (fields field-name)))
