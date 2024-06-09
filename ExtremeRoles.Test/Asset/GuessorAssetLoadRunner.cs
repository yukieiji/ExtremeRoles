using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;

namespace ExtremeRoles.Test.Asset;

internal sealed class GuessorAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:TeleporterImgLoad Test -----");

		LoadFromExR(ExtremeRoleId.Guesser);
	}
}
