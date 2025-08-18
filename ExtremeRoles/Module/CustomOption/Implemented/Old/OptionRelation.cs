using System;
using System.Collections.Generic;
using ExtremeRoles.Module.CustomOption.Interfaces;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Implemented.Old;

public sealed class NoRelation() : IOptionRelation
{
	public List<IOldOption> Children => throw new NotImplementedException();
}

public sealed class DefaultRelation() : IOptionRelation
{
	public List<IOldOption> Children { get; } = new List<IOldOption>();
}

public sealed class DefaultChain(in Func<bool> hook) : IOptionRelation, IOptionChain
{
	public List<IOldOption> Children { get; } = new List<IOldOption>();
	private readonly Func<bool> hook = hook;

	public bool IsChainEnable => hook.Invoke();
}

public sealed class WithParentRelation(IOldOption parent) : IOptionRelation, IOptionParent, IOptionChain
{
	public List<IOldOption> Children { get; } = new List<IOldOption>();
	public IOldOption Parent { get; } = parent;

	public bool IsChainEnable
		=> ParentChainEnable(Parent, false);

	public static bool ParentChainEnable(IOldOption? parent, bool defaultInvert)
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

public sealed class WithInvertParent(IOldOption parent) :
	IOptionRelation, IOptionParent, IOptionInvertRelation, IOptionChain
{
	public List<IOldOption> Children { get; } = new List<IOldOption>();
	public IOldOption Parent { get; } = parent;

	public bool IsChainEnable
		=> WithParentRelation.ParentChainEnable(Parent, true);
}

public sealed class WithInvertParentAndCustomHook(IOldOption parent, in Func<bool> hook) :
	IOptionRelation, IOptionParent, IOptionChain, IOptionInvertRelation
{
	public List<IOldOption> Children { get; } = new List<IOldOption>();
	public IOldOption Parent { get; } = parent;
	private readonly Func<bool> hook = hook;

	public bool IsChainEnable
		=> hook.Invoke() &&
		WithParentRelation.ParentChainEnable(Parent, true);
}
