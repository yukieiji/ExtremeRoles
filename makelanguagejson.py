import os
import json
from typing import Dict

from pylightxl import readxl

import pandas as pd

WORKING_DIR = os.path.dirname(os.path.realpath(__file__))

EXTREMERORLS_IN_FILE = os.path.join(WORKING_DIR, 'ExtremeRolesTransData.xlsx')
EXTREMERORLS_OUT_FILE = os.path.join(WORKING_DIR, 'ExtremeRoles', 'Resources', 'JsonData', 'Language.json')

EXTREMESKIN_IN_FILE = os.path.join(WORKING_DIR, 'ExtremeSkinsTransData.xlsx')
EXTREMESKIN_OUT_FILE = os.path.join(WORKING_DIR, 'ExtremeSkins', 'Resources', 'LangData')

DEFAULT_LANG_CSV = 'Japanese.csv'

def is_require_update(new_json : Dict[str, str], output_file : str) -> bool:

  try:
    with open(output_file, 'r') as f:
      old_json = json.load(f)
    old_json_str = json.dumps(old_json)
  except Exception:
    return True
  else:
    new_json_str = json.dumps(new_json)

    return old_json_str != new_json_str


def xlsx_to_json(file_name : str, output_file : str) -> None:

  wb = readxl(file_name)

  xlsx_data = {}

  for name in wb.ws_names:

    sheat = wb.ws(name)

    row, col = sheat.size

    # �s���
    for i in range(2, row + 1):

      data = {}

      # i�s�ڂ�1��ڂ̓L�[
      key = sheat.index(i, 1)
      if key == "":
        continue

      # i�s��j�񂪃f�[�^�Aj��2�ȏ�ł���2��0(�p��)�ł���
      for j in range(2, col + 1):
          cell_data = sheat.index(i, j)

          if type(cell_data) != str or cell_data == "":
            continue

          # I hate excel why did I do this to myself
          data[str(j - 2)] = cell_data.replace("\r", "").replace("_x000D_", "").replace("\\n", "\n")

      if data != {}:
        xlsx_data[key] = data

  if is_require_update(xlsx_data, output_file):

    os.makedirs(os.path.dirname(output_file), exist_ok=True)

    with open(output_file, 'w') as f:
      json.dump(xlsx_data, f, indent=4)

def create_all_trans_dict(file_name : str) -> Dict[str, Dict[str, str]]:

  xlsx = pd.read_excel(file_name, sheet_name=None)

  all_trans_data = {}

  for df in xlsx.values():

    key_data = None

    for header in df.axes[1]:

      if header == 'Unnamed: 0':
        key_data = df[header]
        key_data = key_data.dropna()
        continue

      if key_data is None:
        continue

      lang_data = df[header]

      for index, value in key_data.items():
        data = lang_data[index]
        if pd.isnull(data):
          continue

        if not (header in all_trans_data):
          all_trans_data[header] = {}
        cleaned_data = data.replace('\r', '').replace('_x000D_', '').replace('\\n', '<br>')
        all_trans_data[header][value] = cleaned_data

  return all_trans_data

def no_trans_data_to_ja(output_dir : str) -> None:

  ja_lang_file = os.path.join(output_dir, DEFAULT_LANG_CSV)
  ja_lang_data = pd.read_csv(ja_lang_file, header=None, index_col=0)
  ja_lang_dict = ja_lang_data.to_dict()[1]

  for file in os.listdir(output_dir):

    if not file.endswith('.csv') or file == DEFAULT_LANG_CSV:
      continue

    target_file = os.path.join(output_dir, file)
    target_df = pd.read_csv(target_file, header=None, index_col=0)
    check_dict = target_df.to_dict()[1]

    is_add_new_data = False
    for key, value in ja_lang_dict.items():

      if key in check_dict:
        continue
      else:
        check_dict[key] = f'!-NOTTRANS-!{value}'
        is_add_new_data = True

    if is_add_new_data:
      output_df = pd.DataFrame(check_dict.values(), index=check_dict.keys())
      output_df.to_csv(target_file, index=True, header=False, encoding='utf_8_sig')


def xlsx_to_lang_csv(file_name : str, output_dir : str) -> None:

  all_trans_data = create_all_trans_dict(file_name)

  is_update = False

  for lang, data in all_trans_data.items():

    file = os.path.join(output_dir, f'{lang}.csv')
    if os.path.exists(file):
      prev_df = pd.read_csv(file, header=None, index_col=0)
      if data == prev_df.to_dict()[1]:
        continue

    output_df = pd.DataFrame(data.values(), index=data.keys())
    output_df.to_csv(file, index=True, header=False, encoding='utf_8_sig')
    is_update = True

  if not is_update:
    return

  no_trans_data_to_ja(output_dir)


if __name__ == '__main__':
  xlsx_to_json(EXTREMERORLS_IN_FILE, EXTREMERORLS_OUT_FILE)
  xlsx_to_lang_csv(EXTREMESKIN_IN_FILE, EXTREMESKIN_OUT_FILE)