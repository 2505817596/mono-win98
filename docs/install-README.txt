
WinForms support (verified)
===========================
A standard WinForms project (Win98WinFormsDemo, VS-style .csproj layout)
renders and runs on Windows 98 SE: MenuStrip, ToolStrip, StatusStrip,
GroupBox, TextBox, Button, CheckBox, RadioButton, ComboBox, ListBox,
CheckedListBox, TabControl, TreeView, ListView, NumericUpDown,
DateTimePicker, ProgressBar, TrackBar, PictureBox (GDI+ gradient),
DataGridView, ContextMenuStrip, ToolTip and a Timer.

Run:
  RUN-SELFTEST.BAT              (SelfTest.exe - automated WinForms PASS/FAIL suite)
  RUN-RTSELFTEST.BAT            (RuntimeSelfTest.exe - 36 BCL/runtime checks)
  RUN-GCTEST.BAT                (GCTest.exe - 8 GC checks)
  RUN-CONTROLDEMO.BAT           (Win98ControlDemo.exe - merged all-controls demo)
  RUN-WINFORMS.BAT Win98WinFormsDemo.exe
  RUN-WINFORMS.BAT helloform.exe

RuntimeSelfTest.exe (source in demo/SelfTest) runs 36 checks without WinForms:
core types/collections/LINQ/reflection/expressions/exceptions, IO (UTF-8 and GBK
text, streams, drives, directory create/enumerate/recursive delete), XML
(XmlDocument/XDocument/XmlSerializer), BinaryFormatter, regex, MD5/SHA256/RNG,
culture and codepage 936, DataTable, P/Invoke, GC, Monitor/Mutex/Semaphore/
ThreadPool, Task/Parallel/async/Cancellation, TCP loopback and Uri.  It writes
C:\RTSELFTEST.TXT and returns the number of failures as its exit code.

Win9x runtime/BCL fixes included (see demo/0006-runtime-Win9x-IO-sockets.patch):
  - Directory enumeration and recursive delete use a FindFirstFileW fallback
    (corefx FileSystemEnumerator needs directory handles + NtQueryDirectoryFile,
    which Windows 9x does not have).
  - Sockets: Socket.IsProtocolSupported no longer calls NetworkInterface on
    Windows 9x (its Win32 factory requires XP+); TCP loopback works.
  - k32x.dll strips extended-length (question-mark) paths in file APIs because
    corefx passes them to CreateDirectoryW / RemoveDirectoryW / FindFirstFileW.
  Known limitation: Process.ProcessName is unavailable; Win9x PIDs are full
  32-bit values, so Mono's signed Process.Id often prints negative (a valid PID).

GCTest.exe (source in demo/SelfTest) runs 8 GC checks: API basics,
collection counters, scoped WeakReference collection, finalizers,
reclaim after 100k allocations, large-object (LOH) reclaim, GCHandle
rooting/release and thread-churn allocation.  All 8 pass on Win98.
(mono --stats itself crashes on Win98; use GC.GetTotalMemory/CollectionCount
from inside the app instead.)

SelfTest.exe (source in demo/SelfTest) runs 32 automated checks covering
controls, GDI+ drawing, ASCII/Chinese clipboard, simulated DBCS WM_CHAR,
async/await on the UI context, Task.WhenAll/TaskCompletionSource,
Control.Invoke/BeginInvoke, ManualResetEvent, System.Threading.Timer,
WinForms Timer, modal ShowDialog (self-closing), NotifyIcon and MDI.
It writes C:\SELFTEST.TXT and prints SUMMARY pass=N fail=N.

FEATURE-MATRIX.md contains the full runtime/BCL/WinForms feature matrix
with OK/PART/NO/N-T status.

Win98ControlDemo.exe (source in demo/Win98ControlDemo) is the merged demo:
8 tabs - Basics, Text, Lists, Ranges, Data, Layout, Dialogs, More - plus
MenuStrip, ToolStrip (Say Hello / Clear / Next Tab / Open / Save / Folder),
StatusStrip clock, MessageBox, ContextMenuStrip, ToolTip, DataGridView
editing, RichTextBox styling, MDI, NotifyIcon and the three file dialogs.
  (The Threads tab was added later: Background Thread + Control.Invoke,
  ThreadPool + BeginInvoke, System.Threading.Timer, lock + ManualResetEvent,
  BackgroundWorker, async/await with UI SynchronizationContext, Task.WhenAll
  and TaskCompletionSource.  Async continuations were verified to resume on
  the UI thread with InvokeRequired=False, which depends on the 0x173
  WM_ASYNC_MESSAGE fix above.)

