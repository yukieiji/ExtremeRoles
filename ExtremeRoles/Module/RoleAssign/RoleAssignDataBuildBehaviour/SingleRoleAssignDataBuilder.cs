using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Module.Interface;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;

public sealed class SingleRoleAssignDataBuilder(
	IImpostorSingleRoleAssignDataBuilder impostorBuilder,
	INeutralSingleRoleAssignDataBuilder neutralBuilder,
	ILiberalSingleRoleAssignDataBuilder liberalBuilder,
	ICrewmateSingleRoleAssignDataBuilder crewmateBuilder,
	IModLogger logger) : IRoleAssignDataBuildBehaviour
{
	public int Priority => (int)ExtremeRoleAssignDataBuilder.Priority.Single;

	public void Build(in PreparationData data)
	{
		logger.LogTrace("----------------------------- SingleRoleAssign - Start -----------------------------");
		impostorBuilder.Build(data);
		neutralBuilder.Build(data);
		liberalBuilder.Build(data);
		crewmateBuilder.Build(data);
		logger.LogTrace("----------------------------- SingleRoleAssign - End -----------------------------");
	}
}
