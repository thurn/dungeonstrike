(defproject dungeonstrike "0.1.0-SNAPSHOT"
  :description "The dungeonstrike driver"
  :license {:name "Apache License 2.0"
            :url "https://www.apache.org/licenses/LICENSE-2.0"}

  :dependencies [[org.clojure/clojure "1.9.0-alpha15"]
                 [org.clojure/core.async "0.3.442"]
                 [com.stuartsierra/component "0.3.2"]
                 [com.taoensso/timbre "4.8.0"]
                 [com.taoensso/sente "1.11.0"]
                 [io.aviso/pretty "0.1.33"]
                 [ring "1.5.0"]
                 [ring/ring-defaults "0.2.1"]
                 [http-kit "2.2.0"]
                 [compojure "1.5.2"]
                 [seesaw "1.4.5"]
                 [commons-io "2.5"]]

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
