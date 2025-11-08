using System;
using System.Collections.Generic;

using ExtremeRoles.Module.CustomOption.Interfaces;


namespace ExtremeRoles.Module.CustomOption.Implemented.Value;

public sealed class SelectionOptionValue :
	OptionRange<string>,
	IValue<int>,
	IValueHolder
{
	public SelectionOptionValue(string[] range, string defaultValue = "") : base(range)
	{
		this.defaultValue = defaultValue;
	}

	public SelectionOptionValue(IEnumerable<string> range, string defaultValue = "") : base(range)
	{
		this.defaultValue = defaultValue;
	}

	private readonly string defaultValue;

	public int DefaultIndex => this.GetIndex(defaultValue);

	public int Value => this.Selection;
	public string StrValue => Tr.GetString(this.RangedValue);

	public static SelectionOptionValue CreateFromEnum<T>()
		where T : struct, Enum
		=> new SelectionOptionValue(GetEnumString<T>());
}
