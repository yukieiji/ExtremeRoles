using ExtremeRoles.Module.NewOption.Factory;
using System;
using System.Collections.Generic;

using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.NewOption;

public sealed class OptionManager
{
	public readonly static OptionManager Instance = new ();

	private readonly Dictionary<OptionTab, OptionTabContainer> options = new ();
	private int id = 0;

	private OptionManager()
	{
		foreach (var tab in Enum.GetValues<OptionTab>())
		{
			options.Add(tab, new OptionTabContainer(tab));
		}
	}

	public OptionGroupFactory CreateOptionGroup(
		string name,
		in OptionTab tab)
	{
		var factory = new OptionGroupFactory(name, this.id, this.registerOptionGroup, tab);
		this.id++;

		return factory;
	}

	public SequentialOptionGroupFactory CreateSequentialOptionGroup(
		string name,
		in OptionTab tab)
	{
		var factory = new SequentialOptionGroupFactory(name, this.id, this.registerOptionGroup, tab);
		this.id++;

		return factory;
	}

	public ColorSyncOptionFactory CreateColorSyncOptionGroup(
		string name,
		in OptionTab tab,
		in Color color)
	{
		var internalFactory = CreateOptionGroup(name, tab);
		var factory = new ColorSyncOptionFactory(color, internalFactory);

		return factory;
	}

	public AutoParentSetFactory CreateColorSyncOptionGroup(
		string name,
		in OptionTab tab,
		in IOptionInfo? parent = null)
	{
		var internalFactory = CreateOptionGroup(name, tab);
		var factory = new AutoParentSetFactory(internalFactory, parent);

		return factory;
	}

	private void registerOptionGroup(OptionTab tab, OptionGroup group)
	{
		if (!this.options.TryGetValue(tab, out var container))
		{
			throw new ArgumentException($"Tab {tab} is not registered.");
		}
		container.AddGroup(group);
	}
}
