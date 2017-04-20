(ns dungeonstrike.core
  "The entry point for the application. Utilizies the 'component' microframework
  to manage the lifecycle for each independent piece of the application. Should
  not be imported by any code."
  (:require [clojure.core.async :as async]
            [clojure.spec.test :as spec-test]
            [com.stuartsierra.component :as component]
            [dungeonstrike.code-generator :as code-generator]
            [dungeonstrike.gui :as gui]
            [dungeonstrike.log-tailer :as log-tailer]
            [dungeonstrike.logger :as logger]
            [dungeonstrike.websocket :as websocket]
            [dev]))
(dev/require-dev-helpers)

(defn- driver-log-file
  "The path to the log file for driver logs."
  []
  (str (System/getProperty "user.dir")
       "/logs/driver_logs.txt"))

(defn- client-log-file
  "The path to the log file for client logs."
  []
  (str (System/getProperty "user.dir")
       "/../DungeonStrike/Logs/client_logs.txt"))

(defn- code-generator-output-file
  "The path to the file containing generated C# code produced by
   CodeGenerator."
  []
  (str (System/getProperty "user.dir")
       "/../DungeonStrike/Assets/Source/Messaging/Generated.cs"))

(defn- subscriber-channel
  "Creates and returns a new channel subscribed to `topic` on the publication
   `pub`."
  [pub topic]
  (let [channel (async/chan)]
    (async/sub pub topic channel)
    channel))

(defn- create-system
  "Creates a new component framework 'System Map' which instantiates all
   components used by the application and binds their dependencies."
  []
  (let [driver-log-file (driver-log-file)
        client-log-file (client-log-file)
        inbound-message-channel (async/chan 1024)
        message-pub (async/pub inbound-message-channel :event-type)]
    (component/system-map
     ::logger
     (logger/new-logger driver-log-file :clear-on-stop? false)

     ::driver-log-tailer
     (component/using (log-tailer/new-log-tailer driver-log-file)
                      {:dungeonstrike.log-tailer/logger ::logger})

     ::client-log-tailer
     (component/using (log-tailer/new-log-tailer client-log-file)
                      {:dungeonstrike.log-tailer/logger ::logger})

     ::code-generator
     (component/using (code-generator/new-code-generator
                       (code-generator-output-file))
                      {:dungeonstrike.code-generator/logger ::logger})

     ::websocket
     (component/using (websocket/new-websocket 59005 inbound-message-channel)
                      {:dungeonstrike.websocket/logger ::logger})

     ::debug-gui
     (component/using (gui/new-debug-gui
                       (subscriber-channel message-pub :status))
                      {:dungeonstrike.gui/logger ::logger
                       :dungeonstrike.gui/message-sender ::websocket}))))

; Atom containing the system -- for development use only
(defonce system (atom nil))

(defn stop!
  "Stops all components in the current system. For development use only."
  []
  (when @system (component/stop @system))
  (reset! system nil))

(defn start!
  "Creates a new system and starts all components in it. For development use
   only."
  []
  (when @system (stop!))
  (spec-test/instrument)
  (reset! system (component/start (create-system))))

(defn -main [& args]
  (component/start (create-system)))
