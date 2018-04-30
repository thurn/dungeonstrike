(ns dungeonstrike.message-specs
  "Contains clojure.spec specifications for messages and actions"
  (:require [clojure.spec.alpha :as s]
            [clojure.set :as set]
            [effects.core :as effects]
            [dungeonstrike.messages :as messages]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(s/def :m/message-type keyword?)
(s/def :m/message-id string?)
(s/def :m/action-type keyword?)
(s/def :m/action-id string?)

(defn- value-spec
  "Generates specification for a message or action field"
  [field-name]
  (let [field-type (messages/field-type field-name)]
    (case field-type
      :integer
      `integer?
      :string
      `string?
      :enum
      `(messages/values ~field-name)
      :object
      `(s/keys :req [~@(messages/values field-name)])
      :union-type
      `any?
      :union-value
      `any?
      :seq
      `(s/coll-of ~(messages/seq-type field-name))
      (throw (RuntimeException.
              (str "Unknown type for field-name" field-name))))))

(defmacro generate-field-specs
  []
  (let [field-spec (fn [[field-name value]]
                     `(s/def ~field-name ~(value-spec field-name)))]
    `(do ~@(map field-spec messages/fields))))
(generate-field-specs)

(defmulti message-type :m/message-type)

(defn- message-spec
  "Generates multimethod specs for each type of message."
  [[type fields]]
  `(defmethod ~'message-type ~type [~'_]
     (s/keys :req [:m/message-id :m/message-type ~@fields])))

(defmacro generate-message-specs
  "Generates multimethod specs for each type of message."
  []
  `(do
     ~@(map message-spec messages/messages)))

(generate-message-specs)

(s/def :m/message (s/multi-spec message-type :m/message-type))

(defmulti action-type :a/action-type)

(defn- action-spec
  "Generates multimethod specs for each type of action."
  [[type fields]]
  `(defmethod ~'action-type ~type [~'_]
     (s/keys :req [:a/action-id :a/action-type ~@fields])))

(defmacro generate-action-specs
  "Generates multimethod specs for each type of action."
  []
  `(do
     ~@(map action-spec messages/actions)))

(generate-action-specs)

(s/def :a/action (s/multi-spec action-type :a/action-type))
