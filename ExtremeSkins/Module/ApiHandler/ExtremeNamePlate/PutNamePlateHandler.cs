﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;
using ExtremeSkins.Core.API;
using ExtremeSkins.Helper;

namespace ExtremeSkins.Module.ApiHandler.ExtremeNamePlate;

#if WITHNAMEPLATE
public sealed class PutNamePlateHandler : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		NewCosmicData newNamePate = IRequestHandler.DeserializeJson<NewCosmicData>(context.Request);

		var hatMng = HatManager.Instance;

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

		if (!CosmicStorage<CustomNamePlate>.TryGet(id, out var np))
		{
			IRequestHandler.SetStatusNG(response);
			response.Abort();
			return;
		}

		bool hasReloadNamePlate =
			PlayerControl.LocalPlayer != null &&
			PlayerControl.LocalPlayer.Data.DefaultOutfit.NamePlateId == id;

		if (hasReloadNamePlate)
		{
			PlayerControl.LocalPlayer!.RpcSetNamePlate(NamePlateData.EmptyId);
		}

		CosmicStorage<CustomNamePlate>.TryAdd(id, customNamePlate);

		List<NamePlateData> namePlateData = hatMng.allNamePlates.ToList();
		namePlateData.RemoveAll(x => x.ProductId == id);
		namePlateData.Add(customNamePlate.GetData());
		hatMng.allNamePlates = namePlateData.ToArray();

		ExtremeSkinsPlugin.Logger.LogInfo($"NamePlate Reloaded :\n{customNamePlate}");
		if (hasReloadNamePlate)
		{
			PlayerControl.LocalPlayer!.RpcSetNamePlate(id);
		}

		IRequestHandler.SetStatusOK(response);
		response.Close();
	}
}
#endif