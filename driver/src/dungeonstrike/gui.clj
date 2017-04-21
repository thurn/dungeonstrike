(ns dungeonstrike.gui
  "A Component which renders a Swing UI window showing various debugging
   tools."
  (:require [clojure.core.async :as async :refer [<!]]
            [clojure.spec :as s]
            [clojure.string :as string]
            [com.stuartsierra.component :as component]
            [dungeonstrike.logger :as logger :refer [log error]]
            [dungeonstrike.messages :as messages]
            [seesaw.core :as seesaw]
            [seesaw.font :as font]
            [seesaw.mig :as mig]
            [seesaw.table :as table]
            [dev])
  (:import (java.awt Color)
           (javax.swing ListSelectionModel SwingUtilities JOptionPane)
           (javax.swing.event ListSelectionListener)
           (javax.swing.table DefaultTableCellRenderer)))
(dev/require-dev-helpers)

(defn- title-font
  "Font to use in section headers"
  [] (font/font :style #{:bold} :size 18))

(def ^:private message-types
  "Message types to show in the 'send message' window"
  [:load-scene :create-entity :update-entity :delete-entity])

(def ^:private scene-names
  "Possible scene names"
  [:flat :empty])

(defn- form-for-field [field-name]
  (let [field (field-name messages/message-fields)]
    [[(seesaw/label (str field-name))]
     [(cond
        (set? field)
        (seesaw/combobox :id field-name :model field)
        (#{:m/entity-id} field)
        (seesaw/text :id field-name (str (java.util.UUID/randomUUID)))
        (= messages/position-spec field)
        (seesaw/text :id field-name "")
        :otherwise
        (seesaw/text "Unknown field type")) "wrap"]]))

(defn- message-form-items [message-picker message-type]
  (concat
   [[(seesaw/label :text "Send Message" :font (title-font))
     "alignx center, pushx, span, wrap 20px"]
    [(seesaw/label "Message Type")]
    [message-picker "wrap"]
    [(seesaw/label "Message ID")]
    [(seesaw/text :id :m/message-id
                  :text (str (java.util.UUID/randomUUID))) "wrap"]]
   (mapcat form-for-field (message-type messages/messages))
   [[(seesaw/button :text "Send!" :id :send-button :enabled? false)
     "skip, span, wrap"]]))

(defn- message-selected-fn [panel message-picker]
  (fn [event]
    (seesaw/config! panel :items
                    (message-form-items message-picker
                                        (seesaw/selection message-picker)))))

(defn- left-content
  "UI for 'Send Message' window."
  [message-sender]
  (let [message-types (keys messages/messages)
        message-picker (seesaw/combobox :id :m/message-type
                                        :model message-types)
        form-items (message-form-items message-picker :m/load-scene)
        panel (mig/mig-panel :id :message-form :items form-items)]
    (seesaw/selection! message-picker :m/load-scene)
    (seesaw/listen message-picker :selection
                   (message-selected-fn panel message-picker))
    panel))

(defn- on-log-selected
  "Callback when a log entry is selected"
  [log-table log-info event]
  (let [index (.getSelectedRow log-table)
        log-entry (.getValueAt (.getModel log-table) index 0)
        info-model (.getModel log-info)]
    (when-not (.getValueIsAdjusting event)
      (.setRowCount info-model 0)
      (doseq [[key value] log-entry :when value]
        (.addRow info-model (to-array [(name key) value]))))))

(defn- log-table
  "Returns the JTable instance for showing log entries."
  [log-info]
  (let [table (seesaw/table :id :logs
                            :model [:columns [:message :source]])
        selection-model (.getSelectionModel table)
        table-model (.getModel table)
        column-model (.getColumnModel table)
        column0 (.getColumn column-model 0)
        column1 (.getColumn column-model 1)]
    (.setCellRenderer
     column0
     (proxy [DefaultTableCellRenderer][]
       (getTableCellRendererComponent [table value s f r c]
         (if (= :client (:log-type value))
           (if (:error? value)
             (.setBackground this (Color. 0xFF 0xCC 0xBC))
             (.setBackground this (Color. 0xE0 0xF7 0xFA)))
           (if (:error? value)
             (.setBackground this (Color. 0xFF 0xCD 0xD2))
             (.setBackground this (Color. 0xF1 0xF8 0xE9))))
         (proxy-super getTableCellRendererComponent
                      table (:message value) s f r c))))
    (.setSelectionMode table ListSelectionModel/SINGLE_SELECTION)
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
          value (.getValueAt (.getModel table) row 1)]
      (when (> (count (str value)) 100)
        (JOptionPane/showMessageDialog nil value)))))

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
                             log-info
                             :divider-location 3/4)))

