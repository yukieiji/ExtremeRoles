using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.CustomOption.Implemented;

public sealed class BoolCustomOption : CustomOptionBase<bool, string>
{
	public BoolCustomOption(
		IOptionInfo info,
		bool defaultValue,
		IOptionRelation relation) : base(
			info, new OptionRange<string>(["optionOff", "optionOn"]),
			relation, defaultValue ? "optionOn" : "optionOff")
	{ }

	public override bool Value => OptionRange.Selection > 0;
}
