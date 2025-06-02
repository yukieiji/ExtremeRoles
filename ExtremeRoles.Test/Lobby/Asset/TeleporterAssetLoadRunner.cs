using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;

namespace ExtremeRoles.Test.Lobby.Asset;

public class TeleporterAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:TeleporterImgLoad Test -----");

		LoadFromExR(ExtremeRoleId.Teleporter, ObjectPath.TeleporterNoneActivatePortal);
		LoadFromExR(ExtremeRoleId.Teleporter, ObjectPath.TeleporterFirstPortal);
		LoadFromExR(ExtremeRoleId.Teleporter, ObjectPath.TeleporterSecondPortal);
		LoadFromExR(ExtremeRoleId.Teleporter, ObjectPath.TeleporterPortalBase);
		yield break;
	}
}
