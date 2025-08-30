#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Cleanup orphaned ConstraintMcpServer processes that may prevent builds
    
.DESCRIPTION
    This utility detects and terminates orphaned ConstraintMcpServer.exe processes
    that may be left behind by test timeouts or debugging sessions. This prevents
    file locks that would block dotnet build and clean operations.
    
    Designed for safe use in development workflow and CI/CD pipelines.
    
.PARAMETER Force
    Force cleanup without confirmation prompts
    
.PARAMETER DryRun  
    Show what would be cleaned up without actually terminating processes
    
.PARAMETER Verbose
    Show detailed information about process detection and cleanup
    
.EXAMPLE
    .\scripts\cleanup-orphaned-processes.ps1
    Interactive cleanup with confirmation
    
.EXAMPLE  
    .\scripts\cleanup-orphaned-processes.ps1 -Force
    Automatic cleanup without confirmation
    
.EXAMPLE
    .\scripts\cleanup-orphaned-processes.ps1 -DryRun
    Preview what would be cleaned up
#>

param(
    [switch]$Force = $false,
    [switch]$DryRun = $false, 
    [switch]$Verbose = $false
)

# Enhanced logging function
function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "INFO"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $colorMap = @{
        "INFO" = "White"
        "WARN" = "Yellow" 
        "ERROR" = "Red"
        "SUCCESS" = "Green"
        "DEBUG" = "Cyan"
    }
    
    if ($Verbose -or $Level -ne "DEBUG") {
        Write-Host "[$timestamp] [$Level] $Message" -ForegroundColor $colorMap[$Level]
    }
}

# Function to find orphaned ConstraintMcpServer processes
function Find-OrphanedProcesses {
    Write-Log "Scanning for orphaned ConstraintMcpServer processes..." -Level "DEBUG"
    
    try {
        # Look for both .exe and .dll versions (could be run via dotnet)
        $processes = @()
        
        # Find .exe processes
        $exeProcesses = Get-Process -Name "ConstraintMcpServer" -ErrorAction SilentlyContinue
        if ($exeProcesses) {
            $processes += $exeProcesses
        }
        
        # Find dotnet processes running ConstraintMcpServer.dll
        $dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object {
            try {
                $cmdLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $($_.Id)").CommandLine
                return $cmdLine -like "*ConstraintMcpServer.dll*"
            } catch {
                return $false
            }
        }
        if ($dotnetProcesses) {
            $processes += $dotnetProcesses
        }
        
        return $processes
    } catch {
        Write-Log "Error scanning for processes: $($_.Exception.Message)" -Level "ERROR"
        return @()
    }
}

# Function to check if process is likely orphaned
function Test-ProcessOrphaned {
    param([System.Diagnostics.Process]$Process)
    
    try {
        # Check if process has been running for more than 30 seconds
        $runningTime = (Get-Date) - $Process.StartTime
        $isLongRunning = $runningTime.TotalSeconds -gt 30
        
        # Check if process has no window (likely background process)
        $hasNoWindow = -not $Process.MainWindowTitle
        
        # Additional check: See if process is consuming CPU (active) or idle
        $cpuTime = $Process.TotalProcessorTime
        Start-Sleep -Milliseconds 100
        $Process.Refresh()
        $newCpuTime = $Process.TotalProcessorTime
        $isIdle = ($newCpuTime - $cpuTime).TotalMilliseconds -lt 10
        
        Write-Log "Process $($Process.Id): Runtime=$([math]::Round($runningTime.TotalSeconds,1))s, NoWindow=$hasNoWindow, Idle=$isIdle" -Level "DEBUG"
        
        # Consider orphaned if long-running, no window, and idle
        return $isLongRunning -and $hasNoWindow -and $isIdle
        
    } catch {
        Write-Log "Error checking process $($Process.Id): $($_.Exception.Message)" -Level "DEBUG"
        # If we can't determine, assume it might be orphaned for safety
        return $true
    }
}

# Function to safely terminate a process
function Stop-ProcessSafely {
    param(
        [System.Diagnostics.Process]$Process,
        [switch]$DryRunMode = $false
    )
    
    $processInfo = "PID $($Process.Id) ($($Process.ProcessName))"
    
    if ($DryRunMode) {
        Write-Log "Would terminate: $processInfo" -Level "WARN"
        return $true
    }
    
    try {
        Write-Log "Terminating process: $processInfo" -Level "WARN"
        
        # Step 1: Try graceful shutdown
        if ($Process.CloseMainWindow()) {
            Write-Log "Sent close window signal to $processInfo" -Level "DEBUG"
            if ($Process.WaitForExit(3000)) {
                Write-Log "Process $processInfo terminated gracefully" -Level "SUCCESS"
                return $true
            }
        }
        
        # Step 2: Force termination
        Write-Log "Force terminating process: $processInfo" -Level "WARN"
        $Process.Kill()
        
        if ($Process.WaitForExit(5000)) {
            Write-Log "Process $processInfo force terminated successfully" -Level "SUCCESS"
            return $true
        } else {
            Write-Log "Process $processInfo did not terminate within timeout" -Level "ERROR"
            return $false
        }
        
    } catch {
        Write-Log "Failed to terminate process $processInfo : $($_.Exception.Message)" -Level "ERROR"
        return $false
    }
}

