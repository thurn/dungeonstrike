(defproject effects "0.1.0"
  :description "effects - a design pattern and minimal helper library for managing side effects in a Clojure application."
  :url "http://github.com/thurn/effects"
  :license {:name "Apache License 2.0"
            :url "https://www.apache.org/licenses/LICENSE-2.0"}
  :dependencies [[org.clojure/clojure "1.9.0-alpha16"]
                 [org.clojure/core.async "0.3.442"]
                 [org.clojure/test.check "0.10.0-alpha2"]
                 [org.clojure/spec.alpha "0.1.108"]]
  :profiles {:test
             {:dependencies [[orchestra "0.3.0"]]}
             :dev
             {:dependencies [[cider/cider-nrepl "0.14.0"]
                             [orchestra "0.3.0"]
                             [org.clojure/tools.namespace "0.2.11"]]}})
