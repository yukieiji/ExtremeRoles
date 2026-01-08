using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Lobby.Asset;

public sealed class InspectorAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:InspectorAssetLoad Test -----");

		LoadFromExR(ExtremeRoleId.Inspector);
		yield break;
	}
}
