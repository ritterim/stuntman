@echo Off
pushd %~dp0
setlocal

set CACHED_NUGET=%LOCALAPPDATA%\NuGet\NuGet.exe
if exist %CACHED_NUGET% goto CopyNuGet

echo Downloading latest version of NuGet.exe...
if not exist %LOCALAPPDATA%\NuGet @mkdir %LOCALAPPDATA%\NuGet
@powershell -NoProfile -ExecutionPolicy Unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%CACHED_NUGET%'"

:CopyNuGet
if exist .nuget\nuget.exe goto Build
if not exist .nuget @mkdir .nuget
@copy %CACHED_NUGET% .nuget\nuget.exe > nul

:Build

:: Find the most recent 32bit MSBuild.exe on the system. Also handle x86 operating systems, where %PROGRAMFILES(X86)%
:: is not defined. Always quote the %MSBUILD% value when setting the variable and never quote %MSBUILD% references.
set MSBUILD="%PROGRAMFILES(X86)%\MSBuild\14.0\Bin\MSBuild.exe"
if not exist %MSBUILD% @set MSBUILD="%PROGRAMFILES%\MSBuild\14.0\Bin\MSBuild.exe"
if not exist %MSBUILD% @set MSBUILD="%PROGRAMFILES(X86)%\MSBuild\12.0\Bin\MSBuild.exe"
if not exist %MSBUILD% @set MSBUILD="%PROGRAMFILES%\MSBuild\12.0\Bin\MSBuild.exe"
if not exist %MSBUILD% @set MSBUILD="%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

%MSBUILD% build\Build.msbuild /nologo /m /v:m /fl /flp:LogFile=msbuild.log;Verbosity=Detailed /nr:false %*

if %ERRORLEVEL% neq 0 goto BuildFail
goto BuildSuccess

:BuildFail
echo.
echo *** BUILD FAILED ***
goto End

:BuildSuccess
echo.
echo *** BUILD SUCCEEDED ***
goto End

:End
echo.
popd
exit /B %ERRORLEVEL%
