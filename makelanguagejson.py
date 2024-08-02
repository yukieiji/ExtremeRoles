import os
import json
import glob

from typing import Dict

from pylightxl import readxl

WORKING_DIR = os.path.dirname(os.path.realpath(__file__))

EXTREMERORLS_IN_FILE = os.path.join(WORKING_DIR, "ExtremeRolesTransData.xlsx")
EXTREMERORLS_OUT_FILE = os.path.join(WORKING_DIR, "ExtremeRoles", "Resources", "JsonData", "Language.json")

EXTREMESKIN_IN_FILE = os.path.join(WORKING_DIR, "ExtremeSkinsTransData.xlsx")
EXTREMESKIN_OUT_FILE = os.path.join(WORKING_DIR, "ExtremeSkins", "Resources", "LangData", "stringData.json")

PUBLIC_BETA_IN_FILE = os.path.join(WORKING_DIR, 'ExtremeRoles.Beta', '**', "*.xlsx")
PUBLIC_BETA_OUT_FILE = os.path.join(WORKING_DIR, 'ExtremeRoles.Beta', "Resources", "JsonData")

def create_public_beta_transdata():
  all_file = glob.glob(PUBLIC_BETA_IN_FILE, recursive=True)

  for file_path in all_file:
    file_name = os.path.basename(file_path)
    file_name = os.path.splitext(file_name)[0]
    xlsx_to_json(file_path, os.path.join(PUBLIC_BETA_OUT_FILE, f'{file_name}.json'))


def is_require_update(new_json : Dict[str, str], output_file : str) -> bool:

  try:
    with open(output_file, "r") as f:
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

    # 行を回す
    for i in range(2, row + 1):

      data = {}

      # i行目の1列目はキー
      key = sheat.index(i, 1)
      if key == "":
        continue

      # i行目j列がデータ、jは2以上であり2が0(英語)である
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

    with open(output_file, "w") as f:
      json.dump(xlsx_data, f, indent=4)

def main() -> None:
  # xlsx_to_json(EXTREMERORLS_IN_FILE, EXTREMERORLS_OUT_FILE)
  xlsx_to_json(EXTREMESKIN_IN_FILE, EXTREMESKIN_OUT_FILE)
  create_public_beta_transdata()

if __name__ == "__main__":
  main()