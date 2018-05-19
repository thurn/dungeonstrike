(ns dungeonstrike.gui
  "A Component which renders a Swing UI window showing various debugging
   tools."
  (:require [clojure.core.async :as async :refer [<!]]
            [clojure.edn :as edn]
            [clojure.java.io :as io]
            [clojure.pprint :as pprint]
            [clojure.spec.alpha :as s]
            [clojure.string :as string]
            [clojure-watch.core :as watch]
            [dungeonstrike.logger :as logger]
            [dungeonstrike.messages :as messages]
            [dungeonstrike.message-gui :as message-gui]
            [dungeonstrike.paths :as paths]
            [dungeonstrike.requests :as requests]
            [dungeonstrike.test-runner :as test-runner]
            [dungeonstrike.test-message-values :as test-values]
            [dungeonstrike.websocket :as websocket]
            [dungeonstrike.uuid :as uuid]
            [effects.core :as effects]
            [mount.core :as mount]
            [seesaw.core :as seesaw]
            [seesaw.font :as font]
            [seesaw.mig :as mig]
            [seesaw.table :as table]
            [dungeonstrike.dev :as dev])
  (:import (java.awt Color)
           (java.util Comparator)
           (javax.swing ListSelectionModel SwingUtilities JOptionPane)
           (javax.swing.event ListSelectionListener)
           (javax.swing.table DefaultTableCellRenderer TableRowSorter)))
(dev/require-dev-helpers)

(mount/defstate ^:private log-channel
  :start (async/tap logger/debug-log-mult (async/chan)))

(mount/defstate ^:private recording-state
  :start (atom {:recording? false}))

(defn- load-gui-state
  []
  (let [file (io/file paths/driver-gui-state-path)]
    (if (.exists file)
      (atom (edn/read-string (slurp file)))
      (atom {}))))

(defn- save-gui-state
  [state]
  (let [file (io/file paths/driver-gui-state-path)]
    (spit file (str state))))

;; State which should be persisted between restarts, mostly for convenience so
;; that you don't constantly need to re-navigate to the same view. Use with
;; caution.
(mount/defstate ^:private persistent-state
  :start (load-gui-state)
  :stop (save-gui-state @persistent-state))

(mount/defstate ^:private gui-state
  :start (atom {}))

(declare update-frame!)

