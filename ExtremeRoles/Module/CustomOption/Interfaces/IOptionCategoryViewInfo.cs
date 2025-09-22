using UnityEngine;

namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IOptionCategoryViewInfo
{
	public OptionTab Tab { get; }
	public string Name { get; }
	public string TransedName { get; }
}
