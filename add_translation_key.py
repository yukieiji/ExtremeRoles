import sys
import os
import re
from lxml import etree as ET
from dataclasses import dataclass, field

@dataclass
class ParsedOptionsData:
    """ロールに定義され、実装されたオプションのセットを保持します。"""
    defined: set[str] = field(default_factory=set)
    implemented: set[str] = field(default_factory=set)

@dataclass
class ClassParseResult:
    """ファイルの内容からクラス定義を解析した結果を保持します。"""
    class_name: str
    class_body: str

@dataclass
class ParsedRoleData:
    """解析されたロールのすべての情報（場所を含む）を保持します。"""
    class_name: str
    file_path: str
    options: ParsedOptionsData

def find_and_parse_role_in_project(role_name: str) -> ParsedRoleData | None:
    """プロジェクト内のすべてのC#ファイルをスキャンして、特定のロールクラスを見つけて解析します。

    Args:
        role_name: 検索するロールクラスの英語名（大文字と小文字を区別しない）。

    Returns:
        ロールの情報が見つかった場合はParsedRoleDataオブジェクト、それ以外の場合はNone。
    """
    for root, _, files in os.walk("ExtremeRoles/Roles"):
        for file in files:
            if not file.endswith(".cs"):
                continue

            file_path = os.path.join(root, file)
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()

            class_parse_result = parse_class_from_content(content, role_name)

            if class_parse_result and class_parse_result.class_body:
                options_data = parse_options_from_class_body(class_parse_result.class_body, class_parse_result.class_name)
                return ParsedRoleData(
                    class_name=class_parse_result.class_name,
                    file_path=file_path,
                    options=options_data
                )
    return None

def parse_class_from_content(content: str, role_name: str) -> ClassParseResult | None:
    """C#ファイルの内容を解析してクラスを見つけ、その本体を抽出します。

    Args:
        content: C#ファイルの文字列の内容。
        role_name: 検索するロールクラスの名前。

    Returns:
        クラスとその本体が見つかった場合はClassParseResultオブジェクト、それ以外の場合はNone。
    """
    class_match = re.search(r'public (?:sealed )?class\s+(' + re.escape(role_name) + r'|' + re.escape(role_name) + r'Role)\b', content)
    if not class_match:
        return None

    actual_class_name = class_match.group(1)

    try:
        class_start_index = class_match.end()
        brace_start_index = content.find('{', class_start_index)
        if brace_start_index != -1:
            brace_level = 1
            scan_index = brace_start_index + 1
            while scan_index < len(content) and brace_level > 0:
                if content[scan_index] == '{': brace_level += 1
                elif content[scan_index] == '}': brace_level -= 1
                scan_index += 1
            class_body_content = content[brace_start_index + 1 : scan_index - 1]
            return ClassParseResult(class_name=actual_class_name, class_body=class_body_content)
    except Exception:
        return None

    return None

def parse_options_from_class_body(class_body: str, class_name: str) -> ParsedOptionsData:
    """クラス本体の文字列の内容を解析して、定義済みおよび実装済みのオプションを見つけます。

    Args:
        class_body: C#クラスの文字列の内容。
        class_name: 解析対象のクラスの名前。

    Returns:
        enumで定義されたオプションとファクトリで実装されたオプションの2つのセットを含むParsedOptionsDataオブジェクト。
    """
    defined_options: set[str] = set()
    options_match = re.search(r'public enum (?:' + class_name + r'Option|Option)\s*{([^}]+)}', class_body, re.DOTALL)
    if options_match:
        options_block = options_match.group(1)
        defined_options = set(re.findall(r'\b(\w+)\b', options_block))

    implemented_options: set[str] = set()
    method_match = re.search(r'protected(?: override)? void CreateSpecificOption\([^)]*\)\s*{([^}]+)}', class_body, re.DOTALL)
    if method_match:
        method_block = method_match.group(1)
        enum_type_name_1 = f"{class_name}Option"
        enum_type_name_2 = "Option"
        implemented_options_1 = set(re.findall(rf'\b{enum_type_name_1}\.(\w+)\b', method_block))
        implemented_options_2 = set(re.findall(rf'\b{enum_type_name_2}\.(\w+)\b', method_block))
        implemented_options = implemented_options_1.union(implemented_options_2)

    return ParsedOptionsData(defined=defined_options, implemented=implemented_options)

