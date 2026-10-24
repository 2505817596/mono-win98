# Third-party notices

This source repository contains only our patches, shims and test code. The
binary release archives additionally contain the following third-party
components. Verify their redistribution terms before republishing the
archives.

| Component | Where | Notes |
|---|---|---|
| Mono 6.13.0 runtime and BCL | `bin/`, `lib/mono/4.5/` | MIT license. Source: https://gitlab.winehq.org/mono/mono |
| .NET Framework reference assemblies | `lib/mono/4.5/*.dll` | Built from the Mono tree (MIT). |
| Microsoft Layer for Unicode (`unicows.dll`) | `bin/` | Microsoft redistributable; required for W APIs on Windows 9x. Redistribution is permitted only as part of an application; do not redistribute it standalone. |
| Microsoft GDI+ (`gdiplus.dll`) | `bin/`, `lib/mono/4.5/` | Windows 9x compatible GDI+ redistributable used by System.Drawing. Check the terms of the specific package you redistribute. |
| AOT images (`aot/*.dll.dll`) | AOT archive | Generated from the Mono assemblies above. |

Our shim DLLs (`k32x.dll`, `w98user.dll`, `w98gdi.dll`, `w98shl.dll`,
`w98adv32.dll`, `ole98.dll`, `w98crt.dll`, `w98ws2.dll`, `bcrypt.dll`) are
built from `win98/*.c` in this repository and covered by the MIT license.

If you cannot redistribute `unicows.dll` or `gdiplus.dll` under your
conditions, publish the release without them and document that users must
supply those components themselves.
