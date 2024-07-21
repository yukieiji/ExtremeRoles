using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Impostor;
using System;

namespace ExtremeRoles.Test.Asset;

internal sealed class HypnotistAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:HypnotistImgLoad Test -----");

		foreach (var ability in Enum.GetValues<Hypnotist.AbilityModuleType>())
		{
			LoadFromExR(ExtremeRoleId.Hypnotist, ability.ToString());
		}

		LoadFromExR(ExtremeRoleId.Hypnotist);
	}
}
