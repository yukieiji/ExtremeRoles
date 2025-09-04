import sys
import os
import re
from lxml import etree as ET
from dataclasses import dataclass, field
from typing import Set, List, Optional, Tuple

@dataclass
class ParsedOptionsData:
    """Holds the sets of defined and implemented options for a role."""
    defined: Set[str] = field(default_factory=set)
    implemented: Set[str] = field(default_factory=set)

@dataclass
class ClassParseResult:
    """Holds the result of parsing a class definition from a file's content."""
    class_name: str
    class_body: str

@dataclass
class ParsedRoleData:
    """Holds all the parsed information about a role, including its location."""
    class_name: str
    file_path: str
    options: ParsedOptionsData

def find_and_parse_role_in_project(role_name: str) -> Optional[ParsedRoleData]:
    """Scans all C# files in the project to find and parse a specific role class.

    Args:
        role_name: The English name of the role class to find (case-insensitive).

    Returns:
        A ParsedRoleData object containing the role's information if found,
        otherwise None.
    """
    for root, _, files in os.walk("ExtremeRoles/Roles"):
        for file in files:
            if not file.endswith(".cs"):
                continue

            file_path = os.path.join(root, file)
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()

            class_parse_result = parse_class_from_content(content, role_name)

            if class_parse_result:
                options_data = parse_options_from_class_body(class_parse_result.class_body, class_parse_result.class_name)
                return ParsedRoleData(
                    class_name=class_parse_result.class_name,
                    file_path=file_path,
                    options=options_data
                )
    return None

def parse_class_from_content(content: str, role_name: str) -> Optional[ClassParseResult]:
    """Parses C# file content to find a class and extract its body.

    Args:
        content: The string content of the C# file.
        role_name: The name of the role class to find.

    Returns:
        A ClassParseResult object if the class and its body are found,
        otherwise None.
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
        return None # Return None if body parsing fails

    return None

def parse_options_from_class_body(class_body: str, class_name: str) -> ParsedOptionsData:
    """Parses the string content of a class body to find defined and implemented options.

    Args:
        class_body: The string content of the C# class.
        class_name: The name of the class being parsed.

    Returns:
        A ParsedOptionsData object containing two sets: one for options
        defined in the enum, and one for options implemented in the factory.
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

def generate_translation_keys(class_name: str, option_names: set[str]) -> List[str]:
    """Generates a list of standard translation keys for a role."""
    keys = [class_name, f"{class_name}FullDescription", f"{class_name}ShortDescription", f"{class_name}IntroDescription"]
    for option in option_names:
        keys.append(f"{class_name}{option}")
    return keys

def update_resx_file(file_path: str, keys_to_add: List[str]) -> int:
    """Adds new translation keys to a .resx file using a text-based approach."""
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
    """The main entry point of the script."""
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
