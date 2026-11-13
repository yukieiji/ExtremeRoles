using ExtremeRoles.Roles;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Test.Lobby.Asset;

public class BlackmailerAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:BlackmailerAsset Test -----");

		LoadFromExR(ExtremeRoleId.Blackmailer);
		LoadFromExR(ExtremeRoleId.Blackmailer, ObjectPath.MeetingBk);
		yield break;
	}
}
