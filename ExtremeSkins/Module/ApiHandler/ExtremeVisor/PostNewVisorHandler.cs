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
using ExtremeSkins.Helper;

namespace ExtremeSkins.Module.ApiHandler.ExtremeVisor;
#if WITHVISOR
public sealed class PostNewVisorHandler : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		NewCosmicData newVisor = IRequestHandler.DeserializeJson<NewCosmicData>(context.Request);

		JsonSerializerOptions options = new()
		{
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
		};
		using var jsonStream = new StreamReader(newVisor.GetInfoJsonPath());
		VisorInfo? info = JsonSerializer.Deserialize<VisorInfo>(jsonStream.ReadToEnd(), options);
		var hatMng = HatManager.Instance;

		if (info == null || hatMng == null)
		{
			IRequestHandler.SetStatusNG(response);
			response.Abort();
			return;
		}
		string folderPath = newVisor.GetSkinFolderPath();
		CustomVisor customVisor = info.Animation == null ?
			new CustomVisor(folderPath, info) : new AnimationVisor(folderPath, info);

		Translation.AddTransData(newVisor.AutherName, newVisor.TransedAutherName);
		Translation.AddTransData(newVisor.SkinName  , newVisor.TransedSkinName);

		if (CosmicStorage<CustomVisor>.TryAdd(customVisor.Id, customVisor))
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
		visorData.Add(customVisor.Data);
		hatMng.allVisors = visorData.ToArray();

		IRequestHandler.SetStatusOK(response);
		response.Close();
	}
}
#endif