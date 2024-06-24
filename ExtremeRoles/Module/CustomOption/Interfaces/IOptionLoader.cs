using System;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IOptionLoader
{
	public bool TryGet(int id, [NotNullWhen(true)] out IOption? option);
	public IOption Get<T>(T id) where T : Enum;

	public bool TryGetValueOption<W, T>(W id, [NotNullWhen(true)] out IValueOption<T>? option)
		where W : Enum
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>;

	public bool TryGetValueOption<T>(int id, [NotNullWhen(true)] out IValueOption<T>? option)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>;

	public IValueOption<T> GetValueOption<W, T>(W id)
		where W : Enum
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>;
	public IValueOption<T> GetValueOption<T>(int id)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>;

	public T GetValue<W, T>(W id) where W : Enum;
	public T GetValue<T>(int id);
}
