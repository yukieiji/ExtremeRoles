using System;
using System.Text;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.CustomOption.Interfaces.Old;
using ExtremeRoles.Module.CustomOption.OLDS;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Implemented.Old;

public sealed class PresetOption : IValueOption<int>
{
	public int Value { get; }
	public IOptionInfo Info { get; init; }

	public IOptionRelation Relation { get; init; }

	public bool IsEnable => true;

	public bool IsActiveAndEnable => true;

	public string Title => Tr.GetString(Info.Name);

	public string ValueString
	{
		get
		{
			int value = optionRange.RangedValue;
			string format = this.Info.Format;
			return Tr.GetString(format, value);
		}
	}

	public int Range => optionRange.Range;

	public int Selection
	{
		get => optionRange.Selection;

		set
		{
			optionRange.Selection = value;
		}
	}

	private IOptionRange<int> optionRange;


	private const int maxPresetNum = 20;
	private const int optionId = 0;
	private const int categoryId = 0;

	public enum OptionKey : int
	{
		Selection = optionId,
	}

	public PresetOption(
		IOptionInfo info,
		IOptionRange<int> range)
	{
		Info = info;
		optionRange = range;
		Relation = new NoRelation();

		int defaultIndex = optionRange.GetIndex(0);
		optionRange.Selection = defaultIndex;

		ExtremeRolesPlugin.Logger.LogInfo($"---- Create new Option ----\n{this}\n--------");
	}

	public static bool IsPreset(int categoryId, int optionId)
		=> categoryId == PresetOption.categoryId && optionId == PresetOption.optionId;

	public static void Create(string name)
	{
		using (var commonOptionFactory = OldOptionManager.CreateOptionCategory(
			categoryId, name, color: OptionCreator.DefaultOptionColor))
		{
			var presetOption = new PresetOption(
				new PresetOptionInfo(categoryId, $"{name}{OptionKey.Selection}"),
				OptionRange<int>.Create(1, maxPresetNum, 1));
			commonOptionFactory.AddOption(optionId, presetOption);
		}
	}

	public override string ToString()
	{
		var builder = new StringBuilder();
		builder
			.AppendLine(Info.ToString())
			.Append(optionRange.ToString());
		return builder.ToString();
	}

	public void AddWithUpdate(IDynamismOption<int> option)
	{
		throw new NotImplementedException();
	}

	public void SwitchPreset()
	{ }
}
