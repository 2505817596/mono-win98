@echo off
rem Build against the Win98 Mono runtime assemblies.
rem Set MONO_REF to <install>\lib\mono\4.5, and CSC to Roslyn csc.dll.
if "%MONO_REF%"=="" set MONO_REF=C:\MONO\lib\mono\4.5
if "%CSC%"=="" set CSC=C:\Program Files\dotnet\sdk\8.0.421\Roslyn\bincore\csc.dll
set OUT=%~dp0HelloWin98.exe

dotnet exec "%CSC%" /nologo /noconfig /nostdlib+ /target:winexe /platform:x86 ^
  /out:"%OUT%" ^
  /r:"%MONO_REF%\mscorlib.dll" /r:"%MONO_REF%\System.dll" ^
  /r:"%MONO_REF%\System.Core.dll" /r:"%MONO_REF%\System.Drawing.dll" ^
  /r:"%MONO_REF%\System.Windows.Forms.dll" "%~dp0Program.cs"
if errorlevel 1 exit /b 1
echo Built %OUT%
