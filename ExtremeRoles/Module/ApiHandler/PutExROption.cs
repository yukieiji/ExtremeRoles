using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct ExROptionPutRequest(int TabId, int CategoryId, int OptionId, int Selection);
public readonly record struct CategoryOptionDto(int Id, IReadOnlyList<ExROptionDto> Options);
public readonly record struct UpdatedOptions(ExRCategoryDto? UpdatedCategory, IReadOnlyList<CategoryOptionDto> ChainUpdatedOption);

public sealed class PutExROption : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		if (AmongUsClient.Instance == null ||
			!AmongUsClient.Instance.AmHost ||
			LobbyBehaviour.Instance == null ||
			GameOptionsManager.Instance == null ||
			GameOptionsManager.Instance.CurrentGameOptions == null)
		{
			IRequestHandler.SetStatusNG(response);
			response.Close();
			return;
		}

		var newOptionSelection = IRequestHandler.DeserializeJson<ExROptionPutRequest>(context.Request);

		var tab = (OptionTab)newOptionSelection.TabId;
		int categoryId = newOptionSelection.CategoryId;
		int optionId = newOptionSelection.OptionId;

		if (!OptionManager.Instance.TryGetCategory(tab, categoryId, out var category))
		{
			response.StatusCode = (int)HttpStatusCode.BadRequest;
			response.Close();
			return;
		}

		// プリセットは全更新が入るので再度Getで再レンダリングをかける
		if (PresetOption.IsPreset(categoryId, optionId))
		{
			OptionManager.Instance.Update(tab, categoryId, optionId, newOptionSelection.Selection);
			response.StatusCode = (int)HttpStatusCode.Accepted;
			response.Close();
			return;
		}

		// それ以外はレコードして返す
		using var recordResult = OptionUpdateRecorder.Instance.StartRecord();
		OptionManager.Instance.Update(tab, categoryId, optionId, newOptionSelection.Selection);

		var updatedCategory = GetExrOption.CreateCategoryDto(category);

		recordResult.Result.Remove(category.Id);

		IRequestHandler.SetStatusOK(response);
		IRequestHandler.Write(response, new UpdatedOptions(
			updatedCategory,
			recordResult.Result.Select(x =>
			{
				var registed = new HashSet<int>();
				var options = x.Value.Select(x => GetExrOption.CreateOptionDto(x, registed));

				return new CategoryOptionDto(x.Key, options.ToList());
			}).ToList())
		);
	}
}