What WinForms additionally needs (all included):
  bin\w98user.dll and lib\mono\4.5\user32.dll, gdi32.dll
      Forwarder shims. Mono's DllImport loader tries the assembly directory
      first; these shims forward every WinForms API to USER32/GDI32, and the
      W variants to UNICOWS (Win98's user32 W functions are non-functional).
      w98user.dll also implements:
        RegisterClassW  - converts WNDCLASSW->WNDCLASSA and maps an empty
                          menu name ("", used by Mono's XplatUIWin32) to NULL.
        CS_DROPSHADOW  - the 0x20000 class style is stripped; Win98 rejects
                          unknown styles and RegisterClass fails with err=0.
      Do not rename or move them.
  bin\k32x.dll
      kernel32 DllImport shim (mapped by etc\mono\config). Provides Win9x
      replacements for APIs managed code calls directly:
        SetThreadErrorMode (no-op), FindFirstFileExW/A (built on
        FindFirstFileA), SRW locks, condition variables, GetTickCount64,
        GetFileSizeEx, GlobalMemoryStatusEx, K32* (psapi), global-memory
        helpers used by the clipboard (GlobalLock/GlobalUnlock/GlobalSize,
        CopyMemory -> RtlMoveMemory), ...
      Without the kernel32 dllmap the runtime dies while probing
      'SetThreadErrorMode' during WinForms startup.
  bin\gdiplus.dll and lib\mono\4.5\gdiplus.dll
      Windows 9x compatible GDI+ (System.Drawing backend).
  lib\mono\4.5\Accessibility.dll
      Referenced by System.Windows.Forms.
  etc\mono\config
      dllmap entries for user32/gdi32/kernel32/shell32.
  bin\w98shl.dll
      shell32 shim mapped by etc\mono\config: forwards Shell_NotifyIconW,
      DragQueryFileW etc. to UNICOWS and implements SHGetFolderPathW /
      CommandLineToArgvW / ShellExecuteExW.  Without the Shell_NotifyIconW
      mapping NotifyIcon.Visible does nothing on Win98 (the shell32 W entry
      point is a stub).

Additional source patches (on top of the hello-world set)
  win98/compat2.c (linked into libmonosgen)
      GetSystemTimes: Win9x has no per-CPU counters and the old stub returned
      FALSE, which makes mono_cpu_usage() call g_error() and abort the process
      as soon as the thread pool or Thread.Start computes CPU load.  Derive an
      idle-only clock from GetTickCount instead.
  mono/utils/mono-dl-windows.c
      mono_dl_open_file: fall back to LoadLibraryExA/LoadLibraryA when the
      W variant fails (Win9x stubs).
  mono/metadata/object.c
      alloc_vtable: align the vtable to 8 bytes explicitly (Win9x allocators
      only guarantee 4).
  mcs/class/System.Windows.Forms/System.Windows.Forms/Form.cs
      WmActivate: only bounce activation back to the top modal dialog when the
      disabled form is actually being activated (wParam != WA_INACTIVE).
      Win9x delivers WM_ACTIVATE(WA_INACTIVE) synchronously when the dialog is
      activated; the original unconditional bounce recursed
      (Activate -> WM_ACTIVATE -> Activate -> ...) until the stack overflowed.
      The System.Windows.Forms.dll shipped in this package already contains
      this fix, so MessageBox.Show and Form.ShowDialog work.
  mcs/class/System.Windows.Forms/System.Windows.Forms/XplatUIWin32.cs
      Win9x user32 returns WM_USER+3 (WM_ASYNC_MESSAGE, used by
      Control.Invoke/BeginInvoke) as message 0x173 from GetMessageA; treat
      0x173 as WM_ASYNC_MESSAGE.  Without this, Invoke deadlocks and
      BeginInvoke never runs.
      patch: demo/0005-SWF-Win9x-async-message.patch
  mcs/class/System.Windows.Forms/System.Windows.Forms/TextBoxTextRenderer.cs
      On Win9x/ME use Graphics.DrawString (GDI+) instead of the GDI
      TextRenderer path.  The GDI path silently rendered nothing, so TextBox
      content (initial text and typed characters) was invisible.
      patch: demo/0002-SWF-TextBox-GDIplus-on-Win9x.patch
  mcs/class/System.Windows.Forms/System.Windows.Forms/XplatUIWin32.cs
      Clipboard CF_TEXT is converted with the system ANSI code page
      (GetACP(), e.g. 936/GBK) in both directions.  The old UTF-8 assumption
      threw ExecutionEngineException ("Illegal byte sequence") when pasting
      Chinese text.  Requires lib/mono/4.5/I18N.CJK.dll (included).
      patch: demo/0003-SWF-clipboard-ANSI-codepage.patch
  mcs/class/System.Windows.Forms/System.Windows.Forms/NativeWindow.cs
      Win9x IME: WM_CHAR arrives as one byte per DBCS element; lead/trail
      pairs are now decoded with GetACP() and re-dispatched as a single
      Unicode character, so Chinese IME input no longer shows mojibake.
      patch: demo/0004-SWF-IME-DBCS-WM_CHAR.patch

Optional AOT images (separate download)
  mono-6.13.0-win98-winforms-aot.zip contains AOT images for mscorlib, System,
  System.Core, System.Xml, System.Drawing, Accessibility and
  System.Windows.Forms.  Copy the *.dll.dll files into lib\mono.5 (next to
  the managed assemblies) or pass -aot-path=<dir>.  Measured on the test VM
  (CreateProcess -> Form.Shown, warm cache): JIT only ~950-1100 ms,
  AOT ~201-210 ms (about 2.4x faster).  First launch after installing them reads
  ~67 MB of images from disk and can take ~1.6-1.8 s.  (Startup measured with
  the quiet runtime, see below; enabling the Win98 diagnostics adds ~0.4-0.5 s.)

Win98 diagnostics (MONO98_VERBOSE)
  The runtime used to print per-thread / small-id / domain diagnostics to
  stderr on every start (visible in the MONO console window and costly on
  Win98's console).  They are now disabled by default; set MONO98_VERBOSE=1
  before starting mono to get them back when debugging.  Startup to
  Form.Shown on the test VM: JIT ~495-545 ms, AOT ~201-210 ms.

Known limitations
  uxtheme is absent on Win98, so visual styles are disabled (expected).
  Printing (winspool) and drag/drop APIs are not shimmed.  PrintDialog,
  PrintPreviewDialog and PageSetupDialog fail with EntryPointNotFoundException
  (GetDefaultPrinter) because Win9x has no GetDefaultPrinter; the test VM also
  has no printer installed.  OpenFileDialog/SaveFileDialog/FolderBrowserDialog
  work (Mono's managed dialogs).
  Modal dialogs (MessageBox.Show, Form.ShowDialog) work only with the patched
  System.Windows.Forms.dll included here (see the WmActivate entry above).
  With the unpatched 6.13 assembly they crash with a stack overflow
  (SEH c00000fd, endless Form.Activate -> WM_ACTIVATE recursion).
  A native unicows MessageBoxW (MBTEST.EXE) is also verified standalone.

Context-menu crash fixed in this build
  Right-clicking a TextBox and then left-clicking elsewhere used to kill the
  process (c0000005, read at 0xffffffff).  The fault was inside the compiled
  System.Windows.Forms.MenuTracker.HideSubPopups path (deactivate menu ->
  deselect item -> PopUpWindow.HideWindow -> HideSubPopups).  The packaged
  System.Windows.Forms.dll was rebuilt from the same patched sources with the
  .NET SDK Roslyn 4.11 (win98/build-swf.sh); with that assembly the menu is
  dismissed correctly, verified with projects/MiniCtx and the ControlDemo
  Text tab.  The old AOT image System.Windows.Forms.dll.dll no longer
  matches the rebuilt DLL and gives no AOT speedup; it was removed from
  the AOT package.  Regenerate it from the new assembly before re-enabling
  AOT for System.Windows.Forms.
