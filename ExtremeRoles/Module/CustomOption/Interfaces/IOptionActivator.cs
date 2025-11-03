#nullable enable

namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IOptionActivator
{
	public IOption? Parent { get; }
	public bool IsActive { get; }
}
