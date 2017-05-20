#!/bin/bash
set -e

if [[ $(pwd) != *"dungeonstrike" ]]
then
  echo "You must invoke this script from the root directory."
  exit
fi

# Run Uncrustify formatter
./scripts/uncrustify.sh

# Run Clojure formatter
./scripts/cljfmt.sh

# Check for ThirdParty modifications
./scripts/checksum.py

# Create a mirror of the code to run tests on
./scripts/backup.sh

# Chang to mirror directory
cd ~/backup

# Run Editor tests
./scripts/runEditorTests.sh

# Run Integration tests
./scripts/runIntegrationTests.sh all
