using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;
using UnityEngine.Video;

namespace ExtremeRoles.Test.Lobby.Asset;

public class YokoAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:YokofLoad Test -----");

		LoadFromExR(ExtremeRoleId.Yoko);
		LoadUnityObjectFromExR<GameObject, ExtremeRoleId>(
			ExtremeRoleId.Yoko,
			ObjectPath.GetRoleMinigamePath(ExtremeRoleId.Yoko));
		yield break;
	}
}
