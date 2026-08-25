$ErrorActionPreference = 'Stop'

$ScriptDir = $PSScriptRoot

# Check if dotnet-stryker tool is installed globally
$strykerInstalled = dotnet tool list -g | Select-String "dotnet-stryker"
if (-not $strykerInstalled) {
    Write-Host "dotnet-stryker tool not found. Installing globally..."
    dotnet tool install -g dotnet-stryker
}

Write-Host "=== Building ExtremeRoles with Mock ==="
& (Join-Path $ScriptDir "ExtremeRoles.UnitTest\build_with_mock.ps1")

Write-Host "=== Running Mutation Tests with Stryker ==="
& dotnet stryker --open-report html $args
