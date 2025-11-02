using System;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Interfaces;


namespace ExtremeRoles.Module.CustomOption.Factory;

public static class OptionActivatorFactory
{
	public static IOptionActivator Create(
		IOption parent = null,
		bool invert = false)
	{
		bool isParentNull = parent is null;
		if (isParentNull && invert)
		{
			throw new ArgumentException("Invalided Parent");
		}
		else if (isParentNull)
		{
			return null;
		}
		else if (invert)
		{
			return new InvertActive(parent);
		}
		else
		{
			return new DefaultParentActive(parent);
		}
	}
}
