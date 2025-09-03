@echo off
chcp 65001

where uv >nul 2>nul
if %errorlevel% == 0 (
    echo "uv found, using uv to install dependencies."
    uv pip install -r requirements.txt
) else (
    echo "uv not found, using pip to install dependencies."
    pip install -r requirements.txt
)

git submodule update --init --recursive
call run_python.bat makelanguagejson.py
mkdir ExtremeRoles\Resources\Asset
robocopy /mir UnityAsset\ExtremeRoles ExtremeRoles\Resources\Asset
mkdir ExtremeSkins\Resources\Asset
robocopy /mir UnityAsset\ExtremeSkins ExtremeSkins\Resources\Asset