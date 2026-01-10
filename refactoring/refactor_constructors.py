
import re

def refactor_content(content: str) -> str:
    """
    Removes default RolePropPresets arguments (any property ending in 'Default')
    from C# method calls.

    Args:
        content: A string containing the C# code.

    Returns:
        The refactored C# code as a string.
    """
    # This generalized regex looks for ", RolePropPresets." followed by any identifier
    # ending in "Default", and removes it if it's right before a closing parenthesis.
    # The identifier part `\w+` matches property names like `CrewmateDefault`, `ImpostorDefault`, etc.
    content = re.sub(
        r',\s*RolePropPresets\.\w+Default\s*\)',
        ')',
        content,
        flags=re.DOTALL
    )

    return content
