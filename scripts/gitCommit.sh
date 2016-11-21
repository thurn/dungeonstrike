#!/bin/sh
# Script to run pre-commit checks and then tag a commit with a KEY indicating
# it was checked. Optionally, an argument of --amendHead can be passed, which
# will cause the previous commit to be amended without changing its commit
# message.

set -e

if [[ $(pwd) != *"dungeonstrike" ]]
then
  echo "You must invoke this script from the root directory."
  exit
fi

branch=$(git branch)
if [[ $branch != *"master"* ]]
then
  echo "Not on master, aborting checks."
  git commit "$@"
  exit
fi

echo "Starting pre-commit checks..."

./scripts/checksum.py

./scripts/backup.sh

key="KEY: $(date "+%Y-%m-%d %H:%M" | md5)"

for arg in "$@";
do echo "$arg";
   if [[ "$arg" == "--amendHead" ]]
   then
       echo "Amending previous commit."
       previousMessage=$(git log -1 --pretty=%B)
       withoutKey="${previousMessage/KEY:*/}"
       git commit -a --amend -m "$withoutKey" -m "$key"
       exit
   fi
done

git commit "$@" -m $key
