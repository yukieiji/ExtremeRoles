import sys
from pathlib import Path
import re
from lxml import etree as ET
from dataclasses import dataclass, field


@dataclass
class ParsedOptionsData:
    """ロールに定義され、実装されたオプションのセットを保持します。

    Attributes:
        defined (set[str]): enumで定義されたオプション名のセット。
        implemented (set[str]): CreateSpecificOptionで実装されたオプション名のセット。
    """

    defined: set[str] = field(default_factory=set)
    implemented: set[str] = field(default_factory=set)


@dataclass
class ClassParseResult:
    """ファイルの内容からクラス定義を解析した結果を保持します。

    Attributes:
        class_name (str): 見つかったクラスの名前。
        class_body (str): クラスの波括弧内の完全な内容。
    """

    class_name: str
    class_body: str


@dataclass
class ParsedRoleData:
    """解析されたロールのすべての情報（場所を含む）を保持します。

    Attributes:
        class_name (str): 役職クラスの名前。
        file_path (str): 役職クラスが定義されているファイルへのパス。
        options (ParsedOptionsData): 役職の定義済みおよび実装済みオプション。
    """

    class_name: str
    file_path: str
    options: ParsedOptionsData


def find_and_parse_role_in_project(role_name: str) -> ParsedRoleData | None:
    """プロジェクト内のすべてのC#ファイルをスキャンして、特定のロールクラスを見つけて解析します。

    Args:
        role_name (str): 検索するロールクラスの英語名（大文字と小文字を区別しない）。

    Returns:
        ParsedRoleData | None: ロールの情報が見つかった場合はParsedRoleDataオブジェクト、
                                それ以外の場合はNone。
    """
    search_dirs = [Path("ExtremeRoles/Roles"), Path("ExtremeRoles/GhostRoles")]
    for search_dir in search_dirs:
        for file_path in search_dir.rglob("*.cs"):
            with open(file_path, "r", encoding="utf-8") as f:
                content = f.read()

            class_parse_result = parse_class_from_content(content, role_name)

            if class_parse_result and class_parse_result.class_body:
                options_data = parse_options_from_class_body(
                    class_parse_result.class_body, class_parse_result.class_name
                )
                return ParsedRoleData(
                    class_name=class_parse_result.class_name,
                    file_path=str(file_path),
                    options=options_data,
                )
    return None


def parse_class_from_content(content: str, role_name: str) -> ClassParseResult | None:
    """C#ファイルの内容を解析してクラスを見つけ、その本体を抽出します。

    Args:
        content (str): C#ファイルの文字列の内容。
        role_name (str): 検索するロールクラスの名前。

    Returns:
        ClassParseResult | None: クラスとその本体が見つかった場合はClassParseResultオブジェクト、
                                  それ以外の場合はNone。
    """
    class_match = re.search(
        r"public (?:sealed )?class\s+("
        + re.escape(role_name)
        + r"|"
        + re.escape(role_name)
        + r"Role)\b",
        content,
    )
    if not class_match:
        return None

    actual_class_name = class_match.group(1)
    class_start_index = class_match.end()

    try:
        brace_start_index = content.index("{", class_start_index)
    except ValueError:
        return None  # { が見つからない

    brace_level = 1
    scan_index = brace_start_index + 1
    while scan_index < len(content):
        char = content[scan_index]
        if char == "{":
            brace_level += 1
        elif char == "}":
            brace_level -= 1

        if brace_level == 0:
            class_body_content = content[brace_start_index + 1 : scan_index]
            return ClassParseResult(
                class_name=actual_class_name, class_body=class_body_content
            )

        scan_index += 1

    return None  # マッチする } が見つからない


def parse_options_from_class_body(
    class_body: str, class_name: str
) -> ParsedOptionsData:
    """クラス本体の文字列の内容を解析して、定義済みおよび実装済みのオプションを見つけます。

    Args:
        class_body (str): C#クラスの文字列の内容。
        class_name (str): 解析対象のクラスの名前。

    Returns:
        ParsedOptionsData: enumで定義されたオプションとファクトリで実装されたオプションの
                           2つのセットを含むParsedOptionsDataオブジェクト。
    """
    defined_options: set[str] = set()
    options_match = re.search(
        r"public enum (?:" + class_name + r"Option|Option)\s*{([^}]+)}",
        class_body,
        re.DOTALL,
    )
    if options_match:
        options_block = options_match.group(1)
        defined_options = set(re.findall(r"\b(\w+)\b", options_block))

    # クラス本体全体で実装されたオプションを検索します。
    # これにより、CreateSpecificOptionから呼び出されるヘルパーメソッド内のオプションも確実に見つけることができます。
    enum_type_pattern = rf"\b(?:{class_name}Option|Option)\.(\w+)\b"
    implemented_options = set(re.findall(enum_type_pattern, class_body))

    return ParsedOptionsData(defined=defined_options, implemented=implemented_options)


def generate_translation_keys(option_name_prefix: str, option_names: set[str]) -> list[str]:
    """ロールの標準的な翻訳キーのリストを生成します。

    Args:
        option_name_prefix (str): オプション名のプレフィックス。
        option_names (set[str]): オプション名のセット。

    Returns:
        list[str]: 生成された翻訳キーのリスト。
    """
    keys = [
        option_name_prefix,
        f"{option_name_prefix}FullDescription",
        f"{option_name_prefix}ShortDescription",
        f"{option_name_prefix}IntroDescription",
    ]
    for option in option_names:
        keys.append(f"{option_name_prefix}{option}")
    return keys


