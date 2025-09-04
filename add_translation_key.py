import sys
import os
import re
import shutil
import xml.etree.ElementTree as ET

def find_role_file(role_name: str) -> str | None:
    """Finds the C# file for a given role name in the project directory.

    Args:
        role_name: The English name of the role to find (case-insensitive).

    Returns:
        The full path to the C# file if found, otherwise None.
    """
    role_file_name = f"{role_name}.cs"
    for root, _, files in os.walk("ExtremeRoles/Roles"):
        for file in files:
            if file.lower() == role_file_name.lower():
                return os.path.join(root, file)
    return None

def parse_cs_file(file_path: str) -> tuple[str | None, set[str], set[str]]:
    """Parses a C# role file to extract key information.

    This function reads a C# file and uses regular expressions to find the
    class name, the set of options defined in the role's enum, and the set
    of options actually implemented in the CreateSpecificOption factory method.

    Args:
        file_path: The path to the C# role file.

    Returns:
        A tuple containing:
            - The class name as a string.
            - A set of option names defined in the enum.
            - A set of option names implemented in the factory method.
        Returns (None, set(), set()) if the class name cannot be parsed.
    """
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    class_name_match = re.search(r'public (?:sealed )?class (\w+)', content)
    if not class_name_match:
        return None, set(), set()

    class_name = class_name_match.group(1)

    defined_options: set[str] = set()
    options_match = re.search(r'public enum \w+Option\s*{([^}]+)}', content, re.DOTALL)
    if options_match:
        options_block = options_match.group(1)
        defined_options = set(re.findall(r'\b(\w+)\b', options_block))

    implemented_options: set[str] = set()
    method_match = re.search(r'protected override void CreateSpecificOption\([^)]*\)\s*{([^}]+)}', content, re.DOTALL)
    if method_match:
        method_block = method_match.group(1)
        enum_type_name = f"{class_name}Option"
        implemented_options = set(re.findall(rf'\b{enum_type_name}\.(\w+)\b', method_block))

    return class_name, defined_options, implemented_options


def generate_translation_keys(class_name: str, option_names: set[str]) -> list[str]:
    """Generates a list of standard translation keys for a role.

    Args:
        class_name: The name of the role's class.
        option_names: A set of the role's implemented option names.

    Returns:
        A list of strings, where each string is a translation key.
    """
    keys = [
        class_name,
        f"{class_name}FullDescription",
        f"{class_name}ShortDescription",
        f"{class_name}IntroDescription"
    ]
    for option in option_names:
        keys.append(f"{class_name}{option}")
    return keys

def update_resx_file(file_path: str, keys_to_add: list[str]) -> int:
    """Adds new translation keys to a .resx file.

    Parses the given .resx file, checks for the existence of each key
    in keys_to_add, and adds any missing keys with the value "STRMISS".

    Args:
        file_path: The path to the .resx file to update.
        keys_to_add: A list of translation keys to add if they don't exist.

    Returns:
        The number of new keys that were added to the file.
    """
    try:
        ET.register_namespace('', 'http://www.w3.org/2001/XMLSchema')
        ET.register_namespace('xml', 'http://www.w3.org/XML/1998/namespace')
    except AttributeError:
        pass

    tree = ET.parse(file_path)
    root = tree.getroot()
    existing_keys: set[str | None] = {data.get('name') for data in root.findall('data')}

    new_keys_count = 0
    for key in keys_to_add:
        if key not in existing_keys:
            data_elem = ET.Element('data')
            data_elem.set('name', key)
            data_elem.set('{http://www.w3.org/XML/1998/namespace}space', 'preserve')
            value_elem = ET.Element('value')
            value_elem.text = "STRMISS"
            data_elem.append(value_elem)
            root.append(data_elem)
            new_keys_count += 1
            print(f"  新しいキーを追加中: {key}")

    if new_keys_count > 0:
        tree.write(file_path, encoding='utf-8', xml_declaration=True)
    return new_keys_count

def main() -> None:
    """The main entry point of the script."""
    if len(sys.argv) != 2:
        print("使い方: python add_translation_key.py <役職名>")
        sys.exit(1)

    role_name = sys.argv[1]

    print(f"役職を検索中: {role_name}...")
    cs_file_path = find_role_file(role_name)

    if not cs_file_path:
        print(f"エラー: 役職 '{role_name}' のC#ファイルが見つかりませんでした。")
        sys.exit(1)

    print(f"役職ファイルを発見: {cs_file_path}")

    class_name, defined_options, implemented_options = parse_cs_file(cs_file_path)

    if not class_name:
        print(f"エラー: '{cs_file_path}' からクラス名を解析できませんでした。")
        sys.exit(1)

    print(f"クラス名を解析: {class_name}")
    print(f"定義済みのオプション {len(defined_options)}個、実装済みのオプション {len(implemented_options)}個を発見しました。")

    unimplemented = defined_options - implemented_options
    if unimplemented:
        print("\nエラー: 定義と実装の間に矛盾が発見されました。", file=sys.stderr)
        print("以下のオプションはenumで定義されていますが、CreateSpecificOptionで実装されていません:", file=sys.stderr)
        for opt in sorted(list(unimplemented)):
            print(f"- {opt}", file=sys.stderr)
        sys.exit(1)

    keys = generate_translation_keys(class_name, implemented_options)

    path_parts = cs_file_path.split(os.sep)
    team_name = "Unknown"
    if 'Roles' in path_parts:
        roles_index = path_parts.index('Roles')
        if len(path_parts) > roles_index + 2:
            team_name = path_parts[roles_index + 2]
    if "GhostRoles" in path_parts:
        team_name = "Ghost" + team_name
    if "Combination" in path_parts:
        team_name = "Combination"

    translation_dir = "ExtremeRoles/Translation/resx"
    default_resx_path = os.path.join(translation_dir, f"{team_name}.resx")

    print(f"対象の翻訳ファイル: {default_resx_path}")

    if not os.path.exists(default_resx_path):
        print(f"'{default_resx_path}' が見つかりません。新しい空のファイルを作成します。")

        root = ET.Element('root')
        resheaders = {
            "resmimetype": "text/microsoft-resx",
            "version": "2.0",
            "reader": "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
            "writer": "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        }
        for name, value_text in resheaders.items():
            header = ET.SubElement(root, 'resheader')
            header.set('name', name)
            value = ET.SubElement(header, 'value')
            value.text = value_text

        tree = ET.ElementTree(root)
        tree.write(default_resx_path, encoding='utf-8', xml_declaration=True)
        print(f"新しい空の翻訳ファイルを作成しました: {default_resx_path}")

    added_count = update_resx_file(default_resx_path, keys)

    if added_count > 0:
        print(f"\n{added_count}個の新しい翻訳キーを '{default_resx_path}' に正常に追加しました。")
    else:
        print(f"\n追加する新しいキーはありません。'{default_resx_path}' は役職 '{class_name}' に対して既に最新の状態です。")

if __name__ == "__main__":
    main()
