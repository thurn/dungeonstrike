(ns dungeonstrike.logger
  "Component which provides utilities for logging application behavior. Any
   non-trivial application state transition should be logged."
  (:require [clojure.core.async :as async]
            [clojure.edn :as edn]
            [clojure.java.io :as io]
            [clojure.spec :as s]
            [clojure.string :as string]
            [dungeonstrike.uuid :as uuid]
            [com.stuartsierra.component :as component]
            [io.aviso.exception :as exception]
            [taoensso.timbre :as timbre]
            [taoensso.timbre.appenders.core :as appenders]
            [dev]))
(dev/require-dev-helpers)

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
  (map->LogContext {::driver-id (uuid/new-driver-id)}))

(s/fdef component-log-context :args (s/cat :logger ::logger
                                           :component-name string?))
(defn component-log-context
  "Creates a log context for a component. Each component should create and
   store its own log context for use in log calls that it makes."
  [logger component-name]
  (merge (::system-log-context logger) {::component-name component-name}))

(s/fdef log-helper :args (s/cat :log-context ::log-context
                                :message string?
                                :error? boolean?
                                :line integer?
                                :rest (s/* any?)))
(defn log-helper
  "Helper function invoked by the `log` and `error` macros. Returns a log entry
   map which includes the provided log context and a `:message` entry with a
   formatted version of the log message and its arguments. Do not invoke this
   function directly."
  [log-context message error? line & [arg-names & arguments]]
  (let [args (map list arg-names arguments)
        format-fn (fn [[key value]] (str key "=" (info value)))
        formatted (if (empty? arg-names)
                    message
                    (str message " ["
                         (string/join "; " (map format-fn args)) "]"))]
    (apply merge log-context {:message formatted
                              :error? error?
                              :line line}
           (filter map? arguments))))

(defmacro log
  "Logs a message with an associated LogContext and optional details. Argument
   expressions are are preserved as strings in the log output, so this macro
   should *not* be invoked with complex function calls as arguments. Argument
   values are passed to the `info` function to be summarized."
  [log-context message & arguments]
  `(timbre/info (log-helper ~log-context
                            ~message
                            false
                            ~(:line (meta &form))
                            '~arguments
                            ~@arguments)))

(defmacro error
  "Logs an error in the style of `log` with an associated LogContext and
   optional details."
  [log-context message & arguments]
  `(timbre/info (log-helper ~log-context
                            ~message
                            true
                            ~(:line (meta &form))
                            '~arguments
                            ~@arguments)))

(defmacro dbg!
  "Logs a message as in `log` without a LogContext for debugging use only.
   Code containing calls to this macro should not be checked in to version
   control."
  [message & arguments]
  `(timbre/info (log-helper (map->LogContext {})
                            ~message
                            ~(:line (meta &form))
                            '~arguments
                            ~@arguments)))

(s/fdef debug-log-channel :args (s/cat :logger ::logger))
(defn debug-log-channel
  "Returns a channel which consumes unparsed log entry lines and produces parsed
   log entries. Intended to be used for debugging tools."
  [logger]
  (::debug-log-channel logger))

(def ^:private metadata-separator
  "The unique string used to separate log messages from log metadata in lines of
   a log file."
  "\t\t»")

(defn- logger-output-fn
  "Log output function as defined by the timbre logging library. Processes
   timbre log maps into our custom log string format."
  [{:keys [instant vargs error-level? ?ns-str ?file trace ?msg-fmt] :as data}]
  (when error-level?)
  (let [[{:keys [message] :as entry} & rest] vargs]
    (str (or message ?msg-fmt (str "<" ?ns-str ">"))
         metadata-separator
         (into {} (remove (comp nil? val)
                          (merge (dissoc entry :message)
                                 {:source ?ns-str
                                  :timestamp (.getTime instant)
                                  :log-type :driver
                                  :rest rest
                                  :error? error-level?
                                  :file ?file}))))))

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

(defn- timbre-config
  "Builds the custom timbre config."
  [driver-log-file]
  {:level :info
   :output-fn logger-output-fn
   :appenders {:spit (appenders/spit-appender {:fname driver-log-file})}})

(defn- error-info
  "Helper function which uses various heuristics to produce a helpful summary of
  an exception when one is logged."
  [thread exception]
  (binding [exception/*fonts* {}]
    (let [builder (StringBuilder.)
          analyzed (exception/analyze-exception exception {:properties false
                                                           :frame-limit 10})
          root-cause (last analyzed)

         ;; Find the first stack frame that contains 'dungeonstrike', since it
         ;; likely to be relevant:

          relevant? (fn [trace]
                      (string/includes? (:formatted-name trace) "dungeonstrike"))

          proximate-cause (first (filter relevant? (:stack-trace root-cause)))]

      (exception/write-exception builder exception)
      {:message (if proximate-cause
                  (str (:formatted-name proximate-cause) "("
                       (:line proximate-cause) "): "
                       (:message root-cause))
                  (str (:class-name root-cause) ": "
                       (:message root-cause)))
       :exception-class (:class-name root-cause)
       :thead-name (.getName thread)
       :stack-trace (str builder)})))

(defrecord Logger []
  component/Lifecycle

  (start [{:keys [::driver-log-file] :as component}]
    (let [system-log-context (new-system-log-context)
          debug-log-channel (async/chan (async/sliding-buffer 1024)
                                        (map parse-log-entry))]
      (timbre/set-config! (timbre-config driver-log-file))
      (Thread/setDefaultUncaughtExceptionHandler
       (reify Thread$UncaughtExceptionHandler
         (uncaughtException [_ thread ex]
           (timbre/error (error-info thread ex))
           (throw ex))))
      (assoc component
             ::system-log-context system-log-context
             ::debug-log-channel debug-log-channel)))

  (stop [{:keys [::driver-log-file ::debug-log-channel ::system-log-context
                 ::clear-on-stop?]
          :as component}]
    (when clear-on-stop?
      (io/delete-file driver-log-file true))
    (when debug-log-channel (async/close! debug-log-channel))
    (dissoc component ::system-log-context ::debug-log-channel)))

(s/def ::logger #(instance? Logger %))

(s/fdef new-logger :args (s/cat :driver-log-file string? :rest (s/* any?)))
(defn new-logger
  "Create a new logger for the provided log file. If :clear-on-stop? is true,
   delete the logfile on component stop."
  [driver-log-file & {:keys [clear-on-stop?]}]
  (map->Logger {::driver-log-file driver-log-file
                ::clear-on-stop? clear-on-stop?}))
