(ns dungeonstrike.logger
  "Component which provides utilities for logging application behavior. Any
   non-trivial application state transition should be logged."
  (:require [clojure.core.async :as async]
            [clojure.edn :as edn]
            [clojure.java.io :as io]
            [clojure.spec :as s]
            [clojure.string :as string]
            [dungeonstrike.nodes :as nodes]
            [dungeonstrike.paths :as paths]
            [dungeonstrike.uuid :as uuid]
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

(defn- current-entry?
  "Returns true if the provided map entry uses the current driver id or the most
   recent client id."
  [message]
  (or (= driver-id (::driver-id message))
      (and (some? @current-client-id)
           (= @current-client-id (:client-id message)))))

(mount/defstate ^:private debug-log-transducer
  "A transducer for parsing log entry strings."
  :start (comp (map parse-log-entry) (filter current-entry?)))

(mount/defstate debug-log-channel
  "Channel on which new log entries should be published. Consumes log entry
   strings and produces parsed log entries."
  :start (async/chan 1 debug-log-transducer)
  :stop (async/close! debug-log-channel))

(mount/defstate debug-log-mult
  "Mult on `debug-log-channel`."
  :start (async/mult debug-log-channel))

(defn- reduce-info-map
  "Reducer helper function for use by `info`. Creates a summarized version of a
   map entry."
  [map key-form value-form]
  (let [key (str key-form)
        value (str value-form)
        shorten (fn [s]
                  (if (> (count s) 10)
                    (str (subs s 0 10) "...")
                    s))]
    (assoc map (shorten key) (shorten value))))

(defn- info
  "Returns a concise summary of `value` appropriate for including in a log
   message."
  [value]
  (cond
    (string? value) value
    (:m/message-type value) (str "<[" (name (:m/message-type value)) "] "
                                 (:m/message-id value) ">")
    (map? value) (str (reduce-kv reduce-info-map {} value))
    :otherwise (str value)))

;; Private record for storing log metadata. Should only be created/modified
;; by this component.
(defrecord LogContext [])
(s/def ::log-context #(instance? LogContext %))

(defn- new-system-log-context
  "Creates a new system-level log context. This log context should be used for
   entries which are not scoped to any specific external stimulus -- i.e. they
   are operations of the system itself. Only one system log context should ever
   be created."
  []
  (map->LogContext {::driver-id driver-id}))

(defn- format-message
  "Formats a message and message arguments for output as a log message."
  [message arg-names arguments]
  (let [args (map list arg-names arguments)
        format-fn (fn [[key value]] (str key "=" (info value)))]
    (if (empty? arg-names)
      message
      (str message " [" (string/join "; " (map format-fn args)) "]"))))

(s/fdef log-helper :args (s/cat :log-context ::log-context
                                :message string?
                                :important? boolean?
                                :error? boolean?
                                :line any?
                                :rest (s/* any?)))
(defn log-helper
  "Helper function invoked by the `log` and `error` macros. Returns a log entry
   map which includes the provided log context and a `:message` entry with a
   formatted version of the log message and its arguments. Do not invoke this
   function directly."
  [log-context message important? error? line & [arg-names & arguments]]
  (apply merge log-context {:message (format-message message
                                                     arg-names
                                                     arguments)
                            :important? important?
                            :error? error?
                            :line line}
         (filter map? arguments)))

(defmacro log-important!
  "Logs a message as in `log`, but flags it as 'important' for special handling
   in the logging UI."
  [log-context message & arguments]
  `(timbre/info (log-helper ~log-context
                            ~message
                            true
                            false
                            ~(:line (meta &form))
                            '~arguments
                            ~@arguments)))

(defmacro log
  "Logs a message with an associated LogContext and optional details. Argument
   expressions are are preserved as strings in the log output, so this macro
   should *not* be invoked with complex function calls as arguments. Argument
   values are passed to the `info` function to be summarized."
  [log-context message & arguments]
  `(timbre/info (log-helper ~log-context
                            ~message
                            false
                            false
                            ~(:line (meta &form))
                            '~arguments
                            ~@arguments)))

(defmacro error
  "Logs an error in the style of `log` with an associated LogContext and
   optional details."
  [log-context message & arguments]
  `(timbre/error (log-helper ~log-context
                             ~message
                             false
                             true
                             ~(:line (meta &form))
                             '~arguments
                             ~@arguments)))

(defn throw-exception
  "Helper function invoked by the `die!` macro to throw an exception."
  [message [arg-names & arguments]]
  (throw (RuntimeException.
          (format-message message arg-names arguments))))

(defmacro die!
  "Logs an error in the style of `log` with an associated LogContext and
   optional details, and then throws an exception. Unlike with the previous two
   macros, `log-context` can be nil for this call."
  [log-context message & arguments]
  `(do
     (timbre/error (log-helper (or ~log-context (map->LogContext {}))
                               ~message
                               false
                               true
                               ~(:line (meta &form))
                               '~arguments
                               ~@arguments))
     (throw-exception ~message '~arguments ~@arguments)))

(defn- logger-output-fn
  "Log output function as defined by the timbre logging library. Processes
   timbre log maps into our custom log string format."
  [{:keys [instant vargs error-level? ?ns-str ?file trace ?msg-fmt] :as data}]
  (when error-level?)
  (let [[{:keys [message] :as entry} & rest] vargs
        strip-newlines (fn [msg] (string/replace msg "\n" " \\n "))
        msg (or message ?msg-fmt (str "<" ?ns-str ">"))]
    (str (strip-newlines msg)
         metadata-separator
         (into {} (remove #(nil? (val %))
                          (merge (dissoc entry :message :?msg-fmt)
                                 {:source ?ns-str
                                  :formatted msg
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

(defn- new-logger
  "Configures timbre and stores a new system log context."
  []
  (timbre/set-config! (timbre-config paths/driver-log-path))
  {::system-log-context (new-system-log-context)})

(mount/defstate ^:private logger :start (new-logger))

(s/fdef component-log-context :args (s/cat :component-name string?))
(defn component-log-context
  "Creates a log context for a component. Each component should create and
   store its own log context for use in log calls that it makes."
  [component-name]
  (merge (::system-log-context logger) {::component-name component-name}))

(nodes/defnode :m/client-connected
  [:m/client-id]
  (nodes/new-effect :logger client-id))

(defrecord LoggerEffector []
  nodes/EffectHandler
  (execute-effect! [_ client-id]
    (reset! current-client-id client-id)))
