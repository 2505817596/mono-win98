# Developing for the Win98 Mono runtime

Two separate workflows: writing applications against the prebuilt runtime, and
changing the runtime itself.

## 1. Applications (no Mono rebuild)

You need a release zip (runtime + reference assemblies) and a .NET SDK on the
build machine (Roslyn `csc`).

1. Install the runtime on the Win98 machine, e.g. extract to `C:\MONO`:

```
C:\MONO\bin\mono.exe
C:\MONO\lib\mono\4.5\mscorlib.dll, System.dll, System.Drawing.dll,
                       System.Windows.Forms.dll, ...
```

2. Compile against those assemblies, not the Microsoft reference assemblies.
   The easiest is to invoke Roslyn directly (see `samples/HelloWin98/build.bat`):

```
set MONO_REF=C:\MONO\lib\mono\4.5
dotnet exec "%CSC%" /nologo /noconfig /nostdlib+ /target:winexe /platform:x86 ^
  /out:HelloWin98.exe ^
  /r:"%MONO_REF%\mscorlib.dll" /r:"%MONO_REF%\System.dll" ^
  /r:"%MONO_REF%\System.Core.dll" /r:"%MONO_REF%\System.Drawing.dll" ^
  /r:"%MONO_REF%\System.Windows.Forms.dll" Program.cs
```

   A classic .csproj with `<HintPath>$(MonoRef)\...` also works if your
   msbuild can be pointed at the Mono reference assemblies; `csc` is the
   tested path.

3. Copy the exe to the Win98 machine and run it with the required environment:

```
set MONO_PATH=C:\MONO\lib\mono\4.5
set MONO_DEBUG=use-fallback-tls
set MONO_THREADS_SUSPEND=preemptive
C:\MONO\bin\mono.exe C:\APPS\HelloWin98.exe
```

`MONO_DEBUG=use-fallback-tls` is required because Windows 9x has no NT TLS
slots; `MONO_THREADS_SUSPEND=preemptive` is needed for WinForms and blocking
threads. The release `RUN-*.BAT` scripts show the same pattern.

Limits to keep in mind: no uxtheme/visual styles, no printing (Win9x has no
`GetDefaultPrinter`), `Process.ProcessName` is unavailable, no browser engine.
See `docs/FEATURE-MATRIX.md`.

## 2. Runtime work

Prerequisites: MSYS2 mingw32, the mono 6.13.0 source tree, .NET SDK.

1. Apply `patches/*.patch` to the mono tree. The patches cover the build
   system, Win9x TLS/threads/GC, SEH logging, the WinForms fixes and the
   Win98 compatibility object wiring (`configure.ac`, `mono/mini/Makefile.am.in`).
2. `win98/build-runtime.sh` configures and builds `libmonosgen-2.0.dll` and
   `mono.exe`, and redirects the native imports to the shim DLLs.
3. `win98/build-shims.sh` builds all shim DLLs from `win98/*.c` (verified
   against Win98 with the full self-test suite).
4. `win98/build-swf.sh` rebuilds `System.Windows.Forms.dll` with Roslyn 4.x
   (the 3.9 build in the tree produces the legacy ContextMenu crash).
5. Copy the BCL assemblies from `mcs/class/lib/net_4_x-win32`, the shims,
   `unicows.dll`, `gdiplus.dll` and `etc/mono/config` into the install layout.

### Test protocol on Windows 98

```
C:\MONO\bin\mono.exe RuntimeSelfTest.exe   (36 BCL checks)
C:\MONO\bin\mono.exe SelfTest.exe          (32 WinForms/threading checks)
C:\MONO\bin\mono.exe GCTest.exe            (8 GC checks)
C:\MONO\bin\mono.exe ..\MiniCtx\MiniCtx.exe (legacy ContextMenu dismissal)
```

All four must pass before shipping a runtime change. `SelfTest` writes
`C:\SELFTEST.TXT`, `RuntimeSelfTest` writes `C:\RTSELFTEST.TXT`, `GCTest`
writes `C:\GCTEST.TXT`; the MiniCtx log ends with `phase3 ALIVE`.

A Win98 machine or VM is mandatory: the port relies on Win9x-specific
behaviour (ANSI windows, MSLU, no NT TLS slots, no directory handles) that
cannot be validated on a modern Windows.

## 3. Debugging helpers

- `MONO98_VERBOSE=1` re-enables the runtime startup diagnostics.
- `MONO_LOG_LEVEL=debug MONO_LOG_MASK=asm,dll` logs assembly/DLL loading.
- `win98/vnctool.py` is a minimal VNC client used to drive a VMware/QEMU
  Windows 98 guest (set `VNC_HOST`, `VNC_PORT`, `VNC_PASS`).
- `win98/checkwin98.py <dll>` lists imports that Windows 98 does not export.
