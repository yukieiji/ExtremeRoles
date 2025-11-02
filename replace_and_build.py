import os
import subprocess
from typing import List, Tuple

TARGET_ROLE_DIR = "ExtremeRoles/Roles/"
TARGET_GHOST_DIR = "ExtremeRoles/GhostRoles/"
SLN_FILE = "ExtremeRoles.sln"
REPLACE_MAP = {
    ".CreateBool": ".CreateNewBool",
    ".CreateFloat": ".CreateNewFloat",
    ".CreateInt": ".CreateNewInt",
}

def force_read(filepath: str) -> str:
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            original_content = f.read()
    except Exception as e:
        try:
            with open(filepath, 'r', encoding='utf-8-bom') as f:
                original_content = f.read()
            with open(filepath, "w", encoding='utf-8') as f:
                f.write(original_content)
            return original_content
        except Exception as e:
            try:
                with open(filepath, 'r', encoding='shift-jis') as f:
                    original_content = f.read()
                with open(filepath, "w", encoding='utf-8') as f:
                    f.write(original_content)
                return original_content
            except Exception as e:
                print(f"Could not read file {filepath}: {e}")
                return ""

def find_cs_files(directory: str) -> List[str]:
    """指定されたディレクトリ内のすべての.csファイルを再帰的に検索する"""
    cs_files = []
    for root, _, files in os.walk(directory):
        for file in files:
            if file.endswith(".cs"):
                cs_files.append(os.path.join(root, file))
    return cs_files

def find_occurrences(filepath: str, patterns: List[str]) -> List[Tuple[int, int, str]]:
    """ファイル内の指定されたパターンの出現箇所（行番号、列番号、パターン）を検索する"""
    occurrences = []
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            for line_num, line in enumerate(f, 1):
                for pattern in patterns:
                    col = 0
                    while True:
                        col = line.find(pattern, col)
                        if col == -1:
                            break
                        occurrences.append((line_num, col, pattern))
                        col += len(pattern)
    except Exception as e:
        print(f"Error reading file {filepath}: {e}")
    return occurrences

def run_build() -> bool:
    """ビルドを実行し、成功したかどうかを返す"""
    print("--- Running build ---")
    try:
        result = subprocess.run(
            ["msbuild", SLN_FILE, "-nologo", "/t:Rebuild"],
            capture_output=True,
            text=True,
            check=True,
            encoding='utf-8'
        )
        # stdoutにビルド警告が含まれる場合があるため、エラー出力がないことをもって成功とみなす
        if result.stderr:
            print("Build failed with warnings/errors:")
            print(result.stderr)
            return False
        print("Build successful!")
        return True
    except subprocess.CalledProcessError as e:
        print("Build failed!")
        print(e.stdout)
        print(e.stderr)
        return False
    except FileNotFoundError:
        print("Error: 'dotnet' command not found. Is .NET SDK installed and in your PATH?")
        return False
    except Exception as e:
        print(f"An unexpected error occurred during build: {e}")
        return False

def main():
    """メイン処理"""
    cs_files = find_cs_files(TARGET_ROLE_DIR)
    cs_files += find_cs_files(TARGET_GHOST_DIR)
    patterns = list(REPLACE_MAP.keys())

    total_changes_attempted = 0
    total_successful_changes = 0

    for filepath in cs_files:
        print(f"\\n--- Processing file: {filepath} ---")

        try:
            with open(filepath, 'r', encoding='utf-8') as f:
                original_content = f.read()
        except Exception as e:
            try:
                with open(filepath, 'r', encoding='utf-8-bom') as f:
                    original_content = f.read()
            except Exception as e:
                try:
                    with open(filepath, 'r', encoding='shift-jis') as f:
                        original_content = f.read()
                except Exception as e:
                    print(f"Could not read file {filepath}: {e}")
                    continue
            

        # 変更が成功するたびにその状態を保存する
        last_successful_content = original_content

        # ファイルの末尾から置換を試すことで、前方への位置のずれを防ぐ
        occurrences = sorted(find_occurrences(filepath, patterns), key=lambda x: (x[0], x[1]), reverse=True)

        if not occurrences:
            print("No occurrences found, skipping.")
            continue

        for line_num, col, pattern in occurrences:
            total_changes_attempted += 1
            print(f"Attempting to replace '{pattern}' at {filepath}:{line_num}:{col+1}")

            # 現在の成功している状態から置換を試みる
            current_lines = last_successful_content.splitlines(True)

            line_idx = line_num - 1
            if line_idx >= len(current_lines):
                 print(f"  -> Skipping: Line number {line_num} is out of bounds.")
                 continue

            line_content = current_lines[line_idx]

            # パターンが期待される場所にあるか最終確認
            if line_content[col:col+len(pattern)] != pattern:
                print(f"  -> Skipping: Pattern '{pattern}' no longer found at the expected location.")
                continue

            # 置換を実行
            replacement = REPLACE_MAP[pattern]
            new_line_content = line_content[:col] + replacement + line_content[col+len(pattern):]
            current_lines[line_idx] = new_line_content

            # ファイルに変更を書き込む
            try:
                with open(filepath, 'w', encoding='utf-8') as f:
                    f.writelines(current_lines)
            except Exception as e:
                print(f"Could not write to file {filepath}: {e}")
                continue

            # ビルドを実行
            if run_build():
                print(f"  -> SUCCESS: Build passed. Keeping the change.")
                total_successful_changes += 1
                # 成功したので、現在の内容を「最後に成功した状態」として更新
                last_successful_content = "".join(current_lines)
            else:
                print(f"  -> FAILURE: Build failed. Reverting the change for this attempt.")
                # 失敗したので、ファイルの内容を「最後に成功した状態」に戻す
                try:
                    with open(filepath, 'w', encoding='utf-8') as f:
                        f.write(last_successful_content)
                except Exception as e:
                     print(f"FATAL: Could not revert file {filepath}: {e}")
                     # 復元に失敗した場合は、このファイルの処理を中断
                     break

        # ファイルごとの最終的な変更をオリジナルと比較して表示
        if original_content != last_successful_content:
            print(f"File {filepath} was modified.")
        else:
            print(f"File {filepath} has no successful changes.")


    print("\\n--- Script finished ---")
    print(f"Total changes attempted: {total_changes_attempted}")
    print(f"Successful changes kept: {total_successful_changes}")

if __name__ == "__main__":
    main()
