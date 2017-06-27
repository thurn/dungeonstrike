(ns dungeonstrike.effects
  "A simple library for transforming incoming requests into effects."
  (:require [clojure.spec.alpha :as s]
            [clojure.string :as string]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(defn- namespaced-keyword? [k] (and (keyword? k) (namespace k)))

(s/def ::namespaced-map (s/map-of namespaced-keyword? some?))

(s/fdef query
        :args (s/cat :key namespaced-keyword? :value some?)
        :ret ::namespaced-map)
(defmulti query
  "Multimethod which fetches additional state which is required to process a
   request.

   For each key-value pair in an incoming request map object, the `query`
   multimethod is invoked to obtain additional required state (e.g. via a
   database fetch) needed to process that request. The implementation must
   return a map containing the newly fetched state as entries with namespaced
   keyword keys. This map will be merged into the request map before
   evaluation. Consequently, query results should use different keys from the
   request object. Any conflicting query result keys will be silently ignored.

   For example, a `query` implementation could be provided which fetches
   relevant user information from the database whenever a request is received
   containing a `::user-id` key.

   Note that queries are based on keys in the request, *not* the request type.
   It is assumed that the same key should have the same query behavior across
   all request types."
  (fn [key value] key)
  :default ::not-found)

(defmethod query ::not-found [_ _] ::not-found)

(s/def ::effect (s/keys :req [::effect-type ::args] ::opt [::optional?]))

(s/fdef effect
        :args (s/cat :effect-type namespaced-keyword? :args (s/* some?))
        :ret ::effect)
(defn effect
  "Creates a new effect object, indicating some required mutation to the global
   system state. Effects are returned from `evaluate` method implemenetations."
  [effect-type & args]
  {::effect-type effect-type ::args args})

(s/fdef optional-effect
        :args (s/cat :effect-type namespaced-keyword? :args (s/* some?))
        :ret ::effect)
(defn optional-effect
  "Creates an optional effect object. Optional effects are the same as regular
  effects, except that they are silently ignored if no `apply!` implementation
  has been provided. Optional effects can be used for e.g. taking special
  actions in development builds."
  [effect-type & args]
  {::effect-type effect-type ::optional? true ::args args})

(s/fdef evaluate
        :args (s/cat :request-type namespaced-keyword?
                     :request ::namespaced-map)
        :ret (s/coll-of ::effect))
(defmulti evaluate
  "Pure function multimethod which transforms a request into effect objects.
   Must not have side effects!

   Implementations will be passed a `request-type` keyword and a `request` map
  describing the desired system change, potentially populated with additional
  state obtained via `query`. The implementation must return a collection of
  effect objects created via the `effect` or `optional-effect` functions. These
  effects are applied by calling the appropriate `apply!` handlers when
  `execute!` is called.

   No default implementation is provided for `evaluate`, so an exception will be
  thrown if a request is received for an unrecognized type. This behavior can be
  changed by providing an implementation for the ::not-found key."
  (fn [request-type request] request-type)
  :default ::not-found)

(s/fdef apply!
        :args (s/cat :effect-type namespaced-keyword? :arguments (s/* some?))
        :ret any?)
(defmulti apply!
  "Multimethod for implementing changes to the external world as a result of
  requests. When an effect object is returned from `evaluate`, its appropriate
  `apply!` handler is invoked based on its `effect-type`. The implementation of
  `apply!` must not invoke `execute!`.

  If no `apply!` implementation is found and the effect was not created via
  `optional-effect`, an exception is thrown. Return value is ignored."
  (fn [effect-type & arguments] effect-type)
  :default ::not-found)

(defmethod apply! ::not-found [_] ::not-found)

(defn- run-query-fn
  "Returns a function which runs a query for a given key-value pair in a request
   map and logs the result using 'log' if a value is returned."
  [log]
  (fn [[key value]]
    (let [result (query key value)]
      (if (= result ::not-found)
        {}
        (do
          (log :query-completed {:request-key key
                                 :request-value value
                                 :query-result result})
          result)))))

(defn- apply-effect!
  "Applies an effect via its `apply!` handler and logs the result via 'log'."
  [{:keys [::effect-type ::optional? ::args]} log]
  (let [result (apply apply! effect-type args)
        not-found? (= result ::not-found)]
    (cond
      (and not-found? optional?)
      (log :optional-effect-ignored {:effect-type effect-type})

      not-found?
      (throw (RuntimeException.
              (str "No apply! method implementation found for effect type: "
                   effect-type)))

      :otherwise
      (log :effect-applied {:effect-type effect-type}))))

(s/def ::logger fn?)

(s/fdef execute!
        :args (s/cat :request-type namespaced-keyword?
                     :request ::namespaced-map
                     :options (s/keys :opt-un [::logger]))
        :ret nil?)
(defn execute!
  "Transforms a request into a collection of effects, and then applies those
  effects.

   A 'request' is a map describing some required change to the state of the
  system, usually based on some external stimulus such as a network request or a
  user input. Each request has an associated 'request-type' which identifies
  which piece of code knows how to handle a given request.

    An 'effect' is some change to the state of the system, represented by an
  effect object returned by the `effect` function. When the `execute!` function
  receives a new request, it transforms that request into a collection of
  effects by invoking the appropriate `evaluate` multimethod based on the
  request type. The resulting effects are then sent to the `apply!` multimethod
  for execution based on their 'effect type'.

   In addition, most requests require some additional state for processing. For
  example, we may receive a '::increment-age' request for {::user-id 123}
  indicating that they are one year older. In order to process this request, we
  need to first fetch the user's current age. This is the role of the `query`
  multimethod. For each key-value pair in the input 'request' object, the
  `query` method is invoked to look up any required associated state and return
  it in a map which is merged into the request map. In this case, we may define
  a `query` on ::user-id which looks up user 123 and returns the user's
  information in a map under the ::user key. This allows all requests which
  receive a ::user-id to have access to the associated ::user as well.

  The 'options' map contains optional configuration for the execution.
  Currently, the only available option is `:logger`, which is a function which
  will be invoked throughout the execution process. The logger function receives
  two arguments, a log-type keyword and a map of associated values, as follows:

    - :request-received, when a new request is received. Keys will be
      :request-type and :request.
    - :query-completed, when a query completes. Keys will be :request-key
      :request-value, and :query-result.
    - :evaluated, when evaluation completes. Keys will be :request-type and
      :effects, a collection of the resulting effect objects.
    - :effect-applied, after an effect has been applied. Key will be
      :effect-type.
    - :optional-effect-ignored, after an optional effect is ignored. Key will
      be :effect-type."
  [request-type request {:keys [:logger]}]
  (let [log (or logger (constantly nil))
        run-query (run-query-fn log)]
    (log :request-received {:request-type request-type :request request})
    (let [query-results (mapv run-query request)
          input (apply merge (conj query-results request))
          effects (evaluate request-type input)]
      (log :evaluated {:request-type request-type :effects effects})
      (doseq [effect effects]
        (apply-effect! effect log)))))
