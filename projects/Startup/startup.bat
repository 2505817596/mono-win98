@echo off
if "%MONO_HOME%"=="" set MONO_HOME=C:\MONO
if "%APP_DIR%"=="" set APP_DIR=%~dp0
set MONO_PATH=%MONO_HOME%\lib\mono\4.5
set MONO_DEBUG=use-fallback-tls
set MONO_THREADS_SUSPEND=preemptive
del C:\STARTUP_RESULT.TXT
C:\TIMEIT.EXE %MONO_HOME%\bin\"%MONO_HOME%\bin\mono.exe" "%APP_DIR%\StartupProbe.exe"
C:\TIMEIT.EXE %MONO_HOME%\bin\"%MONO_HOME%\bin\mono.exe" "%APP_DIR%\StartupProbe.exe"
C:\TIMEIT.EXE %MONO_HOME%\bin\"%MONO_HOME%\bin\mono.exe" "%APP_DIR%\StartupProbe.exe"
