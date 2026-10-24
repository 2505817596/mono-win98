#!/bin/sh
# Rebuild System.Windows.Forms.dll from the patched Mono sources with the
# .NET SDK's Roslyn (4.x).  The in-tree Roslyn 3.9 binary produces an
# assembly whose MenuTracker.HideSubPopups faults on Win98 when a legacy
# ContextMenu is dismissed by an outside click; this rebuild fixes it.
#
# Usage:  MONO_SRC=/path/to/mono-src sh build-swf.sh [out.dll]
set -e
: "${MONO_SRC:?set MONO_SRC to the patched mono source tree}"
ROOT=$MONO_SRC
REF=${MONO_REF:-$ROOT/mcs/class/lib/net_4_x-win32}
OUT=${1:-System.Windows.Forms.dll}
if [ -z "$CSC" ]; then
  CSC=$(ls -d "C:/Program Files/dotnet/sdk/"*/Roslyn/bincore/csc.dll 2>/dev/null | tail -1)
fi
: "${CSC:?set CSC to Roslyn csc.dll (dotnet SDK)}"

cd "$ROOT/mcs/class/System.Windows.Forms"
MSYS_NO_PATHCONV=1 dotnet exec "$CSC" \
  /codepage:65001 /nologo /noconfig /deterministic \
  -d:NET_4_0 -d:NET_4_5 -d:NET_4_6 -d:MONO -d:WIN_PLATFORM \
  -nowarn:1699 -nostdlib /debug:portable /optimize /langversion:latest /unsafe \
  @System.Windows.Forms.dll.resources -nowarn:618,612,809 \
  -r:$REF/System.dll -r:$REF/System.Xml.dll -r:$REF/System.Drawing.dll \
  -r:$REF/Accessibility.dll -r:$REF/System.Data.dll -r:$REF/Mono.Posix.dll \
  -r:$REF/Mono.WebBrowser.dll -r:$REF/System.Configuration.dll \
  -r:$REF/System.Runtime.Serialization.Formatters.Soap.dll -r:$REF/mscorlib.dll \
  /keyfile:../ecma.pub -target:library -out:"$OUT" \
  @$ROOT/mcs/build/deps/win32_net_4_x__System.Windows.Forms.dll.response
echo "built $OUT"
