using ExtremeRoles.Roles;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Test.Lobby.Asset;

public class RaiderAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:RaiderAsset Test -----");

		LoadFromExR(ExtremeRoleId.Raider);
		LoadFromExR(ExtremeRoleId.Raider, ObjectPath.Bomb);
		yield break;
	}
}
