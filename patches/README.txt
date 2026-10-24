Mono 6.13.0 → Windows 98 patch set
==================================

Baseline: gitlab.winehq.org/mono/mono @ main (6.13.0)

A. Build-system fixes (host=MSYS2/mingw32, i686-w64-mingw32)
-----------------------------------------------------------
1. mcs/build/rules.make
   - BUILD_TOOLS_PROFILE = build-$(HOST_PLATFORM)
     (build tools land in class/lib/build-win32, matching the profile dir
      the rest of the build actually creates)
   - all MONO_PATH entries: absolute path + no empty trailing separator
     ($${MONO_PATH:+;$$MONO_PATH}); an empty entry is resolved by Mono on
     Windows as a path relative to the *current* directory and breaks loading.
   - tool exe paths made absolute.

2. mcs/build/library.make
   - sn.exe / resx2sr.exe / MONO_PATH made absolute.
   - sn -R / sn -v / cil-stringreplacer wrapped in a retry loop: on Windows the
     freshly written assembly is briefly locked (AV / indexer), causing
     "Sharing violation" from Mono.Security.StrongName.Sign.

3. mcs/build/executable.make
   - sn.exe path absolute.

4. mcs/build/profiles/net_4_x.make
   - BOOTSTRAP_PROFILE = build-$(HOST_PLATFORM)
   - BOOTSTRAP_MCS MONO_PATH absolute, no trailing separator.

5. mcs/tools/security/Makefile
   - $(PROFILE) → $(PROFILE_DIRECTORY) so sn.exe finds net_4_x-win32/mscorlib.dll.

6. mcs/class/RabbitMQ.Client/docs/specs/Makefile
   - same PROFILE_DIRECTORY + absolute MONO_PATH fix for Apigen.

7. mcs/class/Mono.Cecil/Mono.Cecil.dll.sources
   - add CodeAnalysis.cs + GenericParameterResolver.cs + MethodReferenceComparer.cs
     + TypeComparisonMode.cs + TypeReferenceEqualityComparer.cs + TypeResolver.cs
     (mono/cecil @ main added these; the 6.13 source list predates them)

B. Windows 98 compatibility layer
---------------------------------
configure.ac (mingw block):                 patch: configure_ac.patch
    WIN98_COMPAT_OBJ='$(top_srcdir)/../win98/compat2.lo'
    WIN98_COMPAT_LDFLAGS='-Wl,--allow-multiple-definition'
    AC_SUBST both
mono/mini/Makefile.am.in:                   patch: mono_mini_Makefile.am.in.patch
    libmonosgen_2_0_la_LIBADD = $(WIN98_COMPAT_OBJ) libmini.la ...
    libmonosgen_2_0_la_LDFLAGS += $(WIN98_COMPAT_LDFLAGS)
    rule: $(top_srcdir)/../win98/compat2.lo from win98/compat2.c

win98/compat2.c defines the 19 NT-only kernel32 entry points under their real
names; with --allow-multiple-definition the linker prefers these over the
kernel32 import stubs, so the runtime no longer imports them at all.

19 APIs implemented:
  GetTickCount64                     tick wrap accumulator
  InitializeSRWLock                  critical-section backed
  Acquire/ReleaseSRWLock{Exclusive,Shared}
  TryAcquireSRWLockExclusive
  InitializeConditionVariable        event + generation counter
  SleepConditionVariableCS / SRW
  WakeConditionVariable / WakeAllConditionVariable
  InitializeCriticalSectionEx        → InitializeCriticalSection
  IsWow64Process                     → always FALSE
  GetSystemTimes                     → ERROR_CALL_NOT_IMPLEMENTED
  GetCurrentProcessorNumberEx        → group 0, cpu 0
  GetModuleHandleExW                 → GetModuleHandleW
  GetFileInformationByHandleEx       → ERROR_CALL_NOT_IMPLEMENTED
  SetThreadStackGuarantee            → TRUE
  Add/RemoveVectoredExceptionHandler → no-op (Win9x has no vectored EH)

C. Import redirection + shims (post-link)
-----------------------------------------
win98/redirect_dlls.py renames PE import descriptors so the loader picks the
shim next to mono.exe instead of the system DLL:

  KERNEL32.dll → k32x.dll       (19 NT APIs implemented; other 151 exports are
                                 forwarders back to KERNEL32)
  ADVAPI32.dll → advapi32.dll   5 Win2K security APIs (ACL) → graceful failure
  OLE32.dll    → ole32.dll      CoInitializeEx, CoCreateFreeThreadedMarshaler
  WS2_32.dll   → ws2_32.dll     WSASocketW, WSAIoctl, getaddrinfo/freeaddrinfo,
                                getnameinfo
  SHELL32.dll  → shell32.dll    CommandLineToArgvW, SHGetFolderPathW,
                                ShellExecuteExW
  bcrypt.dll   → bcrypt.dll     BCryptGenRandom via CryptGenRandom
  msvcrt.dll   → msvcrt.dll     _ecvt_s

