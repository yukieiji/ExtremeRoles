using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;
using UnityEngine.Video;

namespace ExtremeRoles.Test.Lobby.Asset;

public class YoYoAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:YoYoLoad Test -----");
		LoadButtonIconFromExR(ExtremeRoleId.YoYo, "Mark");
		LoadButtonIconFromExR(ExtremeRoleId.YoYo, "Teleport");
		yield break;
	}
}
