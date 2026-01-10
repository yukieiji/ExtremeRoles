import os
import re
import logging
import sys

# ロギングの設定
logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(message)s')

def find_balanced_parens_content(text: str, start_pos: int) -> tuple[str, int]:
    """
    指定された開始位置（通常は'('の直後）から、対応する閉じ括弧までの内容を抽出します。
    """
    if start_pos >= len(text) or text[start_pos] != '(':
        return None, -1

    content_start = start_pos + 1
    balance = 1
    i = content_start
    while i < len(text):
        if text[i] == '(':
            balance += 1
        elif text[i] == ')':
            balance -= 1
        
        if balance == 0:
            return text[content_start:i], i + 1
        i += 1
    return None, -1 # マッチする閉じ括弧が見つからなかった場合

def parse_arguments(args_str: str) -> list[str]:
    """
    ネストされた括弧を考慮して、引数文字列をリストに分割します。
    """
    args = []
    balance = 0
    current_arg = ""
    for char in args_str:
        if char == ',' and balance == 0:
            args.append(current_arg.strip())
            current_arg = ""
        else:
            if char == '(':
                balance += 1
            elif char == ')':
                balance -= 1
            current_arg += char
    if current_arg:
        args.append(current_arg.strip())
    return [arg for arg in args if arg]

