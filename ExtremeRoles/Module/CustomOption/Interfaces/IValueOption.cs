using System;

namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IValueOption<ValueType> : IOldOption
	where ValueType :
		struct, IComparable, IConvertible,
		IComparable<ValueType>, IEquatable<ValueType>
{
	public ValueType Value { get; }
	public void AddWithUpdate(IDynamismOption<ValueType> option);
}
