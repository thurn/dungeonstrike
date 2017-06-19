(ns dungeonstrike.dev
  "Tools for interactive development with the REPL. This file should
  not be included in a production build of the application.")

(defmacro require-dev-helpers
  "Helper macro to require functions and macros which assist with development.
   Should be included in every file, but should not be used in production."
  []
  '(require '[clojure.repl :refer [apropos dir doc find-doc pst source]]
            '[clojure.pprint :refer [pprint]]
            '[clojure.reflect :refer [reflect]]
            '[clojure.java.javadoc :refer [javadoc]]
            '[dungeonstrike.dev :refer [p]]))

(defmacro p
  "Logs a message to the log console and to stdout."
  [message & arguments]
  `(do
     (require '[dungeonstrike.logger])
     (dungeonstrike.logger/log
      (dungeonstrike.logger/map->LogContext {})
      ~message
      ~@arguments)
     (println ~message (str ~@arguments))))
