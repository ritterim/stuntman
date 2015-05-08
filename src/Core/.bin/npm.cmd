@echo off
SET PATH=%PATH%;%~dp0
SET npm_config_git=%~dp0git.cmd
"%~dp0node" "%~dp0..\..\..\packages\Npm.1.4.4\node_modules\npm\bin\npm-cli.js" %*
