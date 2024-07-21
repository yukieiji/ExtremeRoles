using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Impostor;
using System;

namespace ExtremeRoles.Test.Asset;

internal sealed class AcceleratorAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:AcceleratorAssetLoad Test -----");

		LoadFromExR(CombinationRoleType.Accelerator);
		LoadFromExR(CombinationRoleType.Accelerator, ObjectPath.AcceleratorAcceleratePanel);
	}
}
