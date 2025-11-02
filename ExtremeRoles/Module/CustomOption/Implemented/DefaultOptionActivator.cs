using ExtremeRoles.Module.CustomOption.Interfaces;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Implemented;

public sealed class AlwaysActive : IOptionActivator
{
	public IOption? Parent { get; } = null;
	public bool IsActive => true;
}

public sealed class ParentActive(IOption parent) : IOptionActivator
{
	public IOption Parent { get; } = parent;
	public bool IsActive => this.Parent.IsActive;
}


public sealed class DefaultParentActive(IOption parent) : IOptionActivator
{
	public IOption Parent { get; } = parent;
	public bool IsActive => 
		this.Parent.Selection != this.Parent.DefaultSelection &&
		this.Parent.IsActive;
}

public sealed class InvertActive(IOption parent) : IOptionActivator
{

	public IOption Parent { get; } = parent;
	public bool IsActive =>
		this.Parent.Selection == this.Parent.DefaultSelection &&
		this.Parent.IsActive;
}
