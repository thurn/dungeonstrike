#!/bin/sh
set -e

if [[ $(pwd) != *"dungeonstrike" ]]
then
  echo "You must invoke this script from the root directory."
  exit
fi

# TODO: This only works on OSX, update to support GNU find
find -s ./checksums -type f -exec md5deep {} \; | md5deep
