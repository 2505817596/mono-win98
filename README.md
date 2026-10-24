# Mono 6.13.0 on Windows 98 SE

[![build](https://github.com/2505817596/mono-win98/actions/workflows/build.yml/badge.svg)](https://github.com/2505817596/mono-win98/actions/workflows/build.yml)

Unofficial port of **Mono 6.13.0** and its **System.Windows.Forms** to Windows 98 SE.
This repository contains the patch set, the Win9x compatibility shims and the test
applications; prebuilt runtime packages are published as GitHub Releases.

> Not affiliated with the Mono project or Microsoft. Use at your own risk on
> machines you control.

## What works

Verified on a Windows 98 SE VM (128 MB RAM, 800x600):

- **WinForms**: Form, MenuStrip, ToolStrip, StatusStrip, TabControl, TextBox,
  RichTextBox, Button, CheckBox/RadioButton, ComboBox, ListBox, CheckedListBox,
  ListView, TreeView, DataGridView, PropertyGrid, MonthCalendar, DateTimePicker,
  NumericUpDown/DomainUpDown, ProgressBar, TrackBar, SplitContainer,
  FlowLayoutPanel, TableLayoutPanel, PictureBox (GDI+), ToolTip,
  ContextMenuStrip, legacy ContextMenu, MDI, NotifyIcon, MessageBox,
  Form.ShowDialog, OpenFileDialog/SaveFileDialog/FolderBrowserDialog.
- **Runtime/BCL**: strings/collections/LINQ/reflection/expression trees,
  encoding 936 (GBK) and UTF-8, streams, drives, directory enumeration and
  recursive delete, XML (XmlDocument/XDocument/XmlSerializer),
  BinaryFormatter, regex, MD5/SHA256/RNG, DataTable, P/Invoke, GC (sgen),
  Monitor/Mutex/Semaphore, ThreadPool, Task/Parallel/async-await,
  Cancellation, TCP loopback sockets.
- **Input/clipboard**: Chinese clipboard (CF_TEXT via ACP), DBCS IME WM_CHAR
  pairing, Control.Invoke/BeginInvoke.
- **AOT**: all assemblies except System.Windows.Forms can run from AOT images
  (startup 201-210 ms vs 495-545 ms JIT on the test VM).

Known limitations: no uxtheme visual styles (Win98 has none), printing dialogs
fail (no `GetDefaultPrinter` on Win9x), `Process.ProcessName` unavailable,
no WebBrowser engine, some NT-only APIs shimmed as no-ops. See
`docs/FEATURE-MATRIX.md` and `docs/install-README.txt` for details.

## Quick start (prebuilt)

1. Download a release zip and extract it to `C:\MONO` (so you have
   `C:\MONO\bin\mono.exe`, `C:\MONO\lib\mono\4.5\...`).
2. Copy a test executable next to the `.BAT` files (or set `APP_DIR`), then run,
   for example, `RUN-SELFTEST.BAT`, `RUN-RTSELFTEST.BAT`, `RUN-GCTEST.BAT`,
   `RUN-CONTROLDEMO.BAT`.
   The scripts set the required environment:
   `MONO_PATH`, `MONO_DEBUG=use-fallback-tls`, `MONO_THREADS_SUSPEND=preemptive`.
3. Optional: copy the files from the AOT zip (`aot\*.dll.dll`) into
   `C:\MONO\lib\mono\4.5\` for faster startup.

## Repository layout

```
UPSTREAM.md  pinned mono/corefx revisions used by the patch series
.github/     CI: applies the patch series to the pinned tree and builds the shims
patches/     patch series against mono-6.13.0 + documented runtime/BCL fixes
win98/       kernel32/user32/... shims, import redirection, build scripts
             (build-runtime.sh, build-shims.sh, build-swf.sh)
projects/    self tests and demos (SelfTest, RuntimeSelfTest, GCTest,
             ControlDemo, MiniCtx context-menu repro)
samples/     minimal app showing how to build against the runtime
docs/        build notes, developer guide, feature matrix, install notes
```

## Building from source

See `docs/BUILDING.md` and `docs/DEVELOPING.md`. Fetch the mono and corefx
revisions listed in `UPSTREAM.md`, check the series with
`win98/verify-patches.sh <mono-src>` and apply it with `MODE=apply`, then run
`win98/build-runtime.sh` (configure/make/import redirection) and
`win98/build-shims.sh`; rebuild `System.Windows.Forms.dll` with the .NET
SDK's Roslyn via `win98/build-swf.sh`. CI performs the patch check and the
shim build on every push (`runtime` build is a manual, experimental job).

The exact environment used for the published binaries is described in
`docs/BUILDING.md`; the build is not a one-command turnkey build.

## Context-menu crash fix

Right-clicking a TextBox and then left-clicking elsewhere used to kill the
process inside `System.Windows.Forms.MenuTracker.HideSubPopups`. The assembly
shipped in the releases is rebuilt from the same patched sources with Roslyn
4.x (see `win98/build-swf.sh`); the legacy ContextMenu now dismisses correctly.
`projects/MiniCtx` is the self-driving repro used to verify it.

## License

Our patches, shims and test code are released under the MIT license
(see `LICENSE`). Mono itself is MIT-licensed; see its source tree for the
original notices. The release archives additionally contain third-party
redistributable components (`unicows.dll`, `gdiplus.dll`, ...); see
`THIRD-PARTY-NOTICES.md`. They are not part of this source repository.
