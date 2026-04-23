using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;

using AmongUs.GameOptions;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.Interface;
namespace ExtremeRoles.Module.ApiHandler;

public enum OptionValueType
{
	Bool,
	Byte,
	Int,
	UInt,
	Float,
	RoleBase,
}

public readonly record struct VanillaOptionPutRequest(OptionValueType ValueType, int OptionName, JsonElement NewValue);

public sealed class PutAuOption : IRequestHandler
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

		var newOption = IRequestHandler.DeserializeJson<VanillaOptionPutRequest>(context.Request);
		using var recordResult = OptionUpdateRecorder.Instance.StartRecord();

		var curOption = GameOptionsManager.Instance.CurrentGameOptions;
		int name = newOption.OptionName;
		var val = newOption.NewValue; 

		switch (newOption.ValueType)
		{
			case OptionValueType.Bool:
				curOption.SetBool((BoolOptionNames)name, val.GetBoolean());
				break;
			case OptionValueType.Byte:
				var strName = (ByteOptionNames)name;
				byte value = val.GetByte();
				curOption.SetByte(strName, value);
				updateMapButton(strName, value); // マップボタンだけ特殊なので
				break;
			case OptionValueType.Int:
				curOption.SetInt((Int32OptionNames)name, val.GetInt32());
				break;
			case OptionValueType.UInt:
				curOption.SetUInt((UInt32OptionNames)name, val.GetUInt32());
				break;
			case OptionValueType.Float:
				curOption.SetFloat((FloatOptionNames)name, val.GetSingle());
				break;
			case OptionValueType.RoleBase:
				if (curOption.RoleOptions == null)
				{
					IRequestHandler.SetStatusNG(response);
					response.Close();
					return;
				}
				RoleTypes roleName = (RoleTypes)name;
				int maxCount = val.GetProperty("Count").GetInt32();
				int roleChance = val.GetProperty("Chance").GetInt32();
				curOption.RoleOptions.SetRoleRate(roleName, maxCount, roleChance);
				curOption.SetInt(Int32OptionNames.RulePreset, 100);
				curOption.SetBool(BoolOptionNames.IsDefaults, false);
				break;
			default:
				IRequestHandler.SetStatusNG(response);
				response.Close();
				return;
		}

		var menu = GameSettingMenu.Instance;
		if (menu != null)
		{
			if (menu.GameSettingsTab.isActiveAndEnabled)
			{
				menu.GameSettingsTab.RefreshChildren();
			}
			if (menu.RoleSettingsTab.isActiveAndEnabled)
			{
				menu.RoleSettingsTab.RefreshChildren();
			}
		}

		GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
		if (GameManager.Instance != null &&
			GameManager.Instance.LogicOptions != null)
		{
			GameManager.Instance.LogicOptions.SyncOptions();
		}

		IRequestHandler.SetStatusOK(response);
		IRequestHandler.Write(response, new UpdatedOptions(
			null,
			recordResult.Result.Select(x =>
			{
				var registed = new HashSet<int>();
				var options = x.Value.Select(x => GetExrOption.CreateOptionDto(x, registed));

				return new CategoryOptionDto(x.Key, options.ToList());
			}).ToList())
		);
	}
	private void updateMapButton(ByteOptionNames strName, byte value)
	{
		if (GameSettingMenu.Instance == null ||
			strName is not ByteOptionNames.MapId)
		{
			return;
		}

		var mapOptionButton = GameSettingMenu.Instance.GameSettingsTab.Children.Find(
			(Il2CppSystem.Predicate<OptionBehaviour>)(x => x.TryCast<GameOptionsMapPicker>() != null));
		if (!mapOptionButton.IsTryCast<GameOptionsMapPicker>(out var mapPicker))
		{
			return;
		}

		foreach (var mapSelectButton in mapPicker.mapButtons)
		{
			if (mapSelectButton.MapID == value)
			{
				mapSelectButton.Button.SelectButton(true);
				mapPicker.selectedButton = mapSelectButton;
				mapPicker.SelectMap(value);
				mapPicker.oldValue = value;
			}
			else
			{
				mapSelectButton.Button.SelectButton(false);
			}
		}
	}
}
