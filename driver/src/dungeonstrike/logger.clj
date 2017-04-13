(ns dungeonstrike.logger
  (:require [clojure.java.io :as io]
            [clojure.string :as string]
            [clojure.edn :as edn]
            [clojure.core.async :as async]
            [com.stuartsierra.component :as component]
            [taoensso.timbre :as timbre]
            [taoensso.timbre.appenders.core :as appenders]
            [io.aviso.exception :as exception]
            [dev]))
(dev/require-dev-helpers!)

(defrecord LogContext [driver-id])

(defn- new-system-log-context
   "Creates a new system-level log context. This log context should be used for
   entries which are not scoped to any specific external stimulus -- i.e. they
   are operations of the system itself. Only one system log context should ever
   be created."
  []
  (map->LogContext {:driver-id (str (java.util.UUID/randomUUID))}))

(defn log-helper [log-context message line & [arg-names & arguments]]
  (when-not (instance? LogContext log-context)
    (throw (IllegalArgumentException.
            (str "The first argument to log must be a LogContext, got "
                 log-context))))
  (when (nil? message)
    (throw (IllegalArgumentException.
            "The second argument to log cannot be nil.")))
  (let [args (map list arg-names arguments)
        format-fn (fn [[key value]] (str key "=" value))
        formatted (if (empty? arg-names)
                    message
                    (str message " ["
                         (string/join "; " (map format-fn args)) "]"))]
    (merge log-context {:message formatted
                        :line line})))

(defmacro dbg!
  "logs a message without a LogContext for debugging use only. Code containing
   calls to this macro should not be checked in to version control."
  [message]
  `(timbre/info (log-helper (map->LogContext {})
                            ~message
                            ~(:line (meta &form)))))

(defmacro log
  "Logs a message with an associated LogContext and optional details."
  [log-context message & arguments]
  `(timbre/info (log-helper ~log-context
                            ~message
                            ~(:line (meta &form))
                            '~arguments
                            ~@arguments)))

(def metadata-separator "\t\t»")

(defn omit-from-metadata?
  [[key value]]
  (or (nil? value) (#{:async-channel} key)))

(defonce dat (atom nil))

(defn logger-output-fn [{:keys [instant vargs error-level? ?ns-str ?file trace
                                ?msg-fmt]
                         :as data}]
  (when error-level?
    (reset! dat data))
  (let [[{:keys [message] :as entry} & rest] vargs]
    (str (or message ?msg-fmt (str "<" ?ns-str ">"))
         metadata-separator
         (into {} (remove omit-from-metadata?
                          (merge (dissoc entry :message)
                                 {:source ?ns-str
                                  :timestamp (.getTime instant)
                                  :log-type :driver
                                  :rest rest
                                  :error? error-level?
                                  :file ?file}))))))

(defn parse-log-entry
  "Parses a log entry line into a log entry map."
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

(defn timbre-config [driver-log-file]
  {:level :info
   :output-fn logger-output-fn
   :appenders {:spit (appenders/spit-appender {:fname driver-log-file})}})

(def e (atom nil))

(defn error-info [thread exception]
  (reset! e exception)
  (binding [exception/*fonts* {}]
   (let [builder (StringBuilder.)
         analyzed (exception/analyze-exception exception {:properties false
                                                          :frame-limit 10})
         root-cause (last analyzed)
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

(defrecord Logger [driver-log-file clear-on-stop?
                   system-log-context debug-log-channel]
  component/Lifecycle

  (start [{:keys [driver-log-file] :as component}]
    (let [system-log-context (new-system-log-context)
          debug-log-channel (async/chan (async/sliding-buffer 1024)
                                        (map parse-log-entry))]
      (timbre/set-config! (timbre-config driver-log-file))
      (Thread/setDefaultUncaughtExceptionHandler
       (reify Thread$UncaughtExceptionHandler
         (uncaughtException [_ thread ex]
           (timbre/error (error-info thread ex)))))
      (assoc component
             :system-log-context system-log-context
             :debug-log-channel debug-log-channel)))

  (stop [{:keys [driver-log-file debug-log-channel system-log-context
                 clear-on-stop?]
          :as component}]
    (when clear-on-stop?
      (io/delete-file driver-log-file true))
    (when debug-log-channel (async/close! debug-log-channel))
    (dissoc component :system-log-context :debug-log-channel)))

(defn new-logger [driver-log-file & {:keys [clear-on-stop?]}]
  (map->Logger {:driver-log-file driver-log-file
                :clear-on-stop? clear-on-stop?}))
