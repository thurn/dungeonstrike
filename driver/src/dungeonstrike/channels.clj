(ns dungeonstrike.channels
  "A simple wrapper component for a core.async channel which is created on
   system startup and closed on system shutdown. Allows channel subscribers to
   be specified at creation, in order to ensure that subscribers do not miss any
   messages sent before their startup."
  (:require [clojure.core.async :as async :refer [<!]]
            [clojure.spec :as s]
            [com.stuartsierra.component :as component]
            [dev]))
(dev/require-dev-helpers)

(defrecord Channel [channel taps]
  component/Lifecycle
  (start [{:keys [::args ::subscribers] :as component}]
    (let [channel (apply async/chan args)
          mult (async/mult channel)
          create-tap (fn [key] [key (async/tap mult (async/chan))])]
      (assoc component
             :channel channel
             :taps taps (into {} (map create-tap subscribers)))))
  (stop [{:keys [channel taps ::mult] :as component}]
    (when channel
      (async/close! channel))
    (dissoc component :channel)))

(s/fdef new-channel :args (s/cat :args seq? :subscribers (s/* keyword?)))
(defn new-channel
  "Creates a new channel component. `args` should be a sequence of arguments
   which will be passed to `core.async/chan`. `subscribers` should be a sequence
   of keywords. A `mult` will be created on the channel and a `tap` channel will
   be available in the record's `taps` map, keyed by subscriber. Taps will be
   closed on system stop."
  [args subscribers]
  (map->Channel {::args args ::subscribers subscribers}))
