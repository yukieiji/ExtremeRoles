using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Asset;

internal sealed class HijackerAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:HijackerAsset Test -----");

		LoadFromExR(ExtremeRoleId.Hijacker);
	}
}
