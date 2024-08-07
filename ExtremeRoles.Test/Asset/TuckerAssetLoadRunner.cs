﻿using ExtremeRoles.Resources;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Asset;

internal sealed class TuckerAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:TuckerAsset Test -----");

		LoadFromExR(ExtremeRoleId.Tucker, ObjectPath.TuckerShadow);
		LoadFromExR(ExtremeRoleId.Tucker, ObjectPath.TuckerCreateChimera);
		LoadFromExR(ExtremeRoleId.Tucker, ObjectPath.TuckerRemoveShadow);
	}
}
