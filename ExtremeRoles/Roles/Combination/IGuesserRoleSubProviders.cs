using System.Collections.Generic;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Combination;

public interface IGuesserVanillaRoleProvider
{
	void AddVanillaRoles(
		List<GuessBehaviour.RoleInfo> result,
		Dictionary<ExtremeRoleType, List<ExtremeRoleId>> separatedRoleId,
		bool includeNoneRole,
		Guesser.DefaultGuessRole defaultRole,
		bool liberalOn,
		bool militantOn);

	void AddAmongUsRoles(
		List<GuessBehaviour.RoleInfo> result,
		Dictionary<ExtremeRoleType, List<ExtremeRoleId>> separatedRoleId);
}

public interface IGuesserNormalRoleProvider
{
	void AddExRNormalRoles(
		List<GuessBehaviour.RoleInfo> result,
		Dictionary<ExtremeRoleType, List<ExtremeRoleId>> separatedRoleId,
		out GuesserNormalRoleAssignState assignState);
}

public interface IGuesserCombRoleProvider
{
	void AddExRCombRoles(
		List<GuessBehaviour.RoleInfo> result,
		Dictionary<ExtremeRoleType, List<ExtremeRoleId>> separatedRoleId,
		GuesserNormalRoleAssignState assignState);
}

public sealed class GuesserNormalRoleAssignState
{
	public bool IsJackalOn { get; set; } = false;
	public bool IsJackalForceReplaceLover { get; set; } = false;
	public bool IsQueenOn { get; set; } = false;
}
