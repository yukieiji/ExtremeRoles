import os
import sys
import glob

from xml.etree import ElementTree as ET

from pylightxl import readxl

WORKING_DIR = os.path.dirname(os.path.realpath(__file__))

EXTREMERORLS_FILE = os.path.join(WORKING_DIR, "ExtremeRoles", "Translation", "resx")
EXTREMESKIN_IN_FILE = os.path.join(WORKING_DIR, "ExtremeSkinsTransData.xlsx")

SUPPORT_LANG = {0:'English', 11:'Japanese', 13: 'SChinese', 14: "TChinese"}
SUPPORT_RESX_LANG = {'en-US':'English', "zh-Hans": 'SChinese', "zh-Hant": "TChinese"}
TAG = 'report'

RESX_KEY_TAG = "data"
RESX_KEY_ATTR = "name"
RESX_VALUE_TAG = "value"

def get_trans_data_check(file_name:str)-> dict[str, list[str]]:
  wb = readxl(file_name)

  missing_data = {}
  try:
    for name in wb.ws_names:

      sheat = wb.ws(name)

      row, col = sheat.size
      # 行を回す
      for i in range(2, row + 1):

        # i行目の1列目はキー
        key = sheat.index(i, 1)
        if key == "":
          continue

        # i行目j列がデータ、jは2以上であり2が0(英語)である
        for j in range(2, col + 1):

            lang_enm = j - 2
            if (not (lang_enm in SUPPORT_LANG) or
              (lang_enm == 11 and (key == 'langTranslate' or key == 'translatorMember'))):
              continue

            cell_data = sheat.index(i, j)
            if type(cell_data) != str:
              continue

            lang = SUPPORT_LANG[lang_enm]


            if not (lang in missing_data):
              missing_data[lang] = []

            if cell_data == '':
              missing_data[lang].append(key)
  except Exception as e:
    print(f"Warning: An error occurred while processing Excel file {file_name}: {e}", file=sys.stderr)
  return missing_data

class ResXMissingData:
  def __init__(self):
    self.data : dict[str, list[str]] = {}
  def append(self, lang:str, key:str):
    if not (lang in self.data):
      self.data[lang] = []
    self.data[lang].append(key)


def get_resx_trans_missing_data(path:str) -> dict[str, dict[str, list[str]]]:
  ja_resx = set(glob.glob(os.path.join(path, "*.resx")))

  # `*.*.resx` を検索
  other_resx = set(glob.glob(os.path.join(path, "*.*.resx")))

  base_trans : dict[str, dict[str, str]] = {}

  for resx_file in ja_resx:
    try:
      tree = ET.parse(resx_file)
    except (ET.ParseError, Exception) as e:
      print(f"Warning: Could not parse base resx file {resx_file}: {e}", file=sys.stderr)
      continue
    root = tree.getroot()

    file_name = resx_file.split('.')[0]
    base_trans[file_name] = {}

    for data in root.findall(RESX_KEY_TAG):
        key = data.get(RESX_KEY_ATTR)  # <data name="Key"> の "Key" を取得
        if key is None:
          continue
        value_element = data.find(RESX_VALUE_TAG)
        if value_element is None:
          continue
        val = value_element.text
        if val is None:
          continue
        base_trans[file_name][key] = val  # <value> の内容を辞書に格納
  result : dict[str, dict[str, list[str]]] = {}
  for resx in other_resx:

    splited = resx.split('.')
    lang = splited[1]
    if lang not in SUPPORT_RESX_LANG:
      continue
    lang = SUPPORT_RESX_LANG[lang]
    file_name = splited[0]

    try:
      tree = ET.parse(resx)
    except (ET.ParseError, Exception) as e:
      print(f"Warning: Could not parse language resx file {resx}: {e}", file=sys.stderr)
      continue
    root = tree.getroot()

    missing_data = ResXMissingData()

    for key in base_trans[file_name].keys():
      data_element = root.find(f".//data[@name='{key}']")
      if data_element is None:
        missing_data.append(lang, key)
        continue
      value_element = data_element.find(RESX_VALUE_TAG)
      if value_element is None:
        missing_data.append(lang, key)
        continue
      if value_element.text is None or value_element.text == '':
          missing_data.append(lang, key)
    if len(missing_data.data) == 0:
      continue
    result[resx] = missing_data.data
  return result

def convert_md_table(data: dict[str, list[str]]) -> str:
  result = '| Languages | Missing TransKeys |\n| --- | --- |'

  for lang, miss_keys in data.items():
    if len(miss_keys) == 0:
      continue
    for index, key in enumerate(miss_keys):
      line = '\n'
      if index == 0:
        line = f'{line}| {lang} | {key} |'
      else:
        line = f'{line}| {lang} | {key} |'
      result = f'{result}{line}'

  return result

def output_md_report(build_result:str, check_resx:list[str], check_xlsx_file:list[str]) -> None:
  
  result = f'### GitHub Actions MSBuilds\n\n - Build Result : {build_result}\n\n'
  result = f'{result}### Translation Checker Report\n'

  for path in check_resx:
    try:
      resx_result = get_resx_trans_missing_data(path)
    except Exception as e:
      print(f"Warning: Failed to get RESX translation data for path {path}: {e}", file=sys.stderr)
      resx_result = {}
    for resx, data in resx_result.items():
      result = f'{result}\n - FileName:**{os.path.basename(resx)}**\n\n'
      result = f'{result}{convert_md_table(data)}\n'

  for file in check_xlsx_file:
    try:
      xlsx_data = get_trans_data_check(file)
    except Exception as e:
      print(f"Warning: Failed to get XLSX translation data for file {file}: {e}", file=sys.stderr)
      xlsx_data = {}
    result = f'{result}\n - FileName:**{os.path.basename(file)}**\n\n'
    result = f'{result}{convert_md_table(xlsx_data)}\n'

  with open(os.path.join(WORKING_DIR, '.github/workflows/comment.md'), 'w', encoding='UTF-8') as md:
      md.write(result)


if __name__ == "__main__":
  output_md_report(sys.argv[1], [EXTREMERORLS_FILE], [EXTREMESKIN_IN_FILE])