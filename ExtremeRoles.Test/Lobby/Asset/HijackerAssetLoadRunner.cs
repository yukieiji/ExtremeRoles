using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Lobby.Asset;

public class HijackerAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:HijackerAsset Test -----");

		LoadFromExR(ExtremeRoleId.Hijacker);
	}
}
