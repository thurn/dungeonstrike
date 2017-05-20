#!/bin/sh
set -e

echo "Building Driver..."

cd driver
lein uberjar
mkdir -p out
mv target/dungeonstrike-0.1.0-SNAPSHOT-standalone.jar out/

# Interactive development via tools like clojure/tools.namespace is broken by
# having compiled artifacts on the classpath, so we always need to clean after
# building.
lein clean
