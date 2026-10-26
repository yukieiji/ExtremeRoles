using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Combination.Guesser;

public interface IGuesserVanillaRoleProvider
{
	void AddVanillaRoles(
		IGuesserRoleInfoContainer container,
		bool includeNoneRole,
		Guesser.DefaultGuessRole defaultRole,
		bool liberalOn,
		bool militantOn);

	void AddAmongUsRoles(IGuesserRoleInfoContainer container);
}

public interface IGuesserNormalRoleProvider
{
	void AddExRNormalRoles(
		IGuesserRoleInfoContainer container,
		out GuesserNormalRoleAssignState assignState);
}

public interface IGuesserCombRoleProvider
{
	void AddExRCombRoles(
		IGuesserRoleInfoContainer container,
		GuesserNormalRoleAssignState assignState);
}

public sealed class GuesserNormalRoleAssignState
{
	public bool IsJackalOn { get; set; } = false;
	public bool IsJackalForceReplaceLover { get; set; } = false;
	public bool IsQueenOn { get; set; } = false;
}
