using ExtremeRoles.Module.NewOption.Factory;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

#nullable enable

using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Module.NewOption;

public sealed class NewOptionManager
{
	public readonly static NewOptionManager Instance = new ();

	private readonly Dictionary<OptionTab, OptionTabContainer> options = new ();
	private int id = 0;

	private NewOptionManager()
	{
		foreach (var tab in Enum.GetValues<OptionTab>())
		{
			options.Add(tab, new OptionTabContainer(tab));
		}
	}
	public bool TryGetTab(OptionTab tab, [NotNullWhen(true)] out OptionTabContainer? container)
		=> this.options.TryGetValue(tab, out container) && container is not null;

	public OptionGroupFactory CreateOptionGroup(
		string name,
		in OptionTab tab = OptionTab.General)
	{
		var factory = new OptionGroupFactory(name, this.id, this.registerOptionGroup, tab);
		this.id++;

		return factory;
	}

	public SequentialOptionGroupFactory CreateSequentialOptionGroup(
		string name,
		in OptionTab tab = OptionTab.General)
	{
		var factory = new SequentialOptionGroupFactory(name, this.id, this.registerOptionGroup, tab);
		this.id++;

		return factory;
	}

	public ColorSyncOptionGroupFactory CreateColorSyncOptionGroup(
		string name,
		in Color color,
		in OptionTab tab = OptionTab.General)
	{
		var internalFactory = CreateOptionGroup(name, tab);
		var factory = new ColorSyncOptionGroupFactory(color, internalFactory);

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

	private void registerOptionGroup(OptionTab tab, OptionCategory group)
	{
		if (!this.options.TryGetValue(tab, out var container))
		{
			throw new ArgumentException($"Tab {tab} is not registered.");
		}
		container.AddGroup(group);
	}
}
