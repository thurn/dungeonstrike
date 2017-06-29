(defproject dungeonstrike "0.1.0-SNAPSHOT"
  :description "The dungeonstrike driver"
  :license {:name "Apache License 2.0"
            :url "https://www.apache.org/licenses/LICENSE-2.0"}

  :plugins [[lein-kibit "0.1.3"]
            [jonase/eastwood "0.2.3"]
            [lein-cljfmt "0.5.6"]
            [lein-codox "0.10.3"]
            [lein-expectations "0.0.8"]]

  :main dungeonstrike.main

  :dependencies [[org.clojure/clojure "1.9.0-alpha16"]
                 [org.clojure/core.async "0.3.442"]
                 [org.clojure/data.json "0.2.6"]
                 [org.clojure/tools.cli "0.3.5"]
                 [org.clojure/spec.alpha "0.1.108"]
                 [camel-snake-kebab "0.4.0"]
                 [clojure-watch "0.1.13"]
                 [com.taoensso/timbre "4.8.0"]
                 [commons-io "2.5"]
                 [de.ubercode.clostache/clostache "1.4.0"]
                 [http-kit "2.2.0"]
                 [io.aviso/pretty "0.1.33"]
                 [mount "0.1.11"]
                 [orchestra "0.3.0"]
                 [seesaw "1.4.5"]
                 [selmer "1.10.7"]]

  :repl-options {:init-ns dungeonstrike.core
                 :nrepl-middleware [com.gfredericks.debug-repl/wrap-debug-repl]
                 :init
                 (do
                   (println "Initializing...")
                   (require 'clojure.tools.namespace.repl 'cider)
                   (clojure.tools.namespace.repl/set-refresh-dirs "src"
                                                                  "test"))}

  :profiles {:uberjar {:aot [dungeonstrike.main]}
             :dev {:dependencies [[org.clojure/tools.namespace "0.2.11"]
                                  [com.stuartsierra/component.repl "0.2.0"]
                                  [cider/cider-nrepl "0.14.0"]
                                  [debugger "0.2.0"]
                                  [expectations "2.2.0-beta1"]
                                  [philoskim/debux "0.3.1"]
                                  [com.gfredericks/debug-repl "0.0.8"]]
                   :source-paths ["dev"]}})
