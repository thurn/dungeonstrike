(ns dungeonstrike.main.core
  (:require [cljs.nodejs :as nodejs]
            [com.stuartsierra.component :as component]
            [dungeonstrike.main.app :as app]
            [dungeonstrike.main.log-processor :as log-processor]
            [dungeonstrike.main.logger :as logger]
            [dungeonstrike.main.web-socket :as web-socket]))

(defn driver-system []
  (component/system-map
   :app (app/new-app)
   :log-processor (component/using (log-processor/new-log-processor) [:app])
   :logger (component/using (logger/new-logger) [:app])
   :web-socket (component/using (web-socket/new-web-socket) [:logger])))

(defonce system (atom nil))

(defn init! []
  (reset! system (driver-system)))

(defn start! []
  (println "Starting System")
  (swap! system component/start))

(defn stop! []
  (println "Stopping System")
  (swap! system (fn [s] (when s (component/stop s)))))

(defn -main [& args]
  (enable-console-print!)
  (init!)
  (start!))

(defn restart! []
  (println "== Restarting ==")
  (stop!)
  (init!)
  (start!))

(set! *main-cli-fn* -main)
