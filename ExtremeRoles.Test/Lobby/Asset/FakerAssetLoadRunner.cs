using ExtremeRoles.Resources;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Lobby.Asset;

public class FakerAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:FakerAsset Test -----");

		LoadFromExR(ExtremeRoleId.Faker, ObjectPath.FakerDummyPlayer);
		LoadFromExR(ExtremeRoleId.Faker, ObjectPath.FakerDummyDeadBody);
		yield break;
	}
}
