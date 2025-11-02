using System;

using UnityEngine;

using ExtremeRoles.Extension;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;


namespace ExtremeRoles.Module.CustomOption;

public static class OptionCategoryAssembler
{
	public static OptionCategoryFactory CreateOptionCategory(
		int id,
		string name,
		in OptionTab tab = OptionTab.GeneralTab,
		Color? color = null)
	{
		var mng = OptionManager.Instance;
		return new OptionCategoryFactory(name, id, mng.RegisterChild, mng.RegisterOptionGroup, tab, color);
	}

	public static OptionCategoryFactory CreateOptionCategory<T>(
		T option,
		in OptionTab tab = OptionTab.GeneralTab,
		Color? color = null) where T : Enum
		=> CreateOptionCategory(
			option.FastInt(),
			option.ToString(), tab, color);

	public static SequentialOptionCategoryFactory CreateSequentialOptionCategory(
		int id,
		string name,
		in OptionTab tab = OptionTab.GeneralTab,
		Color? color = null)
	{
		var mng = OptionManager.Instance;
		return new SequentialOptionCategoryFactory(name, id, mng.RegisterChild, mng.RegisterOptionGroup, tab, color);
	}

	public static AutoParentSetOptionCategoryFactory CreateAutoParentSetOptionCategory(
		int id,
		string name,
		in OptionTab tab,
		Color? color = null,
		in IOption? parent = null)
	{
		var internalFactory = CreateOptionCategory(id, name, tab, color);
		var factory = new AutoParentSetOptionCategoryFactory(internalFactory, parent);

		return factory;
	}

	public static AutoParentSetOptionCategoryFactory CreateAutoParentSetOptionCategory<T>(
		T option,
		in OptionTab tab = OptionTab.GeneralTab,
		Color? color = null,
		in IOption? parent = null) where T : Enum
		=> CreateAutoParentSetOptionCategory(
			option.FastInt(),
			option.ToString(),
			tab, color, parent);
}
