import os
import sys

def refactor_content(content):
    """
    Replaces RoleCore. with RoleArgs. in the given C# code content.
    """
    return content.replace("RoleCore.", "RoleArgs.")

def refactor_csharp_files(directory):
    """
    Walks through a directory, finds all .cs files, and applies refactoring.
    Skips RoleCore.cs and RoleArgs.cs to avoid breaking them.
    Logs the files that were changed.
    """
    modified_files = []
    for root, dirs, files in os.walk(directory):
        for file in files:
            
            if not file.endswith(".cs"):
                continue

            filepath = os.path.join(root, file)

            if file in ["RoleCore.cs", "RoleArgs.cs"]:
                print(f"Skipping file: {filepath}")
                continue

            try:
                with open(filepath, 'r', encoding='utf-8') as f:
                    original_content = f.read()
            except Exception as e:
                print(f"Error reading {filepath}: {e}", file=sys.stderr)
                continue

            refactored_content = refactor_content(original_content)

            if original_content == refactored_content:
                continue
            try:
                with open(filepath, 'w', encoding='utf-8') as f:
                    f.write(refactored_content)
                print(f"Modified file: {filepath}")
                modified_files.append(filepath)
            except Exception as e:
                print(f"Error writing to {filepath}: {e}", file=sys.stderr)

    return modified_files

def main():
    if len(sys.argv) < 2:
        print("Usage: python refactor_constructors.py <target_directory>", file=sys.stderr)
        sys.exit(1)

    target_dir = sys.argv[1]
    if not os.path.isdir(target_dir):
        print(f"Error: '{target_dir}' is not a valid directory.", file=sys.stderr)
        sys.exit(1)

    print("Starting refactoring...")
    changed_files = refactor_csharp_files(target_dir)
    print("\nRefactoring finished.")
    print(f"Total files modified: {len(changed_files)}")

if __name__ == "__main__":
    main()