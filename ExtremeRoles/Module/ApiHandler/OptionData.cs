using System;
using System.Collections.Generic;
using System.Reflection;

using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.CustomOption.Implemented.Value;

using ExREption = ExtremeRoles.Module.CustomOption.Implemented.CustomOption;

namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct OptionDTO(
	long ID,
	string TransedName,
	object ValueRange,
	int Selection,
	string TransedFormat,
	IReadOnlyList<OptionDTO>? Childs = null);

public readonly record struct CategoryDTO(int ID, string Name, IReadOnlyList<OptionDTO> Options);
public readonly record struct TabDTO(int ID, string Name, IReadOnlyList<CategoryDTO> Categories);

public readonly record struct PutOptionRequest(long ID, int Value);

public static class OptionData
{
	private static readonly FieldInfo? holderField = typeof(ExREption).GetField("holder", BindingFlags.NonPublic | BindingFlags.Instance);

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
				var allChildren = new HashSet<IOption>();
				foreach (var opt in category.Options)
				{
					if (OptionManager.Instance.TryGetChild(opt, out var children))
					{
						foreach (var child in children)
						{
							allChildren.Add(child);
						}
					}
				}

				var options = new List<OptionDTO>();
				foreach (var option in category.Options)
				{
					if (allChildren.Contains(option))
					{
						continue;
					}
					options.Add(createDTO(tab, category, option));
				}
				categories.Add(new CategoryDTO(category.Id, category.TransedName, options));
			}
			result.Add(new TabDTO((int)tab, Tr.GetString(tab.ToString()), categories));
		}
		return result;
	}

	public static long GetGlobalId(OptionTab tab, int categoryId, int optionId)
	{
		return ((long)tab << 40) | ((long)categoryId << 20) | (long)optionId;
	}

	public static (OptionTab tab, int categoryId, int optionId) FromGlobalId(long globalId)
	{
		int optionId = (int)(globalId & 0xFFFFF);
		int categoryId = (int)((globalId >> 20) & 0xFFFFF);
		OptionTab tab = (OptionTab)(globalId >> 40);
		return (tab, categoryId, optionId);
	}

	private static OptionDTO createDTO(OptionTab tab, OptionCategory category, IOption option)
	{
		List<OptionDTO>? childrenDTO = null;
		if (OptionManager.Instance.TryGetChild(option, out var children))
		{
			childrenDTO = new List<OptionDTO>();
			foreach (var child in children)
			{
				childrenDTO.Add(createDTO(tab, category, child));
			}
		}

		return new OptionDTO(
			ID: GetGlobalId(tab, category.Id, option.Info.Id),
			TransedName: option.TransedTitle,
			ValueRange: getValueRange(option),
			Selection: option.Selection,
			TransedFormat: Tr.GetString(option.Info.Format),
			Childs: childrenDTO
		);
	}

	private static object getValueRange(IOption option)
	{
		if (holderField == null) return option.Range;
		var holder = holderField.GetValue(option);
		if (holder == null) return option.Range;

		try
		{
			object rangeObj = holder;
			var innerRangeProp = holder.GetType().GetProperty("InnerRange");
			if (innerRangeProp != null)
			{
				var inner = innerRangeProp.GetValue(holder);
				if (inner != null) rangeObj = inner;
			}

			var optionField = rangeObj.GetType().GetField("option", BindingFlags.NonPublic | BindingFlags.Instance);
			if (optionField != null)
			{
				var values = optionField.GetValue(rangeObj);
				if (values is string[] strings)
				{
					var transedStrings = new string[strings.Length];
					for (int i = 0; i < strings.Length; i++)
					{
						transedStrings[i] = Tr.GetString(strings[i]);
					}
					return transedStrings;
				}
				return values;
			}
		}
		catch { }

		return option.Range;
	}
}
