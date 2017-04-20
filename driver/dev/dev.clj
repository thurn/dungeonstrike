(ns dev
  "Tools for interactive development with the REPL. This file should
  not be included in a production build of the application."
  (:require [clojure.tools.namespace]
            [clojure.tools.namespace.repl]))

(defmacro require-dev-helpers
  "Helper macro to require functions and macros which assist with development.
   Should be included in every file, but should not be used in production."
  []
  '(require '[clojure.repl :refer [apropos dir doc find-doc pst source]]
            '[com.gfredericks.debug-repl :refer [break! unbreak!]]
            '[clojure.pprint :refer [pprint]]
            '[clojure.reflect :refer [reflect]]
            '[clojure.java.javadoc :refer [javadoc]]))

(defn stop! []
  (println "Stopping system..."))

(defn start! []
  (println "Starting system... "))
