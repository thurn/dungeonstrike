(ns dungeonstrike.main.web-messaging)

(defonce web-contents (atom nil))

(defn send-log [log]
  (println "sending" (log :type) (log :message))
  (if @web-contents
    (.send @web-contents "log" (clj->js log))
    (println "NO CONNECTION: " (log :message))))

(defn log [type message]
  (send-log {:type type
             :timestamp (.getTime (js/Date.))
             :message [message]
             :trace []}))

(defn start! [main-window]
  (reset! web-contents (.-webContents main-window))
  (println "Started Web Messaging"))
