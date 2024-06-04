using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;

namespace ExtremeRoles.Test.Img;

internal sealed class MeryImgLoadRunner
	: AssetImgLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:MeryImgLoad Test -----");

		for (int index = 0; index < 18; ++index)
		{
			LoadFromExR(ExtremeRoleId.Mery, $"{index}");
		}

		LoadFromExR(
			ExtremeRoleId.Mery,
			Path.MeryNoneActive);

		LoadFromExR(ExtremeRoleId.Mery);
	}
}
