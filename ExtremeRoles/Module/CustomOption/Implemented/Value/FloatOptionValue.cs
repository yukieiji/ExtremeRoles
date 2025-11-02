using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.CustomOption.Implemented.Value;

#nullable enable


public sealed class FloatOptionValue(
	float @default, float min, float max, float step) : 
	IValue<float>,
	IValueHolder
{
	public OptionRange<float> InnerRange { get; set; } = new OptionRange<float>(
		OptionRange<float>.GetFloatRange(min, max, step));

	private readonly float @default = @default;

	public int DefaultIndex => this.InnerRange.GetIndex(@default);
	public float Value => this.InnerRange.RangedValue;
	public string StrValue => this.Value.ToString();

	public int Selection
	{
		get => this.InnerRange.Selection;
		set => this.InnerRange.Selection = value;
	}

	public int Range => this.InnerRange.Range;
	public override string ToString()
		=> this.Range.ToString();
}
