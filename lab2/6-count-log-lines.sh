#!/usr/bin/env bash
# count-log-lines.sh

total=$(find /var/log -type f -name '*.log' -exec cat {} + 2>/dev/null | wc -l)
echo "Total lines in *.log files: $total"
