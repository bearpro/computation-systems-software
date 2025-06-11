#!/usr/bin/env bash
# xorg-messages.sh

LOG=/var/log/Xorg.0.log
OUT=full.log
: > info.tmp
: > warn.tmp

grep '(II)' "$LOG" 2>/dev/null | sed 's/.*(II)/Information:/' > info.tmp

grep '(WW)' "$LOG" 2>/dev/null | sed 's/.*(WW)/Warning:/' > warn.tmp

cat info.tmp warn.tmp > "$OUT"
rm info.tmp warn.tmp

cat "$OUT"
