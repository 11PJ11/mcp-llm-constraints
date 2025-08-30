@echo off
REM Windows batch wrapper for cleanup-orphaned-processes.ps1
REM Usage: cleanup-orphaned-processes.cmd [options]
REM   
REM Options:
REM   -force     : Force cleanup without confirmation
REM   -dryrun    : Preview what would be cleaned up
REM   -verbose   : Show detailed information

setlocal EnableDelayedExpansion

REM Build PowerShell command with arguments
set "PS_ARGS="
if /i "%1"=="-force" set "PS_ARGS=%PS_ARGS% -Force"
if /i "%1"=="-dryrun" set "PS_ARGS=%PS_ARGS% -DryRun"
if /i "%1"=="-verbose" set "PS_ARGS=%PS_ARGS% -Verbose"
if /i "%2"=="-force" set "PS_ARGS=%PS_ARGS% -Force" 
if /i "%2"=="-dryrun" set "PS_ARGS=%PS_ARGS% -DryRun"
if /i "%2"=="-verbose" set "PS_ARGS=%PS_ARGS% -Verbose"
if /i "%3"=="-force" set "PS_ARGS=%PS_ARGS% -Force"
if /i "%3"=="-dryrun" set "PS_ARGS=%PS_ARGS% -DryRun"
if /i "%3"=="-verbose" set "PS_ARGS=%PS_ARGS% -Verbose"

REM Execute PowerShell script
powershell -ExecutionPolicy Bypass -File "%~dp0cleanup-orphaned-processes.ps1" %PS_ARGS%

endlocal