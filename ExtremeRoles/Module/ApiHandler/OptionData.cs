using System.Collections.Generic;

using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct OptionDTO(string Id, string Title, string Value, int Selection, int Range);
public readonly record struct CategoryDTO(int Id, string Name, IReadOnlyList<OptionDTO> Options);
public readonly record struct TabDTO(int Id, string Name, IReadOnlyList<CategoryDTO> Categories);

public readonly record struct PutOptionRequest(string Id, int Value);

public static class OptionData
{
	public static IReadOnlyList<TabDTO> GetCurrentOptions()
	{
		var result = new List<TabDTO>();
		foreach (var kvp in OptionManager.Instance)
		{
			var tab = kvp.Key;
			var container = kvp.Value;

			var categories = new List<CategoryDTO>();
			foreach (var category in container.Category)
			{
				var options = new List<OptionDTO>();
				foreach (var option in category.Options)
				{
					options.Add(new OptionDTO(
						Id: $"{(int)tab}_{category.Id}_{option.Info.Id}",
						Title: option.TransedTitle,
						Value: option.TransedValue,
						Selection: option.Selection,
						Range: option.Range
					));
				}
				categories.Add(new CategoryDTO(category.Id, category.TransedName, options));
			}
			result.Add(new TabDTO((int)tab, Tr.GetString(tab.ToString()), categories));
		}
		return result;
	}
}
