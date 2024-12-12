using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Asset;

internal sealed class RaiderAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:RaiderAsset Test -----");

		LoadFromExR(ExtremeRoleId.Raider);
	}
}
