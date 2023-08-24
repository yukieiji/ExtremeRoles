using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;
using ExtremeSkins.Core.API;
using ExtremeSkins.SkinManager;
using ExtremeSkins.Helper;

namespace ExtremeSkins.Module.ApiHandler.ExtremeNamePlate;

public sealed class PutNamePlateHandler : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		NewCosmicData newNamePate = IRequestHandler.DeserializeJson<NewCosmicData>(context.Request);

		var hatMng = FastDestroyableSingleton<HatManager>.Instance;

		if (hatMng == null)
		{
			IRequestHandler.SetStatusNG(response);
			response.Abort();
			return;
		}

		CustomNamePlate customNamePlate = new CustomNamePlate(
			Path.Combine(newNamePate.ParentPath, newNamePate.AutherName),
			newNamePate.AutherName, newNamePate.SkinName);

		Translation.AddTransData(customNamePlate.Author, newNamePate.TransedAutherName);
		Translation.AddTransData(customNamePlate.Name, newNamePate.TransedSkinName);

		string id = customNamePlate.Id;

		if (!ExtremeNamePlateManager.NamePlateData.TryGetValue(id, out var np))
		{
			IRequestHandler.SetStatusNG(response);
			response.Abort();
			return;
		}

		bool hasReloadNamePlate =
			CachedPlayerControl.LocalPlayer != null &&
			CachedPlayerControl.LocalPlayer.PlayerControl.Data.DefaultOutfit.NamePlateId == id;

		if (hasReloadNamePlate)
		{
			CachedPlayerControl.LocalPlayer!.PlayerControl.RpcSetNamePlate(NamePlateData.EmptyId);
		}

		ExtremeNamePlateManager.NamePlateData[id] = customNamePlate;

		List<NamePlateData> namePlateData = hatMng.allNamePlates.ToList();
		namePlateData.RemoveAll(x => x.ProductId == id);
		namePlateData.Add(customNamePlate.GetData());
		hatMng.allNamePlates = namePlateData.ToArray();

		ExtremeSkinsPlugin.Logger.LogInfo($"NamePlate Reloaded :\n{customNamePlate}");
		if (hasReloadNamePlate)
		{
			CachedPlayerControl.LocalPlayer!.PlayerControl.RpcSetNamePlate(id);
		}

		IRequestHandler.SetStatusOK(response);
		response.Close();
	}
}