HelloWin98 - minimal WinForms app for the Win98 Mono runtime
===========================================================

1. Install a release zip on the Windows 98 machine, e.g. to C:\MONO.
2. On the build machine (with the .NET SDK):

     set MONO_REF=C:\MONO\lib\mono\4.5
     set CSC=C:\Program Files\dotnet\sdk\<version>\Roslyn\bincore\csc.dll
     build.bat

   MONO_REF is copied from the Windows 98 install; the assemblies are
   Mono's, not the Microsoft .NET Framework reference assemblies.
3. Copy HelloWin98.exe to the Windows 98 machine and run:

     set MONO_PATH=C:\MONO\lib\mono\4.5
     set MONO_DEBUG=use-fallback-tls
     set MONO_THREADS_SUSPEND=preemptive
     C:\MONO\bin\mono.exe HelloWin98.exe

The program opens a small form; clicking the button changes the label.