# Function to verify build system is working
function Test-BuildSystem {
    Write-Log "Testing build system health..." -Level "DEBUG"
    
    try {
        # Test if we can run dotnet clean without file locks
        $cleanResult = & dotnet clean 2>&1
        $cleanExitCode = $LASTEXITCODE
        
        if ($cleanExitCode -eq 0) {
            Write-Log "Build system test: dotnet clean succeeded" -Level "SUCCESS"
            return $true
        } else {
            Write-Log "Build system test: dotnet clean failed (exit code: $cleanExitCode)" -Level "ERROR"
            Write-Log "Clean output: $cleanResult" -Level "DEBUG"
            return $false
        }
        
    } catch {
        Write-Log "Build system test failed: $($_.Exception.Message)" -Level "ERROR"
        return $false
    }
}

# Main execution
function Main {
    Write-Log "=== ConstraintMcpServer Orphaned Process Cleanup ===" -Level "INFO"
    Write-Log "Force: $Force, DryRun: $DryRun, Verbose: $Verbose" -Level "DEBUG"
    
    # Find orphaned processes
    $allProcesses = Find-OrphanedProcesses
    
    if (-not $allProcesses -or $allProcesses.Count -eq 0) {
        Write-Log "No ConstraintMcpServer processes found" -Level "SUCCESS"
        
        # Still test build system health
        if (Test-BuildSystem) {
            Write-Log "Build system is healthy" -Level "SUCCESS"
        } else {
            Write-Log "Build system has issues despite no orphaned processes" -Level "WARN"
        }
        return
    }
    
    Write-Log "Found $($allProcesses.Count) ConstraintMcpServer process(es)" -Level "INFO"
    
    # Filter to likely orphaned processes
    $orphanedProcesses = @()
    foreach ($proc in $allProcesses) {
        if (Test-ProcessOrphaned -Process $proc) {
            $orphanedProcesses += $proc
        }
    }
    
    if ($orphanedProcesses.Count -eq 0) {
        Write-Log "No orphaned processes detected - all processes appear to be active" -Level "SUCCESS"
        return
    }
    
    Write-Log "Detected $($orphanedProcesses.Count) orphaned process(es):" -Level "WARN"
    foreach ($proc in $orphanedProcesses) {
        $runningTime = ((Get-Date) - $proc.StartTime).TotalMinutes
        Write-Log "  - PID $($proc.Id): $($proc.ProcessName) (running $([math]::Round($runningTime,1)) minutes)" -Level "WARN"
    }
    
    if ($DryRun) {
        Write-Log "DRY RUN MODE: Would terminate $($orphanedProcesses.Count) process(es)" -Level "INFO"
        foreach ($proc in $orphanedProcesses) {
            Stop-ProcessSafely -Process $proc -DryRunMode
        }
        return
    }
    
    # Confirm termination (unless Force is specified)
    $shouldTerminate = $Force
    if (-not $Force) {
        $response = Read-Host "Terminate $($orphanedProcesses.Count) orphaned process(es)? (y/N)"
        $shouldTerminate = $response -match "^[yY]"
    }
    
    if (-not $shouldTerminate) {
        Write-Log "Process cleanup cancelled by user" -Level "INFO"
        return
    }
    
    # Terminate orphaned processes
    $successCount = 0
    foreach ($proc in $orphanedProcesses) {
        if (Stop-ProcessSafely -Process $proc) {
            $successCount++
        }
    }
    
    Write-Log "Successfully terminated $successCount of $($orphanedProcesses.Count) orphaned processes" -Level "SUCCESS"
    
    # Give processes time to release file handles
    if ($successCount -gt 0) {
        Write-Log "Waiting for file handles to be released..." -Level "DEBUG"
        Start-Sleep -Seconds 2
    }
    
    # Test build system after cleanup
    if (Test-BuildSystem) {
        Write-Log "Build system is now healthy after cleanup" -Level "SUCCESS"
    } else {
        Write-Log "Build system still has issues after cleanup - may need manual intervention" -Level "ERROR"
    }
}

# Execute main function
Main