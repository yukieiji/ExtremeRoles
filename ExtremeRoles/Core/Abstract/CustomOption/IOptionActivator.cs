#nullable enable

using ExtremeRoles;


#nullable enable

using ExtremeRoles.Core.Abstract.CustomOption;

namespace ExtremeRoles.Core.Abstract.CustomOption;

public interface IOptionActivator
{
	public IOption? Parent { get; }
	public bool IsActive { get; }
}
