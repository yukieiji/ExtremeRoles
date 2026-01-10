
import re

def refactor_content(content: str) -> str:
    """
    Removes the default RolePropPresets arguments from RoleArgs constructor calls in C# code.

    Args:
        content: A string containing the C# code.

    Returns:
        The refactored C# code as a string.
    """
    # Pattern to find and remove ", RolePropPresets.CrewmateDefault" when it's the last argument.
    # It handles various whitespace and newlines before the closing parenthesis.
    content = re.sub(
        r',\s*RolePropPresets\.CrewmateDefault\s*\)',
        ')',
        content,
        flags=re.DOTALL
    )

    # Pattern to find and remove ", RolePropPresets.ImpostorDefault" when it's the last argument.
    content = re.sub(
        r',\s*RolePropPresets\.ImpostorDefault\s*\)',
        ')',
        content,
        flags=re.DOTALL
    )

    return content
