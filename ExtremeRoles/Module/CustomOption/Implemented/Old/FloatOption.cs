using ExtremeRoles.Module.CustomOption.Interfaces.Old;

namespace ExtremeRoles.Module.CustomOption.Implemented.Old;

public sealed class FloatCustomOption : CustomOptionBase<float, float>
{
	public FloatCustomOption(
		IOptionInfo info,
		float defaultValue,
		float min, float max, float step,
		IOptionRelation relation) : base(
			info, OptionRange<float>.Create(min, max, step),
			relation, defaultValue)
	{ }

	public override float Value => OptionRange.Value;
}

public sealed class FloatDynamicCustomOption : CustomOptionBase<float, float>, IDynamismOption<float>
{
	private float step;
	public FloatDynamicCustomOption(
		IOptionInfo info,
		float defaultValue,
		float min, float step,
		IOptionRelation relation,
		float tempMaxValue = 0.0f) : base(
			info, OptionRange<float>.Create(
				min, CreateMaxValue(min, step, defaultValue, tempMaxValue), step),
			relation, defaultValue)
	{
		this.step = step;
	}

	public override float Value => OptionRange.Value;

	public void Update(float newValue)
	{
		var newRange = OptionRange<float>.Create(
			OptionRange.Min, newValue, step);
		int prevValue = OptionRange.Selection;
		OptionRange = newRange;
		Selection = prevValue;
	}

	private static float CreateMaxValue(float min, float step, float defaultValue, float tempMaxValue)
		=> tempMaxValue == 0.0f ?
			min + step < defaultValue ? defaultValue : min + step :
			tempMaxValue;
}
