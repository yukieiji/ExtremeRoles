using ExtremeRoles.Roles;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Test.Lobby.Asset;

public class MonikaAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:MonikaAsset Test -----");

		LoadFromExR(ExtremeRoleId.Monika);
		LoadFromExR(ExtremeRoleId.Monika, ObjectPath.MeetingBk);
		yield break;
	}
}
