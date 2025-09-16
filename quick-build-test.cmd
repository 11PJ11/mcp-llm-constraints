@echo off
echo === Quick Build Test - ATDD DEVELOP Infrastructure ===

echo.
echo Testing compilation with new Installation domain...
dotnet build --configuration Debug --no-restore --verbosity normal

echo.
echo === Build Status Check Complete ===