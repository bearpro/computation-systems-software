#!/usr/bin/env bash
# compare_strings.sh

if [ "$#" -ne 2 ]; then
  echo "Usage: $0 string1 string2"
  exit 1
fi

if [ "$1" = "$2" ]; then
  echo "eq"
else
  echo "ne"
fi
