#!/usr/bin/env bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "=== Building ExtremeRoles with Mock ==="
bash "$SCRIPT_DIR/ExtremeRoles.UnitTest/build_with_mock.sh"

echo "=== Running Unit Tests with Coverage ==="
COVERAGE_CONFIG="$SCRIPT_DIR/ExtremeRoles.UnitTest/coverage.config"
PROJ_PATH="$SCRIPT_DIR/ExtremeRoles.UnitTest/ExtremeRoles.UnitTest.csproj"

dotnet test --project "$PROJ_PATH" -- \
  --coverage \
  --coverage-output-format cobertura \
  --coverage-output coverage.cobertura.xml \
  --coverage-settings "$COVERAGE_CONFIG" \
  "$@"
