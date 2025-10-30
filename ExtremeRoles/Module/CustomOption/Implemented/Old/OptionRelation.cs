using System;
using System.Collections.Generic;
using ExtremeRoles.Module.CustomOption.Interfaces;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Implemented.Old;

public sealed class NoRelation() : IOptionRelation
{
	public List<IOption> Children => throw new NotImplementedException();
}

public sealed class DefaultRelation() : IOptionRelation
{
	public List<IOption> Children { get; } = new List<IOption>();
}

public sealed class DefaultChain(in Func<bool> hook) : IOptionRelation, IOptionChain
{
	public List<IOption> Children { get; } = new List<IOption>();
	private readonly Func<bool> hook = hook;

	public bool IsChainEnable => hook.Invoke();
}

public sealed class WithParentRelation(IOption parent) : IOptionRelation, IOptionParent, IOptionChain
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
			isInvert = parent.Relation is IOptionInvertRelation;

			parent =
				parent.Relation is IOptionParent hasParent ?
				hasParent.Parent : null;
		}
		return active;
	}
}

public sealed class WithInvertParent(IOption parent) :
	IOptionRelation, IOptionParent, IOptionInvertRelation, IOptionChain
{
	public List<IOption> Children { get; } = new List<IOption>();
	public IOption Parent { get; } = parent;

	public bool IsChainEnable
		=> WithParentRelation.ParentChainEnable(Parent, true);
}

public sealed class WithInvertParentAndCustomHook(IOption parent, in Func<bool> hook) :
	IOptionRelation, IOptionParent, IOptionChain, IOptionInvertRelation
{
	public List<IOption> Children { get; } = new List<IOption>();
	public IOption Parent { get; } = parent;
	private readonly Func<bool> hook = hook;

	public bool IsChainEnable
		=> hook.Invoke() &&
		WithParentRelation.ParentChainEnable(Parent, true);
}
