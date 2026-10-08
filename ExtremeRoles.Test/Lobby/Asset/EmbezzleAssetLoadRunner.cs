using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;
using UnityEngine.Video;

namespace ExtremeRoles.Test.Lobby.Asset;

public class EmbezzleAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:EmbezzleLoad Test -----");
		LoadFromExR(ExtremeRoleId.Embezzle);
		yield break;
	}
}
