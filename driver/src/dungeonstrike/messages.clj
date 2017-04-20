(ns dungeonstrike.messages
  "Contains specifications for all messages sent between the driver and the
   client."
  (:require [clojure.spec :as s]
            [dev]))
(dev/require-dev-helpers)

(defprotocol MessageSender
  "Abstraction layer for the act of sending messages from the driver to the
   client."
  (send-message! [this message]
    "Sends `message` to the client."))

(s/def :m/x integer?)
(s/def :m/y integer?)
(def position-spec
  "Spec for an entity's position. This abstract position is translated into
   'real' world coordinates by the display layer."
  (s/keys :req [:m/x :m/y]))

(def scene-names
  "Possible names of scenes, can be loaded via the :m/load-scene message type"
  #{:empty
    :flat})

(def entity-types
  "Possible types of entities which can be created by a :m/create-entity
   message."
  #{:soldier
    :orc})

(defmacro deffield
  "Helper macro to specify field specifications. Because clojure.spec does not
   currently provide the ability to program with specs as values, we create a
   parallel data structure for each specification to enable tools like the code
   generator to introspect on the specifications defined here."
  [keyword predicate]
  `(do
    (s/def ~keyword ~predicate)
    [~keyword ~predicate]))

(def message-fields
  "A map containing all possible fields which can be found in a message, keyed
   by field keyword."
  (into {}
        [(deffield :m/scene-name scene-names)
         (deffield :m/entity-id uuid?)
         (deffield :m/entity-type entity-types)
         (deffield :m/position position-spec)]))

(defmacro defmessage
  "As with `deffield`, this macro both defines a specification and returns a
   parallel version of the specification as a data structure for metaprogramming
   purposes."
  [type fields]
  `(do
     (defmethod message-type ~type [~'_]
       (s/keys :req [:m/message-id :m/message-type ~@fields]))
     [~type ~fields]))

(s/def :m/message-id uuid?)

(s/def :m/message-type keyword?)

(defmulti message-type :m/message-type)

(defmessage :m/load-scene [:m/scene-name])

(def messages
  "All possible message specifications. A map from message type keywords to the
   required field keywords for that message type."
  (into {}
        [(defmessage :m/load-scene [:m/scene-name])
         (defmessage :m/create-entity [:m/entity-id :m/entity-type :m/position])
         (defmessage :m/destroy-entity [:m/entity-id])
         (defmessage :m/move-to-position [:m/entity-id :m/position])]))

;; The core message specification, a multi-spec which identifies the message
;; type in question via the :m/message-type key and then validates it.
(s/def :m/message (s/multi-spec message-type :m/message-type))
