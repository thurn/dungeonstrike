(ns dungeonstrike.channels
  "A simple wrapper component for a core.async channel which is created on
   system startup and closed on system shutdown. Allows channel subscribers to
   be specified at creation, in order to ensure that subscribers do not miss any
   messages sent before their startup."
  (:require [clojure.core.async :as async :refer [<!]]
            [clojure.spec :as s]
            [com.stuartsierra.component :as component]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(defrecord Channel []
  component/Lifecycle

  (start [{:keys [::args ::subscribers] :as component}]
    (if (empty? subscribers)
      ;; Avoid creating a mult if there are no subscribers since this causes
      ;; messages to be dropped
      (assoc component ::channel (apply async/chan args))
      (let [channel (apply async/chan args)
            mult (async/mult channel)
            create-tap (fn [key] [key (async/tap mult (async/chan))])]
        (assoc component
               ::channel channel
               ::mult mult
               ::taps (into {} (map create-tap subscribers))))))

  (stop [{:keys [::channel] :as component}]
    (when channel
      (async/close! channel))
    (dissoc component ::channel ::taps)))

(s/def ::channel-wrapper #(instance? Channel %))

(s/fdef get-tap
        :args (s/cat :channel ::channel-wrapper :key keyword?)
        :ret some?)
(defn get-tap
  "Returns the tap with key `key` from the provided channel wrapper."
  [{:keys [::taps]} key]
  (if-let [tap (taps key)]
    tap
    (throw (RuntimeException. (str "No tap found for key '" key "'")))))

(s/fdef unwrap
        :args (s/cat :channel ::channel-wrapper)
        :ret some?)
(defn unwrap
  "Returns the underlying channel object or mult object from the provided
   channel wrapper."
  [{:keys [::mult ::channel]}]
  (or mult channel))

(s/fdef put!
        :args (s/cat :channel ::channel-wrapper :value some?)
        :ret boolean?)
(defn put!
  "Simple wrapper around `core.async/put!` which extracts the wrapped channel
   from a channel component."
  [channel-wrapper value]
  (async/put! (::channel channel-wrapper) value))

(s/fdef new-channel :args (s/cat :arguments sequential?
                                 :subscribers (s/spec (s/* keyword?))))
(defn new-channel
  "Creates a new channel component. `arguments` should be a sequence of
   arguments which will be passed to `core.async/chan`. `subscribers` should be
   a sequence of keywords. A `mult` will be created on the channel and a `tap`
   channel will be available in the record's `taps` map, keyed by subscriber.
   Taps will be closed on system stop."
  [arguments subscribers]
  (map->Channel {::args arguments ::subscribers subscribers}))
