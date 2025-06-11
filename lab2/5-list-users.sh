#!/usr/bin/env bash
# list-users.sh

cut -d: -f1,3 /etc/passwd | sort -t: -k2,2n
