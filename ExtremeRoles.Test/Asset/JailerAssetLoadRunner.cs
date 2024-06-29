using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Asset;

internal sealed class JailerAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:JailerAsset Test -----");

		LoadFromExR(ExtremeRoleId.Jailer);
	}
}
