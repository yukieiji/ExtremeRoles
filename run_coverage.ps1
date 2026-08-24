$ErrorActionPreference = 'Stop'

$ScriptDir = $PSScriptRoot

Write-Host "=== Building ExtremeRoles with Mock ==="
& (Join-Path $ScriptDir "ExtremeRoles.UnitTest\build_with_mock.ps1")

Write-Host "=== Ensuring ReportGenerator is installed ==="
if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
    Write-Host "reportgenerator tool not found. Installing dotnet-reportgenerator-globaltool..."
    try {
        & dotnet tool install --global dotnet-reportgenerator-globaltool
    } catch {
        # Already installed or failed to install
    }
}

Write-Host "=== Running Unit Tests with Coverage ==="
$coverageDir = Join-Path $ScriptDir "TestResults"
if (-not (Test-Path $coverageDir)) {
    New-Item -ItemType Directory -Path $coverageDir | Out-Null
}
$coverageXml = Join-Path $coverageDir "report.cobertura.xml"
$projPath = Join-Path $ScriptDir "ExtremeRoles.UnitTest\ExtremeRoles.UnitTest.csproj"

$testArgs = @(
    "test",
    "--project", $projPath,
    "--",
    "--coverage",
    "--coverage-output-format", "xml",
    "--coverage-output", $coverageXml
) + $args

$testExitCode = 0
try {
    & dotnet $testArgs
    $testExitCode = $LASTEXITCODE
} catch {
    $testExitCode = $LASTEXITCODE
}

if (Test-Path $coverageXml) {
    Write-Host "=== Generating Coverage Report ==="
    $reportDir = Join-Path $ScriptDir "CoverageReport"
    & reportgenerator "-reports:$coverageXml" "-targetdir:$reportDir" "-reporttypes:Html;TextSummary" "-assemblyfilters:+ExtremeRoles*;-ExtremeRoles.UnitTest*"

    Write-Host "=== Coverage Summary ==="
    $summaryPath = Join-Path $reportDir "Summary.txt"
    if (Test-Path $summaryPath) {
        Get-Content $summaryPath
    }
} else {
    Write-Warning "Coverage XML file was not generated."
}

if ($testExitCode -ne 0) {
    exit $testExitCode
}
