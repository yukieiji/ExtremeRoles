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

public sealed class PostNewNamePlateHandler : IRequestHandler
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
		Translation.AddTransData(customNamePlate.Name  , newNamePate.TransedSkinName);

		if (SkinContainer<CustomNamePlate>.TryAdd(customNamePlate.Id, customNamePlate))
		{
			ExtremeSkinsPlugin.Logger.LogInfo($"NamePlate Loaded :\n{customNamePlate}");
		}
		else
		{
			IRequestHandler.SetStatusNG(response);
			response.Abort();
			return;
		}
		List<NamePlateData> namePlateData = hatMng.allNamePlates.ToList();
		namePlateData.Add(customNamePlate.GetData());
		hatMng.allNamePlates = namePlateData.ToArray();

		IRequestHandler.SetStatusOK(response);
		response.Close();
	}
}