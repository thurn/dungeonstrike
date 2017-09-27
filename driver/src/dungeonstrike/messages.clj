(ns dungeonstrike.messages
  "Contains specifications for all 'messages' sent to the client and all
  'actions' sent to the driver."
  (:require [clojure.spec.alpha :as s]
            [clojure.set :as set]
            [effects.core :as effects]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(s/def :m/x integer?)
(s/def :m/y integer?)
(def position-spec
  "Spec for an entity or tile position. This abstract position is translated
   into 'real' world coordinates by the display layer."
  (s/keys :req [:m/x :m/y]))

(def position-coll-spec
  "Spec for a collection of positions"
  (s/coll-of position-spec))

(def scene-names
  "Possible names of scenes, can be loaded via the :m/load-scene message type"
  #{:empty
    :flat})

(def entity-types
  "Possible types of entities which can be created by a :m/create-entity
   message."
  #{:soldier})

(def enum-name-for-set
  "Maps the above sets to the name they should be called in generated code."
  {scene-names "SceneName"
   entity-types "EntityType"})

(def set-for-enum-name
  "Maps enumeration names to their associated set."
  (clojure.set/map-invert enum-name-for-set))

(defmacro deffield
  "Helper macro to specify field specifications. Because clojure.spec does not
   currently provide the ability to program with specs as values, we create a
   parallel data structure for each specification to enable tools like the code
   generator to introspect on the specifications defined here."
  [keyword predicate]
  `(do
     (s/def ~keyword ~predicate)
     [~keyword ~predicate]))

(def action-fields
  "A map containing all possible fields which can be found in an action, keyed
   by field keyword."
  (into {}
        [(deffield :a/client-id string?)

         (deffield :a/client-log-file-path string?)]))

(def message-fields
  "A map containing all possible fields which can be found in a message, keyed
   by field keyword."
  (into {}
        [(deffield :m/scene-name scene-names)

         (deffield :m/entity-id string?)

         (deffield :m/new-entity-id string?)

         (deffield :m/entity-type entity-types)

         (deffield :m/position position-spec)

         (deffield :m/positions position-coll-spec)]))

(defmulti message-type :m/message-type)

(defmacro defmessage
  "As with `deffield`, this macro both defines a specification and returns a
   parallel version of the specification as a data structure for metaprogramming
   purposes. Describes values sent *to* the client from the driver."
  [type docstring fields]
  `(do
     (defmethod message-type ~type [~'_]
       (s/keys :req [:m/message-id :m/message-type ~@fields]))
     [~type ~fields]))

(s/def :m/message-id string?)

(s/def :m/message-type keyword?)

(defmulti action-type :a/action-type)

(defmacro defaction
  "As `defmessage`, except that it describes values sent *from* the
  client to the driver."
  [type docstring fields]
  `(do
     (defmethod action-type ~type [~'_]
       (s/keys :req [:a/action-id :a/action-type ~@fields]))
     (defmethod effects/request-spec ~type [~'_]
       (s/and (s/keys :req [:a/action-id ~@fields])))
     [~type ~fields]))

(s/def :m/action-id string?)

(s/def :m/action-type keyword?)

(def actions
  "All possible action specifications. An action is a value sent from the client
   to the driver representing user input. Map from action type keywords to field
   keywords."
  (into {}
        [(defaction :a/client-connected
           "Action sent by the client when a connection is first established."
           [:a/client-log-file-path :a/client-id])]))

(def messages
  "All possible message specifications. A message is a value sent from the
  driver to the client. A map from command type keywords to the required field
  keywords for that command type."
  (into {}
        [(defmessage :m/test
           "Command for use in unit tests."
           [:m/entity-id :m/scene-name])

         (defmessage :m/load-scene
           "Loads a scene by name"
           [:m/scene-name])

         (defmessage :m/quit-game
           "Exits the client."
           [])

         (defmessage :m/create-entity
           "Creates a new entity with the specified type and position"
           [:m/new-entity-id :m/entity-type :m/position])

         (defmessage :m/destroy-entity
           "Destroys an entity by ID."
           [:m/entity-id])

         (defmessage :m/move-to-position
           "Moves an entity to a given position."
           [:m/entity-id :m/position])

         (defmessage :m/show-move-selector
           "Displays a UI for selecting a destination for an entity to move to
           from a collection of available positions"
           [:m/entity-id :m/positions])]))

;; The core message specification, a multi-spec which identifies the message
;; type in question via the :m/message-type key and then validates it.
(s/def :m/message (s/multi-spec message-type :m/message-type))

;; The core action specification, a multi-spec which identifies the action
;; type in question via the :a/action-type key and then validates it.
(s/def :a/action (s/multi-spec action-type :a/action-type))