E. Runtime / BCL patches added for WinForms
--------------------------------------------
  mono/utils/mono-tls.h          Win9x TEB has no NT TlsSlots: read TLS through
                                 TlsGetValue (preserving LastError), skip index 0
  mono/utils/mono-threads.c      small_id range check, stale registration cleanup
  mono/utils/hazard-pointer.c    RW hazard table, bounds check for small_id
  mono/utils/mono-filemap.c      _wfopen fails on Win9x -> fopen fallback
  mono/utils/mono-mmap-windows.c log file_map failures
  mono/utils/mono-dl-windows.c   LoadLibraryExA/LoadLibraryA fallback for W stubs
                                 patch: mono_utils_mono-dl-windows.c.patch
  mono/metadata/object.c         alloc_vtable: explicit 8-byte alignment
                                 patch: mono_metadata_object.c.patch
  mono/mini/exceptions-x86.c     SEH filter logging
  mono/metadata/domain.c, appdomain.c   startup diagnostics
  mcs/class/System.Windows.Forms/System.Windows.Forms/Form.cs
                                 WmActivate: only bounce activation back to the
                                 top modal dialog when wParam != WA_INACTIVE
                                 (Win9x sends WA_INACTIVE synchronously, the old
                                 code recursed Form.Activate -> WM_ACTIVATE ->
                                 Form.Activate until the stack overflowed).
                                 patch: 0001-SWF-WmActivate-WA_INACTIVE.patch
  mcs/class/System.Windows.Forms/System.Windows.Forms/TextBoxTextRenderer.cs
                                 use GDI+ (Graphics.DrawString) for TextBox text
                                 when OS major version < 5: the GDI path
                                 (user32!DrawTextW via MSLU) silently renders
                                 nothing on Win9x, leaving text boxes blank.
                                 patch: 0002-SWF-TextBox-GDIplus-on-Win9x.patch
  mcs/class/System.Windows.Forms/System.Windows.Forms/XplatUIWin32.cs
                                 clipboard CF_TEXT <-> string conversion uses
                                 GetACP() (e.g. 936/GBK); the old
                                 Marshal.PtrToStringAnsi path assumed UTF-8 and
                                 threw ExecutionEngineException on non-ASCII
                                 paste.  Needs I18N.CJK.dll for cp936.
                                 patch: 0003-SWF-clipboard-ANSI-codepage.patch
  mcs/class/System.Windows.Forms/System.Windows.Forms/NativeWindow.cs
                                 Win9x IME: pair DBCS lead/trail bytes from
                                 WM_CHAR, decode with GetACP(), re-dispatch a
                                 single Unicode WM_CHAR (fixes Chinese IME
                                 mojibake).  k32x.dll exports IsDBCSLeadByteEx.
                                 patch: 0004-SWF-IME-DBCS-WM_CHAR.patch
  mcs/class/System.Windows.Forms/System.Windows.Forms/XplatUIWin32.cs
                                 Win9x user32 returns WM_USER+3 (WM_ASYNC_MESSAGE,
                                 used by Control.Invoke/BeginInvoke) as 0x173
                                 from GetMessageA; handle it as WM_ASYNC_MESSAGE
                                 (otherwise Invoke deadlocks).
                                 patch: 0005-SWF-Win9x-async-message.patch
  external/corefx FileSystemEnumerableFactory.cs + Directory.cs
                                 Win9x directory enumeration / recursive delete
                                 fallback using FindFirstFileW (the NT enumerator
                                 needs directory handles + NtQueryDirectoryFile).
  mcs/class/System/System.Net.Sockets/Socket.cs
                                 skip NetworkInterface on Windows 9x (factory is
                                 XP+ only) so Socket construction works.
                                 patch: 0006-runtime-Win9x-IO-sockets.patch
  win98/compat2.c                GetSystemTimes must not return FALSE: mono_cpu_usage
                                 calls g_error() and aborts when the thread pool
                                 computes CPU load.  Emulate with GetTickCount.

F. Legacy ContextMenu dismissal crash (fixed)
---------------------------------------------
Symptom: right-click a TextBox (Mono's built-in ContextMenu), then
left-click elsewhere.  The process dies with an access violation
(c0000005, read at 0xffffffff) instead of dismissing the menu.

Analysis: with JIT method logging (MONO98_JITMAP=1) the faulting EIP maps
to System.Windows.Forms.MenuTracker:HideSubPopups.  The observed call
chain is:
  Control.WmLButtonDown -> Control.ProcessActiveTracker
  -> MenuTracker.OnMouseDown -> MenuTracker.Deactivate
  -> MenuTracker.DeselectItem -> PopUpWindow.HideWindow
  -> MenuTracker.HideSubPopups
The fault address moves with the code generator (JIT and AOT builds of
the same assembly both fault), so it is tied to the compiled form of this
method in the shipped System.Windows.Forms.dll.  Rebuilding the same
patched sources with the .NET SDK's Roslyn (4.11) produces an assembly
that runs this path correctly: the menu is dismissed and the app keeps
running.  The exact JIT/AOT codegen root cause was not isolated.

Repro: projects/MiniCtx (self-driving right-click on a TextBox followed
by a left-click on the form); the real demo (ControlDemo, Text tab) was
also verified by hand.

Build: win98/build-swf.sh rebuilds System.Windows.Forms.dll with Roslyn
4.11 into the install tree.

AOT caveat: the old System.Windows.Forms.dll.dll image was compiled from
the previous assembly and no longer matches the rebuilt DLL.  It gives no
AOT speedup for System.Windows.Forms and should not be shipped as is; it
was removed from the AOT package.  Regenerate it from the rebuilt assembly
on a host with the matching as/ld to restore AOT for System.Windows.Forms.

D. Verify
---------
win98/checkwin98.py <dll>   lists imports that do not exist on Windows 98.
