(ns dungeonstrike.ui.core
  (:require [reagent.core :as reagent]
            [reagent-forms.core :as forms]
            [cljsjs.react-bootstrap]
            [clojure.spec :as spec]))

;; -------------------------
;; Views
(def button (reagent/adapt-react-class
             (.-Button js/ReactBootstrap)))
(def navbar (reagent/adapt-react-class
             (.-Navbar js/ReactBootstrap)))
(def navbar-header (reagent/adapt-react-class
                    (.-Header (.-Navbar js/ReactBootstrap))))
(def navbar-brand (reagent/adapt-react-class
                   (.-Brand (.-Navbar js/ReactBootstrap))))
(def nav (reagent/adapt-react-class
          (.-Nav js/ReactBootstrap)))
(def nav-item (reagent/adapt-react-class
               (.-NavItem js/ReactBootstrap)))
(def form (reagent/adapt-react-class
           (.-Form js/ReactBootstrap)))
(def form-group (reagent/adapt-react-class
                 (.-FormGroup js/ReactBootstrap)))
(def control-label (reagent/adapt-react-class
                    (.-ControlLabel js/ReactBootstrap)))
(def form-control (reagent/adapt-react-class
                   (.-FormControl js/ReactBootstrap)))
(def help-block (reagent/adapt-react-class
                 (.-HelpBlock js/ReactBootstrap)))
(def col (reagent/adapt-react-class
          (.-Col js/ReactBootstrap)))
(def form-control-static (reagent/adapt-react-class
                          (.-Static (.-FormControl js/ReactBootstrap))))

(defonce welcome-state (reagent/atom "reagent"))
(defonce click-count (reagent/atom 0))

(defn navigation []
  [navbar
   [navbar-header
    [navbar-brand "DungeonStrike Driver"]]
   [nav {:bs-style "pills", :active-key 1}
    [nav-item {:event-key 1} "Console"]
    [nav-item {:event-key 2} "Integration Tests"]]])

(defn send-message [event]
  (.preventDefault event)
  (println "send message")
  (doseq [group (.from js/Array (.-elements (.-target event)))]
    (println group)))

(defonce form-state (reagent/atom {}))

(defn row [label input]
  [:div.row
   [:div.col-xs-3 [:label label]]
   [:div.col-xs-9 input]])

(defn input [label type id]
  (row label [:input.form-control {:field type :id id}]))

(defn message-form []
  [:div.container-fluid
   (input "Message Type" :text :person.first-name)
   (input "Scene Name" :text :person.last-name)
   (row "Message ID" "-KYpPNj3P339QPj1BPhY")
   (row "Game Version" "0.1.0")])

(defn message-input []
  [form {:horizontal true, :on-submit send-message}
   [form-group
    [col {:sm 3}
     [control-label "Message Type"]]
    [col {:sm 9}
     [form-control {:component-class "select"}
      [:option {:value "load-scene"} "LoadScene"]
      [:option {:value "create-entity"} "CreateEntity"]
      [:option {:value "update-entity"} "UpdateEntity"]
      [:option {:value "destroy-entity"} "DestroyEntity"]]]]
   [form-group
    [col {:sm 3}
     [control-label "Scene Name"]]
    [col {:sm 9}
     [form-control {:component-class "select"}
      [:option {:value "Empty"} "Empty"]
      [:option {:value "Flat"} "Flat"]]]]
   [form-group
    [col {:sm 3}
     [control-label "Message ID"]]
    [col {:sm 9}
     [form-control-static "-KYpPNj3P339QPj1BPhY"]]]
   [form-group
    [col {:sm 3}
     [control-label "Game Version"]]
    [col {:sm 9}
     [form-control-static "0.1.0"]]]
   [button {:type "submit" :bs-style "primary"}
    "Send Message"]])

(defn root []
  [:div
   [navigation]
   [:div.container-fluid
    [:div.row
     [:div.col-xs-6
      [:h2 "New Message"]
      [forms/bind-fields message-form form-state]]
     [:div.col-xs-6
      [:h2 "Logs"]]]]])

;; -------------------------
;; Initialize app

(defn mount-root
  "Creates the Reagent root component"
  []
  (reagent/render [root] (.getElementById js/document "app")))

(defn init! []
  (mount-root))

(def dthurn 234)

(defn prix [] (str dthurn "13"))
