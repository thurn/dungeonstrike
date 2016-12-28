(ns dungeonstrike.main.core
  (:require [cljs.nodejs :as nodejs]
            [clojure.spec :as spec]))

(def electron (js/require "electron"))
(def app (.-app electron))
(def BrowserWindow (.-BrowserWindow electron))
(def main-window (atom nil))

(nodejs/enable-util-print!)

(defn -main [& args]
  (println "Starting Main Process..."))

(set! *main-cli-fn* -main)

(defn create-window []
  (println "Opening window")
  (reset! main-window (BrowserWindow.))
  (.maximize @main-window)
  (.loadURL @main-window
            (str "file://" js/__dirname "/../../../../index.html"))
  (.openDevTools (.-webContents @main-window))
  (.on @main-window "closed" #(reset! main-window nil)))

(.on app "ready" create-window)

(.on app "window-all-closed"
     #(when-not (= js/process.platform "darwin")
        (.quit app)))

(.on app "activate"
     #(when (nil? @main-window)
        (create-window)))

(defonce my-state (atom "dthurn"))

(def dthurn 322)

(def load-scene
  {::message-type :load-scene,
   ::message-id (uuid "12345"),
   ::game-version "0.1.0",
   ::scene-name "flat"})

(defn what [] (str "foo" dthurn @my-state))

(defn message-type? [value]
  (value #{
           :frontend-error
           :host-game
           :join-game
           :game-action
           :driver-error
           :update-required
           :game-invite
           :start-game
           :load-scene
           :update-game-actions
           :create-object
           :destroy-object
           :object-action
           :draw-card
           :update-card
           :destroy-card
           }))

(spec/def ::message (spec/keys :req [::message-type
                                     ::message-id
                                     ::game-version]))

(spec/def ::message-type message-type?)
(spec/def ::message-id uuid?)
(spec/def ::game-version string?)

(spec/def ::load-scene (spec/and ::message (spec/keys :req [::scene-name])))

(spec/def ::scene-name string?)
