using System;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IOptionLoader
{
	public bool TryGet(int id, [NotNullWhen(true)] out IOption? option);
	public IOption Get<T>(T id) where T : Enum;

	public T GetValue<W, T>(W id)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T> 
		where W : Enum;
	public T GetValue<T>(int id)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>;
}
