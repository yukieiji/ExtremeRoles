$ErrorActionPreference = 'Stop'

$ScriptDir = $PSScriptRoot

Write-Host "=== Building ExtremeRoles with Mock ==="
& (Join-Path $ScriptDir "ExtremeRoles.UnitTest\build_with_mock.ps1")

Write-Host "=== Running Unit Tests ==="
$projPath = Join-Path $ScriptDir "ExtremeRoles.UnitTest\ExtremeRoles.UnitTest.csproj"
& dotnet test --project $projPath $args
