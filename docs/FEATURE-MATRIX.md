# Mono 6.13 on Windows 98 - feature matrix

Status: OK = verified on Win98 SE (128MB VM), PART = limited/partially verified,
NO = unsupported/known broken, N/T = not tested yet.

## Runtime
| Area | Status | Notes |
|---|---|---|
| i686 JIT | OK | all tests run JIT-only |
| AOT (7 core assemblies) | OK | ~0.2 s to Form.Shown vs ~0.5 s JIT |
| sgen GC | OK | 8/8 GCTest checks |
| Generics / LINQ / expression trees | OK | |
| Reflection / Activator / attributes | OK | |
| Exceptions / SEH crash logging | OK | C:\CRASH.TXT |
| P/Invoke + Win98 shims | OK | |
| Threads / TLS / thread pool | OK | fallback TLS, preemptive suspend |
| async/await / Task / Parallel | OK | continuations resume on UI thread |
| AppDomain | OK | |
| Remoting / WCF / System.Web | N/T | assemblies shipped, not validated |

## BCL
| Area | Status | Notes |
|---|---|---|
| Core types, string, DateTime, TimeZoneInfo | OK | timezone via GetDynamicTimeZoneInformation shim |
| Math / decimal / BigInteger | OK | |
| Collections (generic + legacy) | OK | |
| IO: files, streams, drives | OK | |
| Directory create/enumerate/recursive delete | OK | FindFirstFile-based Win9x fallback |
| Text encodings UTF-8 / GBK(936) | OK | I18N.CJK included |
| XML (XmlDocument/XDocument/XmlSerializer) | OK | |
| BinaryFormatter | OK | |
| Regex | OK | |
| Crypto MD5/SHA256/RNG | OK | RNG through bcrypt shim |
| AES / X509 / TLS | N/T | expected weak on Win9x |
| Globalization (zh-CN, cp936) | OK | |
| DataTable | OK | DataSet/DataAdapter not exercised |
| Sockets TCP (loopback/LAN) | OK | 500 round-trips ~145 ms |
| UDP | N/T | |
| DNS | PART | needs HOSTS/DNS server; "localhost" fails without HOSTS |
| HTTPS / TLS | NO | not validated, SChannel too old |
| IPv6 / NetworkInterface | NO | factory requires XP+ |
| Process Id / current process | OK | Win9x PID is full 32-bit, may print negative |
| Process.ProcessName | NO | throws on Win9x PID |
| Process.Start | N/T | |
| Stopwatch / Environment / P/Invoke | OK | |
| Compression / JSON / DataContract | N/T | |

## WinForms
| Area | Status | Notes |
|---|---|---|
| Form, modal Form.ShowDialog, MDI parent/child | OK | WmActivate recursion fixed |
| MessageBox (managed) | OK | |
| MenuStrip / ToolStrip(button,label,combo,textbox,dropdown) / StatusStrip | OK | rendering verified |
| Label, LinkLabel, Button, CheckBox, RadioButton, GroupBox, Panel | OK | |
| TextBox (single/multiline), MaskedTextBox, RichTextBox styles | OK | TextBox forced to GDI+ on Win9x |
| NumericUpDown, DomainUpDown, ComboBox (2 styles) | OK | |
| ListBox, CheckedListBox, ListView(+images), TreeView(checkboxes/images), ImageList | OK | |
| TabControl, SplitContainer, FlowLayoutPanel, TableLayoutPanel | OK | |
| ProgressBar, TrackBar, HScrollBar, VScrollBar | OK | |
| DateTimePicker, MonthCalendar | OK | |
| PictureBox, GDI+ drawing | OK | |
| DataGridView (render + edit), PropertyGrid | OK | |
| ContextMenuStrip popup, ToolTip popup | OK | |
| NotifyIcon (tray) | OK | needs shell32 shim |
| Clipboard ASCII/Chinese | OK | ACP conversion fixed |
| Chinese IME input | OK | DBCS WM_CHAR pairing |
| OpenFileDialog / SaveFileDialog / FolderBrowserDialog | OK | |
| Printing dialogs | NO | Win9x has no GetDefaultPrinter |
| WebBrowser / ActiveX | NO | |
| Visual styles (uxtheme) | NO | expected on Win98 |
| Drag & drop | N/T | shell32 forwarders present |
| Legacy DataGrid | N/T | |
| Menu dropdown opening | N/T | rendering only |

## Win98 shim layer (all included in bin/)
k32x.dll (kernel32 ~200 exports incl. NT APIs, file API prefix stripping,
GetSystemTimes emulation), w98user.dll (RegisterClassW, MessageBoxW etc. via
unicows), w98gdi.dll, w98shl.dll (Shell_NotifyIconW, SHGetFolderPathW,
ShellExecuteExW), w98adv32.dll, ole98.dll, w98crt.dll, w98ws2.dll,
bcrypt.dll, unicows.dll, gdiplus.dll.

## Verification suites
SelfTest.exe (WinForms) 32/32 PASS
RuntimeSelfTest.exe (BCL) 36/36 PASS
GCTest.exe (GC) 8/8 PASS
Bench (JIT, 128MB VM): 50M int loop 56ms, LINQ 1M 56ms, sort 200k 70ms,
50 threads 531ms, 500 async delays 145ms, 1MB file IO 78ms,
500 TCP loopbacks 145ms.
Startup to Form.Shown: JIT ~0.50 s, AOT ~0.21 s (warm cache).
