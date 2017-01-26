(ns ^:figwheel-no-load dungeonstrike.ui.dev
  (:require [dungeonstrike.ui.core :as ui-core]
            [figwheel.client :as figwheel :include-macros true]))

(enable-console-print!)

(figwheel/watch-and-reload
  :websocket-url "ws://localhost:3449/figwheel-ws"
  :jsload-callback ui-core/mount-root)

(ui-core/init!)
