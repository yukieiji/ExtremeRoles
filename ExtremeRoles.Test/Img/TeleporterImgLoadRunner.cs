using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;

namespace ExtremeRoles.Test.Img;

internal sealed class TeleporterImgLoadRunner
	: AssetImgLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:TeleporterImgLoad Test -----");

		LoadFromExR(ExtremeRoleId.Teleporter, Path.TeleporterNoneActivatePortal);
		LoadFromExR(ExtremeRoleId.Teleporter, Path.TeleporterFirstPortal);
		LoadFromExR(ExtremeRoleId.Teleporter, Path.TeleporterSecondPortal);
		LoadFromExR(ExtremeRoleId.Teleporter, Path.TeleporterPortalBase);
	}
}
