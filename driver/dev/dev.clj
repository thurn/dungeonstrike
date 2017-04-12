(ns dev
  "Tools for interactive development with the REPL. This file should
  not be included in a production build of the application."
  (:require [clojure.tools.namespace]
            [clojure.tools.namespace.repl]))

(defn stop! []
  (println "Stopping system..."))

(defn start! []
  (println "Starting system... "))

(def dev-helpers
  '[[clojure.java.javadoc :refer [javadoc]]
    [clojure.pprint :refer [pprint]]
    [clojure.reflect :refer [reflect]]
    [clojure.tools.namespace]
    [clojure.tools.namespace.repl]
    [clojure.test :as test]
    [clojure.string :as string]
    [com.gfredericks.debug-repl :refer [break! unbreak!]]
    [clojure.repl :refer [apropos dir doc find-doc pst source]]])

(defmacro require-dev-helpers!
  "Require helpers from the dev-requires list"
  []
  `(apply require dev/dev-helpers))
