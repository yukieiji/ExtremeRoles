using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using UnityEngine;
using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.GameMode;
using ExtremeRoles.Extension;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Event;


#nullable enable


namespace ExtremeRoles.Module.CustomOption;

public sealed class OptionManager : IEnumerable<KeyValuePair<OptionTab, OptionTabContainer>>
{
	public readonly static OptionManager Instance = new ();

	private readonly Dictionary<OptionTab, OptionTabContainer> options = new ();

	public string ConfigPreset
	{
		get => $"Preset:{selectedPreset}";
	}
	private int selectedPreset = 0;
	private const int skipStep = 10;

	private const int chunkSize = 50;

	private const string OptionChangeFontPlace = "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">{0}</font>";

	private OptionManager()
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
			bool isShow = reader.ReadByte() == byte.MinValue;
			OptionTab tab = (OptionTab)reader.ReadByte();
			int categoryId = reader.ReadPackedInt32();
			Instance.syncOption(
				isShow, tab, categoryId, reader);

			EventManager.Instance.Invoke(ModEvent.OptionUpdate);
		}
		catch (Exception e)
		{
			Logging.Error($"Error while deserializing options:{e.Message}");
		}
	}

	public IEnumerator<KeyValuePair<OptionTab, OptionTabContainer>> GetEnumerator() => this.options.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() { throw new Exception(); }

	public bool TryGetTab(OptionTab tab, [NotNullWhen(true)] out OptionTabContainer? container)
		=> this.options.TryGetValue(tab, out container) && container is not null;

	public bool TryGetCategory(OptionTab tab, int categoryId, [NotNullWhen(true)] out IOptionCategory? category)
	{
		category = null;
		return this.TryGetTab(tab, out var container) && container.TryGetCategory(categoryId, out category) && category is not null;
	}

	public static OptionCategoryFactory CreateOptionCategory(
		int id,
		string name,
		in OptionTab tab = OptionTab.GeneralTab,
		Color? color = null)
		=> new OptionCategoryFactory(name, id, Instance.registerOptionGroup, tab, color);

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
		=> new SequentialOptionCategoryFactory(name, id, Instance.registerOptionGroup, tab, color);

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

	public void UpdateToStep(in IOptionCategory category, in int id, int step)
	{
		var option = category.Loader.Get(id);
		UpdateToStep(category, option, step);
	}

	public void UpdateToStep(in IOptionCategory category, in IOption option, int step)
	{
		int newSelection = 0;
		if (Key.IsControlDown())
		{
			newSelection = step > 0 ? option.Range - 1 : 0;
		}
		else
		{
			newSelection = option.Selection + (Key.IsShift() ? step * skipStep : step);
		}
		Update(category, option, newSelection);
	}

	public void Update(in IOptionCategory category, in int id, int newIndex)
	{
		var option = category.Loader.Get(id);
		Update(category, option, newIndex);
	}

	public void Update(in IOptionCategory category, in IOption option, int newIndex)
	{
		option.Selection = newIndex;

		int id = option.Info.Id;
		if (PresetOption.IsPreset(category.Id, id))
		{
			this.selectedPreset = newIndex;

			// プリセット切り替え
			switchPreset();
			ShereAllOption();
		}
		else
		{
			shareOptionCategory(category);
			category.IsDirty = true;
		}
		EventManager.Instance.Invoke(ModEvent.OptionUpdate);
	}

	public void ShereAllOption()
	{
		foreach (var tabContainer in this.options.Values)
		{
			foreach (var category in tabContainer.Category)
			{
				shareOptionCategory(category, false);
			}
		}
	}

	public void registerOptionGroup(IOptionCategory group)
	{
		if (!this.options.TryGetValue(group.View.Tab, out var container))
		{
			throw new ArgumentException($"Tab {group.View.Tab} is not registered.");
		}
		container.AddGroup(group);
	}

	public void registerOptionGroup(OptionTab tab, IOptionCategory group)
	{
		if (!this.options.TryGetValue(tab, out var container))
		{
			throw new ArgumentException($"Tab {tab} is not registered.");
		}
		container.AddGroup(group);
	}

	private static void shareOptionCategory(
		in IOptionCategory category, bool isShow = true)
	{
		int size = category.Loader.Size;

		if (size <= chunkSize)
		{
			shareOptionCategoryWithSize(category, size, isShow);
		}
		else
		{
			int mod = size;
			do
			{
				shareOptionCategoryWithSize(category, chunkSize, isShow);
				mod -= chunkSize;
			} while (mod > chunkSize);
			shareOptionCategoryWithSize(category, mod, isShow);
		}
	}

	private static void shareOptionCategoryWithSize(
		in IOptionCategory category, int size, bool isShow=true)
	{
		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.ShareOption))
		{
			caller.WriteByte(isShow ? byte.MinValue : byte.MaxValue);
			caller.WriteByte((byte)category.View.Tab);
			caller.WritePackedInt(category.Id);
			caller.WriteByte((byte)size);
			foreach (var option in category.Loader.Options)
			{
				caller.WritePackedInt(option.Info.Id);
				caller.WritePackedInt(option.Selection);
			}
		}
	}

	private void syncOption(
		bool isShow,
		OptionTab tab, int categoryId,
		in MessageReader reader)
	{
		lock(this.options)
		{
			StringNames key = (StringNames)5000;

			if (!this.options.TryGetValue(tab, out var container) ||
				!container.TryGetCategory(categoryId, out var category))
			{
				return;
			}

			string tabName = Tr.GetString(tab.ToString());
			int size = reader.ReadPackedInt32();

			for (int i = 0; i < size; i++)
			{
				int id = reader.ReadPackedInt32();
				int selection = reader.ReadPackedInt32();
				if (!category.Loader.TryGet(id, out var option))
				{
					continue;
				}
				int curSelection = option.Selection;
				option.Selection = selection;

				// 値が変更されたのでポップアップ通知
				if (isShow && curSelection != option.Selection)
				{
					string showStr = Tr.GetString(
						"OptionSettingChange",
						tabName, category.View.TransedName,
						option.Title, option.ValueString);

					HudManager.Instance.Notifier.SettingsChangeMessageLogic(
						key, string.Format(OptionChangeFontPlace, showStr),
						true);
					key++;
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
				foreach (var option in category.Loader.Options)
				{
					option.SwitchPreset();
				}
				category.IsDirty = true;
			}
		}
	}
}
