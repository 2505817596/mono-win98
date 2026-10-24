@echo off
if "%MONO_REF%"=="" set MONO_REF=C:\MONO\lib\mono\4.5
if "%CSC%"=="" set CSC=C:\Program Files\dotnet\sdk\8.0.421\Roslyn\bincore\csc.dll
set OUT=%~dp0MiniCtx.exe
dotnet exec "%CSC%" /nologo /noconfig /nostdlib+ /target:winexe /platform:x86 /optimize- /debug- /out:"%OUT%" /r:"%MONO_REF%\mscorlib.dll" /r:"%MONO_REF%\System.dll" /r:"%MONO_REF%\System.Core.dll" /r:"%MONO_REF%\System.Drawing.dll" /r:"%MONO_REF%\System.Windows.Forms.dll" "%~dp0Program.cs"
