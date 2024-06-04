using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Combination;

using System;

namespace ExtremeRoles.Test.Img;

internal sealed class KidsImgLoadRunner
	: AssetImgLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:KidsImgLoad Test -----");

		for (int index = 0; index < 10; ++index)
		{
			LoadFromExR(
				Path.KidsAsset,
				string.Format(
					Path.RoleImgPathFormat,
					$"{CombinationRoleType.Kids}.{index}"));
		}

		try
		{
			var sprite = Wisp.TorchSprite;
			if (sprite == null)
			{
				throw new Exception("TorchSprite is Null");
			}
			else
			{
				Log.LogInfo($"TorchSprite load success");
			}
		}
		catch (Exception ex)
		{
			Log.LogError($"TorchSprite not load   {ex.Message}");
		}
	}
}
