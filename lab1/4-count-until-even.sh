#!/usr/bin/env bash
# count_until_even.sh

count=0
while true; do
  read -rp "Input integer (even to stop): " num
  if ! [[ "$num" =~ ^-?[0-9]+$ ]]; then
    echo "Not an integer"
    continue
  fi
  (( count++ ))
  if (( num % 2 == 0 )); then
    break
  fi
done

echo "Count: $count"
