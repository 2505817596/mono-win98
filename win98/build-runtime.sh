#!/bin/sh
# Cross-build Mono 6.13.0 (libmonosgen + mono.exe) for Windows 98.
#
# Reference procedure used for the published binaries.  It assumes:
#   - the patch series in ../patches has been applied to $MONO_SRC
#     (including the configure.ac / mono/mini/Makefile.am.in Win98 wiring)
#   - win98/ (this directory) is a sibling of the mono source tree, so
#     $(top_srcdir)/../win98/compat2.c resolves
#   - MSYS2 mingw32 toolchain + MSYS2 make in PATH
#
# Environment:
#   MONO_SRC  path to the patched mono-6.13.0 source tree (required)
#   OUT       install layout root: $OUT/bin, $OUT/lib/mono/4.5 (required)
#   JOBS      parallel make jobs (default 4)
set -e

: "${MONO_SRC:?set MONO_SRC to the patched mono source tree}"
: "${OUT:?set OUT to the install layout root}"
SHIM_DIR=$(cd "$(dirname "$0")" && pwd)
JOBS=${JOBS:-4}

test -f "$MONO_SRC/../win98/compat2.c" || {
    echo "error: expected $MONO_SRC/../win98/compat2.c (sibling layout)" >&2
    exit 1
}

cd "$MONO_SRC"
./configure --enable-maintainer-mode --enable-compile-warnings \
            --host=i686-w64-mingw32 --disable-nls

# shwrap.sh re-exports TMP/TEMP so gcc can create its temp .s files.
make SHELL="$SHIM_DIR/shwrap.sh" -j"$JOBS"
make -C mono/mini SHELL="$SHIM_DIR/shwrap.sh" -j"$JOBS"

mkdir -p "$OUT/bin"
python "$SHIM_DIR/redirect_dlls.py" \
    mono/mini/.libs/libmonosgen-2.0.dll "$OUT/bin/libmonosgen-2.0.dll" \
    advapi32.dll=w98adv32.dll kernel32.dll=k32x.dll msvcrt.dll=w98crt.dll \
    ole32.dll=ole98.dll shell32.dll=w98shl.dll ws2_32.dll=w98ws2.dll
cp mono/mini/mono.exe "$OUT/bin/mono.exe"

echo
echo "Built: $OUT/bin/mono.exe, $OUT/bin/libmonosgen-2.0.dll"
echo "Next:"
echo "  1. sh win98/build-shims.sh and copy the DLLs to $OUT/bin"
echo "  2. copy the BCL assemblies to $OUT/lib/mono/4.5 (from mcs/class/lib/net_4_x-win32)"
echo "  3. rebuild System.Windows.Forms: MONO_SRC=... sh win98/build-swf.sh $OUT/lib/mono/4.5/System.Windows.Forms.dll"
echo "  4. copy unicows.dll/gdiplus.dll, etc/mono/config and run the self tests on Win98"
