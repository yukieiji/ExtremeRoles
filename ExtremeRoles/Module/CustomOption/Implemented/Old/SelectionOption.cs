using System;
using System.Collections.Generic;
using System.Linq;
using ExtremeRoles.Extension;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.CustomOption.Implemented.Old;

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

public sealed class SelectionMultiEnableCustomOption : CustomOptionBase<int, string>
{
	private readonly IReadOnlySet<int> defaults;

	public SelectionMultiEnableCustomOption(
		IOptionInfo info,
		OptionRange<string> range,
		IOptionRelation relation,
		IReadOnlyList<string> defaults,
		string defaultValue = "") : base(
			info, range,
			relation, defaultValue)
	{
		this.defaults = defaults
			.Select(
				range.GetIndex)
			.ToHashSet();
	}

	public SelectionMultiEnableCustomOption(
		IOptionInfo info,
		OptionRange<string> range,
		IOptionRelation relation,
		IReadOnlySet<int> anotherDefaults,
		string defaultValue = "") : base(
			info, range,
			relation, defaultValue)
	{
		this.defaults = anotherDefaults;
	}

	public SelectionMultiEnableCustomOption(
		IOptionInfo info,
		string[] range,
		IOptionRelation relation,
		IReadOnlyList<string> anotherDefaults,
		string defaultValue = "") : base(
			info, new OptionRange<string>(range),
			relation, defaultValue)
	{
		this.defaults = defaults
			.Select(x => Array.IndexOf(range, x))
			.ToHashSet();
	}

	public SelectionMultiEnableCustomOption(
		IOptionInfo info,
		string[] range,
		IOptionRelation relation,
		IReadOnlySet<int> anotherDefaults,
		string defaultValue = "") : base(
			info, new OptionRange<string>(range),
			relation, defaultValue)
	{
		this.defaults = anotherDefaults;
	}

	public static SelectionMultiEnableCustomOption CreateFromEnum<T>(
		IOptionInfo info,
		IReadOnlyList<T> anotherDefaults,
		IOptionRelation relation,
		string defaultValue = "") where T : struct, Enum
	{
		var range = OptionRange<string>.Create<T>();
		return new SelectionMultiEnableCustomOption(
			info, range, relation,
			anotherDefaults.Select(x => x.FastInt()).ToHashSet(),
			defaultValue);
	}

	public override int Value => OptionRange.Selection;
	public override bool IsEnable =>
		base.IsEnable &&
		!this.defaults.Contains(this.Selection);
}
