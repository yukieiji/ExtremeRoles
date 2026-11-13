using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;
using UnityEngine.Video;

namespace ExtremeRoles.Test.Lobby.Asset;

public class MayorAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:MayorLoad Test -----");
		LoadFromExR(ExtremeRoleId.Mayor);
		yield break;
	}
}
