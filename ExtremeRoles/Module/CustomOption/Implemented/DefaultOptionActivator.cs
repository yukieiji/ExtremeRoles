using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.Extension.Il2Cpp;
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

public sealed class VanillaRoleActive(RoleTypes target) : IOptionActivator
{
	public IOption? Parent { get; } = null;
	private readonly RoleTypes target = target;
	public bool IsActive => 
		GameOptionsManager.Instance.CurrentGameOptions.IsTryCast<NormalGameOption>(out var normal) &&
		normal.RoleOptions.GetChancePerGame(target) > 0;
}

public sealed class MultiActive(params IOptionActivator[] activators) : IOptionActivator
{
	public IOption? Parent { get; } = activators[0].Parent;
	public bool IsActive => activators.All(x => x.IsActive);
}
