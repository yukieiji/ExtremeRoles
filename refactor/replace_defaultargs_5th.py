import re
import os # Added import
import sys # Added import

def refactor_content(content: str) -> str:
    """
    Removes the default RolePropPresets arguments from RoleArgs.BuildCrewmate and
    RoleArgs.BuildImpostor constructor calls in C# code.

    Args:
        content: A string containing the C# code.

    Returns:
        The refactored C# code as a string.
    """
    # Pattern to find and remove ", RolePropPresets.CrewmateDefault" specifically from
    # RoleArgs.BuildCrewmate calls. The non-greedy `.*?` ensures we don't accidentally
    # skip over other arguments.
    content = re.sub(
        r'(RoleArgs\.BuildCrewmate\(.*?),?\s*RolePropPresets\.CrewmateDefault\s*\)',
        r'\1)',
        content,
        flags=re.DOTALL
    )

    # Pattern to find and remove ", RolePropPresets.ImpostorDefault" specifically from
    # RoleArgs.BuildImpostor calls.
    content = re.sub(
        r'(RoleArgs\.BuildImpostor\(.*?),?\s*RolePropPresets\.ImpostorDefault\s*\)',
        r'\1)',
        content,
        flags=re.DOTALL
    )

    return content

def process_files_in_directory(directory_path):
    modified_count = 0
    for root, _, files in os.walk(directory_path):
        for file_name in files:
            if not file_name.endswith(".cs"):
                continue

            file_path = os.path.join(root, file_name)
            print(f"Processing: {file_path}")

            with open(file_path, 'r', encoding='utf-8') as f:
                original_content = f.read()

            new_content = refactor_content(original_content)

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
        print("Usage: python replace_defaultargs_5th.py <target_directory>", file=sys.stderr)
        sys.exit(1)

    target_directory = sys.argv[1]
    if not os.path.isdir(target_directory):
        print(f"Error: '{target_directory}' is not a valid directory.", file=sys.stderr)
        sys.exit(1)

    print(f"Starting default arguments refactoring in {target_directory}...")
    changes = process_files_in_directory(target_directory)
    print(f"Default arguments refactoring finished. {changes} files modified.")