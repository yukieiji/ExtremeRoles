@echo off
where uv >nul 2>nul
if %errorlevel% == 0 (
    uv run %*
) else (
    python %*
)
