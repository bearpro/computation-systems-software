#!/usr/bin/env bash
# top-man-words.sh

man bash 2>/dev/null \
  | tr -c '[:alpha:]' '[\n*]' \
  | tr '[:upper:]' '[:lower:]' \
  | awk 'length($0)>=4' \
  | sort \
  | uniq -c \
  | sort -rn \
  | head -n3 \
  | awk '{print $2}'
