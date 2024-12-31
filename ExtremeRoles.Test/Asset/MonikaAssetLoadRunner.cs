using ExtremeRoles.Roles;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Test.Asset;

internal sealed class MonikaAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:MonikaAsset Test -----");

		LoadFromExR(ExtremeRoleId.Monika);
		LoadFromExR(ExtremeRoleId.Monika, ObjectPath.MeetingBk);
	}
}
