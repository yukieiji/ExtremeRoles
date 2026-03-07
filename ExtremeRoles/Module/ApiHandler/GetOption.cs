using System;
using System.Collections.Generic;
using System.Net;

using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct OptionDto(
	int Id,
	bool IsActive,
	string TransedName,
	int Selection,
	string Format,
	IOptionRangeMeta RangeMeta,
	IReadOnlyList<OptionDto> Childs);

public readonly record struct CategoryDto(int Id, string Name, IReadOnlyList<OptionDto> Options);
public readonly record struct TabDto(OptionTab Id, string Name, IReadOnlyList<CategoryDto> Categories);

public readonly record struct PutOptionRequest(long ID, int Value);

public sealed class GetOption : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);
		IRequestHandler.Write(response, getCurrentOptions());
	}

	private static IReadOnlyList<TabDto> getCurrentOptions()
	{
		var result = new List<TabDto>();
		foreach (var tabId in Enum.GetValues<OptionTab>())
		{
			var categories = new List<CategoryDto>();
			if (!OptionManager.Instance.TryGetTab(tabId, out var container))
			{
				continue;
			}
			foreach (var category in container.Category)
			{
				var options = new List<OptionDto>();
				var idHash = new HashSet<int>();
				foreach (var option in category.Options)
				{
					if (idHash.Contains(option.Info.Id))
					{
						continue;
					}
					var dto = createOptionDTO(option, idHash);
					options.Add(dto);
				}
				var categoryDto = new CategoryDto(category.Id, category.TransedName, options);
				categories.Add(categoryDto);
			}
			var tabDtos = new TabDto(tabId, Tr.GetString(tabId.ToString()), categories);
			result.Add(tabDtos);
		}
		return result;
	}

	private static OptionDto createOptionDTO(IOption option, HashSet<int> regsted)
	{
		var childsResult = new List<OptionDto>();
		if (OptionManager.Instance.TryGetChild(option, out var childs))
		{
			foreach (var child in childs)
			{
				regsted.Add(option.Info.Id);
				childsResult.Add(createOptionDTO(child, regsted));
			}
		}
		return new OptionDto(
			option.Info.Id,
			option.IsViewActive,
			option.TransedTitle,
			option.Selection,
			option.Info.Format,
			option.RangeMetaData,
			childsResult);
	}
}
