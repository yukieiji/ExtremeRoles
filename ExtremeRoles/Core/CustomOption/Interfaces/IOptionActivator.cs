#nullable enable

using ExtremeRoles;

#nullable enable

using ExtremeRoles.Core.CustomOption.Interfaces;

namespace ExtremeRoles.Core.CustomOption.Interfaces;

public interface IOptionActivator
{
	public IOption? Parent { get; }
	public bool IsActive { get; }
}
