(ns dungeonstrike.exception-handler
  "Function for setting the default uncaught exception handler."
  (:require [clojure.string :as string]
            [io.aviso.exception :as exception]
            [mount.core :as mount]
            [taoensso.timbre :as timbre]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

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
          relevant? (fn [trace] (string/includes? (:formatted-name trace)
                                                  "dungeonstrike"))

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

(defn set-default-exception-handler!
  "Sets a Java default uncaught exception handler."
  []
  (Thread/setDefaultUncaughtExceptionHandler
   (reify Thread$UncaughtExceptionHandler
     (uncaughtException [_ thread ex]
       (println "==== Uncaught Exception! ===")
       (println ex)
       ; Lazily resolve logger to prevent direct dependency:
       (let [log-helper (resolve (symbol "dungeonstrike.logger" "log-helper"))
             error (error-info thread ex)
             message (:message error)]
         (when log-helper
           (timbre/error (log-helper message true 0 error))))
       (when (:crash-on-exceptions (mount/args))
         (println "Terminating.")
         (System/exit 1))))))
