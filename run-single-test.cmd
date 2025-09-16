@echo off
echo === Testing Basic Installation E2E Test ===

echo.
echo Running single E2E test to identify current failure point...
dotnet test --filter "FullyQualifiedName~ProductionInstallationE2E.BasicInstallation_Should_Create_Required_Directory_Structure" --verbosity normal

echo.
echo === Test Status Check Complete ===