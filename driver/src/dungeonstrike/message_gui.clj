(ns dungeonstrike.message-gui
  "User interface helpers for interacting with messages"
  (:require [clojure.spec.alpha :as s]
            [seesaw.core :as seesaw]
            [seesaw.mig :as mig]
            [dungeonstrike.messages :as messages]
            [dungeonstrike.test-message-values :as test-values]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(declare map-items-for-message-field)

(defn- editor-for-message-field
  "Returns an appropriate GUI widget for editing the message field. The widget
  will have an ':id' of the field name."
  [field-name]
  [[(seesaw/label (str field-name))]
   [(if (= :enum (messages/field-type field-name))
      (seesaw/combobox :id field-name
                       :model (messages/message-values field-name))
      (seesaw/combobox :id field-name
                       :model (test-values/test-values-for-field-name
                               field-name)))
    "width 200px, wrap"]])

(defn editor-for-message-type
  "Returns a vector of form items implementing an editor for the supplied
  message type."
  [message-type]
  (mapcat editor-for-message-field (message-type messages/messages)))

(defn- map-items-for-message-field
  "Returns a series of GUI items representing a 'map' message field."
  [values]
  nil)
