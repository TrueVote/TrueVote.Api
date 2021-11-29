#!/bin/bash

# Copy the source to target
echo copying from $1/version.json to $2
mkdir $2
cp $1/version.json $2
