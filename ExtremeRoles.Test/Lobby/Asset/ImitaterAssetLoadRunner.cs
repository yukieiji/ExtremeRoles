using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Lobby.Asset;

public class ImitaterAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:ImitaterAssetLoad Test -----");

		LoadFromExR(ExtremeRoleId.Imitater);
		yield break;
	}
}
