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
using ExtremeSkins.Core.ExtremeHats;
using ExtremeSkins.SkinManager;

namespace ExtremeSkins.Module.ApiHandler.ExtremeHat;

public sealed class PutHatHandler : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		NewCosmicData newHat = IRequestHandler.DeserializeJson<NewCosmicData>(context.Request);

		JsonSerializerOptions options = new()
		{
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
		};
		using var jsonStream = new StreamReader(newHat.GetInfoJsonPath());
		HatInfo? info = JsonSerializer.Deserialize<HatInfo>(jsonStream.ReadToEnd(), options);
		var hatMng = FastDestroyableSingleton<HatManager>.Instance;

		if (info == null || hatMng == null)
		{
			IRequestHandler.SetStatusNG(response);
			response.Abort();
			return;
		}

		string folderPath = newHat.GetSkinFolderPath();

		CustomHat customHat = info.Animation == null ?
			new CustomHat(folderPath, info) : new AnimationHat(folderPath, info);
		string id = customHat.Id;

		if (!ExtremeHatManager.HatData.TryGetValue(id, out var hat))
		{
			IRequestHandler.SetStatusNG(response);
			response.Abort();
			return;
		}

		bool hasReloadHat =
			CachedPlayerControl.LocalPlayer != null &&
			CachedPlayerControl.LocalPlayer.PlayerControl.cosmetics.hat.Hat.ProductId == id;

		if (hasReloadHat)
		{
			int colorId = CachedPlayerControl.LocalPlayer!.Data.DefaultOutfit.ColorId;
			CachedPlayerControl.LocalPlayer!.PlayerControl.cosmetics.SetHat(
				HatData.EmptyId, colorId);
		}

		ExtremeHatManager.HatData[id] = customHat;

		List<HatData> hatData = hatMng.allHats.ToList();
		hatData.RemoveAll(x => x.ProductId == hat.Id);
		hatData.Add(customHat.Data);
		hatMng.allHats = hatData.ToArray();

		ExtremeSkinsPlugin.Logger.LogInfo($"Hat Reloaded :\n{customHat}");
		if (hasReloadHat)
		{
			int colorId = CachedPlayerControl.LocalPlayer!.Data.DefaultOutfit.ColorId;
			CachedPlayerControl.LocalPlayer!.PlayerControl.cosmetics.SetHat(id, colorId);
		}

		IRequestHandler.SetStatusOK(response);
		response.Close();
	}
}