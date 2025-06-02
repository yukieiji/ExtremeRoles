using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Lobby.Asset;

public class TimeBreakerAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:TimeBreakerAsset Test -----");

		LoadFromExR(ExtremeRoleId.TimeBreaker);
		yield break;
	}
}