(defn- title-font
  "Font to use in section headers"
  []
  (font/font :style #{:bold} :size 18))

(defn- process-position
  [value]
  (if (= value "")
    {:m/x 0, :m/y 0}
    (let [[x y] (string/split value #",")]
      {:m/x (Integer/parseInt x), :m/y (Integer/parseInt y)})))

(defn- process-form-values
  [message key value]
  (let [processed-value (if (and (keyword? value)
                                 (= "test-values" (namespace value)))
                          (test-values/lookup-test-value-key value)
                          value)]
    (if (= "m" (namespace key))
      (assoc message key processed-value)
      message)))

(defn- on-send-button-clicked
  "Returns a click handler function for the 'send' button."
  [message-form]
  (fn [event]
    (let [form-value (seesaw/value message-form)
          message (reduce-kv process-form-values {} form-value)]
      (when (:recording? @recording-state)
        (swap! recording-state assoc
               :message->client message))
      (websocket/send-message! message)
      (update-frame!))))

(defn- message-form-items [send-button message-picker message-type]
  (concat
   [[(seesaw/label :text "Send Message" :font (title-font))
     "alignx center, pushx, span, wrap 20px"]
    [(seesaw/label "Message Type")]
    [message-picker "wrap"]
    [(seesaw/label "Message ID")]
    [(seesaw/label :id :m/message-id
                   :text (uuid/new-message-id)) "wrap"]]
   (message-gui/editor-for-message-type message-type)
   [[send-button "skip, span, wrap"]]))

(defn- message-selected-fn [message-picker]
  (fn [e]
    (requests/send-request!
     (effects/request :r/message-selected
                      :m/message-type (seesaw/selection message-picker)))))

(defmethod effects/evaluate :r/message-selected
  [{:keys [:m/message-type]}]
  [(effects/effect ::config :selector :#message-form :key :selected
                   :value message-type :persistent? true)])

(defn- send-message-panel
  "UI for 'Send Message' panel."
  []
  (let [message-types (keys messages/messages)
        message-picker (seesaw/combobox :id :m/message-type
                                        :model message-types)
        enabled? (get-in @gui-state [:#send-button :enabled?] false)
        send-button (seesaw/button :text "Send!"
                                   :id :send-button
                                   :enabled? enabled?)
        selected (get-in @persistent-state
                         [:#message-form :selected] :m/load-scene)
        form-items (message-form-items send-button message-picker selected)
        panel (mig/mig-panel :id :message-form :items form-items)]
    (seesaw/selection! message-picker selected)
    (seesaw/listen message-picker :selection
                   (message-selected-fn message-picker))
    (seesaw/listen send-button :action (on-send-button-clicked panel))
    panel))

(defn- recording-file-names
  "Returns all available file names of test recordings in the recordings
   directory."
  []
  (let [files (file-seq (io/file paths/test-recordings-path))]
    (mapv #(.getName %) (filter #(.isFile %) files))))

(defn- test-list-table
  [file-names]
  (let [result (seesaw/table :id :test-list
                             :model [:columns [:test]
                                     :rows (map vector file-names)])]
    (.setSelectionMode result ListSelectionModel/SINGLE_SELECTION)
    result))

(defn- run-test
  "Requests to run the test named in `test-name`, or all tests if the keyword
   `:all-tests` is passed."
  [test-name]
  (logger/log-gui "Running test" test-name)
  (async/go
    (let [{:keys [:status :message] :as result}
          (<! (test-runner/run-integration-test test-name))]
      (if (= status :success)
        (logger/log-gui "Test passed!" test-name)
        (logger/warning message result)))))

(defn- tests-panel
  "UI for the 'Tests' panel"
  []
  (let [file-names (recording-file-names)
        run-selected-button (seesaw/button :text "Run Selected" :enabled? false)
        run-all-button (seesaw/button :text "Run All")
        test-list (test-list-table file-names)]
    (seesaw/listen run-selected-button :action
                   (fn [e]
                     (when-let [test (seesaw/value test-list)]
                       (run-test (file-names (seesaw/selection test-list))))))
    (seesaw/listen run-all-button :action
                   (fn [e] (run-test :all-tests)))
    (seesaw/listen test-list :selection
                   (fn [e]
                     (seesaw/config! run-selected-button :enabled? true)))
    (mig/mig-panel
     :id :tests
     :items [[(seesaw/label :text "Tests" :font (title-font))
              "alignx center, pushx, span, wrap 20px"]
             [run-selected-button]
             [run-all-button "wrap 20px"]
             [test-list "grow, span"]])))

(defn- left-content
  []
  (let [panel (seesaw/tabbed-panel :placement :top
                                   :tabs [{:title "Send Message"
                                           :content (send-message-panel)}
                                          {:title "Tests"
                                           :content (tests-panel)}])]
    (seesaw/listen panel :selection
                   (fn [e] (swap! persistent-state assoc :left-tab-index
                                  (:index (seesaw/selection panel)))))
    (seesaw/selection! panel (get @persistent-state :left-tab-index 0))))

(defn- on-log-selected
  "Callback when a log entry is selected"
  [log-table log-info event]
  (let [index (seesaw/selection log-table)
        info-model (.getModel log-info)]
    (when (and (integer? index)
               (>= index 0)
               (< index (.getRowCount (.getModel log-table)))
               (not (.getValueIsAdjusting event)))
      (let [log-entry (.getValueAt (.getModel log-table) index 0)]
        (.setRowCount info-model 0)
        (doseq [[key value] log-entry :when value]
          (.addRow info-model (to-array [(name key) value])))))))

(defn- set-cell-color!
  "Sets the background color of a cell based on the value it is displaying."
  [cell {:keys [log-type error? log-style]}]
  (if (= :client log-type)
    (if error?
      (.setBackground cell (Color. 0xFF 0xCC 0xBC))
      (.setBackground cell (Color. 0xE0 0xF7 0xFA)))
    (case log-style
      :error (.setBackground cell (Color. 0xFF 0xCD 0xD2))
      :warning (.setBackground cell (Color. 0xFF 0xEB 0x3B))
      :gui (.setBackground cell (Color. 0x8B 0xC3 0x4A))
      :log (.setBackground cell (Color. 0xF1 0xF8 0xE9)))))

(defn- log-table
  "Returns the JTable instance for showing log entries."
  [log-info]
  (let [table (seesaw/table :id :logs
                            :model [:columns [:message :source]])
        selection-model (.getSelectionModel table)
        table-model (.getModel table)
        row-sorter (TableRowSorter. table-model)
        column-model (.getColumnModel table)
        column0 (.getColumn column-model 0)
        column1 (.getColumn column-model 1)]
    (.setComparator row-sorter 0
                    (reify Comparator
                      (compare [this left right]
                        (compare (:timestamp left) (:timestamp right)))))
    (.toggleSortOrder row-sorter 0)
    (.setRowSorter table row-sorter)
    (.setCellRenderer
     column0
     (proxy [DefaultTableCellRenderer] []
       (getTableCellRendererComponent [table value s f r c]
         (when-not (nil? value)
           ;; TODO: why is the value sometimes nil?
           (set-cell-color! this value)
           (proxy-super getTableCellRendererComponent
                        table (:message value) s f r c)))))
    (.setSelectionMode table ListSelectionModel/MULTIPLE_INTERVAL_SELECTION)
    (.setMinWidth column1 200)
    (.setMaxWidth column1 200)
    (.addListSelectionListener selection-model
                               (reify ListSelectionListener
                                 (valueChanged [this event]
                                   (on-log-selected table
                                                    log-info
                                                    event))))
    table))

(defn- on-log-info-selected
  "Callback for when a log entry info table entry is selected."
  [table event]
  (when-not (or (.getValueIsAdjusting event) (== -1 (.getSelectedRow table)))
    (let [row (.getSelectedRow table)
          value (str (.getValueAt (.getModel table) row 1))]
      (when (> (count value) 100)
        (let [text-area (seesaw/text :text value
                                     :multi-line? true
                                     :editable? true
                                     :font (font/font :name :monospaced))]
          (JOptionPane/showMessageDialog nil (seesaw/scrollable text-area)))))))

(defn- log-info-table
  "Returns a JTable instance for showing log entry metadata"
  []
  (let [log-info (seesaw/table :id :log-info :model [:columns [:key :value]])
        selection-model (.getSelectionModel log-info)
        column0 (.getColumn (.getColumnModel log-info) 0)]
    (.setSelectionMode log-info ListSelectionModel/SINGLE_SELECTION)
    (.setMinWidth column0 150)
    (.setMaxWidth column0 150)
    (.addListSelectionListener selection-model
                               (reify ListSelectionListener
                                 (valueChanged [this event]
                                   (on-log-info-selected log-info event))))
    log-info))

(defn- log-split
  "A split view containing the log entry table and the log info table."
  []
  (let [log-info (log-info-table)
        log-table (seesaw/scrollable (log-table log-info))]
    (seesaw/top-bottom-split log-table
                             (seesaw/scrollable log-info)
                             :divider-location 3/4)))

(defn- right-content
  "Log viewer window"
  []
  (mig/mig-panel
   :items [[(seesaw/label :text "Logs" :font (title-font))
            "alignx center, span, wrap 20px"]
           [(seesaw/button :text "Start Recording" :id :recording-button)]
           [(seesaw/button :text "Clear" :id :clear-button) "wrap"]
           [(log-split)
            "aligny top, span, grow, push"]]))

(mount/defstate ^:private frame-right-content :start (right-content))

(defn- frame-content
  "Constructs all UI for the debug window"
  []
  (seesaw/left-right-split (left-content)
                           frame-right-content
                           :divider-location 1/3))

(mount/defstate ^:private frame
  :start (seesaw/frame :title "The DungeonStrike Driver"
                       :minimum-size [1440 :by 878]
                       :content (frame-content))
  :stop (when frame
          (seesaw/config! frame :visible? false)
          (seesaw/dispose! frame)))

(defn- update-frame!
  "Recreates all frame content."
  []
  (seesaw/config! frame :content
                  (seesaw/left-right-split (left-content)
                                           frame-right-content
                                           :divider-location 1/3)))

(defn- start-logs
  "Starts a go loop to pull logs out of the debug log channel and render them
   in the log entry table."
  []
  (let [logs-view (seesaw/select frame [:#logs])]
    (async/go-loop []
      (when-some [{:keys [source] :as entry} (<! log-channel)]
        (when (:recording? @recording-state)
          (swap! recording-state update :entries conj
                 (select-keys entry [:message :source :log-type :error?])))
        (SwingUtilities/invokeLater
         (fn []
           (.addRow (.getModel logs-view) (to-array [entry source]))
           (SwingUtilities/invokeLater
            (fn []
              (seesaw/scroll! logs-view :to :bottom)))))
        (recur)))))

(defn- start-recording
  []
  (let [recording-button (seesaw/select frame [:#recording-button])]
    (seesaw/config! recording-button :text "Stop Recording")
    (reset! recording-state {:recording? true
                             :entries []})))

(defn- pretty-string
  "Returns a pretty-printed string representation of 'form'"
  [form]
  (with-out-str (pprint/pprint form)))

(defn- save-recording-frame
  [initial-message entries on-close]
  (let [save-button (seesaw/button :text "Save")
        cancel-button (seesaw/button :text "Cancel")
        prerequisite (seesaw/combobox
                      :model (into [nil] (recording-file-names)))
        recording-name (seesaw/text)
        recording-timeout (seesaw/text :text "10")
        text-area (seesaw/text :text (pretty-string entries)
                               :multi-line? true
                               :font (font/font :name :monospaced))
        panel (mig/mig-panel
               :items [[(seesaw/label :text "Save Recording" :font (title-font))
                        "alignx center, span, wrap"]
                       [(seesaw/label :text "Prerequisite Recording:")]
                       [prerequisite "wrap"]
                       [(seesaw/label :text "Name (no .edn):")]
                       [recording-name "width 250px, wrap"]
                       [(seesaw/label :text "Timeout (seconds):")]
                       [recording-timeout "width 100px, wrap"]
                       [(seesaw/label :text "Message:")]
                       [(seesaw/scrollable
                         (seesaw/text :text (pretty-string initial-message)
                                      :font (font/font :name :monospaced)))
                        "width 1000px, height 100px, wrap"]
                       [(seesaw/label :text "Recorded Logs:")
                        "alignx center, span, wrap"]
                       [(seesaw/scrollable text-area)
                        "width 1200px, height 400px, span, wrap"]
                       [cancel-button]
                       [save-button "wrap"]])
        new-frame (seesaw/frame :title "The DungeonStrike Driver"
                                :minimum-size [1200 :by 800]
                                :content panel)]
    (.setLocationRelativeTo new-frame nil)
    (seesaw/listen save-button :action
                   (fn [e]
                     (let [result-name (seesaw/value recording-name)
                           result-prerequisite (seesaw/value prerequisite)
                           timeout (Integer/parseInt
                                    (seesaw/value recording-timeout))]
                       (when-not (empty? result-name)
                         (on-close {:recording-name result-name
                                    :prerequisite result-prerequisite
                                    :timeout timeout})

                         (seesaw/dispose! new-frame)))))
    (seesaw/listen cancel-button :action
                   (fn [e]
                     (on-close {})
                     (seesaw/dispose! new-frame)))
    new-frame))

(defn- stop-recording
  []
  (let [recording-button (seesaw/select frame [:#recording-button])
        initial-message (:message->client @recording-state)
        entries (:entries @recording-state)]
    (seesaw/show!
     (save-recording-frame
      initial-message entries
      (fn [{:keys [recording-name prerequisite timeout]}]
        (reset! recording-state {:recording? false
                                 :entries []})
        (seesaw/config! recording-button :text "Start Recording")
        (when recording-name
          (let [output-file (io/file paths/test-recordings-path
                                     (str recording-name ".edn"))
                output (pretty-string {:name recording-name
                                       :prerequisite prerequisite
                                       :timeout timeout
                                       :message->client initial-message
                                       :entries entries})]
            (spit output-file output)
            (logger/log-gui "Wrote new recording" recording-name))))))))

(defn- new-recording-fn
  "Returns a function to act as the listener for the 'Start Recording' button."
  [event]
  (if (:recording? @recording-state)
    (stop-recording)
    (start-recording)))

(defn- register-listeners
  "Registers button listeners for controls."
  []
  (let [clear-button (seesaw/select frame [:#clear-button])
        recording-button (seesaw/select frame [:#recording-button])
        logs-table (seesaw/select frame [:#logs])
        on-clear (fn [e] (.setRowCount (.getModel logs-table) 0))]
    (seesaw/listen recording-button :action new-recording-fn)
    (seesaw/listen clear-button :action on-clear)))

(defn- show-debug-gui
  []
  (seesaw/show! frame)
  (start-logs)
  (register-listeners))

(mount/defstate ^:private debug-gui
  :start (show-debug-gui))

(s/def ::selector keyword?)
(s/def ::key keyword?)
(s/def ::value some?)
(s/def ::persistent? boolean?)

(defmethod effects/effect-spec ::config [_]
  (s/keys :req-un [::selector ::key ::value] :opt-un [::persistent?]))

(defmethod effects/apply! ::config
  [{:keys [:selector :key :value :persistent?]}]
  (if persistent?
    (swap! persistent-state assoc-in [selector key] value)
    (swap! gui-state assoc-in [selector key] value))
  (update-frame!))
