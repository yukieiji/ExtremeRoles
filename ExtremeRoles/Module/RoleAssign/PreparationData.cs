using ExtremeRoles.Module.Interface;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign;

public readonly record struct PreparationData(
	PlayerRoleAssignData Assign,
	ISpawnDataManager RoleSpawn,
	ISpawnLimiter Limit);
