using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;

namespace ExtremeRoles.Test.Asset;

internal sealed class GuessorAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:GuesserLoad Test -----");

		LoadFromExR(ExtremeRoleId.Guesser);
		LoadUnityObjectFromExR<GameObject, ExtremeRoleId>(
			ExtremeRoleId.Guesser,
			Path.GetRolePrefabPath(ExtremeRoleId.Guesser, "UI"));
	}
}
