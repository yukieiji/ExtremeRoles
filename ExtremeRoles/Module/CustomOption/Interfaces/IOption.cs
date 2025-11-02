using System;
using System.Collections.Generic;


namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IOption
{
	public IOptionInfo Info { get; }

	public string TransedTitle { get; }
	public string TransedValue { get; }

	public int Range { get; }
	public int Selection { get; set; }

	public int DefaultSelection { get; }

	public event Action<int> OnValueChanged;
	public bool IsActive { get; }

	public void SwitchPreset();

	public T Value<T>() where T :
		struct, IComparable, IConvertible,
		IComparable<T>, IEquatable<T>;
}