def refactor_constructor_calls(content: str) -> str:
    """
    ファイルのコンテンツを受け取り、対象のコンストラクタ呼び出しをリファクタリングします。
    """
    new_content = content
    # : base(...) を見つけるための正規表現
    # SingleRoleBase と MultiAssignRoleBase の両方を対象とする
    pattern = re.compile(r":\s*base\s*\(", re.MULTILINE)

    # --- 設定: 許容する改行数の上限 ---
    MAX_NEWLINES = 20 
    # -------------------------------

    # 複数回置換に対応するため、オフセットを管理
    offset = 0
    for match in pattern.finditer(content):

        start_pos = match.start()

        # base( の後の引数部分を抽出

        # 1. : base より前の文字列を取得
        before_match = content[:start_pos]
        
        # 2. 直前の非空白文字（通常はコンストラクタ引数の ')'）の位置を探す
        last_code_match = re.search(r'\S', before_match[::-1])
        
        if last_code_match:
            # 非空白文字から : base までの間の文字列を抽出
            last_code_pos = len(before_match) - last_code_match.start()
            gap_text = before_match[last_code_pos:start_pos]
            
            # 3. その間にある改行数をカウント
            newline_count = gap_text.count('\n')
            
            if newline_count > MAX_NEWLINES:
                logging.info(f"Skipping: Newlines ({newline_count}) exceeded limit at pos {start_pos}")
                continue

        args_content, end_pos = find_balanced_parens_content(content, match.end() - 1)
        if args_content is None:
            logging.warning(f"Could not find matching parenthesis for 'base' call at position {match.start()}. Skipping.")
            continue

        original_base_call = content[match.start():end_pos]
        
        # 引数をパース
        old_args = parse_arguments(args_content)
        if not old_args:
            continue
        
        # 引数を分類
        role_core_arg = old_args[0]
        positional_bools = []
        named_args = {}
        for arg in old_args[1:]:
            if ':' in arg:
                key, value = map(str.strip, arg.split(':', 1))
                named_args[key] = value
            else:
                positional_bools.append(arg)
        
        # 9個のブール引数を構築
        def get_arg_value(pos, name, default):
            if len(positional_bools) > pos:
                return positional_bools[pos]
            return named_args.get(name, default)

        new_bool_values = [
            get_arg_value(0, 'canKill', 'false'),
            get_arg_value(1, 'hasTask', 'false'),
            get_arg_value(2, 'useVent', 'false'),
            get_arg_value(3, 'useSabotage', 'false'),
            get_arg_value(4, 'canCallMeeting', 'true'),
            get_arg_value(5, 'canRepairSabotage', 'true'),
            get_arg_value(6, 'canUseAdmin', 'true'),
            get_arg_value(7, 'canUseSecurity', 'true'),
            get_arg_value(8, 'canUseVital', 'true'),
        ]

        # 変更が必要かチェック（単純化のため、引数の数が10未満なら常に書き換える）
        # `role_core` と `tab` などを除いた引数の数を比較
        current_bool_arg_count = len(positional_bools) + len([k for k in named_args if not k == 'tab'])
        if current_bool_arg_count >= 9:
             continue # すでに引数が9個以上あれば、処理済みとみなす

        # 新しい引数リストを作成
        final_args_list = [role_core_arg] + new_bool_values
        
        # `tab` のような他の名前付き引数を維持
        other_named_args = {k: v for k, v in named_args.items() if k not in 
                            ['canKill', 'hasTask', 'useVent', 'useSabotage', 'canCallMeeting', 
                             'canRepairSabotage', 'canUseAdmin', 'canUseSecurity', 'canUseVital']}
        for key, value in other_named_args.items():
            final_args_list.append(f"{key}: {value}")

        # 元のインデントを維持しようと試みる
        match_line_start = content.rfind('\n', 0, match.start()) + 1
        indent_str = content[match_line_start:match.start()]
        indent = " " * len(indent_str.replace('\t', ' ' * 4)) # タブをスペースに変換して長さを計算
        
        # 新しいbase呼び出し文字列を構築
        # フォーマットを維持するのは複雑なので、可読性の高い1行または複数行のフォーマットに統一する
        formatted_args = f"\n{indent}    {final_args_list[0]}"
        
        # ブール引数を4つずつ改行
        bool_args_str = []
        for i, arg in enumerate(final_args_list[1:10]):
            bool_args_str.append(arg)
        
        formatted_args += ",\n"
        
        line_indent = indent + "    "
        for i in range(0, len(bool_args_str), 4):
            line = bool_args_str[i:i+4]
            formatted_args += line_indent + ", ".join(line)
            if i + 4 < len(bool_args_str):
                 formatted_args += ",\n"

        # 他の名前付き引数を追加
        if other_named_args:
            formatted_args += ",\n"
            for i, (key, value) in enumerate(other_named_args.items()):
                 formatted_args += f"{line_indent}{key}: {value}"
                 if i < len(other_named_args) - 1:
                      formatted_args += ",\n"

        new_base_call = f": base({formatted_args}\n{indent})"
        
        # コンテンツを置換
        start_replace = match.start() + offset
        end_replace = end_pos + offset
        new_content = new_content[:start_replace] + new_base_call + new_content[end_replace:]
        
        # オフセットを更新
        offset += len(new_base_call) - len(original_base_call)

    return new_content


def main(filepaths: list[str]):
    """
    指定されたファイルリストに対して、コンストラクタ呼び出しをリファクタリングします。
    """
    refactored_files = []
    for filepath in filepaths:
        if not os.path.exists(filepath):
            logging.warning(f"File not found: {filepath}. Skipping.")
            continue
        try:
            with open(filepath, 'r', encoding='utf-8') as f:
                original_content = f.read()
            
            new_content = refactor_constructor_calls(original_content)

            if new_content != original_content:
                with open(filepath, 'w', encoding='utf-8') as f:
                    f.write(new_content)
                refactored_files.append(filepath)
        except Exception as e:
            logging.error(f"Error processing file {filepath}: {e}")
    
    if refactored_files:
        logging.info("Refactoring complete. Modified files:")
        for path in refactored_files:
            print(f"- {path}")
    else:
        logging.info("No files in the provided list needed refactoring.")


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python refactor_constructors.py <file1.cs> <file2.cs> ...")
        sys.exit(1)
    
    # スクリプト名を除いた引数をファイルパスとして渡す
    main(sys.argv[1:])
