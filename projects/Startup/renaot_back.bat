@echo off
if "%MONO_HOME%"=="" set MONO_HOME=C:\MONO
if "%APP_DIR%"=="" set APP_DIR=%~dp0
ren %MONO_HOME%\lib\mono\4.5\System.dll.aot System.dll.dll
ren %MONO_HOME%\lib\mono\4.5\System.Core.dll.aot System.Core.dll.dll
ren %MONO_HOME%\lib\mono\4.5\System.Xml.dll.aot System.Xml.dll.dll
ren %MONO_HOME%\lib\mono\4.5\System.Drawing.dll.aot System.Drawing.dll.dll
ren %MONO_HOME%\lib\mono\4.5\Accessibility.dll.aot Accessibility.dll.dll
ren %MONO_HOME%\lib\mono\4.5\System.Windows.Forms.dll.aot System.Windows.Forms.dll.dll
ren %MONO_HOME%\lib\mono\4.5\mscorlib.dll.aot mscorlib.dll.dll
