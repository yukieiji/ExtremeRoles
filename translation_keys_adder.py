from pathlib import Path
import re

def add_keys_to_resx_text_based(file_path: str, keys: list[str]) -> None:
    """
    .resxファイルに新しいキーを追加します。

    テキストベースの操作で、コメントとスキーマを完全に保持します。

    Args:
        file_path (str): .resxファイルへのパス。
        keys (list[str]): 追加するキーのリスト。
    """
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()

        if not keys:
            print("追加するキーが指定されていません。")
            return

        new_data_elements = []
        for key in keys:
            # 既存のキーがないかチェック (簡易的なテキスト検索)
            if f'<data name="{key}"' in content:
                print(f"キー '{key}' は既に存在するため、スキップします。")
                continue

            # 新しいdata要素の文字列を作成
            new_element = f'  <data name="{key}" xml:space="preserve">\n    <value>STRMISS</value>\n  </data>'
            new_data_elements.append(new_element)
            print(f"キー '{key}' を追加します。")

        if not new_data_elements:
            print("追加する新しいキーはありませんでした。")
            return

        # </root> タグの直前に新しい要素を挿入
        closing_root_tag = '</root>'
        if closing_root_tag not in content:
            print("エラー: '</root>' タグが見つかりません。有効な .resx ファイルではありません。")
            return

        # 新しい要素の文字列を結合
        new_elements_str = "\n".join(new_data_elements)

        # </root>の前の最後のdataタグの後ろに挿入する
        last_data_tag_match = list(re.finditer(r'</data>\s*</root>', content))
        if last_data_tag_match:
            # 最後の</data>タグの直後を見つける
            insertion_point = last_data_tag_match[-1].start() + len('</data>')
            new_content = content[:insertion_point] + "\n" + new_elements_str + content[insertion_point:]
        else:
             # dataタグがない、もしくは</root>が予期せぬ場所にある場合、</root>の直前に挿入
            insertion_point = content.rfind(closing_root_tag)
            new_content = content[:insertion_point] + new_elements_str + "\n" + content[insertion_point:]

        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(new_content)

        print(f"\nファイルの更新が完了しました: {file_path}")

    except FileNotFoundError:
        # このエラーはメインブロックで処理されるようになったため、ここに来ることはないはず
        print(f"エラー: ファイルが見つかりません - {file_path}")
    except Exception as e:
        print(f"予期せぬエラーが発生しました: {e}")

if __name__ == '__main__':
    try:
        faction_name = input("陣営名 (例: Crewmate, Impostor, Liberal, Text) を入力してください: ").strip()

        if not faction_name:
            print("陣営名が入力されていません。")
        else:
            # 陣営名からファイルパスを構築
            resx_file = Path("ExtremeRoles") / "Translation" / "resx" / f"{faction_name}.resx"

            if not resx_file.exists():
                print(f"エラー: 対応する.resxファイルが見つかりません - {resx_file}")
            else:
                keys_list = []
                print("追加するキーを1つずつ、またはカンマ区切りで入力してください (入力が終わったら何も入力せずにEnter):")
                while True:
                    line_input = input("キー: ").strip()
                    if not line_input:
                        break

                    if ',' in line_input:
                        keys_from_line = [key.strip() for key in line_input.split(',') if key.strip()]
                        keys_list.extend(keys_from_line)
                        print(f"  -> {len(keys_from_line)}個のキーを追加しました。")
                    else:
                        keys_list.append(line_input)

                if not keys_list:
                    print("追加するキーが入力されませんでした。")
                else:
                    add_keys_to_resx_text_based(resx_file, keys_list)

    except KeyboardInterrupt:
        print("\n操作が中断されました。")
    except Exception as e:
        print(f"予期せぬエラーが発生しました: {e}")
