$ErrorActionPreference = 'Stop'

$ScriptDir = $PSScriptRoot

Write-Host "=== Building ExtremeRoles with Mock ==="
& (Join-Path $ScriptDir "ExtremeRoles.UnitTest\build_with_mock.ps1")

Write-Host "=== Running Unit Tests with Coverage ==="
$configPath = Join-Path $ScriptDir "ExtremeRoles.UnitTest\coverage.config"
$projPath = Join-Path $ScriptDir "ExtremeRoles.UnitTest\ExtremeRoles.UnitTest.csproj"

$testArgs = @(
    "test",
    "--project", $projPath,
    "--",
    "--coverage",
    "--coverage-settings", $configPath
) + $args

& dotnet $testArgs
