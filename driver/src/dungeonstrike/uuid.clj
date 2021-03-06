(ns dungeonstrike.uuid
  "Functions for generating ids."
  (:import (java.util UUID Base64)
           (java.nio ByteBuffer)))

(defn- create-id
  "Helper function to create a new Base64 ID"
  []
  (let [encoder (Base64/getEncoder)
        byte-buffer (ByteBuffer/wrap (make-array Byte/TYPE 16))
        uuid (UUID/randomUUID)]
    (.putLong byte-buffer (.getMostSignificantBits uuid))
    (.putLong byte-buffer (.getLeastSignificantBits uuid))
    (let [encoded (.encodeToString encoder (.array byte-buffer))]
      (subs encoded 0 (- (count encoded) 2)))))

(defn new-driver-id
  "Creates a new random driver ID"
  []
  (str "D:" (create-id)))

(defn new-entity-id
  "Creates a new random entity ID"
  []
  (str "E:" (create-id)))

(defn new-message-id
  "Creates a new random message ID."
  []
  (str "M:" (create-id)))
