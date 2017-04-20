(ns dungeonstrike.code-generator
  "Generates C# code for a strongly-typed implementation of the message
   specifications found in `messages.clj`"
  (:require [clojure.java.io :as io]
            [clojure.spec :as s]
            [clojure.string :as string]
            [dungeonstrike.logger :as logger :refer [log error]]
            [dungeonstrike.messages :as messages]
            [camel-snake-kebab.core :as case]
            [clostache.parser :as templates]
            [com.stuartsierra.component :as component]
            [dev]))
(dev/require-dev-helpers)

(def ^:private template
  "using System;

// =============================================================================
// WARNING: Do not modify this file by hand! This file is automatically
// generated by 'code_generator.clj' on driver startup from the message
// specifications found in 'messages.clj'. Refer to the documentation in those
// files for more information.
// =============================================================================

namespace DungeonStrike.Source.Messaging
{
    {{#enums}}
    public enum {{enumName}}
    {
        {{values}}
    }

    {{/enums}}
    {{#messages}}
    public sealed class {{messageName}}Message : Message
    {
        {{#fields}}
        public {{fieldType}} {{fieldName}} { get; set; }
        {{/fields}}
    }

    {{/messages}}

    public sealed class Messages
    {
        public static Message EmptyMessageForType(string messageType)
        {
            switch (messageType)
            {
                {{#messages}}
                case \"{{messageName}}\":
                    return new {{messageName}}Message();
                {{/messages}}
                default:
                    throw new InvalidOperationException(
                        \"Unrecognized message type: \" + messageType);
            }
        }
    }
}")

(defn- enum-name
  "Returns the enum type name to use for a given set."
  [set]
  (cond
    (= set messages/scene-names) "SceneName"
    (= set messages/entity-types) "EntityType"
    :otherwise (throw (RuntimeException. "UnknownEnumType"))))

(defn- enum-sets
  "Returns the set of all sets defined in the message specifications."
  []
  #{messages/scene-names messages/entity-types})

(defn- field-type
  "Returns the C# type to use to represent a given field."
  [field-name]
  (let [spec (field-name messages/message-fields)]
    (cond
      (set? spec) (enum-name spec)
      (= uuid? spec) "string"
      (= string? spec) "string"
      (= messages/position-spec spec) "Position"
      :otherwise (throw (RuntimeException. "UnknownFieldType")))))

(defn- template-parameters
  "Helper function which builds the parameters to the code generation template."
  []
  (let [field-params (fn [field-name]
                       {:fieldName (case/->PascalCase (name field-name))
                        :fieldType (field-type field-name)})
        message-params (fn [[message-name fields]]
                         {:messageName (case/->PascalCase (name message-name))
                          :fields (map field-params
                                       (remove #{:m/entity-id} fields))})
        get-name (comp case/->PascalCase name)
        enum-params (fn [set]
                      {:enumName (enum-name set)
                       :values (string/join ",\n        "
                                            (map get-name set))})]

    {:messages (map message-params messages/messages)
     :enums (map enum-params (enum-sets))}))

(defn generate!
  "Generates C# code based on the message specifications found in
   `dungeonstrike.messages` and outputs it to the configured output file."
  [{:keys [::output-path]}]
  (let [output (templates/render template (template-parameters))]
    (spit output-path output)))

(defrecord CodeGenerator []
  component/Lifecycle
  (start [{:keys [::logger] :as component}]
    (let [log-context (logger/component-log-context logger "CodeGenerator")]
      (log log-context "Generating C# code")
      (generate! component)
      (assoc component ::log-context log-context)))
  (stop [component]
    (dissoc component ::log-context)))

(defn new-code-generator [output-path]
  (map->CodeGenerator {::output-path output-path}))
