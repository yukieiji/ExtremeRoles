using System.Collections.Generic;

using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Combination;

public sealed class GuesserRoleInfoCreator : IGuesserRoleInfoCreator
{
	private readonly IGuesserRoleInfoContainer container;
	private readonly IGuesserVanillaRoleProvider vanillaRoleProvider;
	private readonly IGuesserNormalRoleProvider normalRoleProvider;
	private readonly IGuesserCombRoleProvider combRoleProvider;
	private readonly LiberalDefaultOptionLoader liberalOptionLoader;

	public GuesserRoleInfoCreator(
		IGuesserRoleInfoContainer container,
		IGuesserVanillaRoleProvider vanillaRoleProvider,
		IGuesserNormalRoleProvider normalRoleProvider,
		IGuesserCombRoleProvider combRoleProvider,
		LiberalDefaultOptionLoader liberalOptionLoader)
	{
		this.container = container;
		this.vanillaRoleProvider = vanillaRoleProvider;
		this.normalRoleProvider = normalRoleProvider;
		this.combRoleProvider = combRoleProvider;
		this.liberalOptionLoader = liberalOptionLoader;
	}

	public IReadOnlyList<GuessBehaviour.RoleInfo> Create(
		bool includeNoneRole,
		Guesser.DefaultGuessRole defaultRole)
	{
		bool liberalOn = this.liberalOptionLoader.Get(LiberalGlobalSetting.WinMoney).IsViewActive;
		bool militantOn = this.liberalOptionLoader.Get(LiberalGlobalSetting.LiberalMilitantMini).IsViewActive;

		this.vanillaRoleProvider.AddVanillaRoles(
			this.container,
			includeNoneRole,
			defaultRole,
			liberalOn,
			militantOn);

		this.container.AddSeparatedRoleId(ExtremeRoleType.Crewmate, (ExtremeRoleId)AmongUs.GameOptions.RoleTypes.Crewmate);
		this.container.AddSeparatedRoleId(ExtremeRoleType.Impostor, (ExtremeRoleId)AmongUs.GameOptions.RoleTypes.Impostor);

		if (liberalOn)
		{
			this.container.AddSeparatedRoleId(ExtremeRoleType.Liberal, ExtremeRoleId.Dove);
			if (militantOn)
			{
				this.container.AddSeparatedRoleId(ExtremeRoleType.Liberal, ExtremeRoleId.Militant);
			}
		}

		this.vanillaRoleProvider.AddAmongUsRoles(this.container);
		this.normalRoleProvider.AddExRNormalRoles(this.container, out var assignState);
		this.combRoleProvider.AddExRCombRoles(this.container, assignState);

		return this.container.Result;
	}
}
