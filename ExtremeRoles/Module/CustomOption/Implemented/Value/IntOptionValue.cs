using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.CustomOption.Implemented.Value;

public sealed class IntOptionValue(int @default, int min, int max, int step) :
	OptionRange<int>(GetIntRange(min, max, step)),
	IValue<int>,
	IValueHolder
{
	private readonly int @default = @default;

	public int DefaultIndex => GetIndex(@default);
	public int Value => this.RangedValue;
	public string StrValue => this.Value.ToString();
}