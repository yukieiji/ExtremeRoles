using System;

using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.CustomOption.Implemented;

public sealed class SelectionCustomOption : CustomOptionBase<int, string>
{
	public SelectionCustomOption(
		IOptionInfo info,
		OptionRange<string> range,
		IOptionRelation relation,
		string defaultValue = "") : base(
			info, range,
			relation, defaultValue)
	{ }

	public SelectionCustomOption(
		IOptionInfo info,
		string[] range,
		IOptionRelation relation,
		string defaultValue = "") : base(
			info, new OptionRange<string>(range),
			relation, defaultValue)
	{ }

	public SelectionCustomOption(
		IOptionInfo info,
		string[] range,
		int defaultIndex,
		IOptionRelation relation) : base(
			info, new OptionRange<string>(range),
			relation, range[defaultIndex])
	{ }

	public static SelectionCustomOption CreateFromEnum<T>(
		IOptionInfo info, IOptionRelation relation,
		string defaultValue = "") where T : struct, Enum
	{
		var range = OptionRange<string>.Create<T>();
		return new SelectionCustomOption(info, range, relation, defaultValue);
	}

	public override int Value => OptionRange.Selection;
}
