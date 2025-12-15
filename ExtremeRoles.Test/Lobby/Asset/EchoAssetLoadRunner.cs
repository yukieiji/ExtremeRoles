using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Lobby.Asset;

public sealed class EchoAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:EchoAssetLoad Test -----");

		LoadFromExR(ExtremeRoleId.Echo);
		yield break;
	}
}
