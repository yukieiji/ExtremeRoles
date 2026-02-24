using System;
using System.Collections.Generic;
using System.Reflection;

using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Interfaces;
namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct OptionDTO(
	int ID,
	bool IsActive,
	string TransedName,
	int Selection,
	object[] ValueRange,
	string Format,
	IReadOnlyList<OptionDTO> Childs);

public readonly record struct CategoryDTO(int ID, string Name, IReadOnlyList<OptionDTO> Options);
public readonly record struct TabDTO(int ID, string Name, IReadOnlyList<CategoryDTO> Categories);

public readonly record struct PutOptionRequest(long ID, int Value);

public static class OptionData
{

	public static IReadOnlyList<TabDTO> GetCurrentOptions()
	{
		foreach (var tabId in Enum.GetValues<OptionTab>())
		{
			var categories = new List<CategoryDTO>();
			if (!OptionManager.Instance.TryGetTab(tabId, out var container))
			{
				continue;
			}
			foreach (var category in container.Category)
			{
				var options = new List<OptionDTO>();
				var idHash = new HashSet<int>();
				foreach (var option in category.Options)
				{
					if (idHash.Contains(option.Info.Id))
					{
						continue;
					}
				}
			}
		}
	}

	private static OptionDTO createOptionDTO(IOption option, HashSet<int> regsted)
	{
		var childsResult = new List<OptionDTO>();
		if (OptionManager.Instance.TryGetChild(option, out var childs))
		{
			foreach (var child in childs)
			{
				regsted.Add(option.Info.Id);
				childsResult.Add(createOptionDTO(child, regsted));
			}
		}
		return new OptionDTO(
			option.Info.Id,
			option.IsViewActive,
			option.TransedTitle,
			option.Selection,
			option.Range,
			option.Info.Format,
			childsResult);
	}
}
