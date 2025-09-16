@echo off
echo === Testing Compilation with New Installation Domain ===

echo.
echo Building solution to identify missing components...
dotnet build --no-restore --verbosity minimal

echo.
echo === Compilation Test Complete ===