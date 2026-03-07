using AmongUs.GameOptions;
using ExtremeRoles.Module.Interface;
using System;
using System.Net;
using System.Text.Json;

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
		var newOption = IRequestHandler.DeserializeJson<VanillaOptionPutRequest>(context.Request);

		if (LobbyBehaviour.Instance == null ||
			GameOptionsManager.Instance == null ||
			GameOptionsManager.Instance.CurrentGameOptions == null)
		{
			IRequestHandler.SetStatusNG(response);
			response.Close();
			return;
		}

		var curOption = GameOptionsManager.Instance.CurrentGameOptions;
		int name = newOption.OptionName;
		var val = newOption.NewValue; 

		switch (newOption.ValueType)
		{
			case OptionValueType.Bool:
				curOption.SetBool((BoolOptionNames)name, val.GetBoolean());
				break;
			case OptionValueType.Byte:
				curOption.SetByte((ByteOptionNames)name, val.GetByte());
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
		response.Close();
	}
}