def update_resx_file(file_path: str, keys_to_add: list[str]) -> int:
    """lxmlを使用して、新しい翻訳キーを.resxファイルに追加します。

    ファイルが存在しないか壊れている場合は、新しいファイルを作成します。
    キーが既に存在する場合は、何もしません。

    Args:
        file_path (str): .resxファイルのパス。
        keys_to_add (list[str]): 追加するキーのリスト。

    Returns:
        int: 追加された新しいキーの数。
    """
    try:
        with open(file_path, "rb") as f:
            parser = ET.XMLParser(recover=True)
            tree = ET.parse(f, parser)
            root = tree.getroot()
    except (IOError, ET.XMLSyntaxError):
        # ファイルが存在しないか壊れている場合、新しいものを作成
        root = ET.Element("root")
        tree = ET.ElementTree(root)

    existing_keys = {data.get("name") for data in root.xpath("//data[@name]")}

    XML_NAMESPACE = "http://www.w3.org/XML/1998/namespace"
    added_count = 0
    for key in keys_to_add:
        if key not in existing_keys:
            data_element = ET.SubElement(root, "data")
            data_element.set("name", key)
            data_element.set(f"{{{XML_NAMESPACE}}}space", "preserve")
            value_element = ET.SubElement(data_element, "value")
            value_element.text = "STRMISS"
            print(f"  新しいキーを追加中: {key}")
            added_count += 1

    if added_count > 0:
        ET.indent(root, space="  ")
        with open(file_path, "wb") as f:
            tree.write(f, pretty_print=True, xml_declaration=True, encoding="utf-8")

    return added_count


def get_team_name_from_path(file_path: str) -> str:
    """ファイルパスからチーム名を決定します。

    パスの構造に基づいてチーム名を推測します。
    例: 'ExtremeRoles/Roles/Solo/Crewmate/Sheriff.cs' -> 'Crewmate'
        'ExtremeRoles/GhostRoles/Impostor/SaboEvil.cs' -> 'GhostImpostor'
        'ExtremeRoles/Roles/Combination/Lover.cs' -> 'Combination'

    Args:
        file_path (str): 役職のC#ファイルへのパス。

    Returns:
        str: 決定されたチーム名。不明な場合は "Unknown"。
    """
    path_parts = Path(file_path).parts

    # "Combination" は "Roles" の直下にある特殊なケースです
    if "Combination" in path_parts:
        return "Combination"

    is_ghost = "GhostRoles" in path_parts
    base_dir = "GhostRoles" if is_ghost else "Roles"

    try:
        base_index = path_parts.index(base_dir)
        check_path = path_parts[base_index + 1]

        if is_ghost:
            # 例: GhostRoles/Crewmate/Role.cs -> GhostCrewmate
            team = check_path
            return f"Ghost{team}"
        else:
            # 例: Roles/Solo/Crewmate/Role.cs -> Crewmate
            if check_path == "Solo":
                team = path_parts[base_index + 2]
                if team in ("Crewmate", "Impostor", "Neutral", "Liberal"):
                    return team

    except (ValueError, IndexError):
        # 予期しないパス構造の場合に返されます
        return "Unknown"

    return "Unknown"


def main() -> None:
    """スクリプトのメインエントリポイント。

    コマンドライン引数から役職名を受け取り、プロジェクト内を検索して、
    関連する翻訳キーを適切な.resxファイルに追加します。
    """
    if len(sys.argv) != 2:
        print("使い方: python add_role_translation_keys.py <役職名>")
        sys.exit(1)

    role_name = sys.argv[1]
    print(f"プロジェクト全体で役職を検索中: {role_name}...")

    parsed_data = find_and_parse_role_in_project(role_name)

    if not parsed_data:
        print(f"エラー: 役職 '{role_name}' のクラス定義が見つかりませんでした。")
        sys.exit(1)

    print(
        f"役職クラス '{parsed_data.class_name}' をファイル内で発見: {parsed_data.file_path}"
    )
    print(
        f"定義済みのオプション {len(parsed_data.options.defined)}個、実装済みのオプション {len(parsed_data.options.implemented)}個を発見しました。"
    )

    unimplemented = parsed_data.options.defined - parsed_data.options.implemented
    if unimplemented:
        print("\nエラー: 定義と実装の間に矛盾が発見されました。", file=sys.stderr)
        print(
            "以下のオプションはenumで定義されていますが、CreateSpecificOptionで実装されていません:",
            file=sys.stderr,
        )
        for opt in sorted(list(unimplemented)):
            print(f"- {opt}", file=sys.stderr)
        sys.exit(1)

    keys = generate_translation_keys(
        role_name, parsed_data.options.implemented
    )

    team_name = get_team_name_from_path(parsed_data.file_path)

    # ゴースト役職にはイントロダクション説明が存在しないため、キーを削除します。
    if "Ghost" in team_name:
        intro_key = f"{role_name}IntroDescription"
        if intro_key in keys:
            keys.remove(intro_key)

    default_resx_path = Path("ExtremeRoles/Translation/resx") / f"{team_name}.resx"
    print(f"対象の翻訳ファイル: {default_resx_path}")

    added_count = update_resx_file(str(default_resx_path), keys)

    if added_count > 0:
        print(
            f"\n{added_count}個の新しい翻訳キーを '{default_resx_path}' に正常に追加しました。"
        )
    else:
        print(
            f"\n追加する新しいキーはありません。'{default_resx_path}' は役職 '{parsed_data.class_name}' に対して既に最新の状態です。"
        )


if __name__ == "__main__":
    main()
