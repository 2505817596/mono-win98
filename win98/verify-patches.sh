#!/bin/sh
# Check (or apply) the Win98 patch series on a pristine mono source tree.
#
# Usage:
#   sh win98/verify-patches.sh /path/to/mono-src            # apply (default)
#   MODE=check sh win98/verify-patches.sh /path/to/mono-src # dry-run
#
# The tree must match the revisions in UPSTREAM.md, with the corefx
# submodule populated (see UPSTREAM.md).
#
# Some files in the mono repository are stored with CRLF line endings
# (for example System.Windows.Forms/TextBoxTextRenderer.cs).  The patch
# series is line-ending normalised, so the affected files are converted to
# LF before patching; this only changes line endings, not content.
set -e
SRC=${1:?usage: verify-patches.sh <mono-src-dir>}
HERE=$(cd "$(dirname "$0")" && pwd)
PATCHES="$HERE/../patches"
MODE=${MODE:-apply}

cd "$SRC"

# Normalise the line endings of every file touched by the series.
grep -h '^+++ b/' "$PATCHES"/*.patch | sed 's|^+++ b/||' | while IFS= read -r f; do
    [ -f "$f" ] || continue
    if grep -q "$(printf '\r')" "$f" 2>/dev/null; then
        sed -i 's/\r$//' "$f"
    fi
done

if [ "$MODE" = apply ]; then
    APPLY=""
else
    APPLY="--dry-run"
fi

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
