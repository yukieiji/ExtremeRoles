using ExtremeRoles.Module.NewOption.Factory;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

#nullable enable

using OptionTab = ExtremeRoles.Module.CustomOption.OptionTab;
using OptionUnit = ExtremeRoles.Module.CustomOption.OptionUnit;

using ExtremeRoles.Module.NewOption.Interfaces;
using ExtremeRoles.Module.NewOption.Implemented;
using ExtremeRoles.Helper;
using System.Linq;
using ExtremeRoles.GameMode;
using Hazel;

namespace ExtremeRoles.Module.NewOption;

public sealed class NewOptionManager
{
	public readonly static NewOptionManager Instance = new ();

	private readonly Dictionary<OptionTab, OptionTabContainer> options = new ();
	private int id = 0;

	public string ConfigPreset
	{
		get => $"Preset:{selectedPreset}";
	}
	private int selectedPreset = 0;
	private const int skipStep = 10;

	private const int chunkSize = 50;

	private NewOptionManager()
	{
		foreach (var tab in Enum.GetValues<OptionTab>())
		{
			options.Add(tab, new OptionTabContainer(tab));
		}
	}

	public static void Load()
	{
		// ランダム生成機を設定を読み込んで作成
		RandomGenerator.Initialize();

		// ゲームモードのオプションロード
		ExtremeGameModeManager.Instance.Load();

		// 各役職を設定を読み込んで初期化する
		Roles.ExtremeRoleManager.Initialize();
		GhostRoles.ExtremeGhostRoleManager.Initialize();

		// 各種マップモジュール等のオプション値を読み込む
		Patches.MiniGame.VitalsMinigameUpdatePatch.LoadOptionValue();
		Patches.MiniGame.SecurityHelper.LoadOptionValue();
		Patches.MapOverlay.MapCountOverlayUpdatePatch.LoadOptionValue();

		MeetingReporter.Reset();
	}

	public static void ShareOption(in MessageReader reader)
	{
		try
		{
			OptionTab tab = (OptionTab)reader.ReadByte();
			int categoryId = reader.ReadPackedInt32();
			Instance.syncOption(tab, categoryId, reader);
		}
		catch (Exception e)
		{
			Logging.Error($"Error while deserializing options:{e.Message}");
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
		in IOption? parent = null)
	{
		var internalFactory = CreateOptionGroup(name, tab);
		var factory = new AutoParentSetFactory(internalFactory, parent);

		return factory;
	}

	public void Update(in OptionCategory category, in IOption option, int step)
	{
		int newSelection = option.Selection + (Key.IsShift() ? step * skipStep : step);
		if (Key.IsControlDown())
		{
			newSelection = newSelection > 0 ? option.Range - 1 : 0;
		}
		option.Selection = newSelection;

		int id = option.Info.Id;
		if (id == 0)
		{
			// プリセット切り替え
			switchPreset();
			shereAllOption();
		}
		else
		{
			shareOptionCategory(option.Info.Tab, category);
			category.IsDirty = true;
		}
	}

	private void registerOptionGroup(OptionTab tab, OptionCategory group)
	{
		if (!this.options.TryGetValue(tab, out var container))
		{
			throw new ArgumentException($"Tab {tab} is not registered.");
		}
		container.AddGroup(group);
	}

	private void shereAllOption()
	{
		foreach (var (tab, tabContainer) in this.options)
		{
			foreach (var category in tabContainer.Category)
			{
				shareOptionCategory(tab, category);
			}
		}
	}

	private static void shareOptionCategory(
		in OptionTab tab, in OptionCategory category)
	{
		int size = category.Count;
		if (size <= chunkSize)
		{
			shareOptionCategoryWithSize(tab, category, size);
		}
		else
		{
			int mod = size;
			do
			{
				shareOptionCategoryWithSize(tab, category, chunkSize);
				mod -= chunkSize;
			} while (mod > chunkSize);
			shareOptionCategoryWithSize(tab, category, mod);
		}
	}

	private static void shareOptionCategoryWithSize(
		in OptionTab tab, in OptionCategory category, int size)
	{
		using (var caller = RPCOperator.CreateCaller(
				RPCOperator.Command.ShareOption))
		{
			caller.WriteByte((byte)tab);
			caller.WritePackedInt(category.Id);
			caller.WriteByte((byte)size);
			foreach (var option in category.Options)
			{
				caller.WritePackedInt(option.Info.Id);
				caller.WritePackedInt(option.Selection);
			}
		}
	}

	private void syncOption(OptionTab tab, int categoryId, in MessageReader reader)
	{
		lock(this.options)
		{
			if (!this.options.TryGetValue(tab, out var container) ||
				!container.TryGetCategory(categoryId, out var category))
			{
				return;
			}
			int size = reader.ReadPackedInt32();
			for (int i = 0; i < size; i++)
			{
				int id = reader.ReadPackedInt32();
				int selection = reader.ReadPackedInt32();
				if (category.TryGet(id, out var option))
				{
					option.Selection = selection;
				}
			}
			category.IsDirty = true;
		}
	}

	private void switchPreset()
	{
		foreach (var tab in this.options.Values)
		{
			foreach (var category in tab.Category)
			{
				foreach (var option in category.Options)
				{
					option.SwitchPreset();
				}
				category.IsDirty = true;
			}
		}
	}
}
