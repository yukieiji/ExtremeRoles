using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Impostor;
using System;

namespace ExtremeRoles.Test.Img;

internal sealed class HypnotistImgLoadRunner
	: AssetImgLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:HypnotistImgLoad Test -----");

		foreach (var ability in Enum.GetValues<Hypnotist.AbilityModuleType>())
		{
			LoadFromExR(
				Path.HypnotistAsset,
				string.Format(
					Path.RoleImgPathFormat,
					$"{ExtremeRoleId.Hypnotist}.{ability}"));
		}

		LoadFromExR(ExtremeRoleId.Hypnotist);
	}
}
