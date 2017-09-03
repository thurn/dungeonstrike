(ns dungeonstrike.logger
  "Component which provides utilities for logging application behavior. Any
   non-trivial application state transition should be logged."
  (:require [clojure.core.async :as async]
            [clojure.edn :as edn]
            [clojure.java.io :as io]
            [clojure.spec.alpha :as s]
            [clojure.string :as string]
            [dungeonstrike.paths :as paths]
            [dungeonstrike.uuid :as uuid]
            [effects.core :as effects]
            [mount.core :as mount]
            [taoensso.timbre :as timbre]
            [taoensso.timbre.appenders.core :as appenders]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(mount/defstate ^:private driver-id
  :start (uuid/new-driver-id))

(mount/defstate ^:private current-client-id
  :start (atom nil))

(def ^:private metadata-separator
  "The unique string used to separate log messages from log metadata in lines of
   a log file."
  "\t\t»")

(s/def ::client-id string?)

(defmethod effects/effect-spec ::set-client-id [_]
  (s/keys :req-un [::client-id]))

(defmethod effects/apply! ::set-client-id
  [{:keys [:client-id]}]
  (reset! current-client-id client-id))

(defn- logger-output-fn
  "Log output function as defined by the timbre logging library. Processes
   timbre log maps into our custom log string format."
  [{:keys [instant vargs error-level? ?ns-str ?file trace ?msg-fmt] :as data}]
  (let [[{:keys [message] :as entry} & rest] vargs
        strip-newlines (fn [msg] (string/replace msg "\n" " \\n "))
        msg (or message ?msg-fmt (str "<" ?ns-str ">"))]
    (str (strip-newlines msg)
         metadata-separator
         (into {} (remove #(nil? (val %))
                          (merge (dissoc entry :message :?msg-fmt)
                                 {:source ?ns-str
                                  :timestamp (.getTime instant)
                                  :log-type :driver
                                  :rest rest
                                  :error? error-level?
                                  :file ?file}))))))

(defn- timbre-config
  "Builds the custom timbre config."
  [driver-log-file]
  {:level :info
   :output-fn logger-output-fn
   :appenders {:spit (appenders/spit-appender {:fname driver-log-file})}})

(mount/defstate timbre-state
  :start (timbre/set-config! (timbre-config paths/driver-log-path)))

(defn- parse-log-entry
  "Parses a logfile entry line into a log entry map."
  [line]
  (try
    (let [metadata-start (string/index-of line metadata-separator)
          separator-length (count metadata-separator)
          message (subs line 0 metadata-start)
          metadata (edn/read-string
                    (subs line (+ metadata-start separator-length)))]
      (assoc metadata :message message))
    (catch Exception exception
      {:message (str "Error parsing log entry: " line)})))

(defn- show-entry?
  "Returns true if the provided map entry uses the current driver id or the most
   recent client id."
  [message]
  (or (= driver-id (:driver-id message))
      (and (some? @current-client-id)
           (= @current-client-id (:client-id message)))))

(mount/defstate ^:private debug-log-transducer
  "A transducer for parsing log entry strings."
  :start (comp (map parse-log-entry) (filter show-entry?)))

(mount/defstate debug-log-channel
  "Channel on which new log entries should be published. Consumes log entry
   strings and produces parsed log entries."
  :start (async/chan 1 debug-log-transducer)
  :stop (async/close! debug-log-channel))

(mount/defstate debug-log-mult
  "Mult on `debug-log-channel`."
  :start (async/mult debug-log-channel))

(defn- format-argument
  "Formats an argument to a log function."
  [argument]
  (if (map? argument)
    (cond
      (:m/message-type argument)
      (str "<[" (:m/message-type argument) "] " (:m/message-id argument) ">")
      (:a/action-type argument)
      (str "<[" (:a/action-type argument) "] " (:a/action-id argument) ">")
      :otherwise
      "{...}")
    (str argument)))

(defn- format-message
  "Formats a message and message arguments for output as a log message."
  [message arguments]
  (if (empty? arguments)
    message
    (str message " [" (string/join "; " (map format-argument arguments)) "]")))

(s/fdef log-helper :args (s/cat :message string?
                                :log-type keyword?
                                :line any?
                                :rest (s/* any?)))
(defn log-helper
  "Helper function invoked by the various log macros. Returns a log entry
  map which includes the provided context and a `:message` entry with a
  formatted version of the log message and its arguments. Do not invoke this
  function directly."
  [message log-style line & arguments]
  (apply merge
         {:driver-id driver-id
          :message (format-message message arguments)
          :error? (= :error log-style)
          :log-style log-style
          :line line}
         (filter map? arguments)))

(defmacro log
  "Logs a message with optional details. Map arguments will be merged into log
  output"
  [message & arguments]
  `(timbre/info (log-helper ~message
                            :log
                            ~(:line (meta &form))
                            ~@arguments)))

(defmacro warning
  "Logs a warning message in the style of `log`."
  [message & arguments]
  `(timbre/info (log-helper ~message
                            :warning
                            ~(:line (meta &form))
                            ~@arguments)))

(defmacro log-gui
  "Logs a message which corresponds to direct user input in the developer GUI.
  Used to provide distinctive highlighting to user actions in the logs."
  [message & arguments]
  `(timbre/info (log-helper ~message
                            :gui
                            ~(:line (meta &form))
                            ~@arguments)))

(defn throw-exception
  "Helper function invoked by the `error` macro to throw an exception."
  [message [arg-names & arguments]]
  (throw (RuntimeException.
          (format-message message arguments))))

(defmacro error
  "Logs an error in the style of `log` with optional details."
  [message & arguments]
  `(do (timbre/error (log-helper ~message
                                 :error
                                 ~(:line (meta &form))
                                 ~@arguments))
       (throw-exception ~message ~@arguments)))
