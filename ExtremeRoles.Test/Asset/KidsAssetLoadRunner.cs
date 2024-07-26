using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Combination;

using System;

namespace ExtremeRoles.Test.Asset;

internal sealed class KidsAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:KidsImgLoad Test -----");

		for (int index = 0; index < 10; ++index)
		{
			LoadFromExR(CombinationRoleType.Kids, $"{index}");
		}

		try
		{
			var sprite = Wisp.TorchSprite;
			Log.LogInfo($"TorchSprite loading... ");
			NullCheck(sprite);

		}
		catch (Exception ex)
		{
			Log.LogError($"TorchSprite not load   {ex.Message}");
		}
	}
}
