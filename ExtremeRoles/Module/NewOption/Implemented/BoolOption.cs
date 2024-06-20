using ExtremeRoles.Module.NewOption.Interfaces;

namespace ExtremeRoles.Module.NewOption.Implemented;

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
