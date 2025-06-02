using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;
using UnityEngine.Video;

namespace ExtremeRoles.Test.Lobby.Asset;

public class ThiefAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:ThiefLoad Test -----");

		LoadFromExR(ExtremeRoleId.Thief);
		LoadFromExR(ExtremeRoleId.Thief, ObjectPath.MapIcon);
		LoadUnityObjectFromExR<VideoClip, ExtremeRoleId>(
			ExtremeRoleId.Thief,
			ObjectPath.GetRoleVideoPath(ExtremeRoleId.Thief));
		yield break;
	}
}
