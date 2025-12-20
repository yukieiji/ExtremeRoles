using System;

namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IOptionRange<T>
	where T :
		notnull, IComparable, IConvertible,
		IComparable<T>, IEquatable<T>
{
	public T RangedValue { get; }
	public T Min { get; }
	public T Max { get; }

	public int Range { get; }

	public int Selection { get; set; }

	public event Action OnValueChanged;

	public int GetIndex(T value);

	public string ToString();
}
