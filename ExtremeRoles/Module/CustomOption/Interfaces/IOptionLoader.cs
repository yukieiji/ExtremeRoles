using System;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IOptionLoader
{
	public bool TryGet(int id, [NotNullWhen(true)] out IOption? option);

	public bool TryGet<T>(T id, [NotNullWhen(true)] out IOption? option) where T : Enum;

	public bool TryGetValue<T>(int id, [NotNullWhen(true)] out T value)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>;

	public bool TryGetValue<W, T>(W id, [NotNullWhen(true)] out T value)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		where W : Enum;

	public IOption Get(int id);
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
