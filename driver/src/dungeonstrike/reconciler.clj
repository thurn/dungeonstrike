(ns dungeonstrike.reconciler
  "A library implementing the reconciler design pattern.

  The reconciler is a system for interacting with shared global state in a
  structured way. It maintains state in a single atom which contains key/value
  pairs called the 'universe'. Each entry in the universe map is called a
  'domain', and maps to some specific piece of application state.

  When a mutation is required, a 'request' value should be queued and then
  submitted via the `execute!` function. The resulting state of the system is
  determined by the pure functional `evaluate` multimethod, and then the state
  change is propagated to the rest of the system via the `update!` multimethod."
  (:require [clojure.string :as string]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(defonce ^:private universe (atom {}))

(defmulti evaluate
  "Multimethod for pure business logic functions. Every transformation in
   application state should be represented by an implementation of `evaluate`.

  Implementations of this multimethod must not have side effects!

   Will be passed three arguments:

     `request-type`, a keyword which identifies which request is being
         processed, used for dispatch.
     `request`, a value representing some user-defined external stimulus which
         has triggered the need for a change to application state, as described
         in the docstring for `execute!`.
     `universe`, a map from keywords to values. Each entry in this map is called
         a 'domain' and conceptually represents an independent aspect of the
         current global system state.

   Implementations must return a new universe value based on the request, which
  will be compared against the previous universe value and will cause the
  appropriate `update!` handlers to run, as described in that function's
  docstring."
  (fn [request-type request universe] request-type))

(defmulti update!
  "Multimethod for responding to state changes. The global system state (or
   'universe') is modified when `execute!` is invoked. Each modification causes
   one or more entries in the universe map (called 'domains') to be updated.
   When such an update occurs, the `update!` method is invoked for each
   modified domain in an arbitrary order in order to drive changes to the
   external world.

   The `update!` implementation can have arbitrary side effects including,
  potentially, submitting additional requests for execution, and should return
  nil. Note that `update!` is invoked synchronously *before* the global universe
  state is updated, and thus any new requests should be submitted via a channel
  queue as discussed below."
  (fn [domain-key new-domain] domain-key))

(defmethod update! :default [_ _] nil)

(defmulti valid-request?
  "Multimethod for validating request values before executing them. Will be
  passed the request type and request value. An exception will be thrown from
  `execute!` if a request value is provided which does not satisfy this
  predicate. To opt out of request validation, simply define a default validator
  as follows:

      (defmethod reconciler/valid-request? :default [_ _] true)"

  (fn [request-type request] request-type))

(defmulti valid-domain?
  "Multimethod for validating new domain values before storing them. Will be
   passed the domain key and new domain value. An exception will be thrown from
   `execute!` if a domain value is returned from an `evaluate` implementation
   which does not satisfy this predicate. To opt out of domain validation,
   simply define a default validator as follows:

       (defmethod reconciler/valid-domain? :default [_ _] true)"
  (fn [domain-type domain] domain-type))

(defmulti log
  "Multimethod for logging the behavior of the reconciler. Can be implemented by
  clients to get log output during reconciler execution. Will be invoked with
  one of the `log-type` keywords and additional arguments as follows:

    `:new-request`, indicating a new request has been received. Arguments will
        be the request type keyword and requst value.
    `:new-value`, indicating a new value has been returned for a given
        domain. Arguments will be the domain keyword and the new domain value.
    `:domain-updated`, indicating an `update!` method invocation has completed.
        Argument will be the domain keyword.
    `:universe-updated`, indicating the universe atom has been set to the
        newest state. No arguments.
    `:validation-error`, indicating an invalid value has been received during
        execution. Argument will be the error message. In addition, this method
        can return `:no-exception` to suppress validation exceptions
        (otherwise, a `RuntimeException` is thrown on validation failure).

  The default implementation of this multimethod simply returns nil."
  (fn [log-type & args] log-type))

(defmethod log :default [log-type & args] nil)

(defn- namespaced-keyword? [k] (and (keyword? k) (namespace k)))

(defn- updated-domains
  "Returns the set of domain keys which have changed between `old-universe` and
  `new-universe` as defined by `identical?` in an arbitrary order."
  [old-universe new-universe]
  (let [updated? (fn [domain-key]
                   (not (identical? (old-universe domain-key)
                                    (new-universe domain-key))))]
    (filter updated? (keys new-universe))))

(defn- error!
  "Logs an error with `args` as a description and throws an exception unless
  suppressed by the `log` return value."
  [& args]
  (let [message (string/join " " args)
        output (log :validation-error message)]
    (when-not (= :no-exception output)
      (throw (RuntimeException. message)))))

(defn execute!
  "Function which applies global state mutations in a structured way.

   The reconciler stores the current global state of the system in an atom. This
  state is called the 'universe' map because it should contain all system
  state. Each entry in the universe map is called a 'domain', and should contain
  the state for some independent part of the system.

   The `execute!` function processes requests, which are simply values
  representing some external stimulus that can change the state of the system.
  Each request has a `request-type` keyword to identify it.

   For each request type, there must be a corresponding implementation of the
  `evaluate` multimethod: a pure function which accepts the request value and
  the current universe state and produces a new universe state. When a request
  is executed, this method is invoked to obtain the new universe state. Then,
  for each domain which has changed (as defined by `identical?`), the
  corresponding `update!` method for that domain is invoked with the new
  state. The `update!` method is expected to apply any real-world side effects
  of the state transition, and could potentially even submit new requests to the
  system to react to the new state value. Finally, the universe atom is updated
  to contain the new state value.

   State updates are not applied atomically, which could lead to race conditions
  if requests are submitted from multiple threads. The suggested pattern is to
  submit requests to a shared core.async channel and then have a single go loop
  invoke the `execute!` function.

   The request and domain objects are validated by the `valid-request?` and
  `valid-domain?` multimethods in this namespace before being accepted.

   Returns nil."
  [request-type request]
  (when-not (namespaced-keyword? request-type)
    (error! "'request-type' must be a namespaced keyword, not" request-type))
  (when-not (valid-request? request-type request)
    (error! "Invalid request value!" request-type request))
  (log :new-request request-type request)
  (let [old-universe @universe
        new-universe (evaluate request-type request old-universe)]
    (when-not (map? new-universe)
      (error! "The return value from 'evaluate' must be a map, not"
              new-universe))
    (doseq [updated-key (updated-domains old-universe new-universe)]
      (let [domain (new-universe updated-key)]
        (when-not (namespaced-keyword? updated-key)
          (error! "Domain keys must be namespaced keywords, not" updated-key))
        (when-not (valid-domain? updated-key domain)
          (error! "Invalid domain returned!" updated-key domain))
        (log :new-value updated-key domain)
        (update! updated-key domain)
        (log :domain-updated updated-key)))
    (reset! universe new-universe)
    (log :universe-updated)
    nil))
