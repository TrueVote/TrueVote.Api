#!/bin/bash
echo copy-version.sh

# Copy the source to target
echo copying from $1version.json to $2
if [ ! -d $2 ]; then
  mkdir $2
fi
cp $1version.json $2
