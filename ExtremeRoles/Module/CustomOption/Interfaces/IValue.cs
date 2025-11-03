using System;

namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IValue<out TValue> where TValue :
	notnull, IComparable, IConvertible,
	IComparable<TValue>, IEquatable<TValue>
{
	public TValue Value { get; }
}
