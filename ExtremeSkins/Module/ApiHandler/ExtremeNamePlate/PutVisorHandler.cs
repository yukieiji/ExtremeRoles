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

namespace ExtremeSkins.Module.ApiHandler.ExtremeNamePlate;

public sealed class PutNamePlateHandler : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		InfoData newHat = IRequestHandler.DeserializeJson<InfoData>(context.Request);

		JsonSerializerOptions options = new()
		{
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
		};
		using var jsonStream = new StreamReader(newHat.InfoJsonPath);
		VisorInfo? info = JsonSerializer.Deserialize<VisorInfo>(jsonStream.ReadToEnd(), options);
		var hatMng = FastDestroyableSingleton<HatManager>.Instance;

		if (info == null || hatMng == null)
		{
			IRequestHandler.SetStatusNG(response);
			response.Abort();
			return;
		}

		string folderPath = newHat.FolderPath;

		CustomVisor customVisor = new CustomVisor(folderPath, info);
		string id = customVisor.Id;

		if (!ExtremeVisorManager.VisorData.TryGetValue(id, out var visor))
		{
			IRequestHandler.SetStatusNG(response);
			response.Abort();
			return;
		}

		bool hasReloadVisor =
			CachedPlayerControl.LocalPlayer != null &&
			CachedPlayerControl.LocalPlayer.PlayerControl.cosmetics.visor.currentVisor.ProductId == id;

		if (hasReloadVisor)
		{
			int colorId = CachedPlayerControl.LocalPlayer!.Data.DefaultOutfit.ColorId;
			CachedPlayerControl.LocalPlayer!.PlayerControl.cosmetics.SetHat(
				HatData.EmptyId, colorId);
		}

		ExtremeVisorManager.VisorData[id] = customVisor;

		List<VisorData> visorData = hatMng.allVisors.ToList();
		visorData.RemoveAll(x => x.ProductId == visor.Id);
		visorData.Add(customVisor.GetData());
		hatMng.allVisors = visorData.ToArray();

		ExtremeSkinsPlugin.Logger.LogInfo($"Visor Reloaded :\n{customVisor}");
		if (hasReloadVisor)
		{
			int colorId = CachedPlayerControl.LocalPlayer!.Data.DefaultOutfit.ColorId;
			CachedPlayerControl.LocalPlayer!.PlayerControl.cosmetics.SetVisor(id, colorId);
		}

		IRequestHandler.SetStatusOK(response);
		response.Close();
	}
}