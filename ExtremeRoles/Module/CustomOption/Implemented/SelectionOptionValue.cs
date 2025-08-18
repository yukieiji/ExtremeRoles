using ExtremeRoles.Module.CustomOption.Interfaces;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Implemented;

public sealed class SelectionOptionValue(string[] range, string defaultValue = "") : 
	OptionRange<string>(range),
	IValue<int>,
	IValueHolder
{
	private readonly string defaultValue = defaultValue;
	
	public int DefaultIndex => this.GetIndex(defaultValue);

	public int Value => this.Selection;
	public string StrValue => Tr.GetString(this.RangeValue);
}
