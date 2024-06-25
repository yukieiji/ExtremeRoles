using System;

using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.CustomOption.Implemented;


#nullable enable

namespace ExtremeRoles.Module.CustomOption.Factory;

public static class OptionRelationFactory
{
	public static IOptionRelation Create(IOption? parent = null, bool invert=false)
	{
		if (parent is null && invert)
		{
			throw new ArgumentException("Invalided Parent");
		}
		else if (parent is null)
		{
			return new DefaultRelation();
		}
		else if (invert)
		{
			return new OptionRelationWithInvertParent(parent);
		}
		else
		{
			return new OptionRelationWithParent(parent);
		}
	}
}
