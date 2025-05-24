using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;

namespace ExtremeRoles.Test.Lobby.Asset;

public class GuessorAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:GuesserLoad Test -----");

		LoadFromExR(ExtremeRoleId.Guesser);
		LoadUnityObjectFromExR<GameObject, ExtremeRoleId>(
			ExtremeRoleId.Guesser,
			ObjectPath.GetRolePrefabPath(ExtremeRoleId.Guesser, "UI"));
	}
}
