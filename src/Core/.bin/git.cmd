@echo off
SET PATH=%PATH%;%~dp0
"%~dp0node" "%~dp0..\..\..\packages\NoGit.0.0.5\node_modules\nogit\bin\git.js" %*
