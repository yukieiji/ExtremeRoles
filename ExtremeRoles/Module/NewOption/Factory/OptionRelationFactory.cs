using System;

using ExtremeRoles.Module.NewOption.Interfaces;
using ExtremeRoles.Module.NewOption.Implemented;


#nullable enable

namespace ExtremeRoles.Module.NewOption.Factory;

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
