import re
import os

PROPS = [
    "RoleProp.CanKill",
    "RoleProp.HasTask",
    "RoleProp.UseVent",
    "RoleProp.UseSabotage",
    "RoleProp.CanCallMeeting",
    "RoleProp.CanRepairSabotage",
    "RoleProp.CanUseAdmin",
    "RoleProp.CanUseSecurity",
    "RoleProp.CanUseVital",
]

def _replacer(match):
    # Extract parts from the match
    base_start = match.group(1)
    role_build_part = match.group(2)
    remainder_comma = match.group('remainder_comma') or ""
    remainder = match.group('remainder').strip()

    # Process boolean arguments
    bool_args_str = match.group(4)
    bool_values = [s.strip() == 'true' for s in bool_args_str.split(',')]

    # Map booleans to RoleProp enums
    enabled_props = [PROPS[i] for i, value in enumerate(bool_values) if value]

    # If there are fewer than 9 booleans, the desired "after" state implies
    # the remaining optional props (CanCallMeeting, etc.) are true.
    if len(bool_values) < len(PROPS):
            enabled_props.extend(PROPS[len(bool_values):])
            # Remove duplicates just in case, and preserve order
            enabled_props = sorted(list(set(enabled_props)), key=PROPS.index)

    if not enabled_props:
        props_str = "RoleProp.None"
    else:
        # Format with indentation
        props_str = " |\n            ".join(enabled_props)

    # Rebuild the constructor call
    # Ensure RoleCore is replaced with RoleArgs
    # 多分いらないはず・・・
    # new_role_build = role_build_part.replace("RoleCore.Build", "RoleArgs.Build")

    # Insert the props string into the build call
    new_role_build = role_build_part.rstrip(')') + f",\n            {props_str})"

    # Handle the remainder (e.g., "tab: ...")
    # Add the comma back only if there's a remainder.
    if remainder:
        remainder_str = ", " + remainder
    else:
        remainder_str = ""

    return f"{base_start}{new_role_build}{remainder_str})"

def refactor_constructor(content):
    # This regex is designed to find base constructor calls with a series of boolean arguments.
    # It now captures the optional comma before any remaining arguments.
    # (base\(\s*)                                    # Group 1: Matches "base(" and any whitespace.
    # (Role(?:Args|Core)\.Build\w+\([^)]+\))            # Group 2: Matches the RoleArgs or RoleCore build call.
    # (\s*,\s*)                                      # Group 3: Matches the comma separator.
    # ((?:true|false)(?:,\s*(?:true|false))+)        # Group 4: Matches the block of booleans (at least two).
    # (?P<remainder_comma>\s*,)?                      # Named group 'remainder_comma': Optionally captures a comma.
    # (?P<remainder>.*?)                             # Named group 'remainder': Non-greedily matches any characters until the final parenthesis. This captures things like "tab: ...".
    # (\))                                           # Group 7: Matches the final closing parenthesis of the base() call.
    pattern = re.compile(
        r"(base\(\s*)(Role(?:Args|Core)\.Build\w+\([^)]+\))(\s*,\s*)((?:true|false)(?:,\s*(?:true|false))+)(?P<remainder_comma>\s*,)?(?P<remainder>.*?)(\))",
        re.DOTALL
    )

    content, count = pattern.subn(_replacer, content)

    return content, count

def process_files(directory):
    for root, _, files in os.walk(directory):
        for file in files:
            if not file.endswith(".cs"):
                continue
            filepath = os.path.join(root, file)
            print(f"Checking: {filepath}")
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()

            new_content, changes = refactor_constructor(content)

            if changes <= 0:
                continue
            print(f"Found and refactored {changes} constructor(s) in {filepath}")
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(new_content)

if __name__ == "__main__":
    import sys
    if len(sys.argv) < 2:
        print("Usage: python replace_bool_to_roleprop_3rd.py <target_directory>", file=sys.stderr)
        sys.exit(1)

    target_directory = sys.argv[1]
    if not os.path.isdir(target_directory):
        print(f"Error: '{target_directory}' is not a valid directory.", file=sys.stderr)
        sys.exit(1)

    print("Starting refactoring for boolean RoleProp replacement...")
    process_files(target_directory)
    print("Boolean RoleProp replacement finished.")
