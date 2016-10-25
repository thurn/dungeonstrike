#!/bin/sh

echo "Backing up to ~/backup"
rsync -a -i . ~/backup | pv --progress --line-mode --size $(find . | wc -l) > /dev/null
