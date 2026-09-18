import argparse
import glob
import os
import sys
from xml.etree import ElementTree as ET

WORKING_DIR = os.path.dirname(os.path.realpath(__file__))
RESX_DIR = os.path.join(WORKING_DIR, "ExtremeRoles", "Translation", "resx")

RESX_KEY_TAG = "data"
RESX_KEY_ATTR = "name"
RESX_VALUE_TAG = "value"

# Whitelist for keys in French resx files that are intentionally identical to EN/JA.
# Current existing keys in Text.fr-FR.resx:
EXISTING_FR_TEXT_KEYS = {
    "JFT32",
    "Pcg32XshRr",
    "Pcg64RxsMXs",
    "RomuMono",
    "RomuQuad",
    "RomuTrio",
    "Seiran128",
    "Shioi128",
    "Xorshift128",
    "Xorshift64",
    "Xorshiro256StarStar",
    "Xorshiro512StarStar",
    "MODNAME_TRANS",
    "Xion",
    "Suicide",
    "Vision",
    "LiberalSettingLiberalVison",
    "LiberalSettingLeaderVison",
    "roleHistoryCause",
    "Leader",
    "Militant",
    "sabotageKey",
    "OneToOne",
    "TwoToOne",
    "OneToTwo",
    "TwoToTwo",
    "OneToThree",
    "ThreeToOne",
    "ThreeToTwo",
    "TwoToThree",
    "ThreeToThree",
}

EXISTING_FR_COMBINATION_KEYS = {
    "Marlin",
    "Vigilante",
    "Assistant",
    "DelinquentVision",
    "scribble",
}

EXISTING_FR_NEUTRAL_KEYS = {
    "Alice",
    "Jackal",
    "Sidekick",
    "TaskMaster",
    "Yandere",
    "Yoko",
    "Totocalcio",
    "Madmate",
    "Umbrer",
    "Tucker",
    "IronMate",
    "Monika",
    "MonikaIntroDescription",
    "Furry",
    "Intimate",
    "featVirus",
}

WHITELIST = {
    "Text": EXISTING_FR_TEXT_KEYS,
    "Combination": EXISTING_FR_COMBINATION_KEYS,
    "WebUI": {"ROLE_FILTER_SHORT_LABEL"},
    "Crewmate": {"CEO"},
    "Neutral": EXISTING_FR_NEUTRAL_KEYS,
}


def parse_resx(file_path: str) -> dict[str, str]:
    if not os.path.exists(file_path):
        return {}
    try:
        tree = ET.parse(file_path)
    except Exception as e:
        print(f"Error parsing XML file {file_path}: {e}", file=sys.stderr)
        return {}

    root = tree.getroot()
    entries = {}
    for data in root.findall(RESX_KEY_TAG):
        key = data.get(RESX_KEY_ATTR)
        if key is None:
            continue
        val_elem = data.find(RESX_VALUE_TAG)
        val = val_elem.text if val_elem is not None and val_elem.text is not None else ""
        entries[key] = val
    return entries


def check_french_translations(resx_dir: str, target_file: str | None = None) -> bool:
    has_error = False

    if target_file:
        fr_file_path = os.path.join(resx_dir, target_file)
        if not os.path.exists(fr_file_path):
            print(f"Error: Target file '{target_file}' not found in {resx_dir}", file=sys.stderr)
            return False

        if target_file.endswith(".fr-FR.resx"):
            base_name = target_file[:-len(".fr-FR.resx")]
        else:
            base_name = target_file.split(".")[0]
        base_resx_files = [os.path.join(resx_dir, f"{base_name}.resx")]
    else:
        # Find all base resx files (Japanese base files, e.g., Crewmate.resx)
        all_resx = glob.glob(os.path.join(resx_dir, "*.resx"))
        base_resx_files = [
            f for f in all_resx
            if len(os.path.basename(f).split(".")) == 2
        ]

    missing_fr_files = []
    untranslated_entries = []  # tuple: (file_name, key, value, matched_lang)

    for base_file in sorted(base_resx_files):
        base_name = os.path.basename(base_file).split(".")[0]
        fr_file = os.path.join(resx_dir, f"{base_name}.fr-FR.resx")
        en_file = os.path.join(resx_dir, f"{base_name}.en-US.resx")

        # 1. Check for missing French file
        if not os.path.exists(fr_file):
            missing_fr_files.append(f"{base_name}.fr-FR.resx")
            continue

        # 2. Analyze existing French file for English/Japanese remaining text
        fr_data = parse_resx(fr_file)
        ja_data = parse_resx(base_file)
        en_data = parse_resx(en_file) if os.path.exists(en_file) else {}

        file_whitelist = WHITELIST.get(base_name, set())

        for key, fr_val in fr_data.items():
            if key in file_whitelist:
                continue

            # Compare against JA value
            ja_val = ja_data.get(key)
            if ja_val is not None and ja_val != "" and fr_val == ja_val:
                untranslated_entries.append((f"{base_name}.fr-FR.resx", key, fr_val, "Japanese"))
                continue

            # Compare against EN value
            en_val = en_data.get(key)
            if en_val is not None and en_val != "" and fr_val == en_val:
                untranslated_entries.append((f"{base_name}.fr-FR.resx", key, fr_val, "English"))
                continue

    # Report results
    if missing_fr_files:
        has_error = True
        print("=== Missing French Translation Files ===")
        for missing in missing_fr_files:
            print(f"  - {missing}")
        print()

    if untranslated_entries:
        has_error = True
        print("=== Untranslated Texts (Matches English / Japanese) ===")
        for file_name, key, value, matched_lang in untranslated_entries:
            print(f"  - [{file_name}] Key: '{key}' | Value: '{value}' (Matches {matched_lang})")
        print()

    if not has_error:
        print("All French translation checks passed successfully!")

    return not has_error


def main():
    parser = argparse.ArgumentParser(description="Check French resx translations.")
    parser.add_argument("--target", help="Specific target French resx file to check (e.g. Text.fr-FR.resx)")
    args = parser.parse_args()

    success = check_french_translations(RESX_DIR, target_file=args.target)
    if not success:
        sys.exit(1)


if __name__ == "__main__":
    main()