def generate_translation_keys(class_name: str, option_names: set[str]) -> list[str]:
    """ロールの標準的な翻訳キーのリストを生成します。

    Args:
        class_name: ロールクラスの名前。
        option_names: オプション名のセット。

    Returns:
        生成された翻訳キーのリスト。
    """
    keys = [class_name, f"{class_name}FullDescription", f"{class_name}ShortDescription", f"{class_name}IntroDescription"]
    for option in option_names:
        keys.append(f"{class_name}{option}")
    return keys

def update_resx_file(file_path: str, keys_to_add: list[str]) -> int:
    """テキストベースのアプローチを使用して、新しい翻訳キーを.resxファイルに追加します。

    Args:
        file_path: .resxファイルのパス。
        keys_to_add: 追加するキーのリスト。

    Returns:
        追加された新しいキーの数。
    """
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    parser = ET.XMLParser(recover=True)
    root = ET.fromstring(content.encode('utf-8'), parser=parser)
    existing_keys: set[str | None] = {data.get('name') for data in root.xpath("//*[local-name()='data']")}

    new_keys_to_insert = []
    for key in keys_to_add:
        if key not in existing_keys:
            new_data_string = f"""  <data name="{key}" xml:space="preserve">
    <value>STRMISS</value>
  </data>"""
            new_keys_to_insert.append(new_data_string)
            print(f"  新しいキーを追加中: {key}")

    if not new_keys_to_insert:
        return 0

    closing_root_tag_index = content.rfind("</root>")
    if closing_root_tag_index == -1: return 0

    new_content = content[:closing_root_tag_index] + "\n".join(new_keys_to_insert) + "\n" + content[closing_root_tag_index:]
    content = new_content

    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)

    return len(new_keys_to_insert)

def main() -> None:
    """スクリプトのメインエントリポイント。"""
    if len(sys.argv) != 2:
        print("使い方: python add_translation_key.py <役職名>")
        sys.exit(1)

    role_name = sys.argv[1]
    print(f"プロジェクト全体で役職を検索中: {role_name}...")

    parsed_data = find_and_parse_role_in_project(role_name)

    if not parsed_data:
        print(f"エラー: 役職 '{role_name}' のクラス定義が見つかりませんでした。")
        sys.exit(1)

    print(f"役職クラス '{parsed_data.class_name}' をファイル内で発見: {parsed_data.file_path}")
    print(f"定義済みのオプション {len(parsed_data.options.defined)}個、実装済みのオプション {len(parsed_data.options.implemented)}個を発見しました。")

    unimplemented = parsed_data.options.defined - parsed_data.options.implemented
    if unimplemented:
        print("\nエラー: 定義と実装の間に矛盾が発見されました。", file=sys.stderr)
        print("以下のオプションはenumで定義されていますが、CreateSpecificOptionで実装されていません:", file=sys.stderr)
        for opt in sorted(list(unimplemented)):
            print(f"- {opt}", file=sys.stderr)
        sys.exit(1)

    keys = generate_translation_keys(parsed_data.class_name, parsed_data.options.implemented)

    path_parts = parsed_data.file_path.split(os.sep)
    team_name = "Unknown"
    if 'Roles' in path_parts:
        roles_index = path_parts.index('Roles')
        if len(path_parts) > roles_index + 2:
            team_name = path_parts[roles_index + 2]
    if "GhostRoles" in path_parts: team_name = "Ghost" + team_name
    if "Combination" in path_parts: team_name = "Combination"

    default_resx_path = os.path.join("ExtremeRoles/Translation/resx", f"{team_name}.resx")
    print(f"対象の翻訳ファイル: {default_resx_path}")

    if not os.path.exists(default_resx_path):
        print(f"'{default_resx_path}' が見つかりません。新しい空のファイルを作成します。")
        new_file_content = """<?xml version="1.0" encoding="utf-8"?>\n<root>\n</root>"""
        with open(default_resx_path, 'w', encoding='utf-8') as f:
            f.write(new_file_content)
        print(f"新しい空の翻訳ファイルを作成しました: {default_resx_path}")

    added_count = update_resx_file(default_resx_path, keys)

    if added_count > 0:
        print(f"\n{added_count}個の新しい翻訳キーを '{default_resx_path}' に正常に追加しました。")
    else:
        print(f"\n追加する新しいキーはありません。'{default_resx_path}' は役職 '{parsed_data.class_name}' に対して既に最新の状態です。")

if __name__ == "__main__":
    main()
