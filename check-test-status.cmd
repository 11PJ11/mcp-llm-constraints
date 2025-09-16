@echo off
echo === ATDD DEVELOP - Checking Current E2E Test Status ===

echo.
echo Running E2E tests to understand current implementation state...
dotnet test --filter "Category=E2E" --verbosity minimal

echo.
echo === Analysis Complete ===