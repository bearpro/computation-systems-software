#!/usr/bin/env bash
# extract-acpi.sh

grep -h '^ACPI' /var/log/* 2>/dev/null > errors.log

grep -E '/[^ ]+\.[^ ]+' errors.log
