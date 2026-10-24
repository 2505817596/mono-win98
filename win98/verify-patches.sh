#!/bin/sh
# Check (or apply) the Win98 patch series on a pristine mono source tree.
#
# Usage:
#   sh win98/verify-patches.sh /path/to/mono-src            # dry-run
#   MODE=apply sh win98/verify-patches.sh /path/to/mono-src # apply
#
# The tree must match the revisions in UPSTREAM.md, with the corefx
# submodule populated (see UPSTREAM.md).
set -e
SRC=${1:?usage: verify-patches.sh <mono-src-dir>}
HERE=$(cd "$(dirname "$0")" && pwd)
PATCHES="$HERE/../patches"
MODE=${MODE:-check}

if [ "$MODE" = apply ]; then
    APPLY=""
else
    APPLY="--dry-run"
fi

cd "$SRC"
for p in "$PATCHES"/*.patch; do
    name=$(basename "$p")
    printf '%-48s ' "$name"
    if patch -p1 $APPLY -N -s -f < "$p"; then
        echo OK
    else
        echo FAIL
        exit 1
    fi
done
echo "all patches $([ "$MODE" = apply ] && echo applied || echo verified)"
