using System.Collections.Generic;

using ExtremeRoles.Module.CustomOption.Interfaces;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Implemented;

public sealed class NoRelation() : IOptionRelation
{
	public List<IOption> Children => throw new System.NotImplementedException();
}

public sealed class DefaultRelation() : IOptionRelation
{
	public List<IOption> Children { get; } = new List<IOption>();
}

public sealed class OptionRelationWithParent(IOption parent) : IOptionRelation, IOptionParent
{
	public List<IOption> Children { get; } = new List<IOption>();
	public IOption Parent { get; } = parent;

	public bool IsChainEnable
		=> ParentChainEnable(Parent, false);

	public static bool ParentChainEnable(IOption? parent, bool defaultInvert)
	{
		bool active = true;
		bool isInvert = defaultInvert;

		while (parent != null && active)
		{
			active = parent.IsEnable;

			if (isInvert)
			{
				active = !active;
			}
			isInvert = parent.Relation is OptionRelationWithInvertParent;

			parent =
				parent.Relation is IOptionParent hasParent ?
				hasParent.Parent : null;
		}
		return active;
	}
}

public sealed class OptionRelationWithInvertParent(IOption parent) : IOptionRelation, IOptionParent
{
	public List<IOption> Children { get; } = new List<IOption>();
	public IOption Parent { get; } = parent;

	public bool IsChainEnable
		=> OptionRelationWithParent.ParentChainEnable(Parent, true);
}
