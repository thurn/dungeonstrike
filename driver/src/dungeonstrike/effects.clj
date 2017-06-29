(ns dungeonstrike.effects
  "A simple library for transforming incoming requests into effects. See the
  docstring for `execute!` for an overview."
  (:require [clojure.core.async :as async]
            [clojure.spec.alpha :as s]
            [clojure.string :as string]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(defn- namespaced-keyword? [k] (and (keyword? k) (namespace k)))

(def ^:private channel-class (class (async/chan)))

(defn- channel? [c] (instance? channel-class c))

(s/def ::namespaced-map (s/map-of namespaced-keyword? some?))

(defmulti query
  "Multimethod which fetches additional state which is required to process a
  request.

   For each key-value pair in an incoming request map object, the `query`
  multimethod is invoked to obtain additional required state (e.g. via a
  database fetch) needed to process that request.

   The implementation must return a core.async channel object, and should
  asynchronously put onto this channel a map containing the newly fetched state
  as entries with namespaced keyword keys. The resulting map will be merged into
  the request map before evaluation. Consequently, query results should use
  different keys from the request object. Any conflicting query result keys will
  be silently ignored.

   Note that queries are scoped based on keys in the request, *not* the request
  type.  It is assumed that the same key should have the same query behavior
  across all request types.

   Queries may indicate failure by producing a Throwable object on the returned
  channel. This result will be propagated up and returned to callers of
  `execute!`."
  (fn [key value] key)
  :default ::not-found)

(defmethod query ::not-found [_ _] ::not-found)

(s/fdef -query
        :args (s/cat :key namespaced-keyword? :value some?)
        :ret (s/or :channel channel? :not-found #{::not-found}))
(defn- -query
  "Temporary wrapper for `query` until spec supports multimethods."
  [key value]
  (query key value))

(s/def ::effect-type namespaced-keyword?)

(s/def ::optional? boolean?)

(defmulti effect-spec
  "Multimethod for creating specs for `effect` objects returned from `evaluate`.
  Will receive the returned effect, identified by the ::effect-type key.
  Implementations should return a spec appropriate to the given effect type.

  A default spec for all effects can alternatively be provided under the key
  :default."
  ::effect-type)

(s/def ::effect (s/multi-spec effect-spec ::effect-type))

(s/fdef effect
        :args (s/cat :effect-type ::effect-type :args (s/* some?))
        :ret ::effect)
(defn effect
  "Creates a new effect object, indicating some required mutation to the global
  system state. First argument should be a namespaced keyword which identifies
  the effect, subsequent arguments are bundled into a map object representing
  the effect value. Effects must be returned from `evaluate` method
  implementations."
  [effect-type & {:as args}]
  (assoc args ::effect-type effect-type))

(s/fdef optional-effect
        :args (s/cat :effect-type ::effect-type :args (s/* some?))
        :ret ::effect)
(defn optional-effect
  "Creates an optional effect object. Optional effects are the same as regular
  effects, except that they are silently ignored if no `apply!` implementation
  has been provided. Optional effects can be used for e.g. taking special
  actions in development builds."
  [effect-type & {:as args}]
  (assoc args ::effect-type effect-type ::optional? true))

(s/def ::effect-or-effect-sequence
  (s/or :effect ::effect
        :effect-sequence (s/and sequential?
                                not-empty
                                (s/coll-of ::effect))))

(defmulti request-spec
  "Multimethod for creating specs for requests passed to `evaluate`. Will
  receive the request supplied to `execute!` with a ::request-type key added
  identifying the type of request, as well as all query results added to the
  map.  Implementations should return a spec appropriate to the given request
  type.

   A default spec for all requests can alternatively be provided under the
  key :default."
  ::request-type)

(s/def ::request
  (s/cat :request (s/and ::namespaced-map
                         (s/multi-spec request-spec ::request-type))))

(defmulti evaluate
  "Pure function multimethod which transforms a request into effect objects.
   Must not have side effects!

   Implementations will be passed a 'request' map describing the desired system
  change, potentially populated with additional state obtained via `query`. The
  implementation must return either an effect object created via the `effect` or
  `optional-effect` functions or a sequence of such objects. These effects will
  be applied by calling the appropriate `apply!` handlers when `execute!` is
  called.

   No default implementation is provided for `evaluate`, so an exception will be
  thrown if a request is received for an unrecognized type. This behavior can be
  changed by providing an implementation for the ::not-found key."
  ::request-type
  :default ::not-found)

(s/fdef -evaluate
        :args (s/cat :request ::request)
        :ret ::effect-or-effect-sequence)
(defn- -evaluate
  "Temporary wrapper for `evaluate` until spec supports multimethods."
  [request]
  (evaluate request))

(defmulti apply!
  "Multimethod for implementing changes to the external world as a result of
  requests. When an effect object is returned from `evaluate`, its appropriate
  `apply!` handler is invoked based on its `effect-type`. The implementation of
  `apply!` must not invoke `execute!`. The argument will be a map containing the
  key-value pairs passed to the `effect` function.

  If no `apply!` implementation is found and the effect was not created via
  `optional-effect`, an exception is thrown.

  The implementation can optionally return a core.async channel object. In this,
  case a value must be asynchronously put onto the channel to describe the
  result of applying the effect. Implementations may also indicate failure by
  producing a Throwable object on the returned channel. This result will be
  propagated up and returned to callers of `execute!`."
  ::effect-type
  :default ::not-found)

(defmethod apply! ::not-found [_] ::not-found)

(s/fdef -apply!
        :args (s/cat :effect ::effect)
        :ret any?)
(defn- -apply!
  "Temporary wrapper for `apply!` until spec supports multimethods."
  [effect]
  (apply! effect))

(defn- throw-err
  "Throws 'e' if it is a Throwable, otherwise returns 'e'."
  [e]
  (when (instance? Throwable e) (throw e))
  e)

(defmacro ^:private <?
  "Retrieves a value from a channel. If it is a Throwable value, throws it."
  [ch]
  `(throw-err (async/<! ~ch)))

(defn- async-map
  "Returns a channel in the same way as the one produced by `async/map`, which
  combines 'channels' into vectors of results. If any channel produces a
  Throwable object, puts the first returned Throwable object onto the returned
  channel instead. If 'channels' is empty, returns a channel containing
  'default'."
  [channels default]
  (if (empty? channels)
    (async/to-chan [default])
    (let [combine (fn [& args]
                    (if-let [throwable (first (filter #(instance? Throwable %)
                                                      args))]
                      throwable
                      (apply vector args)))]
      (async/map combine channels))))

(defn- run-query
  "Invokes the associated query handler for the provided 'key' and 'value',
  adding it to 'queries' if it is found."
  [queries key value]
  (let [result (-query key value)]
    (if (= result ::not-found)
      queries
      (assoc queries key result))))

(s/fdef validate-query-results
        :args (s/cat :results (s/coll-of ::namespaced-map)))
(defn- validate-query-results
  "Identity function with an attached spec for validating query results if spec
  instrumentation is enabled."
  [results]
  results)

(defn- apply-effect-fn
  "Returns a function which applies an effect via its `apply!` handler."
  [log]
  (fn [{:keys [::effect-type ::optional?] :as effect}]
    (let [result (-apply! effect)
          not-found? (= result ::not-found)]
      (cond
        (and not-found? optional?)
        (log :optional-effect-ignored {:effect-type effect-type})

        not-found?
        (throw (RuntimeException.
                (str "No apply! method implementation found for effect type: '"
                     effect-type "'")))

        :otherwise result))))

(s/def ::log-fn fn?)

(s/def ::exception-handler fn?)

(s/fdef execute!
        :args (s/cat :request-type namespaced-keyword?
                     :request (s/? ::namespaced-map)
                     :options (s/? (s/keys :opt-un
                                           [::log-fn ::exception-handler])))
        :ret channel?)
(defn execute!
  "Transforms a request into a sequence of effects, and then applies those
  effects.

   A 'request' is a map describing some required change to the state of the
  system, usually based on some external stimulus such as a network request or a
  user input. Each request has an associated 'request-type' which identifies
  which piece of code knows how to handle a given request.

    An 'effect' is some change to the state of the system, represented by an
  effect object returned by the `effect` function. When the `execute!` function
  receives a new request, it transforms that request into a sequence of effects
  by invoking the appropriate `evaluate` multimethod based on the request type,
  which returns effect objects. The resulting effects are then sent to the
  `apply!` multimethod for execution based on their 'effect type'.

   In addition, most requests require some additional state for processing. For
  example, we may receive an '::increment-age' request for {::user-id 123}
  indicating that they are one year older. In order to process this request, we
  need to first fetch the user's current age. This is the role of the `query`
  multimethod. For each key-value pair in the input 'request' object, the
  `query` method is invoked to asynchronously look up any required associated
  state and return it via a channel in a map which is merged into the request
  map.

   In the above case, we may define a `query` on ::user-id which looks up user
  123 and returns the user's information in a map under the ::user key. This
  allows all requests which receive a ::user-id to have access to the
  associated ::user as well.

   `execute!` performs queries and applies effects asynchronously via
  nonblocking core.async calls. It returns a channel object when called. This
  channel will receive the keyword ':done' when execution has completed.

   The 'options' map contains optional configuration for the execution. The
  possible options are ':log-fn' and ':exception-handler'.

   ':log-fn' specifies a function which will be invoked with callbacks
  throughout the execution process. The log function receives two arguments, a
  log-type keyword and a map of associated values, as follows:

    - :request-received, when a new request is received. Keys will be
      :request-type, :request, and :queries, the request keys for which queries
      have been started.
    - :queries-completed, when all queries have completed. Keys will be
      :request-type and :results, a vector of the query results.
    - :evaluated, when evaluation completes. Keys will be :request-type and
      :effects, a sequence of the resulting effect type keywords.
    - :optional-effect-ignored, after an `optional-effect` is ignored. Key will
      be :effect-type.
    - :done, when all effects have been applied. Keys will be :request-type
      and :results, a vector of result objects returned by any asynchronous
      effect handlers.

   ':exception-handler' specifies a function which will be invoked if an
  exception occurs during execution. The implementations of the `query` and
  `apply!` methods can return a Throwable object on their underlying channel to
  indicate failure, which will be rethrown by `execute!`. The handler will
  receive the exception as an argument, and any non-nil return value of the
  handler will be placed in the channel which `execute!` returns. Using
  `identity` as the exception handler will cause the exception to be returned
  from the channel normally for processing by the caller. If no exception
  handler is specified, exceptions will be thrown on the running background
  thread and handled by the system uncaught exception handler."
  ([request-type] (execute! request-type {} {}))
  ([request-type request] (execute! request-type request {}))
  ([request-type request {:keys [:log-fn :exception-handler]}]
   (let [log (or log-fn (constantly nil))
         queries (reduce-kv run-query {} request)]
     (log :request-received
          {:request-type request-type :request request
           :queries (into [] (keys queries))})
     (async/go
       (try
         (let [request-with-type (assoc request ::request-type request-type)
               results (<? (async-map (vals queries) [{}]))
               input (apply merge (conj (validate-query-results results)
                                        request-with-type))]
           (log :queries-completed :request-type request-type :results results)
           (let [output (-evaluate input)
                 effects (if (sequential? output) output [output])]
             (log :evaluated {:request-type request-type
                              :effects (mapv ::effect-type effects)})
             (let [apply-effect! (apply-effect-fn log)
                   channels (filter channel? (mapv apply-effect! effects))
                   effect-results (<? (async-map channels :done))]
               (log :done {:request-type request-type :results effect-results})
               :done)))
         (catch Throwable e
           (if exception-handler
             (exception-handler e)
             (throw e))))))))
