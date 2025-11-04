using System;


namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IOption
{
	public IOptionInfo Info { get; }

	public string TransedTitle { get; }
	public string TransedValue { get; }

	public int Range { get; }

	public bool IsChangeDefault { get; }
	public int Selection { get; set; }

	public event Action OnValueChanged;

	public bool IsViewActive { get; }
	public bool IsActive { get; }

	public void SwitchPreset();

	public T Value<T>() where T :
		struct, IComparable, IConvertible,
		IComparable<T>, IEquatable<T>;
}
