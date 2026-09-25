using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;

public sealed class ImpostorSingleRoleAssignDataBuilder(IVanillaRoleProvider roleProvider) : IImpostorSingleRoleAssignDataBuilder
{
	private readonly IReadOnlySet<RoleTypes> vanillaImpRoleType = roleProvider.ImpostorRole;

	public void Build(in PreparationData data)
	{
		Logging.Debug(
			$"------------------------- SingleRoleAssign - Impostor - Start -------------------------");
		SingleRoleAssignHelper.AddSingleExtremeRoleAssignDataFromTeamAndPlayer(
			data,
			ExtremeRoleType.Impostor,
			data.Assign.GetCanImpostorAssignPlayer(),
			vanillaImpRoleType);
		Logging.Debug(
			$"------------------------- SingleRoleAssign - Impostor - End -------------------------");
	}
}
