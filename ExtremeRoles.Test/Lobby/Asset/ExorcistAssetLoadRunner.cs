using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Lobby.Asset;

public sealed class ExorcistAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:ExorcistAssetLoad Test -----");

		LoadFromExR(ExtremeRoleId.Exorcist);
		yield break;
	}
}
