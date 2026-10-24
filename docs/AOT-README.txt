Optional AOT images for the WinForms runtime (Mono 6.13.0 / Win98)

Generated with:  mono --aot <assembly>.dll
Images included: mscorlib, System, System.Core, System.Xml, System.Drawing,
                 Accessibility.
System.Windows.Forms.dll.dll is intentionally NOT included: it was built
from the previous System.Windows.Forms.dll, and the managed DLL has since
been rebuilt (see win98/build-swf.sh).  The old image no longer matches
and gives no WinForms AOT speedup, so it was dropped from the package.
Regenerate it from the rebuilt assembly on a host with the matching as/ld
to restore AOT speedups for WinForms.

Install (Win98):
  copy *.dll.dll into  <mono>\lib\mono\4.5\   (next to the managed assemblies)
or run mono with:
  mono --aot-path=<dir> your.exe

Measured on the Win98 SE test VM (CreateProcess -> Form.Shown, similar
StartupProbe app, warm file cache, all seven images present):
  JIT only:            ~495-545 ms
  with AOT (warm):     ~201-210 ms   (about 2.4x faster)
  first run after copying the images (cold cache): ~1.6-1.8 s
  (numbers measured with the default quiet startup; MONO98_VERBOSE=1 adds
   ~0.4-0.5 s due to console diagnostics)

Notes:
- The images must match the exact runtime/assembly build; do not mix with a
  different System.Windows.Forms.dll or mscorlib.dll.
- Warm-up cost on first launch is disk I/O for the images; after that Win98's
  file cache keeps them hot.
