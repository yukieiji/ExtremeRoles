
import re

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
