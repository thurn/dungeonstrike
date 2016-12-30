(ns dungeonstrike.main.spec
  (:require [clojure.spec :as spec]))


(spec/def ::message-type keyword?)
(spec/def ::message-id string?)
(spec/def ::game-version string?)

(defmulti message-type ::message-type)

(defmethod message-type ::load-scene [_]
  (spec/keys :req [::scene-name]))

(spec/def ::scene-name string?)

(defmethod message-type ::update-required [_]
  (spec/keys :req [::required-version]))

(spec/def ::required-version string?)

(spec/def ::message (spec/multi-spec message-type ::message-type))
