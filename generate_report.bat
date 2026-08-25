@echo off
setlocal enabledelayedexpansion

:: ドラッグ＆ドロップされたファイルの存在チェック
if "%~1"=="" (
    echo [ERROR] cobertura.xml ファイルをこのバッチファイルにドラッグ＆ドロップしてください。
    pause
    exit /b
)

:: dotnet tool "ReportGenerator" の存在確認（無ければ自動インストール）
dotnet tool list -g | findstr /I "dotnet-reportgenerator-globaltool" > nul
if %errorlevel% neq 0 (
    echo ReportGenerator が見つかりません。グローバルツールとしてインストールします...
    dotnet tool install -g dotnet-reportgenerator-globaltool
)

:: ドロップされたファイルの情報を取得
set "INPUT_FILE=%~1"
set "OUTPUT_DIR=%~dp1CoverageReport"

echo --------------------------------------------------
echo 対象ファイル: !INPUT_FILE!
echo 出力先フォルダ: !OUTPUT_DIR!
echo --------------------------------------------------

:: HTMLレポートの生成
echo レポートを生成中...
reportgenerator -reports:"!INPUT_FILE!" -targetdir:"!OUTPUT_DIR!" -reporttypes:Html

if %errorlevel% equ 0 (
    echo.
    echo [成功] レポートの生成が完了しました！
    echo ブラウザでレポートを開きます...
    start "" "!OUTPUT_DIR!\index.html"
) else (
    echo.
    echo [エラー] レポートの生成に失敗しました。
)

pause