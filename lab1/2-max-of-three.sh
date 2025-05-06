#!/usr/bin/env bash
# max_of_three.sh

if [ "$#" -ne 3 ]; then
  echo "Usage: $0 num1 num2 num3"
  exit 1
fi

max=$1
for n in "$2" "$3"; do
  if (( n > max )); then
    max=$n
  fi
done

echo "max: $max"
