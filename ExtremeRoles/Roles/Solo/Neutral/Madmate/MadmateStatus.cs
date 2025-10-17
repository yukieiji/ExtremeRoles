using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.Madmate;

#nullable enable

public sealed class MadmateStatus : IStatusModel, IRoleFakeIntro, IFakeImpostorStatus
{
	public bool IsFakeImpostor { get; set; } = false;

	public ExtremeRoleType FakeTeam => this.IsFakeImpostor ? ExtremeRoleType.Impostor : ExtremeRoleType.Crewmate;
}