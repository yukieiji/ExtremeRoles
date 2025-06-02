using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Impostor;
using System;

namespace ExtremeRoles.Test.Lobby.Asset;

public class AcceleratorAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:AcceleratorAssetLoad Test -----");

		LoadFromExR(CombinationRoleType.Accelerator);
		LoadFromExR(CombinationRoleType.Accelerator, ObjectPath.AcceleratorAcceleratePanel);
		yield break;
	}
}
