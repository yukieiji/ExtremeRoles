using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;
using ExtremeSkins.Core.API;
using ExtremeSkins.Core.ExtremeVisor;
using ExtremeSkins.SkinManager;

namespace ExtremeSkins.Module.ApiHandler.ExtremeVisor;

public sealed class PostNewVisorHandler : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		InfoData newVisor = IRequestHandler.DeserializeJson<InfoData>(context.Request);

		JsonSerializerOptions options = new()
		{
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
		};
		using var jsonStream = new StreamReader(newVisor.InfoJsonPath);
		VisorInfo? info = JsonSerializer.Deserialize<VisorInfo>(jsonStream.ReadToEnd(), options);
		var hatMng = FastDestroyableSingleton<HatManager>.Instance;

		if (info == null || hatMng == null)
		{
			IRequestHandler.SetStatusNG(response);
			response.Abort();
			return;
		}
		string folderPath = newVisor.FolderPath;
		CustomVisor customVisor = new CustomVisor(folderPath, info);
		if (ExtremeVisorManager.VisorData.TryAdd(customVisor.Id, customVisor))
		{
			ExtremeSkinsPlugin.Logger.LogInfo($"Visor Loaded :\n{customVisor}");
		}
		else
		{
			IRequestHandler.SetStatusNG(response);
			response.Abort();
			return;
		}
		List<VisorData> visorData = hatMng.allVisors.ToList();
		visorData.Add(customVisor.GetData());
		hatMng.allVisors = visorData.ToArray();

		IRequestHandler.SetStatusOK(response);
		response.Close();
	}
}