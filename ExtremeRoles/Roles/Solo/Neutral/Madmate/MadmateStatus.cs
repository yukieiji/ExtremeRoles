using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.Madmate;

#nullable enable

public sealed class MadmateStatus : IStatusModel, IRoleFakeIntro
{
    public bool IsUpdateMadmate { get; set; } = false;

    public ExtremeRoleType FakeTeam => IsUpdateMadmate ? ExtremeRoleType.Impostor : ExtremeRoleType.Crewmate;
}