using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.RoleAssign;

public readonly record struct PreparationData(
	PlayerRoleAssignData Assign,
	ISpawnDataManager RoleSpawn,
	ISpawnLimiter Limit);
