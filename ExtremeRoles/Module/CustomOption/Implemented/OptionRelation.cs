using System.Collections.Generic;

using ExtremeRoles.Module.CustomOption.Interfaces;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Implemented;

public sealed class DefaultRelation() : IOptionRelation
{
	public List<IOption> Children { get; } = new List<IOption>();
}

public sealed class OptionRelationWithParent(IOption parent) : IOptionRelation, IOptionParent
{
	public List<IOption> Children { get; } = new List<IOption>();
	public IOption Parent { get; } = parent;

	public bool IsChainEnable
	{
		get
		{
			IOption? parent = Parent;
			bool active = true;

			while (parent != null && active)
			{
				active = parent.IsEnable;
				parent =
					parent.Relation is IOptionParent hasParent ?
					hasParent.Parent : null;
			}
			return active;
		}
	}
}

public sealed class OptionRelationWithInvertParent(IOption parent) : IOptionRelation, IOptionParent
{
	public List<IOption> Children { get; } = new List<IOption>();
	public IOption Parent { get; } = parent;

	public bool IsChainEnable
	{
		get
		{
			IOption? parent = Parent;
			bool active = true;

			while (parent != null && active)
			{
				bool parentEnable = parent.IsEnable;
				active = Parent != parent ? parentEnable : !parentEnable;
				parent =
					parent.Relation is IOptionParent hasParent ?
					hasParent.Parent : null;
			}
			return active;
		}
	}
}
