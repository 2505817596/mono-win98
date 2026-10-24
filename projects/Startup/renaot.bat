@echo off
if "%MONO_HOME%"=="" set MONO_HOME=C:\MONO
if "%APP_DIR%"=="" set APP_DIR=%~dp0
ren %MONO_HOME%\lib\mono\4.5\System.dll.dll System.dll.aot
ren %MONO_HOME%\lib\mono\4.5\System.Core.dll.dll System.Core.dll.aot
ren %MONO_HOME%\lib\mono\4.5\System.Xml.dll.dll System.Xml.dll.aot
ren %MONO_HOME%\lib\mono\4.5\System.Drawing.dll.dll System.Drawing.dll.aot
ren %MONO_HOME%\lib\mono\4.5\Accessibility.dll.dll Accessibility.dll.aot
ren %MONO_HOME%\lib\mono\4.5\System.Windows.Forms.dll.dll System.Windows.Forms.dll.aot
