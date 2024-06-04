using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Impostor;
using System;

namespace ExtremeRoles.Test.Img;

internal sealed class AcceleratorImgLoadRunner
	: AssetImgLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:AcceleratorImgLoad Test -----");

		LoadFromExR(CombinationRoleType.Accelerator);
		LoadFromExR(CombinationRoleType.Accelerator, Path.AcceleratorAcceleratePanel);
	}
}
