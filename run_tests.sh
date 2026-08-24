#!/usr/bin/env bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "=== Building ExtremeRoles with Mock ==="
bash "$SCRIPT_DIR/ExtremeRoles.UnitTest/build_with_mock.sh"

echo "=== Running Unit Tests ==="
dotnet test --project "$SCRIPT_DIR/ExtremeRoles.UnitTest/ExtremeRoles.UnitTest.csproj" "$@"
