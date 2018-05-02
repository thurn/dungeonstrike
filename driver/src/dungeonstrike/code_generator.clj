(ns dungeonstrike.code-generator
  "Generates C# code for a strongly-typed implementation of the message
   specifications found in `messages.clj`"
  (:require [clojure.java.io :as io]
            [clojure.spec.alpha :as s]
            [clojure.string :as string]
            [dungeonstrike.messages :as messages]
            [dungeonstrike.logger :as logger]
            [dungeonstrike.paths :as paths]
            [camel-snake-kebab.core :as case]
            [clostache.parser :as templates]
            [mount.core :as mount]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(def ^:private template
  "using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
        Unknown,
        {{#values}}
        {{name}},
        {{/values}}
    }

    {{/enums}}
    {{#interfaces}}
    public interface I{{interfaceName}}
    {
      {{#interfaceFields}}
      {{&fieldType}} Get{{fieldName}}();
      {{/interfaceFields}}
    }

    {{/interfaces}}
    {{#unionObjects}}
    public sealed class {{objectName}} : I{{interfaceName}}
    {
      public {{unionType}} {{unionType}};
      {{#objectFields}}
      public {{&fieldType}} {{fieldName}};
      {{/objectFields}}

      public {{unionType}} Get{{unionType}}()
      {
        return {{unionType}};
      }
    }

    {{/unionObjects}}
    {{#unionDeserializers}}
    public sealed class {{objectName}}JsonConverter
        : UnionJsonConverter<I{{objectName}}>
    {
        public override string GetTypeIdentifier()
        {
            return \"{{unionType}}\";
        }

        public override object GetEmptyObjectForType(string type)
        {
            switch (type) {
               {{#unionValues}}
                case \"{{name}}\":
                    return new {{name}}();
                {{/unionValues}}
                default:
                    throw new InvalidOperationException(
                        \"Unrecognized type: \" + type);
            }
        }
    }

    {{/unionDeserializers}}
    {{#objects}}
    public sealed class {{objectName}}{{interfaceDeclaration}}
    {
        {{#objectFields}}
        public {{&fieldType}} {{fieldName}};
        {{/objectFields}}
    }

    {{/objects}}
    {{#messages}}
    public sealed class {{messageName}}Message : Message
    {
        public static readonly string Type = \"{{messageName}}\";

        public {{messageName}}Message() : base(\"{{messageName}}\")
        {
        }

        {{#fields}}
        public {{&fieldType}} {{fieldName}} { get; set; }
        {{/fields}}
    }

    {{/messages}}
    {{#actions}}
    public sealed class {{actionName}}Action : UserAction
    {
        public static readonly string Type = \"{{actionName}}\";

        public {{actionName}}Action() : base(\"{{actionName}}\")
        {
        }

        {{#fields}}
        public {{&fieldType}} {{fieldName}} { get; set; }
        {{/fields}}
    }

    {{/actions}}

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

        public static JsonConverter[] GetJsonConverters()
        {
            return new JsonConverter[] {
                new MessageConverter(),
                new StringEnumConverter(),
                {{#unionTypes}}
                new {{name}}JsonConverter(),
                {{/unionTypes}}
            };
        }
    }
}")

(defn- pascal-name
  "Returns the pascal-cased 'name' of this keyword"
  [keyword]
  (case/->PascalCase (name keyword)))

(defn- union-enum-values
  [type-keyword]
  (for [field (keys messages/fields)
        :when (= type-keyword (messages/union-value-keyword field))]
    {:name (pascal-name field)}))

(defn- union-enums
  "Returns enum specifications for enums required to implement union types."
  []
  (let [union? (fn [f] (= :union-type (messages/field-type f)))
        union-types (filter union? (keys messages/fields))]
    (for [union-type union-types]
      (let [type-keyword (messages/union-type-keyword union-type)]
        {:enumName (pascal-name type-keyword)
         :values (union-enum-values type-keyword)}))))

(defn- enum-info
  "Returns information about all enum messages."
  []
  (for [x messages/fields
        :let [k (key x)]
        :when (= :enum (messages/field-type k))]
    {:enumName (pascal-name k)
     :values (for [v (messages/values k)]
               {:name (pascal-name v)})}))

(defn- csharp-field-type
  "Returns the C# type to use to represent a given message field."
  [field-name]
  (case (messages/field-type field-name)
    :integer
    "int"
    :string
    "string"
    :enum
    (pascal-name field-name)
    :object
    (pascal-name field-name)
    :union-type
    (str "I" (pascal-name field-name))
    :union-value
    (pascal-name field-name)
    :seq
    (str "List<" (csharp-field-type (messages/seq-type field-name)) ">")
    (logger/error "Unknown type for field-name" field-name)))

(defn- interface-info
  "Returns information about interfaces that must be generated."
  []
  (for [field-name (keys messages/fields)
        :when (= :union-type (messages/field-type field-name))
        :let [field-type (pascal-name
                          (messages/union-type-keyword field-name))]]
    {:interfaceName (pascal-name field-name)
     :interfaceFields [{:fieldName field-type
                        :fieldType field-type}]}))

(defn- object-fields
  "Field specifications for the object in field-name"
  [field-name]
  (for [f (messages/all-fields field-name)]
    {:fieldName (pascal-name f)
     :fieldType (csharp-field-type f)}))

(defn- object-info
  []
  "Returns information about all object messages."
  (for [field-name (keys messages/fields)
        :when (= :object (messages/field-type field-name))]
    {:objectName (pascal-name field-name)
     :objectFields (object-fields field-name)}))

(defn- union-field-name-for-type-keyword
  "Looks up the field name for a union type based on its type keyword."
  [type-keyword]
  (let [matches (filter (fn [[k _]]
                          (= type-keyword (messages/union-type-keyword k)))
                        messages/fields)]
    (if (empty? matches)
      (throw (RuntimeException. (str "Keyword not found: " type-keyword)))
      (key (first matches)))))

(defn- union-types
  "Returns specifications for union types for use in json conversion"
  []
  (for [field (keys messages/fields)
        :when (= :union-type (messages/field-type field))]
    {:name (pascal-name field)}))

(defn- union-value-objects
  "Returns specifications for union-value-typed objects."
  []
  (for [field (keys messages/fields)
        :when (= :union-value (messages/field-type field))]
    (let [type-keyword (messages/union-value-keyword field)
          union-field (union-field-name-for-type-keyword type-keyword)]
      {:objectName (pascal-name field)
       :interfaceName (pascal-name union-field)
       :unionType (pascal-name type-keyword)
       :objectFields (object-fields field)})))

(defn- union-deserializers
  "Returns specifications for union type deserializers"
  []
  (for [field (keys messages/fields)
        :when (= :union-type (messages/field-type field))
        :let [type-keyword (messages/union-type-keyword field)]]
    {:objectName (pascal-name field)
     :unionType (pascal-name type-keyword)
     :unionValues (union-enum-values type-keyword)}))

(defn- template-parameters
  "Helper function which builds the parameters to the code generation template."
  []
  (let [message-field-params (fn [field-name]
                               {:fieldName (pascal-name field-name)
                                :fieldType (csharp-field-type field-name)})
        message-params (fn [[message-name fields]]
                         {:messageName (pascal-name message-name)
                          :fields (map message-field-params
                                       (remove #{:m/entity-id} fields))})
        action-field-params (fn [field-name]
                              {:fieldName (pascal-name field-name)
                               :fieldType (csharp-field-type field-name)})
        action-params (fn [[action-name fields]]
                        {:actionName (pascal-name action-name)
                         :fields (map action-field-params fields)})]
    {:messages (map message-params messages/messages)
     :actions (map action-params messages/actions)
     :objects (object-info)
     :unionObjects (union-value-objects)
     :unionDeserializers (union-deserializers)
     :interfaces (interface-info)
     :enums (concat (union-enums) (enum-info))
     :unionTypes (union-types)}))

(defn- generate!
  "Generates C# code based on the message specifications found in
   `dungeonstrike.messages` and outputs it to the configured output file."
  []
  (let [output (templates/render template (template-parameters))]
    (spit paths/code-generator-output-path output)))

(mount/defstate ^:private code-generator :start (generate!))
