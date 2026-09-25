using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;

public sealed class ImpostorSingleRoleAssignDataBuilder(
	IVanillaRoleProvider roleProvider,
	ISingleRoleAssignHelper helper,
	IModLogger logger) : IImpostorSingleRoleAssignDataBuilder
{
	private readonly IReadOnlySet<RoleTypes> vanillaImpRoleType = roleProvider.ImpostorRole;

	public void Build(in PreparationData data)
	{
		logger.LogTrace("------------------------- SingleRoleAssign - Impostor - Start -------------------------");
		helper.AddSingleExtremeRoleAssignDataFromTeamAndPlayer(
			data,
			ExtremeRoleType.Impostor,
			data.Assign.GetCanImpostorAssignPlayer(),
			vanillaImpRoleType);
		logger.LogTrace("------------------------- SingleRoleAssign - Impostor - End -------------------------");
	}
}
