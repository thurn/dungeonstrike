(ns dungeonstrike.gui
  "A Component which renders a Swing UI window showing various debugging
   tools."
  (:require [clojure.core.async :as async :refer [<!]]
            [clojure.java.io :as io]
            [clojure.pprint :as pprint]
            [clojure.spec :as s]
            [clojure.string :as string]
            [clojure-watch.core :as watch]
            [dungeonstrike.channels :as channels]
            [dungeonstrike.logger :as logger :refer [log error log-important!]]
            [dungeonstrike.messages :as messages]
            [dungeonstrike.test-runner :as test-runner]
            [dungeonstrike.uuid :as uuid]
            [com.stuartsierra.component :as component]
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

(defn- title-font
  "Font to use in section headers"
  []
  (font/font :style #{:bold} :size 18))

(defn- form-for-field [field-name]
  (let [field (field-name messages/message-fields)]
    [[(seesaw/label (str field-name))]
     [(cond
        (set? field)
        (seesaw/combobox :id field-name :model field)

        (= messages/position-spec field)
        (seesaw/text :id field-name :text "")

        (= string? field)
        (seesaw/text :id field-name :text "")

        :otherwise
        (seesaw/text "Unknown field type")) "width 200px, wrap"]]))

(defn- process-position
  [value]
  (if (= value "")
    {:m/x 0, :m/y 0}
    (let [[x y] (string/split value #",")]
      {:m/x (Integer/parseInt x), :m/y (Integer/parseInt y)})))

(defn- process-form-values
  [message key value]
  (cond
    (= key :m/position)
    (assoc message key (process-position value))
    (= key :m/new-entity-id)
    (if (= "" value)
      (assoc message key (uuid/new-entity-id))
      (assoc message key value))
    (= "m" (namespace key))
    (assoc message key value)
    :otherwise
    message))

(defn- on-send-button-clicked
  "Returns a click handler function for the 'send' button."
  [{:keys [::message-sender ::recording-state]} message-form]
  (fn [event]
    (let [form-value (seesaw/value message-form)
          message (reduce-kv process-form-values {} form-value)
          message-id (uuid/new-message-id)
          message-id-label (seesaw/select message-form [:#message-id-label])
          message-with-id (assoc message :m/message-id message-id)]
      (when (:recording? @recording-state)
        (swap! recording-state assoc :message->client message-with-id))
      (seesaw/config! message-id-label :text message-id)
      (messages/send-message! message-sender message-with-id))))

(defn- message-form-items [send-button message-picker message-type]
  (concat
   [[(seesaw/label :text "Send Meossage" :font (title-font))
     "alignx center, pushx, span, wrap 20px"]
    [(seesaw/label "Message Type")]
    [message-picker "wrap"]
    [(seesaw/label "Message ID")]
    [(seesaw/label :id :message-id-label
                   :text "M:0000000000000000000000") "wrap"]]
   (mapcat form-for-field (message-type messages/messages))
   [[send-button "skip, span, wrap"]]))

(defn- message-selected-fn [send-button panel message-picker]
  (fn [event]
    (seesaw/config! panel :items
                    (message-form-items send-button
                                        message-picker
                                        (seesaw/selection message-picker)))))
(defn- send-message-panel
  "UI for 'Send Message' panel."
  [{:keys [::send-button-enabled?] :as component}]
  (let [message-types (keys messages/messages)
        message-picker (seesaw/combobox :id :m/message-type
                                        :model message-types)
        send-button (seesaw/button :text "Send!"
                                   :id :send-button
                                   :enabled? @send-button-enabled?)
        form-items (message-form-items send-button message-picker :m/load-scene)
        panel (mig/mig-panel :id :message-form :items form-items)]
    (seesaw/selection! message-picker :m/load-scene)
    (seesaw/listen message-picker :selection
                   (message-selected-fn send-button panel message-picker))
    (seesaw/listen send-button :action (on-send-button-clicked component panel))
    panel))

(defn- test-runner-panel
  [component]
  (seesaw/label "test runner UI"))

(defn- recording-file-names
  "Returns all available file names of test recordings in the recordings
   directory."
  [test-recordings-path]
  (let [files (file-seq (io/file test-recordings-path))]
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
  [{:keys [::test-runner ::log-context]} test-name]
  (log-important! log-context "Running test" test-name)
  (async/go
    (let [{:keys [:status :message] :as result}
          (<! (test-runner/run-integration-test test-runner test-name))]
      (if (= status :success)
        (log-important! log-context "Test passed!" test-name)
        (error log-context message result)))))

(defn- tests-panel
  "UI for the 'Tests' panel"
  [{:keys [::test-recordings-path] :as component}]
  (let [file-names (recording-file-names test-recordings-path)
        run-selected-button (seesaw/button :text "Run Selected" :enabled? false)
        run-all-button (seesaw/button :text "Run All")
        test-list (test-list-table file-names)]
    (seesaw/listen run-selected-button :action
                   (fn [e]
                     (when-let [test (seesaw/value test-list)]
                       (run-test component (file-names
                                            (seesaw/selection test-list))))))
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
  [component]
  (seesaw/tabbed-panel :placement :top
                       :tabs [{:title "Send Message"
                               :content (send-message-panel component)}
                              {:title "Tests"
                               :content (tests-panel component)}]))

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
  [cell {:keys [log-type error? important?]}]
  (if (= :client log-type)
    (if error?
      (.setBackground cell (Color. 0xFF 0xCC 0xBC))
      (if important?
        (.setBackground cell (Color. 0x21 0x96 0xF3))
        (.setBackground cell (Color. 0xE0 0xF7 0xFA))))
    (if error?
      (.setBackground cell (Color. 0xFF 0xCD 0xD2))
      (if important?
        (.setBackground cell (Color. 0x8B 0xC3 0x4A))
        (.setBackground cell (Color. 0xF1 0xF8 0xE9))))))

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
         (set-cell-color! this value)
         (proxy-super getTableCellRendererComponent
                      table (:message value) s f r c))))
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

(defn- frame-content
  "Constructs all UI for the debug window"
  [component]
  (seesaw/left-right-split (left-content component)
                           (right-content)
                           :divider-location 1/3))

(defn- start-logs
  "Starts a go loop to pull logs out of the debug log channel and render them
   in the log entry table."
  [{:keys [::log-channel ::frame ::recording-state]}]
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

(defn- start-send-button
  "Attaches a listener to the send button to send the current message to the
   client on click. Monitors message channel events and enables/disables the
   send button based on connection status."
  [{:keys [::frame ::message-sender ::send-button-enabled? ::status-channel]
    :as component}]
  (let [message-form (seesaw/select frame [:#message-form])
        send-button (seesaw/select frame [:#send-button])]
    (async/go-loop []
      (when-some [status (<! status-channel)]
        (reset! send-button-enabled? (= :connection-opened status))
        (seesaw/config! send-button :enabled? @send-button-enabled?)
        (recur)))))

(defn- start-recording
  [{:keys [::frame ::recording-state] :as component}]
  (let [recording-button (seesaw/select frame [:#recording-button])]
    (seesaw/config! recording-button :text "Stop Recording")
    (reset! recording-state {:recording? true :entries []})))

(defn- pretty-string
  "Returns a pretty-printed string representation of 'form'"
  [form]
  (with-out-str (pprint/pprint form)))

(defn- save-recording-frame
  [initial-message entries test-recordings-path on-close]
  (let [save-button (seesaw/button :text "Save")
        cancel-button (seesaw/button :text "Cancel")
        prerequisite (seesaw/combobox
                      :model (into [nil] (recording-file-names
                                          test-recordings-path)))
        recording-name (seesaw/text)
        text-area (seesaw/text :text (pretty-string entries)
                               :multi-line? true
                               :font (font/font :name :monospaced))
        panel (mig/mig-panel
               :items [[(seesaw/label :text "Save Recording" :font (title-font))
                        "alignx center, span, wrap"]
                       [(seesaw/label :text "Prerequisite Recording:")]
                       [prerequisite "wrap"]
                       [(seesaw/label :text "Name:")]
                       [recording-name "width 250px, wrap"]
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
        frame (seesaw/frame :title "The DungeonStrike Driver"
                            :minimum-size [1200 :by 800]
                            :content panel)]
    (.setLocationRelativeTo frame nil)
    (seesaw/listen save-button :action
                   (fn [e]
                     (let [result-name (seesaw/value recording-name)
                           result-prerequisite (seesaw/value prerequisite)]
                       (when-not (empty? result-name)
                         (on-close result-name result-prerequisite)
                         (seesaw/dispose! frame)))))
    (seesaw/listen cancel-button :action
                   (fn [e]
                     (on-close nil nil)
                     (seesaw/dispose! frame)))
    frame))

(defn- stop-recording
  [{:keys [::frame ::recording-state ::test-recordings-path ::log-context]
    :as component}]
  (let [recording-button (seesaw/select frame [:#recording-button])
        initial-message (:message->client @recording-state)
        entries (:entries @recording-state)]
    (seesaw/show!
     (save-recording-frame
      initial-message entries test-recordings-path
      (fn [recording-name prerequisite]
        (when recording-name
          (reset! recording-state {:recording? false :entries []})
          (seesaw/config! recording-button :text "Start Recording")
          (let [output-file (io/file test-recordings-path
                                     (str recording-name ".edn"))
                output (pretty-string {:name recording-name
                                       :prerequisite prerequisite
                                       :message->client initial-message
                                       :entries entries})]
            (spit output-file output)
            (log log-context "Wrote new recording" recording-name))))))))

(defn- new-recording-fn
  "Returns a function to act as the listener for the 'Start Recording' button."
  [{:keys [::recording-state] :as component}]
  (fn [e]
    (if (:recording? @recording-state)
      (stop-recording component)
      (start-recording component))))

(defn- register-listeners
  "Registers button listeners for controls."
  [{:keys [::frame] :as component}]
  (let [clear-button (seesaw/select frame [:#clear-button])
        recording-button (seesaw/select frame [:#recording-button])
        logs-table (seesaw/select frame [:#logs])
        on-clear (fn [e] (.setRowCount (.getModel logs-table) 0))]
    (seesaw/listen recording-button :action
                   (new-recording-fn component))
    (seesaw/listen clear-button :action on-clear)))

(defrecord DebugGui []
  component/Lifecycle

  (start [{:keys [::logger ::message-sender ::connection-status-channel
                  ::debug-log-channel]
           :as component}]
    ; Instruct Seesaw to try to make things look as native as possible
    (seesaw/native!)

    (let [log-context (logger/component-log-context logger "DebugGui")
          updated (assoc component
                         ::log-context log-context
                         ::log-channel (channels/get-tap debug-log-channel
                                                         ::debug-log-channel)
                         ::status-channel (channels/get-tap
                                           connection-status-channel
                                           ::connection-status-channel)
                         ::send-button-enabled? (atom false)
                         ::recording-state (atom {:recording? false}))
          frame (seesaw/frame :title "The DungeonStrike Driver"
                              :minimum-size [1440 :by 878]
                              :content (frame-content updated))
          result (assoc updated ::frame frame)]
      (start-logs result)
      (register-listeners result)
      (start-send-button result)
      (seesaw/show! frame)
      (log log-context "Started DebugGui")
      result))

  (stop [{:keys [::frame ::log-context ::message-pub] :as component}]
    (when frame
      (seesaw/config! frame :visible? false)
      (seesaw/dispose! frame))
    (log log-context "Stopped DebugGui")
    (dissoc component ::frame ::log-context ::recording-state)))
