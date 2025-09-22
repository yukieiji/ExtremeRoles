using UnityEngine;

namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IOptionCategoryViweInfo
{
	public OptionTab Tab { get; }

	public Color? Color { get; }
	public string Name { get; }
	public string TransedName { get; }
}
