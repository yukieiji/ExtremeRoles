using System;

namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IOption
{
	public IOptionInfo Info { get; }

	public string TransedTitle { get; }
	public string TransedValue { get; }

	public int Range { get; }
	public int Selection { get; set; }

	public event Action<int> OnValueChanged;

	public bool IsEnable { get; }
	public bool IsActiveAndEnable { get; }

	public void SwitchPreset();

	public T GetValue<T>() where T :
		struct, IComparable, IConvertible,
		IComparable<T>, IEquatable<T>;
}
