#!/bin/sh
set -e

if [ "$#" -ne 1 ]; then
    echo "Usage: runIntegrationTests.sh [test]"
    exit
fi

echo "Running integration tests..."

rm DungeonStrike/Out/dungeonstrike.app/Logs/client_logs.txt || true
mkdir -p DungeonStrike/Out/dungeonstrike.app/Logs
touch DungeonStrike/Out/dungeonstrike.app/Logs/client_logs.txt

rm driver/out/logs/driver_logs.txt || true
mkdir -p driver/out/logs
touch ./driver/out/logs/driver_logs.txt

./scripts/buildUnityClient.sh

./scripts/buildDriverJar.sh

./DungeonStrike/Out/dungeonstrike.app/Contents/MacOS/dungeonstrike -batchmode --port 59006  &
process_id=$!
echo "Client started with pid $process_id"

java -jar driver/out/dungeonstrike-0.1.0-SNAPSHOT-standalone.jar --crash-on-exceptions --read-logs-from-start --verbose --port 59006 --client-path DungeonStrike/Out/dungeonstrike.app --driver-path driver/out --tests-path tests --test $1
