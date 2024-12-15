using ExtremeRoles.Resources;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Asset;

internal sealed class FakerAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:FakerAsset Test -----");

		LoadFromExR(ExtremeRoleId.Faker, ObjectPath.FakerDummyPlayer);
		LoadFromExR(ExtremeRoleId.Faker, ObjectPath.FakerDummyDeadBody);
	}
}
