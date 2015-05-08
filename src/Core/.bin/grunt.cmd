@echo off
SET PATH=%PATH%;%~dp0
"%~dp0node" "%~dp0..\..\..\packages\Grunt.0.1.13\node_modules\grunt-cli\bin\grunt" %*
