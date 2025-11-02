using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.CustomOption.Implemented.Value;

#nullable enable


public sealed class FloatOptionValue(
	float @default, float min, float max, float step) :
	OptionRange<float>(GetFloatRange(min, max, step)),
	IValue<float>,
	IValueHolder
{
	private readonly float @default = @default;

	public int DefaultIndex => GetIndex(@default);

	public float Value => this.RangedValue;
	public string StrValue => this.Value.ToString();
}