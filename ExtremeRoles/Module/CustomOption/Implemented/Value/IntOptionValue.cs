using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.CustomOption.Implemented.Value;

public sealed class IntOptionValue(int @default, int min, int max, int step) : 
	IValue<int>,
	IValueHolder
{
	private readonly int @default = @default;

	public OptionRange<int> InnerRange { get; set; } = new OptionRange<int>(
		OptionRange<int>.GetIntRange(min, max, step));

	public int DefaultIndex => this.InnerRange.GetIndex(@default);
	public int Value => this.InnerRange.RangedValue;
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
