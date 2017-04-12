(ns dungeonstrike.core
  (:require [clojure.core.async :as async]
            [com.stuartsierra.component :as component]
            [dungeonstrike.gui :as gui]
            [dungeonstrike.log-tailer :as log-tailer]
            [dungeonstrike.logger :as logger]
            [dungeonstrike.websocket :as websocket]
            [dev]))
(dev/require-dev-helpers!)

(defn driver-log-file []
  (str (System/getProperty "user.dir")
       "/logs/driver_logs.txt"))

(defn client-log-file []
  (str (System/getProperty "user.dir")
       "/../DungeonStrike/Assets/Logs/client_logs.txt"))

(defn create-system []
  (let [driver-log-file (driver-log-file)
        client-log-file (client-log-file)]
    (component/system-map
     :logger
     (logger/new-logger driver-log-file :clear-on-stop? false)

     :driver-log-tailer
     (component/using (log-tailer/new-log-tailer driver-log-file) [:logger])

     :client-log-tailer
     (component/using (log-tailer/new-log-tailer client-log-file) [:logger])

     :debug-gui
     (component/using (gui/new-debug-gui) [:logger])

     :websocket
     (component/using (websocket/new-websocket 59005) [:logger]))))

(defonce system (atom nil))

(defn stop! []
  (when @system (component/stop @system))
  (reset! system nil))

(defn start! []
  (when @system (stop!))
  (reset! system (component/start (create-system))))

(defn -main [& args]
  (component/start (create-system)))
