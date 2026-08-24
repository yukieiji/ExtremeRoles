#!/usr/bin/env bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "=== Building ExtremeRoles with Mock ==="
bash "$SCRIPT_DIR/ExtremeRoles.UnitTest/build_with_mock.sh"

echo "=== Ensuring ReportGenerator is installed ==="
if ! command -v reportgenerator &> /dev/null; then
    echo "reportgenerator tool not found. Installing dotnet-reportgenerator-globaltool..."
    dotnet tool install --global dotnet-reportgenerator-globaltool || true
    export PATH="$HOME/.dotnet/tools:$PATH"
fi

echo "=== Running Unit Tests with Coverage ==="
COVERAGE_DIR="$SCRIPT_DIR/TestResults"
COVERAGE_XML="$COVERAGE_DIR/report.cobertura.xml"
mkdir -p "$COVERAGE_DIR"

set +e
dotnet test --project "$SCRIPT_DIR/ExtremeRoles.UnitTest/ExtremeRoles.UnitTest.csproj" -- \
  --coverage \
  --coverage-output-format xml \
  --coverage-output "$COVERAGE_XML" \
  "$@"
TEST_EXIT_CODE=$?
set -e

if [ -f "$COVERAGE_XML" ]; then
    echo "=== Generating Coverage Report ==="
    REPORT_DIR="$SCRIPT_DIR/CoverageReport"
    reportgenerator \
      -reports:"$COVERAGE_XML" \
      -targetdir:"$REPORT_DIR" \
      -reporttypes:"Html;TextSummary" \
      -assemblyfilters:"+ExtremeRoles*;-ExtremeRoles.UnitTest*"

    echo "=== Coverage Summary ==="
    if [ -f "$REPORT_DIR/Summary.txt" ]; then
        cat "$REPORT_DIR/Summary.txt"
    fi
else
    echo "Warning: Coverage XML file was not generated."
fi

exit $TEST_EXIT_CODE
