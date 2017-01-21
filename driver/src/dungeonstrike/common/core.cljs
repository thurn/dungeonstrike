(ns dungeonstrike.common.core
  (:require [clojure.spec :as spec]))

(def twenty 20)

(def load-scene
  {::message-type :load-scene,
   ::message-id (uuid "12345"),
   ::game-version "0.1.0",
   ::scene-name "flat"})

(defn who [] (str "fxo" twenty))

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
           :destroy-card}))

(spec/def ::message (spec/keys :req [::message-type
                                     ::message-id
                                     ::game-version]))

(spec/def ::message-type message-type?)
(spec/def ::message-id uuid?)
(spec/def ::game-version string?)

(spec/def ::load-scene (spec/and ::message (spec/keys :req [::scene-name])))

(spec/def ::scene-name string?)
