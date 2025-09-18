using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Lobby.Asset;

public sealed class BoxerAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:BoxerAssetLoad Test -----");

		LoadFromExR(ExtremeRoleId.Boxer);
		yield break;
	}
}
