# Backup existing file and replace with updated version
$originalFile = "tests\ConstraintMcpServer.Tests\Unit\Installation\InstallationManagerShould.cs"
$backupFile = "tests\ConstraintMcpServer.Tests\Unit\Installation\InstallationManagerShould.cs.backup"

# Backup original
Copy-Item $originalFile $backupFile -Force

Write-Host "Original file backed up to $backupFile"
Write-Host "Ready to replace with updated version that matches new interface"