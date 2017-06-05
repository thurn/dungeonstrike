(ns dungeonstrike.nodes
  "Defines an abstraction for describing a computation with known side effects
   as a graph."
  (:require [clojure.spec :as s]
            [dungeonstrike.uuid :as uuid]
            [dungeonstrike.dev :as dev]))
(dev/require-dev-helpers)

(def ^:private node-registry (atom {}))

(defn register-node-impl!
  "Adds a new node to the node regstiry. Use the `defnode` macro instead of
   calling this function directly."
  [node-name function inputs file line]
  (swap! node-registry assoc node-name
         {:function function
          :inputs inputs
          :file file
          :line line}))

(defn- namespaced-keyword?
  [k]
  (and (keyword? k) (namespace k)))

(s/fdef defnode :args
        (s/cat :function-keyword namespaced-keyword?
               :docstring (s/? string?)
               :attr-map (s/? map?)
               :params (s/spec (s/* namespaced-keyword?))
               :rest (s/* some?)))
(defmacro defnode
  "Registers a new node in the global node registry under the name `node-name`
   and creates a private function in the current namespace with the same name
   for testing.

   Follows normal function creation syntax, except that the normal function
   parameters vector is replaced with a vector of namespaced keywords. Each
   keyword represents a required input to this function, which must be obtained
   from another node, either directly as a returned value or by executing a
   Query (see below). All node bodies must be pure functions, and may be
   memoized by the system to improve performance.

   A node body can return one of three things:
     1) A Query object, constructed by calling the `new-query` function. This
        indicates that the node's ultimate value must be obtained by querying
        some external system to obtain the needed state.
     2) A vector of Mutation objects, constructed by calling the `new-mutation`
        function. Each mutation describes some change to the state of
        external system as a result of executing the node. Nodes which return
        mutations can still be used as inputs to other nodes, allowing
        composition of mutations.
     3) Any other non-nil value. The returned value, called an intermediate
        value, will be made available as an input to other nodes."
  [node-name & defnode-args]
  (let [take-if (fn [p s d] (if (p (first s)) [(first s) (next s)] [d s]))
        [docstring args] (take-if string? defnode-args "")
        [attr-map args] (take-if map? args {})
        [params & body] args
        function-name (symbol (name node-name))
        symbol-params (into [] (map #(symbol (name %)) params))]
    `(do
       (defn- ~function-name ~docstring ~attr-map ~symbol-params ~@body)
       (register-node-impl! ~node-name
                            ~function-name
                            ~params
                            ~(:file (meta &form))
                            ~(:line (meta &form))))))

(defrecord QueryImpl [query-id type arguments])

(s/fdef new-query :args (s/cat :type keyword? :arguments some?))
(defn new-query
  "Creates a new query object with an associated `type` and `arguments`. When
   the result is required, this object will be passed to an appropriate
   `QueryHandler` for execution."
  [type arguments]
  (QueryImpl. (uuid/new-query-id) type arguments))

(defprotocol QueryHandler
  "Protocol for components which perform queries."
  (query [this arguments]
    "Performs a query with provided arguments, returning the appropriate result.
     May throw an exception on failure."))

(defrecord MutationImpl [mutation-id type arguments])

(s/fdef new-mutation :args (s/cat :type keyword? :arguments some?))
(defn new-mutation
  "Creates a new mutation object with an associated `type` and set of
   `arguments`. Will be massed to the appropriate `MutationHandler` for
   execution."
  [type arguments]
  (MutationImpl. (uuid/new-mutation-id) type arguments))

(defprotocol MutationHandler
  "Protocol for components which perform mutations."
  (mutate! [this arguments]
    "Performs a mutation with the provided arguments, making some change to the
     external state of the system. Should return nil, may throw an exception on
     failure."))

(defn- get-node
  "Returns the node value for the node named `node-name`."
  [node-name]
  (let [node (node-name @node-registry)]
    (when (nil? node)
      (throw (RuntimeException. (str "Node not found: " node-name))))
    node))

(defn- contains-queries?
  "Returns True if any value in the provided map is a Query object."
  [values]
  (some #(instance? QueryImpl %) values))

(defn execution-step
  "Performs a single step of the node execution process. Recursively executes
   the transitive dependencies of `node-name` using the intermediate results in
   the `current-values` map, which should either be a request map or a result of
   a previous call to `execution-step`. Returns a map from node names to their
   output values for nodes which could be executed (i.e. those which do not have
   dependencies which returned a Query object)."
  [current-values node-name]
  (if (node-name current-values)
    current-values ; Short-circuit if a value already exists for this node name.
    (let [{:keys [:inputs :function]} (get-node node-name)
          results (reduce execution-step current-values inputs)
          arguments (map results inputs)]
      (if (contains-queries? arguments)
        results
        (assoc results node-name (apply function arguments))))))

(s/fdef execute-queries :args
        (s/cat :node namespaced-keyword?
               :request (s/map-of namespaced-keyword? some?)
               :query-handlers (s/map-of namespaced-keyword?
                                         #(instance? QueryHandler %))))
(defn execute-queries
  "Executes `node`, supplying its dependencies from `request` and querying for
   any needed values from the provided `query-handlers` map, which should be a
   map from query types to the QueryHandler instance for that type. Returns the
   output of `node`."
  [node request query-handlers])

(s/fdef execute! :args
        (s/cat :node namespaced-keyword?
               :request (s/map-of namespaced-keyword? some?)
               :query-handlers (s/map-of keyword?
                                         #(instance? QueryHandler %))
               :mutation-handlers (s/map-of keyword?
                                            #(instance? MutationHandler %))))
(defn execute!
  "Executes `node` as in `execute-queries` and then obtains any Mutation
   instances returned from `node` and executes them via the provided
   `mutation-handlers` map, which should be a map from mutation types to the
   appropriate MutationHandler for that type. Returns the output of `node`."
  [node request query-handlers mutation-handlers])
