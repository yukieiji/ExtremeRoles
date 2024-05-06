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

public sealed class PutVisorHandler : IRequestHandler
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
		var hatMng = FastDestroyableSingleton<HatManager>.Instance;

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

		string id = customVisor.Id;

		if (CosmicStorage<CustomVisor>.TryGet(id, out var visor))
		{
			IRequestHandler.SetStatusNG(response);
			response.Abort();
			return;
		}

		bool hasReloadVisor =
			CachedPlayerControl.LocalPlayer != null &&
			CachedPlayerControl.LocalPlayer.PlayerControl.cosmetics.visor.visorData.ProductId == id;

		if (hasReloadVisor)
		{
			CachedPlayerControl.LocalPlayer!.PlayerControl.RpcSetVisor(VisorData.EmptyId);
		}

		CosmicStorage<CustomVisor>.TryAdd(id, customVisor);

		List<VisorData> visorData = hatMng.allVisors.ToList();
		visorData.RemoveAll(x => x.ProductId == id);
		visorData.Add(customVisor.Data);
		hatMng.allVisors = visorData.ToArray();

		ExtremeSkinsPlugin.Logger.LogInfo($"Visor Reloaded :\n{customVisor}");
		if (hasReloadVisor)
		{
			CachedPlayerControl.LocalPlayer!.PlayerControl.RpcSetVisor(id);
		}

		IRequestHandler.SetStatusOK(response);
		response.Close();
	}
}