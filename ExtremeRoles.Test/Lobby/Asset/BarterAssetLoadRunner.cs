using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Lobby.Asset;

public sealed class BarterAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:BarterAssetLoad Test -----");

		LoadFromExR(ExtremeRoleId.Barter);
		yield break;
	}
}
