#!/usr/bin/env bash
# menu.sh

while true; do
  cat <<EOF
Options:
1) nano
2) vi
3) links
4) exit
EOF

  read -rp "Select option[1-4]: " choice
  case "$choice" in
    1) nano ;;
    2) vi ;;
    3) links ;;
    4) echo "Exit"; break ;;
    *) echo "Invalid option" ;;
  esac
done
