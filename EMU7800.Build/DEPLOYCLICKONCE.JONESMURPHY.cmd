@echo off

rem .application application/x-ms-application
rem .manifest    application/x-ms-manifest
rem .deploy      application/octet-stream

pushd %~dp0

robocopy ..\DROP\ClickOnce\jones-murphy.org\ \\storage\c$\inetpub\wwwroot\EMU7800\ * /S

popd