using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Lobby.Asset;

public class JailerAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:JailerAsset Test -----");

		LoadFromExR(ExtremeRoleId.Jailer);
		yield break;
	}
}
