using System;
using System.Net;
using System.Text.Json;

using ExtremeRoles.Module.Interface;
using AmongUs.GameOptions;

namespace ExtremeRoles.Module.ApiHandler;

public enum OptionValueType
{
	Bool,
	Byte,
	Int,
	UInt,
	Float,
}

public readonly record struct VanillaOptionPutRequest(OptionValueType ValueType, int OptionName, JsonElement NewValue);

public sealed class PutAuOption : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		var newOption = IRequestHandler.DeserializeJson<VanillaOptionPutRequest>(context.Request);

		if (GameOptionsManager.Instance == null ||
			GameOptionsManager.Instance.currentGameOptions == null)
		{
			IRequestHandler.SetStatusNG(response);
			response.Close();
			return;
		}

		var curOption = GameOptionsManager.Instance.currentGameOptions;

		switch (newOption.ValueType)
		{
			case OptionValueType.Bool:
				curOption.SetBool((BoolOptionNames)newOption.OptionName, newOption.NewValue.GetBoolean());
				break;
			case OptionValueType.Byte:
				curOption.SetByte((ByteOptionNames)newOption.OptionName, newOption.NewValue.GetByte());
				break;
			case OptionValueType.Int:
				curOption.SetInt((Int32OptionNames)newOption.OptionName, newOption.NewValue.GetInt32());
				break;
			case OptionValueType.UInt:
				curOption.SetUInt((UInt32OptionNames)newOption.OptionName, newOption.NewValue.GetUInt32());
				break;
			case OptionValueType.Float:
				curOption.SetFloat((FloatOptionNames)newOption.OptionName, newOption.NewValue.GetSingle());
				break;
			default:
				IRequestHandler.SetStatusNG(response);
				response.Close();
				return;
		}

		IRequestHandler.SetStatusOK(response);
		response.Close();
	}
}