(defn- right-content
  "Log viewer window"
  []
  (mig/mig-panel
   :items [[(seesaw/label :text "Logs" :font (title-font))
            "alignx center, wrap 20px"]
           [(seesaw/button :text "Clear" :id :clear-button) "alignx right, wrap"]
           [(log-split)
            "aligny top, grow, push"]]))

(defn- frame-content
  "Constructs all UI for the debug window"
  [message-sender]
  (seesaw/left-right-split (left-content message-sender)
                           (right-content)
                           :divider-location 1/3))

(defn- start-logs
  "Starts a go loop to pull logs out of the debug log channel and render them
   in the log entry table."
  [logs-view log-channel]
  (async/go-loop []
    (when-some [{:keys [source] :as entry} (<! log-channel)]
      (SwingUtilities/invokeLater
       (fn []
         (.addRow (.getModel logs-view) (to-array [entry source]))
         (seesaw/scroll! logs-view :to :bottom)))
      (recur))))

(defn- on-send-button-clicked
  "Returns a click handler function for the 'send' button."
  [message-sender message-form]
  (fn [event]
    (let [form-value (seesaw/value message-form)
          filter-keys (fn [[key value]] (= "m" (namespace key)))
          message (into {} (filter filter-keys form-value))]
      (messages/send-message! message-sender message))))

(defn- start-send-button
  "Attaches a listener to the send button to send the current message to the
   client on click. Monitors message channel events and enables/disables the
   send button based on connection status."
  [send-button message-form message-sender status-channel]
  (seesaw/listen send-button :action (on-send-button-clicked message-sender
                                                             message-form))
  (async/go-loop []
    (when-some [{:keys [:data]} (<! status-channel)]
      (seesaw/config! send-button :enabled? (= :connection-opened data))
      (recur))))

(defn- register-listeners
  "Registers button listeners for controls."
  [frame]
  (let [clear-button (seesaw/select frame [:#clear-button])
        logs-table (seesaw/select frame [:#logs])
        on-clear (fn [e] (.setRowCount (.getModel logs-table) 0))]
    (seesaw/listen clear-button :action on-clear)))

(defrecord DebugGui []
  component/Lifecycle

  (start [{:keys [::logger ::message-sender ::status-channel] :as component}]
    ; Instruct Seesaw to try to make things look as native as possible
    (seesaw/native!)

    (let [log-context (logger/component-log-context logger "DebugGui")
          frame (seesaw/frame :title "The DungeonStrike Driver"
                              :minimum-size [1440 :by 878]
                              :content (frame-content message-sender))
          logs (seesaw/select frame [:#logs])
          message-form (seesaw/select frame [:#message-form])
          send-button (seesaw/select frame [:#send-button])]
      (start-logs logs (logger/debug-log-channel logger))
      (start-send-button send-button message-form message-sender status-channel)
      (register-listeners frame)
      (seesaw/show! frame)
      (log log-context "Started DebugGui")
      (assoc component ::frame frame ::log-context log-context)))

  (stop [{:keys [::frame ::log-context ::message-pub] :as component}]
    (when message-pub
      (async/unsub-all message-pub :status))
    (when frame
      (seesaw/config! frame :visible? false)
      (seesaw/dispose! frame))
    (log log-context "Stopped DebugGui")
    (dissoc component ::frame ::log-context)))

(s/fdef new-debug-gui :args (s/cat :status-channel some?))
(defn new-debug-gui
  "Creates a new DebugGui component with `status-channel` as a publication
   subscriber for socket status messages."
  [status-channel]
  (map->DebugGui {::status-channel status-channel}))
