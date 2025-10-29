using System;

using UnityEngine;

using ExtremeRoles.Extension;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Factory;

public sealed class OptionCategoryAssembler
{
	public OptionCategoryScope<T> CreateOptionCategory<T>(
		int id,
		string name,
		T builder,
		in OptionTab tab = OptionTab.GeneralTab,
		Color? color = null)
		where T : IOptionBuilder
	{
		var view = new OptionCategoryViewInfo(name, tab, color);
		return new OptionCategoryScope<T>(id, builder, OptionManager.Instance.registerOptionGroup, view);
	}

	public OptionCategoryScope<DefaultBuilder> CreateDefaultOptionCategory(
		int id,
		string name,
		in OptionTab tab = OptionTab.GeneralTab,
		Color? color = null)
	{
		var builder = new DefaultBuilder(name, tab);
		return CreateOptionCategory(id, name, builder, tab, color);
	}

	public OptionCategoryScope<DefaultBuilder> CreateOptionCategory<T>(
		T option,
		in OptionTab tab = OptionTab.GeneralTab,
		Color? color = null) where T : Enum
		=> CreateDefaultOptionCategory(
			option.FastInt(),
			option.ToString(), tab, color);

	public OptionCategoryScope<SequentialBuilder> CreateSequentialOptionCategory(
		int id,
		string name,
		in OptionTab tab = OptionTab.GeneralTab,
		Color? color = null)
	{
		var builder = new SequentialBuilder(name, tab);
		return CreateOptionCategory(id, name, builder, tab, color);
	}

	public OptionCategoryScope<AutoParentSetBuilder> CreateAutoParentSetOptionCategory(
		int id,
		string name,
		in OptionTab tab,
		Color? color = null,
		in IOption? parent = null)
	{
		var innerBuilder = new DefaultBuilder(name, tab);
		var builder = new AutoParentSetBuilder(innerBuilder, parent);
		return CreateOptionCategory(id, name, builder, tab, color);
	}

	public OptionCategoryScope<AutoParentSetBuilder> CreateAutoParentSetOptionCategory<T>(
		T option,
		in OptionTab tab = OptionTab.GeneralTab,
		Color? color = null,
		in IOption? parent = null) where T : Enum
		=> CreateAutoParentSetOptionCategory(
			option.FastInt(),
			option.ToString(),
			tab, color, parent);

	public OptionCategoryScope<T> CreateSubOptionCategory<T>(
		int id,
		OptionCategoryScope<T> parentCategory) where T : IOptionBuilder
	{
		var view = new HiddenCategoryViewInfo(parentCategory.View);
		return new OptionCategoryScope<T>(id, parentCategory.Builder, OptionManager.Instance.registerOptionGroup, view);
	}
}
