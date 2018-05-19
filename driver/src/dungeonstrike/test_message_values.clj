(ns dungeonstrike.test-message-values
  "Stores useful test values for message fields."
  (:require [dungeonstrike.uuid :as uuid]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

;; TODO: Validate specs in this file on load

(def quit-game
  {:m/message-type :m/quit-game
   :m/message-id "M:QUIT"})

(def load-scene-empty
  {:m/message-type :m/load-scene,
   :m/scene-name :empty,
   :m/message-id "M:1RsO5ExNSW25/l6iPNyBZA"})

(def load-scene-flat
  {:m/message-type :m/load-scene,
   :m/scene-name :flat,
   :m/message-id "M:nIBUiVA8R6SnIoWSJ9WtvA"})

(def create-entity-soldier
  #:m{:message-type :m/update,
      :message-id "M:U3VMDB3JTaWzDHSD86dn1w",
      :create-objects
      [#:m{:object-name "Soldier",
           :transform #:m{:transform-type :m/cube-transform
                          :position3d #:m{:x 0, :y 0, :z 0}},
           :prefab-name :assets/soldier,
           :components []}],
      :update-objects
      [#:m{:object-path "Soldier/Body",
           :components
           [#:m{:component-type :m/renderer,
                :material-name :assets/soldier-forest}]}
       #:m{:object-path "Soldier/Helmet",
           :components
           [#:m{:component-type :m/renderer,
                :material-name :assets/soldier-helmet-green}]}
       #:m{:object-path "Soldier/Bags",
           :components
           [#:m{:component-type :m/renderer,
                :material-name :assets/soldier-bags-green}]}
       #:m{:object-path "Soldier/Vest",
           :components
           [#:m{:component-type :m/renderer,
                :material-name :assets/soldier-vest-green}]}],
      :delete-objects []})

(def create-deck
  {:m/message-type :m/update,
   :m/message-id "M:LKYhHswSTo6qd9gJfsl92g",
   :m/create-objects
   [{:m/object-name "Canvas",
     :m/transform
     {:m/transform-type :m/rect-transform,
      :m/position2d {:m/x 0, :m/y 0}},
     :m/components
     [{:m/component-type :m/canvas,
       :m/render-mode :screen-space-overlay}
      {:m/component-type :m/canvas-scaler,
       :m/scale-mode :constant-physical-size}
      {:m/component-type :m/graphic-raycaster}]}
    {:m/object-name "Deck",
     :m/parent-path "Canvas",
     :m/transform
     {:m/transform-type :m/rect-transform,
      :m/position2d {:m/x 0, :m/y 0},
      :m/size {:m/x 0, :m/y 20},
      :m/horizontal-anchor :right,
      :m/vertical-anchor :bottom,
      :m/pivot :lower-right},
     :m/components
     [{:m/component-type :m/image,
       :m/sprite-name :assets/light-card-deck,
       :m/image-type
       {:m/image-subtype :m/simple-image-type,
        :m/preserve-aspect-ratio true}}
      {:m/component-type :m/content-size-fitter,
       :m/horizontal-fit-mode :preferred-size,
       :m/vertical-fit-mode :unconstrained}]}],
   :m/update-objects [],
   :m/delete-objects []})

(def values
  {:test-values/quit-game quit-game
   :test-values/load-scene-empty load-scene-empty
   :test-values/load-scene-flat load-scene-flat
   :test-values/create-entity-soldier create-entity-soldier
   :test-values/create-deck create-deck})
