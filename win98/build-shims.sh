#!/bin/sh
# Build the Windows 9x shim DLLs from the sources in this directory.
#
# Verified on Win98 SE with the rebuilt DLLs: RuntimeSelfTest 36/36,
# SelfTest 32/32, GCTest 8/8, and the legacy ContextMenu dismissal test
# (projects/MiniCtx) all pass.
#
# Requirements: i686-w64-mingw32-gcc (MSYS2 mingw32) and its dlltool.
# The shims import msvcrt.dll, so link with libmsvcrt-os and the msvcrt
# stdio (__USE_MINGW_ANSI_STDIO=0).  -nostdlib avoids the UCRT startup
# objects of newer mingw-w64; DllMain is the entry point.
set -e
cd "$(dirname "$0")"

# Native mingw tools (dlltool/gcc) need a writable Windows temp directory.
BUILD_TMP=$(pwd)/.tmp
mkdir -p "$BUILD_TMP"
command -v cygpath >/dev/null && BUILD_TMP=$(cygpath -w "$BUILD_TMP")
export TMP="$BUILD_TMP" TEMP="$BUILD_TMP"

CC=${CC:-i686-w64-mingw32-gcc}
DLLTOOL=${DLLTOOL:-dlltool}
command -v "$DLLTOOL" >/dev/null || DLLTOOL=i686-w64-mingw32-dlltool
MINGW_LIB=${MINGW_LIB:-$(dirname "$(command -v "$CC")")/../lib}
JOBS=${JOBS:-1}

"$CC" --version >/dev/null

# MSLU import libraries (unicows.dll forwards used by the .def files)
[ -s libudep.a ] || "$DLLTOOL" -d udep.def -l libudep.a
[ -s libunicows_dep.a ] || "$DLLTOOL" -d unicows_dep.def -l libunicows_dep.a
test -s libudep.a && test -s libunicows_dep.a || {
    echo "error: dlltool failed to build the MSLU import libraries" >&2
    exit 1
}

CFLAGS="-O2 -D__USE_MINGW_ANSI_STDIO=0"
LDFLAGS="-shared -nostdlib -Wl,--enable-stdcall-fixup -Wl,--entry,_DllMain@12"
# System import libraries are given by full path: with -nostdlib the -l
# search can pick a mismatched stub on some toolchains.
LIBS="-L. -ludep -lunicows_dep -lmsvcrt-os \
 $MINGW_LIB/libkernel32.a $MINGW_LIB/libuser32.a $MINGW_LIB/libgdi32.a \
 $MINGW_LIB/libadvapi32.a $MINGW_LIB/libshell32.a $MINGW_LIB/libole32.a \
 $MINGW_LIB/libws2_32.a $MINGW_LIB/libbcrypt.a -lgcc"

build () {
    out=$1; src=$2; def=$3
    echo "CC      $out"
    # shellcheck disable=SC2086
    $CC $CFLAGS $LDFLAGS -o "$out" "$src" "$def" $LIBS
}

build k32x.dll     shim.c             k32x.def
build w98user.dll  user32shim.c       w98user.def
build w98shl.dll   shell32_shim.c     w98shl.def
build w98adv32.dll advapi32_shim.c    w98adv32.def
build ole98.dll    ole32_shim.c       ole98.def
build w98ws2.dll   ws2_32_shim.c      w98ws2.def
build w98crt.dll   msvcrt_shim.c      w98crt.def
build bcrypt.dll   bcrypt_shim.c      bcrypt.def

# w98gdi.dll is a def-only forwarder (no C source); build it if dlltool
# output is enough for your toolchain, or use the binary from the release.
ls -l k32x.dll w98user.dll w98shl.dll w98adv32.dll ole98.dll w98ws2.dll w98crt.dll bcrypt.dll
