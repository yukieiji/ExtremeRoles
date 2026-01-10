using ExtremeRoles.Core.Abstract.CustomOption;
using ExtremeRoles.Core.Infrastructure.CustomOption;

namespace ExtremeRoles.Core.Infrastructure.CustomOption.Value;

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
