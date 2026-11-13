using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;
using UnityEngine.Video;

namespace ExtremeRoles.Test.Lobby.Asset;

public class FlasherAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:FlasherLoad Test -----");
		LoadFromExR(ExtremeRoleId.Flasher);
		NullCheck(Sound.GetAudio(Sound.Type.FlasherFlash));
		yield break;
	}
}
