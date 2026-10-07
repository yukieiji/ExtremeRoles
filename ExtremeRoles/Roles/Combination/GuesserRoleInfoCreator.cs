using System.Collections.Generic;

using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Combination;

public sealed class GuesserRoleInfoCreator : IGuesserRoleInfoCreator
{
	private readonly IGuesserVanillaRoleProvider vanillaRoleProvider;
	private readonly IGuesserNormalRoleProvider normalRoleProvider;
	private readonly IGuesserCombRoleProvider combRoleProvider;
	private readonly LiberalDefaultOptionLoader liberalOptionLoader;

	public GuesserRoleInfoCreator(
		IGuesserVanillaRoleProvider vanillaRoleProvider,
		IGuesserNormalRoleProvider normalRoleProvider,
		IGuesserCombRoleProvider combRoleProvider,
		LiberalDefaultOptionLoader liberalOptionLoader)
	{
		this.vanillaRoleProvider = vanillaRoleProvider;
		this.normalRoleProvider = normalRoleProvider;
		this.combRoleProvider = combRoleProvider;
		this.liberalOptionLoader = liberalOptionLoader;
	}

	public IReadOnlyList<GuessBehaviour.RoleInfo> Create(
		bool includeNoneRole,
		Guesser.DefaultGuessRole defaultRole)
	{
		var result = new List<GuessBehaviour.RoleInfo>();

		var separatedRoleId = new Dictionary<ExtremeRoleType, List<ExtremeRoleId>>()
		{
			{ ExtremeRoleType.Crewmate, [] },
			{ ExtremeRoleType.Impostor, [] },
			{ ExtremeRoleType.Neutral, [] },
			{ ExtremeRoleType.Liberal, [] },
		};

		bool liberalOn = this.liberalOptionLoader.Get(LiberalGlobalSetting.WinMoney).IsViewActive;
		bool militantOn = this.liberalOptionLoader.Get(LiberalGlobalSetting.LiberalMilitantMini).IsViewActive;

		this.vanillaRoleProvider.AddVanillaRoles(
			result,
			separatedRoleId,
			includeNoneRole,
			defaultRole,
			liberalOn,
			militantOn);

		separatedRoleId[ExtremeRoleType.Crewmate].Add((ExtremeRoleId)AmongUs.GameOptions.RoleTypes.Crewmate);
		separatedRoleId[ExtremeRoleType.Impostor].Add((ExtremeRoleId)AmongUs.GameOptions.RoleTypes.Impostor);
		if (liberalOn)
		{
			separatedRoleId[ExtremeRoleType.Liberal].Add(ExtremeRoleId.Dove);
			if (militantOn)
			{
				separatedRoleId[ExtremeRoleType.Liberal].Add(ExtremeRoleId.Militant);
			}
		}

		this.vanillaRoleProvider.AddAmongUsRoles(result, separatedRoleId);
		this.normalRoleProvider.AddExRNormalRoles(result, separatedRoleId, out var assignState);
		this.combRoleProvider.AddExRCombRoles(result, separatedRoleId, assignState);

		return result;
	}
}
