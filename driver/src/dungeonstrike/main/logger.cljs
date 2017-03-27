(ns dungeonstrike.main.logger
  (:require [com.stuartsierra.component :as component]
            [dungeonstrike.main.app :as app]))

(defn log [{:keys [app]} tag message & {:keys [error?]
                                        :or {error? false}}]
  (app/send-to-frontend app "log" {:type tag
                                   :timestamp (.getTime (js/Date.))
                                   :message [message]
                                   :error? error?
                                   :source "driver"
                                   :trace []}))

(defn error [logger tag message]
  (log logger tag message :error true))

(defrecord Logger [app]
  component/Lifecycle
  (start [this]
    (println "Starting Logger")
    this)
  (stop [this] this))

(defn new-logger []
  (map->Logger {}))
