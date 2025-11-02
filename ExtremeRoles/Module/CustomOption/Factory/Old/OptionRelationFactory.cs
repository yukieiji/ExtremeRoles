using System;
using ExtremeRoles.Module.CustomOption.Implemented.Old;
using ExtremeRoles.Module.CustomOption.Interfaces.Old;


#nullable enable

namespace ExtremeRoles.Module.CustomOption.Factory.Old;

public static class OptionRelationFactory
{
	public static IOptionRelation Create(
		IOldOption? parent = null,
		bool invert=false,
		in Func<bool>? hook = null)
	{
		bool isParentNull = parent is null;
		bool notUseHook = hook is null;
		if (isParentNull && invert)
		{
			throw new ArgumentException("Invalided Parent");
		}
		else if (isParentNull)
		{
			return notUseHook ?
				new DefaultRelation() :
				new DefaultChain(hook!);
		}
		else if (invert)
		{
			return notUseHook ?
				new WithInvertParent(parent!) :
				new WithInvertParentAndCustomHook(parent!, hook!);
		}
		else
		{
			return new WithParentRelation(parent!);
		}
	}
}
