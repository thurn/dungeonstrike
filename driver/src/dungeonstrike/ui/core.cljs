(ns dungeonstrike.ui.core
  (:require [reagent.core :as reagent]
            [reagent-forms.core :as forms]
            [camel-snake-kebab.core :as case]))

(def electron (js/require "electron"))
(def ipc-renderer (.-ipcRenderer electron))

(defonce click-count (reagent/atom 0))

(def form-state (reagent/atom {}))
(def error (reagent/atom ""))

(defn to-json-format [value]
  (cond
    (keyword? value) (case/->PascalCaseString value)
    :else (str value)))

(defn send-message [event]
  (.preventDefault event)
  (swap! form-state assoc
         :message-id (random-uuid)
         :game-version "0.1.0")
  (let [json (into {} (for [[key value] @form-state]
                        [(to-json-format key) (to-json-format value)]))]
    (println json)
    (.send ipc-renderer "send-message" (clj->js json))))

(defn row [label input]
  [:div.row.form-row.flex-center
   [:div.col-xs-3.flex-center [:label label]]
   [:div.col-xs-9.flex-center input]])

(defn input [label type id & {:as args}]
  (row label
       [:input.form-control
        (merge {:field type, :id id} args)]))

(defn select [label id options]
  (row label
       (into [:select.form-control {:field :list, :id id}]
             (map (fn [option] [:option {:key option} (str option)]))
             options)))

(def message-form
  [:div.container-fluid
   (select "Message Type" :message-type
           [:load-scene :create-entity :update-entity :destroy-entity])
   (select "Scene Name" :scene-name [:flat :empty])
   (input "Message ID" :text :message-id :disabled true)
   (input "Game Version" :text :game-version :disabled true)])

(defn error-alert [message]
  [:div.row.alert.alert-danger {:role "alert"}
   message
   [:button.close {:type "button" :on-click #(reset! error "")} [:span "×"]]])

(defn root []
  [:div
   [:div.container-fluid
    (if-not (empty? @error) [error-alert @error])
    [:div.row
     [:div.col-xs-6
      [:h2 "New Message"]
      [forms/bind-fields message-form form-state]
      [:button.btn.btn-default.send-message-button
       {:on-click send-message}
       "Send Message"]]
     [:div.col-xs-6
      [:h2 "Logs"]]]]])

;; -------------------------
;; Initialize app

(defn mount-root
  "Creates the Reagent root component"
  []
  (reagent/render [root] (.getElementById js/document "app")))

(defn init! []
  (.on ipc-renderer "error"
       (fn [event error-message]
         (reset! error error-message)))
  (mount-root))

(def dthurn 234)

(defn prix [] (str dthurn "13"))
