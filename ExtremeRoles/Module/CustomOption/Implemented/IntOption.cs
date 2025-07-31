using BepInEx.Configuration;

using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.CustomOption.Implemented;

public sealed class IntCustomOption : CustomOptionBase<int, int>, IDynamismOption<int>
{
	private int maxValue;
	private readonly int minValue;
	private readonly int step;

	public IntCustomOption(
		IOptionInfo info,
		int defaultValue,
		int min, int max, int step,
		IOptionRelation relation) : base(
			info, OptionRange<int>.Create(min, max, step),
			relation, defaultValue)
	{
		this.maxValue = this.OptionRange.Max;
		this.minValue = this.OptionRange.Min;
		this.step = step;
	}
	public void Update(int newValue)
	{
		int newMaxValue = this.maxValue / newValue;
		var newRange = OptionRange<int>.Create(OptionRange.Min, newMaxValue, step);
		int prevValue = OptionRange.Selection;
		OptionRange = newRange;
		Selection = prevValue;
	}

	public override int Value => OptionRange.Value;
}

public sealed class IntDynamicCustomOption : CustomOptionBase<int, int>, IDynamismOption<int>
{
	private readonly int step;
	public IntDynamicCustomOption(
		IOptionInfo info,
		int defaultValue,
		int min, int step,
		IOptionRelation relation,
		int tempMaxValue = 0) : base(
			info, OptionRange<int>.Create(min, CreateMaxValue(min, step, defaultValue, tempMaxValue), step),
			relation, defaultValue)
	{
		this.step = step;
	}

	public override int Value => OptionRange.Value;


	public void Update(int newValue)
	{
		var newRange = OptionRange<int>.Create(OptionRange.Min, newValue, step);
		OptionRange = newRange;
		Selection = OptionRange.Selection;
	}
	private static int CreateMaxValue(int min, int step, int defaultValue, int tempMaxValue)
		=> tempMaxValue == 0 ?
			min + step < defaultValue ? defaultValue : min + step :
			tempMaxValue;
}
