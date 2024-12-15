using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Asset;

internal sealed class TimeBreakerAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:TimeBreakerAsset Test -----");

		LoadFromExR(ExtremeRoleId.TimeBreaker);
	}
}
