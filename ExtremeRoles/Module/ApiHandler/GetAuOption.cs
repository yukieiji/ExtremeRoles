using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using AmongUs.GameOptions;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Extension.Manager;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.Interface;


namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct AuRoleOption(int MaxCount, int Chance);

public readonly record struct AuOptionInfo(OptionValueType ValueType, int OptionName);

public readonly record struct AuOptionDto(string TranslatedTitle, string TranslatedFormat, object Value, AuOptionInfo Info, object[]? Range);

public readonly record struct AuOptionCategoryDto(string TranslatedTitle, IReadOnlyList<AuOptionDto> Options);

public sealed class GetAuOption : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		if (LobbyBehaviour.Instance == null ||
			GameManager.Instance == null ||
			GameOptionsManager.Instance == null ||
			GameOptionsManager.Instance.currentGameOptions == null ||
			GameOptionsManager.Instance.gameOptionsFactory == null)
		{
			IRequestHandler.SetStatusNG(response);
			response.Close();
			return;
		}

		var curGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
		List<AuOptionCategoryDto> result = [
			new AuOptionCategoryDto(
				"map", [
				new AuOptionDto("map", "", curGameOptions.GetByte(ByteOptionNames.MapId),
				new AuOptionInfo(OptionValueType.Byte, (int)ByteOptionNames.MapId),
				[ 0, 1, 2, 4, 5 ])])];

		var tr = TranslationController.Instance;

		foreach (var category in GameManager.Instance.GameSettingsList.AllCategories)
		{
			var options = new List<AuOptionDto>(category.AllGameSettings.Count);
			foreach (var baseGameSetting in category.AllGameSettings)
			{
				var dto = convertAuOptionDto(baseGameSetting);
				if (dto.HasValue)
				{
					options.Add(dto.Value);
				}
			}
			result.Add(new AuOptionCategoryDto(tr.GetString(category.CategoryName), options));
		}

		if (curGameOptions.RoleOptions != null)
		{
			foreach (var role in RoleManager.Instance.AllRoles)
			{
				var options = new List<AuOptionDto>(role.AllGameSettings.Count + 1);
				
				int count =　curGameOptions.RoleOptions.GetNumPerGame(role.Role);
				int chance = curGameOptions.RoleOptions.GetChancePerGame(role.Role);

				options.Add(
					new AuOptionDto("DefaultOption", "",
					new AuRoleOption(count, chance),
					new AuOptionInfo(OptionValueType.RoleBase, (int)role.Role),
					null));
				foreach (var baseGameSetting in role.AllGameSettings)
				{
					var dto = convertAuOptionDto(baseGameSetting);
					if (dto.HasValue)
					{
						options.Add(dto.Value);
					}
				}
				result.Add(new AuOptionCategoryDto(tr.GetString(role.StringName), options));
			}
		}

		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);
		IRequestHandler.Write(response, result);
	}

	private static AuOptionDto? convertAuOptionDto(BaseGameSetting setting)
	{
		var tr = TranslationController.Instance;
		var emptyArr = Array.Empty<Il2CppSystem.Object>();
		string title = tr.GetString(setting.Title, emptyArr);
		var curGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
		switch (setting.Type)
		{
			case OptionTypes.Checkbox:
				if (!setting.IsTryCast<CheckboxGameSetting>(out var checkbox))
				{
					return null;
				}
				bool isEnable = curGameOptions.GetValue(checkbox) == 1.0f;
				return new AuOptionDto(
					title, "", isEnable,
					new(OptionValueType.Bool, (int)checkbox.OptionName), null);
			case OptionTypes.String:
				if (!setting.IsTryCast<StringGameSetting>(out var @string))
				{
					return null;
				}
				int selected = (int)curGameOptions.GetValue(@string);
				return new AuOptionDto(
					title, "", selected,
					new(OptionValueType.Int, (int)@string.OptionName),
					@string.Values.Select(x => (object)tr.GetString(x, emptyArr)).ToArray());
			case OptionTypes.Float:
				if (!setting.IsTryCast<FloatGameSetting>(out var @float))
				{
					return null;
				}
				float floatValue = curGameOptions.GetValue(@float);
				var floatRange = OptionRange<float>.GetFloatRange(
					@float.ValidRange.min,
					@float.ValidRange.max,
					@float.Increment);
				return new AuOptionDto(
					title, getSuffix(@float.SuffixType),
					floatValue,
					new(OptionValueType.Float, (int)@float.OptionName),
					floatRange.Select(x => (object)(
						x == 0.0f && @float.ZeroIsInfinity ?
							float.MaxValue : x)).ToArray());
			case OptionTypes.Int:
				if (!setting.IsTryCast<IntGameSetting>(out var @int))
				{
					return null;
				}
				int intValue = (int)curGameOptions.GetValue(@int);

				int max = @int.ValidRange.max;
				int min = @int.ValidRange.min;
				if (@int.Title == StringNames.GameNumImpostors)
				{
					if (AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame ||
						ServerManager.Instance.IsCustomServer())
					{
						min = 0;
						max = GameSystem.MaxImposterNum;
					}
					else
					{
						int playerNum = curGameOptions.GetInt(Int32OptionNames.MaxPlayers);
						int[] maxImps = curGameOptions.GetIntArray(Int32ArrayOptionNames.MaxImpostors);
						max = maxImps[playerNum];
					}
				}
				var intRange = OptionRange<float>.GetFloatRange(
					min, max, @int.Increment);
				return new AuOptionDto(
					title, getSuffix(@int.SuffixType), intValue,
					new(OptionValueType.Int, (int)@int.OptionName),
					intRange.Select(x => (object)(
						x == 0 && @int.ZeroIsInfinity ?
							float.MaxValue : x)).ToArray());
			case OptionTypes.Player:
				if (!setting.IsTryCast<PlayerSelectionGameSetting>(out var @player))
				{
					return null;
				}

				int playerId = curGameOptions.GetInt(Int32OptionNames.ImpostorPlayerID);
				var allPlayer = GameData.Instance.AllPlayers.ToArray().ToList();
				int targetIndex = allPlayer.FindIndex(x => x.PlayerId == playerId);
				return new AuOptionDto(
					title, "", targetIndex,
					new(OptionValueType.Int, (int)@player.OptionName),
					allPlayer.Select(x => (object)x.DefaultOutfit.PlayerName).ToArray());
			default:
				return null;
		}
	}
	private static string getSuffix(NumberSuffixes suffixes)
		=> suffixes switch {
			NumberSuffixes.Multiplier => "x",
			NumberSuffixes.Seconds => "second",
			_ => "",
		};
}
