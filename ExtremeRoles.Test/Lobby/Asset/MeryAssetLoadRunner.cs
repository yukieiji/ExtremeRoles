using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;

namespace ExtremeRoles.Test.Lobby.Asset;

public class MeryAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:MeryImgLoad Test -----");

		for (int index = 0; index < 18; ++index)
		{
			LoadFromExR(ExtremeRoleId.Mery, $"{index}");
		}

		LoadFromExR(
			ExtremeRoleId.Mery,
			ObjectPath.MeryNoneActive);

		LoadFromExR(ExtremeRoleId.Mery);
		yield break;
	}
}
