using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.CustomOption.Implemented.Value;

public sealed class BoolOptionValue(bool @default) :
	OptionRange<string>(boolRange),
	IValue<bool>, IValueHolder
{

	private readonly bool @default = @default;

	public int DefaultIndex => GetIndex(@default ? "optionOn" : "optionOff");
	public bool Value => this.Selection > 0;

	public string StrValue => Tr.GetString(this.RangedValue);

	private static readonly string[] boolRange = ["optionOff", "optionOn"];
}
