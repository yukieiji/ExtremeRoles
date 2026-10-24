using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;
using UnityEngine.Video;

namespace ExtremeRoles.Test.Lobby.Asset;

public class ItakoAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:ItakoLoad Test -----");

		LoadFromExR(ExtremeRoleId.Itako);
		yield break;
	}
}
