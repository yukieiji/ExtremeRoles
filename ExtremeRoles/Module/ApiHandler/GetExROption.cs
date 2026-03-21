using System;
using System.Collections.Generic;
using System.Net;

using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct ExROptionDto(
	int Id,
	bool IsActive,
	string TranslatedName,
	int Selection,
	string Format,
	IOptionRangeMeta RangeMeta,
	IReadOnlyList<ExROptionDto> Childs);

public readonly record struct ExRCategoryDto(int Id, string Name, IReadOnlyList<ExROptionDto> Options);
public readonly record struct ExRTabDto(OptionTab Id, string Name, IReadOnlyList<ExRCategoryDto> Categories);

public sealed class GetExrOption : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);
		IRequestHandler.Write(response, getCurrentOptions());
	}

	private static IReadOnlyList<ExRTabDto> getCurrentOptions()
	{
		var result = new List<ExRTabDto>();
		foreach (var tabId in Enum.GetValues<OptionTab>())
		{
			var categories = new List<ExRCategoryDto>();
			if (!OptionManager.Instance.TryGetTab(tabId, out var container))
			{
				continue;
			}
			foreach (var category in container.Category)
			{
				var categoryDto = CreateCategoryDto(category);
				categories.Add(categoryDto);
			}
			var tabDtos = new ExRTabDto(tabId, Tr.GetString(tabId.ToString()), categories);
			result.Add(tabDtos);
		}
		return result;
	}

	public static ExRCategoryDto CreateCategoryDto(OptionCategory category)
	{
		var options = new List<ExROptionDto>();
		var idHash = new HashSet<int>();
		foreach (var option in category.Options)
		{
			if (idHash.Contains(option.Info.Id))
			{
				continue;
			}
			var dto = CreateOptionDto(option, idHash);
			options.Add(dto);
		}
		var categoryDto = new ExRCategoryDto(category.Id, category.TransedName, options);
		return categoryDto;
	}

	public static ExROptionDto CreateOptionDto(IOption option, HashSet<int> registered)
	{
		var childsResult = new List<ExROptionDto>();
		if (OptionManager.Instance.TryGetChild(option, out var childs))
		{
			foreach (var child in childs)
			{
				registered.Add(option.Info.Id);
				childsResult.Add(CreateOptionDto(child, registered));
			}
		}
		return new ExROptionDto(
			option.Info.Id,
			option.IsViewActive,
			option.TransedTitle,
			option.Selection,
			option.Info.Format,
			option.RangeMetaData,
			childsResult);
	}
}
