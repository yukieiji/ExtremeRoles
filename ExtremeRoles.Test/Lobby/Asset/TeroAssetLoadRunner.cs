using System;
using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;
using UnityEngine.Video;

namespace ExtremeRoles.Test.Lobby.Asset;

public class TeroAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:TeroLoad Test -----");

		LoadFromExR(ExtremeRoleId.Terorist);
		LoadFromExR(ExtremeRoleId.Terorist, ObjectPath.MapIcon);
		LoadUnityObjectFromExR<GameObject, ExtremeRoleId>(
			ExtremeRoleId.Terorist,
			ObjectPath.GetRoleMinigamePath(ExtremeRoleId.Terorist));
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
