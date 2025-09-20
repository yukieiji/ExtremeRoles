using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Neutral.Jackal;
using ExtremeRoles.Roles.Solo.Neutral.Queen;
using ExtremeRoles.Roles.Solo.Neutral.Yandere;


namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataChecker;

public sealed class RoleDependencyRuleFactory : IRoleDependencyRuleFactory
{
	public IReadOnlyList<RoleDependencyRule> Rules => new List<RoleDependencyRule>()
	{
		buildKilledSubTeam(ExtremeRoleId.Shepherd, ExtremeRoleId.Jackal, OptionTab.NeutralTab, ShepherdRole.Option.CanKill, ShepherdRole.Option.IsSubTeam),
		new (ExtremeRoleId.Furry, ExtremeRoleId.Jackal, () => true),
		buildKilledSubTeam(ExtremeRoleId.Intimate, ExtremeRoleId.Yandere, OptionTab.NeutralTab, IntimateRole.Option.CanKill, IntimateRole.Option.IsSubTeam),
		new (ExtremeRoleId.Surrogator, ExtremeRoleId.Yandere, () => true),
		buildKilledSubTeam(ExtremeRoleId.Knight, ExtremeRoleId.Queen, OptionTab.NeutralTab, KnightRole.Option.IsSubTeam),
		new (ExtremeRoleId.Pawn, ExtremeRoleId.Queen, () => true)
	};

	private static RoleDependencyRule buildKilledSubTeam<T>(
		ExtremeRoleId checkRoleId, ExtremeRoleId dependRoleId, OptionTab tab, params T[] options) where T : Enum
		=> new(checkRoleId, dependRoleId,
			() =>
				OptionManager.Instance.TryGetCategory(tab, ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IRoleParentOptionIdGenerator>().Get(checkRoleId), out var category) &&
				!options.All(x => category.GetValue<T, bool>(x))
			);
}
