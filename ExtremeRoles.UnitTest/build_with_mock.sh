#!/usr/bin/env bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
MOCK_DIR="$SCRIPT_DIR/.mockbuilder"
ASSET_NAME="MockedAmongUs-linux-x64.zip"
ZIP_PATH="$MOCK_DIR/$ASSET_NAME"
TARGET_PROJ="$REPO_ROOT/ExtremeRoles"
CONFIG="Debug"

mkdir -p "$MOCK_DIR"

BUILDER_EXEC=""
if [ -f "$MOCK_DIR/MockedAmongUs.Builder" ]; then
    BUILDER_EXEC="$MOCK_DIR/MockedAmongUs.Builder"
elif [ -f "$MOCK_DIR/MockedAmongUs.Builder.dll" ]; then
    BUILDER_EXEC="$MOCK_DIR/MockedAmongUs.Builder.dll"
fi

if [ -z "$BUILDER_EXEC" ]; then
    echo "Mock builder not found in $MOCK_DIR. Fetching latest release..."

    RELEASE_JSON=""
    # Try without token first
    RELEASE_JSON=$(curl -sSL "https://api.github.com/repos/yukieiji/TestableAmongUsModBuilder/releases/latest" 2>/dev/null || true)

    # Check if response contains valid asset info
    if ! echo "$RELEASE_JSON" | grep -q "$ASSET_NAME"; then
        echo "Fetching release info without token failed. Trying with token..."
        TOKEN="${TestableAmongUsAccess:-${GITHUB_TOKEN:-${GH_TOKEN}}}"
        if [ -n "$TOKEN" ]; then
            RELEASE_JSON=$(curl -sSL -H "Authorization: token $TOKEN" "https://api.github.com/repos/yukieiji/TestableAmongUsModBuilder/releases/latest")
        else
            echo "Error: Failed to fetch release info and token environment variable is not set." >&2
            exit 1
        fi
    fi

    # Extract asset API url and browser download url using python3
    ASSET_INFO=$(python3 -c "
import sys, json
data = json.loads(sys.stdin.read())
for asset in data.get('assets', []):
    if asset.get('name') == '$ASSET_NAME':
        print(asset.get('url', '') + '|' + asset.get('browser_download_url', ''))
        sys.exit(0)
sys.exit(1)
" <<< "$RELEASE_JSON" || true)

    if [ -z "$ASSET_INFO" ]; then
        echo "Error: Asset $ASSET_NAME not found in latest release." >&2
        exit 1
    fi

    ASSET_API_URL=$(echo "$ASSET_INFO" | cut -d'|' -f1)
    ASSET_BROWSER_URL=$(echo "$ASSET_INFO" | cut -d'|' -f2)

    # Try downloading without token first
    DOWNLOAD_SUCCESS=0
    if [ -n "$ASSET_BROWSER_URL" ]; then
        curl -sSL "$ASSET_BROWSER_URL" -o "$ZIP_PATH" 2>/dev/null || true
        if [ -f "$ZIP_PATH" ] && unzip -t "$ZIP_PATH" >/dev/null 2>&1; then
            DOWNLOAD_SUCCESS=1
        fi
    fi

    if [ "$DOWNLOAD_SUCCESS" -ne 1 ]; then
        echo "Downloading asset without token failed. Retrying with token..."
        TOKEN="${TestableAmongUsAccess:-${GITHUB_TOKEN:-${GH_TOKEN}}}"
        if [ -n "$TOKEN" ]; then
            curl -sSL -H "Authorization: token $TOKEN" -H "Accept: application/octet-stream" "$ASSET_API_URL" -o "$ZIP_PATH"
        else
            echo "Error: Download failed and token environment variable is not set." >&2
            exit 1
        fi
    fi

    # Verify zip
    if ! unzip -t "$ZIP_PATH" >/dev/null 2>&1; then
        echo "Error: Downloaded zip file is corrupted or invalid." >&2
        exit 1
    fi

    # Extract zip
    unzip -o -q "$ZIP_PATH" -d "$MOCK_DIR"

    # Ensure binary is executable
    if [ -f "$MOCK_DIR/MockedAmongUs.Builder" ]; then
        chmod +x "$MOCK_DIR/MockedAmongUs.Builder"
        BUILDER_EXEC="$MOCK_DIR/MockedAmongUs.Builder"
    elif [ -f "$MOCK_DIR/MockedAmongUs.Builder.dll" ]; then
        BUILDER_EXEC="$MOCK_DIR/MockedAmongUs.Builder.dll"
    fi

    if [ -f "$MOCK_DIR/bin/DllStripper" ]; then
        chmod +x "$MOCK_DIR/bin/DllStripper"
    fi
else
    echo "Mock builder already exists in $MOCK_DIR. Skipping download."
fi

# Run MockedAmongUs.Builder
echo "Running MockedAmongUs.Builder on $TARGET_PROJ ($CONFIG)..."
if [ "$BUILDER_EXEC" = "$MOCK_DIR/MockedAmongUs.Builder" ]; then
    "$BUILDER_EXEC" "$TARGET_PROJ" "$CONFIG"
elif [ -n "$BUILDER_EXEC" ]; then
    dotnet "$BUILDER_EXEC" "$TARGET_PROJ" "$CONFIG"
else
    echo "Error: MockedAmongUs.Builder executable or DLL not found in extracted files." >&2
    exit 1
fi
