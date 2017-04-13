#!/bin/sh

if [[ $(pwd) != *"dungeonstrike" ]]
then
  echo "You must invoke this script from the root directory."
  exit
fi

echo "Backing up to ~/backup"
rsync -a -i --delete . ~/backup | pv --progress --line-mode --size $(find . | wc -l) > /dev/null
