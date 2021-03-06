(defproject dungeonstrike "0.1.0-SNAPSHOT"
  :description "The dungeonstrike driver"
  :license {:name "Apache License 2.0"
            :url "https://www.apache.org/licenses/LICENSE-2.0"}

  :plugins [[lein-kibit "0.1.3"]
            [jonase/eastwood "0.2.3"]
            [lein-cljfmt "0.5.7"]
            [lein-codox "0.10.3"]
            [lein-expectations "0.0.8"]]

  :main dungeonstrike.main

  :dependencies [[org.clojure/clojure "1.9.0"]
                 [org.clojure/core.async "0.3.442"]
                 [org.clojure/data.json "0.2.6"]
                 [org.clojure/test.check "0.10.0-alpha2"]
                 [org.clojure/tools.cli "0.3.5"]
                 [org.clojure/core.specs.alpha "0.1.24"]
                 [camel-snake-kebab "0.4.0"]
                 [clojure-watch "0.1.13"]
                 [com.taoensso/timbre "4.8.0"]
                 [commons-io "2.5"]
                 [de.ubercode.clostache/clostache "1.4.0"]
                 [effects "0.1.0"]
                 [http-kit "2.3.0-beta2"]
                 [io.aviso/pretty "0.1.33"]
                 [mount "0.1.11"]
                 [orchestra "2017.11.12-1"]
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
                                  [cider/cider-nrepl "0.14.0"]
                                  [debugger "0.2.0"]
                                  [com.gfredericks/debug-repl "0.0.8"]]
                   :source-paths ["dev"]}})
