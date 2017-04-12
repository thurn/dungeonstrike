(ns dungeonstrike.gui
  (:require [clojure.core.async :as async :refer [<!]]
            [clojure.string :as string]
            [com.stuartsierra.component :as component]
            [seesaw.core :as seesaw]
            [seesaw.table :as table]
            [seesaw.mig :as mig]
            [seesaw.font :as font]
            [dungeonstrike.logger :refer [dbg! log]]
            [dev])
  (:import (javax.swing.event ListSelectionListener)
           (javax.swing.table DefaultTableCellRenderer)
           (javax.swing ListSelectionModel SwingUtilities JOptionPane)
           (java.awt Color)))
(dev/require-dev-helpers!)

(def title-font (font/font :style #{:bold} :size 18))

(def message-types [:load-scene :create-entity :update-entity :delete-entity])

(def scene-names [:flat :empty])

(defn left-content []
  (mig/mig-panel
   :items [[(seesaw/label :text "Send Message" :font title-font)
            "alignx center, pushx, span, wrap 20px"]
           [(seesaw/label "Message Type")]
           [(seesaw/combobox :model message-types) "wrap"]
           [(seesaw/label "Scene Name")]
           [(seesaw/combobox :model scene-names) "wrap"]
           [(seesaw/label "Message ID")]
           [(seesaw/text (str (java.util.UUID/randomUUID))) "wrap"]
           [(seesaw/label "Game Version")]
           [(seesaw/text "0.1.0") "wrap 20px"]
           [(seesaw/button :text "Send!") "skip, span, wrap"]]))

(defn on-log-selected [log-table log-info event]
  (let [index (.getSelectedRow log-table)
        log-entry (.getValueAt (.getModel log-table) index 0)
        info-model (.getModel log-info)]
    (when-not (.getValueIsAdjusting event)
      (.setRowCount info-model 0)
      (doseq [[key value] log-entry :when value]
        (.addRow info-model (to-array [key value]))))))

(defn log-table [log-info]
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

(defn on-log-info-selected [table event]
  (when-not (.getValueIsAdjusting event)
    (let [row (.getSelectedRow table)
          value (.getValueAt (.getModel table) row 1)]
      (when (> (count (str value)) 100)
        (JOptionPane/showMessageDialog nil value)))))

(defn log-info-table []
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

(defn log-split []
  (let [log-info (log-info-table)
        log-table (seesaw/scrollable (log-table log-info))]
    (seesaw/top-bottom-split log-table
                             log-info
                             :divider-location 3/4)))

(defn right-content []
  (mig/mig-panel
   :items [[(seesaw/label :text "Logs" :font title-font)
            "alignx center, wrap 20px"]
           [(log-split)
            "aligny top, grow, push"]]))

(defn frame-content []
  (seesaw/left-right-split (left-content)
                           (right-content)
                           :divider-location 1/3))

(defn start-logs! [logs-view log-channel]
  (async/go-loop []
    (when-some [{:keys [source] :as entry} (<! log-channel)]
      (SwingUtilities/invokeLater
       (fn []
         (.addRow (.getModel logs-view) (to-array [entry source]))
         (seesaw/scroll! logs-view :to :bottom)))
      (recur))))

(defrecord DebugGui [logger frame]
  component/Lifecycle

  (start [{:keys [logger] :as component}]
    ; Instruct Seesaw to try to make things look as native as possible
    (seesaw/native!)

    (let [frame (seesaw/frame :title "The DungeonStrike Driver"
                              :minimum-size [1440 :by 878]
                              :content (frame-content))
          logs (seesaw/select frame [:#logs])]
      (start-logs! logs (:debug-log-channel logger))
      (seesaw/show! frame)
      (log (:system-log-context logger) "Started DebugGui")
      (assoc component :frame frame)))

  (stop [{:keys [frame logger] :as component}]
    (when frame
      (seesaw/config! frame :visible? false)
      (seesaw/dispose! frame))
    (log (:system-log-context logger) "Stopped DebugGui")
    (assoc component :frame nil)))

(defn new-debug-gui []
  (map->DebugGui {}))
