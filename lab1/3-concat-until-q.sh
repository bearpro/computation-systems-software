#!/usr/bin/env bash
# concat_until_q.sh

result=""
while true; do
  read -rp "Input line ("q" to exit): " line
  if [ "$line" = "q" ]; then
    break
  fi
  result+="$line"
done

echo $result
