using System;

namespace ExtremeRoles.Core.Abstract.CustomOption;

public interface IValueHolder
{
	public int DefaultIndex { get; }
	public string StrValue { get; }
	public int Selection { get; set; }
	public int Range { get; }

	public event Action OnValueChanged;
}
