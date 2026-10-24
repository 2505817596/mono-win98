# Building the Win98 port

This is the environment and procedure used to produce the published runtime
packages. It is a cross-build from a modern Windows host; it is not a
one-command build.

## Prerequisites

- Windows host with MSYS2 (32-bit mingw toolchain, `i686-w64-mingw32-gcc`).
  The published binaries were built with GCC 16.2.0 from MSYS2.
- The mono 6.13.0 source tree (`gitlab.winehq.org/mono/mono`, tag/branch 6.13.0).
- .NET SDK 8+ (Roslyn) for rebuilding `System.Windows.Forms.dll`.
- A Windows 98 SE test machine/VM.

The steps below are also packaged as `win98/build-runtime.sh` (configure,
build, import redirection) and `win98/build-shims.sh` (all shim DLLs).
`win98/build-swf.sh` rebuilds System.Windows.Forms.

## 1. Patch the mono source

Apply the patch series in `patches/` (build-system fixes, Win9x TLS/threads/GC
fixes, SEH logging, WinForms fixes) with:

```
sh win98/verify-patches.sh /path/to/mono-src          # applies the series
MODE=check sh win98/verify-patches.sh /path/to/mono-src  # dry-run
```

The series is generated from the revisions in `UPSTREAM.md` and is checked by
CI on every push. It normalises the line endings of the touched files (a few
mono files are stored CRLF) before applying; only line endings change.
`patches/README.txt` documents every hunk and `patches/notes/` keeps the
older per-file notes.

## 2. Configure and build

```
./configure --enable-maintainer-mode --enable-compile-warnings \
            --host=i686-w64-mingw32 --disable-nls
```

The build recipes lose `TMP`/`TEMP` when MSYS sh starts, which makes gcc fail
with "can't open C:\WINDOWS\ccXXXX.s". Use the included `win98/shwrap.sh` as
`SHELL` for make (it re-exports the Windows temp directory):

```
make SHELL=/path/to/shwrap.sh -j4
make -C mono/mini SHELL=/path/to/shwrap.sh -j4
```

The runtime is `mono/mini/.libs/libmonosgen-2.0.dll`; `mono/mini/mono.exe` is
the launcher. `win98/compat2.c` is linked into the runtime (see
`patches/README.txt` section B) to provide the NT-only kernel32 APIs.

## 3. Build the shims

The sources are in `win98/`:

- `shim.c` + `k32x.def` -> `k32x.dll` (kernel32 shim)
- `user32shim.c` + `w98user.def` -> `w98user.dll`
- `shell32_shim.c`, `ole32_shim.c`, `advapi32_shim.c`, `ws2_32_shim.c`,
  `msvcrt_shim.c`, `bcrypt_shim.c` and the matching `.def` files.
- `udep.def` / `unicows_dep.def` are import libraries for the MSLU
  (`unicows.dll`) W APIs.

Build each as a shared DLL with the mingw32 toolchain, e.g.:

```
sh win98/build-shims.sh
```

The script generates the MSLU import libraries (`udep.def`,
`unicows_dep.def`), links with `-nostdlib -lmsvcrt-os` and the msvcrt stdio
(`-D__USE_MINGW_ANSI_STDIO=0`), and passes `DllMain` as the entry point.
This is the recipe used for the released DLLs; the rebuilt set was validated
on Win98 SE (RuntimeSelfTest 36/36, SelfTest 32/32, GCTest 8/8, MiniCtx).
`w98gdi.dll` is a def-only forwarder and is shipped as a prebuilt binary.

## 4. Redirect native imports

The loader must resolve kernel32/user32/... to the shims next to `mono.exe`.
`win98/redirect_dlls.py` renames the import descriptors in the PE:

```
python win98/redirect_dlls.py \
  mono/mini/.libs/libmonosgen-2.0.dll out/libmonosgen-2.0.dll \
  advapi32.dll=w98adv32.dll kernel32.dll=k32x.dll msvcrt.dll=w98crt.dll \
  ole32.dll=ole98.dll shell32.dll=w98shl.dll ws2_32.dll=w98ws2.dll
```

Copy the result next to `mono.exe` together with the shim DLLs, `unicows.dll`
and `gdiplus.dll`.

## 5. Rebuild System.Windows.Forms.dll

The in-tree Roslyn 3.9 compiler produces an assembly whose legacy ContextMenu
dismissal path faults on Win98. Rebuild the assembly from the same patched
sources with the .NET SDK's Roslyn:

```
MONO_SRC=/d/mono-src sh win98/build-swf.sh /path/to/System.Windows.Forms.dll
```

Place the result in `lib/mono/4.5/`. If you also ship AOT images, regenerate
`System.Windows.Forms.dll.dll` from this rebuilt assembly.

## 6. Test on Win98

Run the self tests from `projects/` (build them against the release install):

```
set MONO_PATH=C:\MONO\lib\mono\4.5
set MONO_DEBUG=use-fallback-tls
set MONO_THREADS_SUSPEND=preemptive
C:\MONO\bin\mono.exe SelfTest.exe
C:\MONO\bin\mono.exe RuntimeSelfTest.exe
C:\MONO\bin\mono.exe GCTest.exe
C:\MONO\bin\mono.exe Win98ControlDemo.exe
```

`SelfTest` reports 32 WinForms checks, `RuntimeSelfTest` 36 BCL checks and
`GCTest` 8 GC checks; they write `C:\SELFTEST.TXT`, `C:\RTSELFTEST.TXT` and
`C:\GCTEST.TXT`.

## Notes / known gaps

- `win98/checkwin98.py <dll>` lists imports that do not exist on Windows 98.
- `win98/vnctool.py` is a small RFB/VNC client used to automate the test VM
  (set `VNC_HOST`, `VNC_PORT`, `VNC_PASS` in the environment).
- The build is sensitive to the toolchain: the patch series documents the
  exact APIs that Win9x lacks. Reproducing the published binaries requires
  a msvcrt-capable i686 mingw toolchain and the same source revision.
