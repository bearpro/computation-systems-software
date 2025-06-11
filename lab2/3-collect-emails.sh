#!/usr/bin/env bash
# collect-emails.sh

grep -rhoE '[[:alnum:]._%+-]+@[[:alnum:].-]+\.[[:alpha:]]{2,}' /etc 2>/dev/null \
  | sort -u \
  | paste -sd, - \
  > emails.lst
