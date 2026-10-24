#!/bin/sh
export TMP='C:\Users\25058\AppData\Local\Temp'
export TEMP='C:\Users\25058\AppData\Local\Temp'
export TMPDIR=/tmp
if [ "$1" = "-c" ]; then shift; fi
eval "$*"
