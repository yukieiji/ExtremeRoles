using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;
using UnityEngine.Video;

namespace ExtremeRoles.Test.Lobby.Asset;

public class DeltaAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:DeltaLoad Test -----");

		LoadFromExR(ExtremeRoleId.Delta);
		yield break;
	}
}
