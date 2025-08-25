using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Lobby.Asset;

public class BarterAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:AcceleratorAssetLoad Test -----");

		LoadFromExR(ExtremeRoleId.Barter);
		yield break;
	}
}
