using System;

namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IOptionRange<SelectionType>
	where SelectionType :
		notnull, IComparable, IConvertible,
		IComparable<SelectionType>, IEquatable<SelectionType>
{
	public SelectionType Value { get; }
	public SelectionType Min { get; }
	public SelectionType Max { get; }

	public int Range { get; }

	public int Selection { get; set; }

	public int GetIndex(SelectionType value);

	public string ToString();
}
