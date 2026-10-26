using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;
using UnityEngine.Video;

namespace ExtremeRoles.Test.Lobby.Asset;

public class ChargerAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:ChargerLoad Test -----");
		LoadFromExR(ExtremeRoleId.Charger);
		NullCheck(Sound.GetAudio(Sound.Type.ChargerCharge));
		yield break;
	}
}
