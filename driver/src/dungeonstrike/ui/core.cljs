(ns dungeonstrike.ui.core
    (:require [reagent.core :as reagent]))

;; -------------------------
;; Views

(defonce welcome-state (reagent/atom "reagent"))

(defn home-page []
  [:div [:h2 (str "Hello, " @welcome-state)]])

;; -------------------------
;; Initialize app

(defn mount-root
  "Creates the Reagent root component"
  []
  (reagent/render [home-page] (.getElementById js/document "app")))

(defn init! []
  (mount-root))

(def dthurn 234)
