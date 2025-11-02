using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.CustomOption.Implemented;

public sealed class AlwaysActive : IOptionActivator
{
	public bool IsActive => true;
}

public sealed class ParentActive(IOption parent) : IOptionActivator
{
	private readonly IOption parent = parent;
	public bool IsActive => this.parent.IsActive;
}


public sealed class DefaultParentActive(IOption parent) : IOptionActivator
{
	private readonly IOption parent = parent;
	public bool IsActive => 
		this.parent.Selection != this.parent.DefaultSelection &&
		this.parent.IsActive;
}

public sealed class InvertActive(IOption parent) : IOptionActivator
{

	private readonly IOption parent = parent;
	public bool IsActive =>
		this.parent.Selection == this.parent.DefaultSelection &&
		this.parent.IsActive;
}
