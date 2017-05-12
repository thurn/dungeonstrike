(ns dungeonstrike.main
  "Main entry point for application code."
  (:require [clojure.tools.cli :as cli]
            [dev]))
(dev/require-dev-helpers)

(def ^:private cli-options
  [[nil "--help" "Print this help message and quit"]
   [nil "--run-test TEST"
    "Connect to the client and run the integration test TEST"]
   [nil "--run-all-tests"
    "Connect to the client and run all integration tests."]])

(defn exit [status msg]
  (println msg)
  (System/exit status))

(defn -main [& args]
  (let [{:keys [options arguments summary]} (cli/parse-opts args cli-options)]
    (cond
      (:help options)
      (exit 0 summary)
      :otherwise
      (exit 0 options))))
