import re
import os # Added import
import sys # Added import
import textwrap

# Define the sets of flags for each preset.
OPTIONAL_DEFAULT_FLAGS = {
    "RoleProp.CanCallMeeting",
    "RoleProp.CanRepairSabotage",
    "RoleProp.CanUseAdmin",
    "RoleProp.CanUseSecurity",
    "RoleProp.CanUseVital",
}

CREWMATE_DEFAULT_FLAGS = OPTIONAL_DEFAULT_FLAGS | {"RoleProp.HasTask"}
IMPOSTOR_DEFAULT_FLAGS = OPTIONAL_DEFAULT_FLAGS | {
    "RoleProp.CanKill",
    "RoleProp.UseVent",
    "RoleProp.UseSabotage",
}

def _normalize_flags(flag_string):
    """Splits a string of flags, strips whitespace, and returns a set."""
    return set(flag.strip() for flag in flag_string.split('|'))

def _crewmate_replacer(match):
    # Group 1: Leading part of the call
    # Group 2: Whitespace
    # Group 3: The flags
    leading, whitespace, flags_str = match.groups()
    flags = _normalize_flags(flags_str)
    if flags == CREWMATE_DEFAULT_FLAGS:
        return f"{leading}{whitespace}RolePropPresets.CrewmateDefault"
    return match.group(0)  # Return original if no match

def _impostor_replacer(match):
    # Group 1: Leading part of the call
    # Group 2: Whitespace
    # Group 3: The flags
    leading, whitespace, flags_str = match.groups()
    flags = _normalize_flags(flags_str)
    if flags == IMPOSTOR_DEFAULT_FLAGS:
        return f"{leading}{whitespace}RolePropPresets.ImpostorDefault"
    return match.group(0) # Return original if no match

def _optional_default_replacer(match):
    leading, whitespace, flags_str = match.groups()
    flags = _normalize_flags(flags_str)
    if flags == OPTIONAL_DEFAULT_FLAGS:
        return f"{leading}{whitespace}RolePropPresets.OptionalDefault"
    return match.group(0)

def apply_role_prop_presets(csharp_code):
    """
    Applies RolePropPresets to C# constructor calls in the given code string.
    """

    # Pattern for BuildCrewmate: captures up to the last comma, whitespace, and then the flags.
    crewmate_pattern = re.compile(
        r"(RoleArgs\.BuildCrewmate\([^,]+,\s*[^,]+,)(\s*)(RoleProp\..*?)(?=\s*\))",
        re.DOTALL
    )

    # Pattern for BuildImpostor: captures up to the last comma, whitespace, and then the flags.
    impostor_pattern = re.compile(
        r"(RoleArgs\.BuildImpostor\([^,]+,)(\s*)(RoleProp\..*?)(?=\s*\))",
        re.DOTALL
    )

    neutral_pattern = re.compile(
        r"(RoleArgs\.BuildNeutral\([^,]+,)(\s*)(RoleProp\..*?)(?=\s*\))",
        re.DOTALL
    )

    liberal_pattern = re.compile(
        r"(RoleArgs\.BuildLiberal\([^,]+,)(\s*)(RoleProp\..*?)(?=\s*\))",
        re.DOTALL
    )

    modified_code = crewmate_pattern.sub(_crewmate_replacer, csharp_code)
    modified_code = crewmate_pattern.sub(_optional_default_replacer, csharp_code)
    modified_code = impostor_pattern.sub(_impostor_replacer, modified_code)
    modified_code = impostor_pattern.sub(_optional_default_replacer, modified_code)
    modified_code = neutral_pattern.sub(_optional_default_replacer, modified_code)
    modified_code = liberal_pattern.sub(_optional_default_replacer, modified_code)

    return modified_code

def process_files_in_directory(directory_path): # 関数名を変更し、引数をディレクトリパスに
    modified_count = 0
    for root, _, files in os.walk(directory_path):
        for file_name in files:
            if not file_name.endswith(".cs"):
                continue

            file_path = os.path.join(root, file_name)
            print(f"Processing: {file_path}")

            with open(file_path, 'r', encoding='utf-8') as f:
                original_content = f.read()

            new_content = apply_role_prop_presets(original_content)

            if new_content != original_content:
                with open(file_path, 'w', encoding='utf-8') as f:
                    f.write(new_content)
                modified_count += 1
                print(f"  Modified: {file_path}")
            else:
                print(f"  No changes to: {file_path}")
    return modified_count


if __name__ == '__main__':
    if len(sys.argv) < 2:
        print("Usage: python replace_to_presets_4th.py <target_directory>", file=sys.stderr)
        sys.exit(1)

    target_directory = sys.argv[1]
    if not os.path.isdir(target_directory):
        print(f"Error: '{target_directory}' is not a valid directory.", file=sys.stderr)
        sys.exit(1)

    print(f"Starting RoleProp Presets refactoring in {target_directory}...")
    changes = process_files_in_directory(target_directory)
    print(f"RoleProp Presets refactoring finished. {changes} files modified.")
