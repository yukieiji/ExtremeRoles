using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Lobby.Asset;

public sealed class DeepOneAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:DeepOneAssetLoad Test -----");

		LoadFromExR(ExtremeRoleId.DeepOne);
		yield break;
	}
}
