#!/usr/bin/env bash
# common-interpreter.sh

declare -A count
while IFS= read -r -d '' file; do
  read -r first_line < "$file"
  if [[ $first_line == \#!* ]]; then
    interp=${first_line#\#!}
    interp=${interp%% *}
    (( count["$interp"]++ ))
  fi
done < <(find /bin -type f -perm /111 -print0 2>/dev/null)

max=0; common=""
for interp in "${!count[@]}"; do
  if (( count[$interp] > max )); then
    max=${count[$interp]}
    common=$interp
  fi
done

echo "$common"
