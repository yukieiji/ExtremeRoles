using System;
using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;
using UnityEngine.Video;

namespace ExtremeRoles.Test.Asset;

internal sealed class TeroAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:TeroLoad Test -----");

		LoadFromExR(ExtremeRoleId.Terorist);
		LoadFromExR(ExtremeRoleId.Terorist, Path.MapIcon);
		LoadUnityObjectFromExR<GameObject, ExtremeRoleId>(
			ExtremeRoleId.Terorist,
			Path.GetRoleMinigamePath(ExtremeRoleId.Terorist));
		try
		{

			var clip = Sound.GetAudio(Sound.Type.TeroristSabotageAnnounce);
			Log.LogInfo($"clip loadings: {Sound.Type.TeroristSabotageAnnounce}");
			NullCheck(clip);
		}
		catch (Exception ex)
		{
			Log.LogError($"Se not load   {ex.Message}");
		}
	}
}
