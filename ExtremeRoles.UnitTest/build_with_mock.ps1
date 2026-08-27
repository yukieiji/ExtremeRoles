$ErrorActionPreference = 'Stop'

$ScriptDir = $PSScriptRoot
$RepoRoot = (Get-Item (Join-Path $ScriptDir "..")).FullName
$MockDir = Join-Path $ScriptDir ".mockbuilder"
$AssetName = "MockedAmongUs-win-x64.zip"
$ZipPath = Join-Path $MockDir $AssetName
$TargetProj = Join-Path $RepoRoot "ExtremeRoles"
$Config = "Debug"

if (-not (Test-Path $MockDir)) {
    New-Item -ItemType Directory -Force -Path $MockDir | Out-Null
}

$builderExe = Join-Path $MockDir "MockedAmongUs.Builder.exe"
$builderDll = Join-Path $MockDir "MockedAmongUs.Builder.dll"

if (-not (Test-Path $builderExe) -and -not (Test-Path $builderDll)) {
    Write-Host "Mock builder not found in $MockDir. Fetching latest release..."

    $releaseJson = $null
    try {
        $response = Invoke-RestMethod -Uri "https://api.github.com/repos/yukieiji/TestableAmongUsModBuilder/releases/latest" -Method Get -ErrorAction SilentlyContinue
        if ($response.assets) {
            $releaseJson = $response
        }
    } catch {
        # Ignore error on unauthenticated attempt
    }

    if ($null -eq $releaseJson) {
        Write-Host "Fetching release info without token failed. Trying with token..."
        $token = $env:TestableAmongUsAccess
        if ([string]::IsNullOrWhiteSpace($token)) {
            $token = $env:GITHUB_TOKEN
        }
        if ([string]::IsNullOrWhiteSpace($token)) {
            $token = $env:GH_TOKEN
        }
        if ([string]::IsNullOrWhiteSpace($token)) {
            Write-Error "Failed to fetch release info and TestableAmongUsAccess environment variable is not set."
            exit 1
        }
        $headers = @{
            "Authorization" = "token $token"
            "User-Agent"    = "PowerShell"
        }
        $releaseJson = Invoke-RestMethod -Uri "https://api.github.com/repos/yukieiji/TestableAmongUsModBuilder/releases/latest" -Headers $headers -Method Get
    }

    $asset = $releaseJson.assets | Where-Object { $_.name -eq $AssetName } | Select-Object -First 1
    if ($null -eq $asset) {
        Write-Error "Asset $AssetName not found in latest release."
        exit 1
    }

    $downloaded = $false
    if ($asset.browser_download_url) {
        try {
            Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $ZipPath -ErrorAction SilentlyContinue
            if ((Test-Path $ZipPath) -and (Get-Item $ZipPath).Length -gt 1000) {
                $downloaded = $true
            }
        } catch {
            # Ignore error on unauthenticated download attempt
        }
    }

    if (-not $downloaded) {
        Write-Host "Downloading asset without token failed. Retrying with token..."
        $token = $env:TestableAmongUsAccess
        if ([string]::IsNullOrWhiteSpace($token)) {
            $token = $env:GITHUB_TOKEN
        }
        if ([string]::IsNullOrWhiteSpace($token)) {
            $token = $env:GH_TOKEN
        }
        if ([string]::IsNullOrWhiteSpace($token)) {
            Write-Error "Download failed and TestableAmongUsAccess environment variable is not set."
            exit 1
        }
        $headers = @{
            "Authorization" = "token $token"
            "Accept"        = "application/octet-stream"
            "User-Agent"    = "PowerShell"
        }
        Invoke-WebRequest -Uri $asset.url -Headers $headers -OutFile $ZipPath
    }

    Write-Host "Extracting $AssetName..."
    Expand-Archive -Path $ZipPath -DestinationPath $MockDir -Force
} else {
    Write-Host "Mock builder already exists in $MockDir. Skipping download."
}

if (Test-Path $builderExe) {
    Write-Host "Running MockedAmongUs.Builder.exe on $TargetProj ($Config)..."
    & $builderExe $TargetProj $Config
} elseif (Test-Path $builderDll) {
    Write-Host "Running MockedAmongUs.Builder.dll on $TargetProj ($Config)..."
    & dotnet $builderDll $TargetProj $Config
} else {
    Write-Error "MockedAmongUs.Builder executable or DLL not found in extracted files."
    exit 1
}
