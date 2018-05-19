(ns dungeonstrike.message-gui
  "User interface helpers for interacting with messages"
  (:require [clojure.spec.alpha :as s]
            [seesaw.core :as seesaw]
            [seesaw.mig :as mig]
            [seesaw.font :as font]
            [seesaw.table :as table]
            [mount.core :as mount]
            [dungeonstrike.messages :as messages]
            [dungeonstrike.test-message-values :as test-values]
            [dungeonstrike.websocket :as websocket]
            [dungeonstrike.dev :as dev])
  (:import (javax.swing ListSelectionModel)))
(dev/require-dev-helpers)

(mount/defstate recording-state
  :start (atom {:recording? false}))

(defn title-font
  "Font to use in section headers"
  []
  (font/font :style #{:bold} :size 18))

(defn- message-list-table
  [message-names]
  (let [result (seesaw/table :id :test-list
                             :model [:columns [:message]
                                     :rows (map vector message-names)])]
    (.setSelectionMode result ListSelectionModel/SINGLE_SELECTION)
    result))

(defn- on-send-message-clicked
  [message-name]
  (when (:recording? @recording-state)
    (swap! recording-state assoc
           :message->client message-name))
  (websocket/send-message! (test-values/values message-name)))

(defn send-panel
  []
  (let [message-names (into [] (keys test-values/values))
        send-button (seesaw/button :id :send-button
                                   :text "Send Message"
                                   :enabled? false)
        message-list (message-list-table message-names)]
    (seesaw/listen send-button :action
                   (fn [e]
                     (on-send-message-clicked
                      (message-names (seesaw/selection message-list)))))
    (seesaw/listen message-list :selection
                   (fn [e]
                     (seesaw/config! send-button :enabled? true)))
    (mig/mig-panel
     :id :tests
     :items [[(seesaw/label :text "Messages" :font (title-font))
              "alignx center, pushx, span, wrap 20px"]
             [send-button "wrap 20px"]
             [message-list "grow, span"]])))
