#!/bin/sh

if [[ $(pwd) != *"dungeonstrike" ]]
then
  echo "You must invoke this script from the root directory."
  exit
fi

cd "driver"

if [ "$1" == "--fix" ]
then
    echo "Formatting Clojure source files..."
    lein cljfmt fix
else
    lein cljfmt check
    if [ "$?" -eq 1 ]
    then
        echo "Clojure Formatting Errors!"
        echo "Run './scripts/cljfmt.sh --fix' to reformat."
        exit 1
    fi
fi
