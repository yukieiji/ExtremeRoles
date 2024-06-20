using BepInEx.Configuration;

using ExtremeRoles.Module.NewOption.Interfaces;

namespace ExtremeRoles.Module.NewOption.Implemented;

public sealed class IntCustomOption : CustomOptionBase<int, int>
{

	public IntCustomOption(
		IOptionInfo info,
		int defaultValue,
		int min, int max, int step,
		IOptionRelation relation) : base(
			info, OptionRange<int>.Create(min, max, step),
			relation, defaultValue)
	{ }

	public override int Value => OptionRange.Value;
}

public sealed class IntDynamicCustomOption : CustomOptionBase<int, int>, IDynamismOption<int>
{
	private int step;
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
