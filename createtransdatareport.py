import os
import sys
from typing import Dict, List

from pylightxl import readxl

WORKING_DIR = os.path.dirname(os.path.realpath(__file__))

EXTREMERORLS_IN_FILE = os.path.join(WORKING_DIR, "ExtremeRolesTransData.xlsx")
EXTREMERORLS_OUT_FILE = os.path.join(WORKING_DIR, "ExtremeRoles", "Resources", "JsonData", "Language.json")

EXTREMESKIN_IN_FILE = os.path.join(WORKING_DIR, "ExtremeSkinsTransData.xlsx")
EXTREMESKIN_OUT_FILE = os.path.join(WORKING_DIR, "ExtremeSkins", "Resources", "LangData", "stringData.json")

SUPPORT_LANG = {0:'English', 11:'Japanese', 13: 'SChinese'}
TAG = 'report'

def get_trans_data_check(file_name:str)-> Dict[str, List[str]]:
  wb = readxl(file_name)

  missing_data = {}

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

  return missing_data

def convert_md_table(data: Dict[str, List[str]]) -> str:
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

def output_md_report(build_result, *check_xlsx_file):
  
  result = f'### GitHub Actions MSBuilds\n\n - Build Result : {build_result}\n\n'
  result = f'{result}### Translation Checker Report\n'

  for file in check_xlsx_file:
    result = f'{result}\n - FileName:{os.path.basename(file)}\n\n'
    result = f'{result}{convert_md_table(get_trans_data_check(file))}\n'

  with open(os.path.join(WORKING_DIR, '.github/workflows/comment.md'), 'w', encoding='UTF-8') as md:
      md.write(result)


if __name__ == "__main__":
  output_md_report(sys.argv[1], EXTREMERORLS_IN_FILE, EXTREMESKIN_IN_FILE)