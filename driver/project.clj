(defproject dungeonstrike "0.1.0-SNAPSHOT"
  :description "The dungeonstrike driver"
  :license {:name "Apache License 2.0"
            :url "https://www.apache.org/licenses/LICENSE-2.0"}

  :plugins [[lein-kibit "0.1.3"]
            [jonase/eastwood "0.2.3"]
            [lein-cljfmt "0.5.6"]
            [lein-codox "0.10.3"]]

  :dependencies [[org.clojure/clojure "1.9.0-alpha15"]
                 [org.clojure/core.async "0.3.442"]
                 [org.clojure/data.json "0.2.6"]
                 [camel-snake-kebab "0.4.0"]
                 [clojure-watch "0.1.13"]
                 [com.stuartsierra/component "0.3.2"]
                 [com.taoensso/timbre "4.8.0"]
                 [commons-io "2.5"]
                 [de.ubercode.clostache/clostache "1.4.0"]
                 [http-kit "2.2.0"]
                 [io.aviso/pretty "0.1.33"]
                 [seesaw "1.4.5"]
                 [selmer "1.10.7"]]

  :repl-options {:init-ns dungeonstrike.core
                 :nrepl-middleware [com.gfredericks.debug-repl/wrap-debug-repl]
                 :init
                 (do
                   (println "Initializing...")
                   (require 'clojure.tools.namespace.repl 'dev 'cider)
                   (clojure.tools.namespace.repl/set-refresh-dirs "dev"
                                                                  "src"
                                                                  "test"))}

  :profiles {:dev {:dependencies [[org.clojure/tools.namespace "0.2.11"]
                                  [com.stuartsierra/component.repl "0.2.0"]
                                  [debugger "0.2.0"]
                                  [com.gfredericks/debug-repl "0.0.8"]]
                   :source-paths ["dev"]}})
