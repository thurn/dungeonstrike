#!/bin/sh
set -e

if [[ $(pwd) != *"dungeonstrike" ]]
then
  echo "You must invoke this script from the root directory."
  exit
fi

status=$(git status)
if [[ $status != *"nothing to commit, working directory clean"* ]]
then
  echo "Commit first!"
  exit
fi

branch=$(git branch)
if [[ $branch != *"master"* ]]
then
  echo "Not on master!"
  echo $branch
  exit
fi

if [[ $status == *"Your branch is up-to-date with 'origin/master'"* ]]
then
  echo "Up to date!"
  echo $status
  exit
fi

./scripts/checksum.py

./scripts/backup.sh

echo "Pushing master to origin"
git push origin master
