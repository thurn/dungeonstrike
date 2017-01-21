(defproject dungeonstrike "0.1.0-SNAPSHOT"
  :description "The dungeonstrike driver."
  :license {:name "Apache License 2.0"
            :url "https://www.apache.org/licenses/LICENSE-2.0"}

  :dependencies [[org.clojure/clojure "1.9.0-alpha14" :scope "provided"]
                 [org.clojure/clojurescript "1.9.293" :scope "provided"]
                 [cljsjs/react-bootstrap "0.30.6-0"]
                 [reagent-forms "0.5.28"]
                 [reagent "0.6.0"]]

  :plugins [[lein-cljsbuild "1.1.5"]
            [lein-figwheel "0.5.8"]
            [lein-marginalia "0.9.0"]]

  :min-lein-version "2.5.3"

  :clean-targets ^{:protect false}
  [:target-path
   [:cljsbuild :builds :app :compiler :output-dir]
   [:cljsbuild :builds :app :compiler :output-to]]

  :resource-paths ["public"]

  :figwheel {:http-server-root "public"
             :nrepl-port 7888
             :nrepl-middleware ["cider.nrepl/cider-middleware"
                                "cemerick.piggieback/wrap-cljs-repl"]
             ;:repl false
             :css-dirs ["public/css"]}


  :cljsbuild {:builds
              [{:id "ui"
                :source-paths ["src/dungeonstrike/ui" "src/dungeonstrike/common" "env"]
                ;:figwheel true
                :compiler {:main "dungeonstrike.ui.dev"
                           :output-to "public/js/app.js"
                           :output-dir "public/js/out"
                           :asset-path "js/out"
                           :preloads [devtools.preload]
                           :external-config {:devtools/config {:features-to-install :all}}
                           :optimizations :none
                           :source-map true
                           :pretty-print true}}
               {:id "main"
                :source-paths ["src/dungeonstrike/main" "src/dungeonstrike/common"]
                ;:figwheel true
                :compiler {:main "dungeonstrike.main.core"
                           :output-to "public/main/main.js"
                           :output-dir "public/main/out"
                           :target :nodejs
                           :optimizations :none
                           :source-map true
                           :pretty-print true}}]}

  :profiles {:repl {:plugins [[cider/cider-nrepl "0.14.0"]]}
             :dev {:repl-options {:nrepl-middleware
                                  [cemerick.piggieback/wrap-cljs-repl]}
                   :dependencies [[figwheel-sidecar "0.5.8"]
                                  [binaryage/devtools "0.8.2"]
                                  [cider/cider-nrepl "0.14.0"]
                                  [com.cemerick/piggieback "0.2.2-SNAPSHOT"]]}})
