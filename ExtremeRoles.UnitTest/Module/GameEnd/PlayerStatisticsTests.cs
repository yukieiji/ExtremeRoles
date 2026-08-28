using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

public sealed class PlayerStatisticsTests
{
    [Fact]
    public void NeutralSeparateTeamContainer_AddAndClear_WorksCorrectly()
    {
        NeutralSeparateTeamContainer container = new NeutralSeparateTeamContainer();
        container.Add(NeutralSeparateTeam.Jackal, 1);
        container.Add(NeutralSeparateTeam.Jackal, 1);

        Assert.Single(container.Team);
        var key = new NeutralSeparateTeamContainer.NeutralTeam(NeutralSeparateTeam.Jackal, 1);
        Assert.Equal(2, container.Team[key]);

        container.Clear();
        Assert.Empty(container.Team);
    }

    [Fact]
    public void NeutralSeparateTeamContainer_AddMultipleDifferentTeams_WorksCorrectly()
    {
        NeutralSeparateTeamContainer container = new NeutralSeparateTeamContainer();
        container.Add(NeutralSeparateTeam.Jackal, 1);
        container.Add(NeutralSeparateTeam.Lover, 2);

        Assert.Equal(2, container.Team.Count);
        var jackalKey = new NeutralSeparateTeamContainer.NeutralTeam(NeutralSeparateTeam.Jackal, 1);
        var loverKey = new NeutralSeparateTeamContainer.NeutralTeam(NeutralSeparateTeam.Lover, 2);

        Assert.Equal(1, container.Team[jackalKey]);
        Assert.Equal(1, container.Team[loverKey]);
    }

    [Fact]
    public void NeutralSeparateTeamContainer_AddSubTeam_WhenMainNotPresent_AppearsInTeam()
    {
        NeutralSeparateTeamContainer container = new NeutralSeparateTeamContainer();
        container.AddSubTeam(NeutralSeparateTeam.Jackal, NeutralSeparateTeam.JackalSub, 1);

        Assert.Single(container.Team);
        var subKey = new NeutralSeparateTeamContainer.NeutralTeam(NeutralSeparateTeam.JackalSub, 1);
        Assert.Equal(1, container.Team[subKey]);
    }

    [Fact]
    public void NeutralSeparateTeamBuilder_Add_AddsTeamCorrectly()
    {
        NeutralSeparateTeamBuilder builder = new NeutralSeparateTeamBuilder();
        builder.Add(null!, ExtremeRoleId.Jackal, 5);

        Assert.Single(builder.Team);
        var key = new NeutralSeparateTeamContainer.NeutralTeam(NeutralSeparateTeam.Jackal, 5);
        Assert.Equal(1, builder.Team[key]);
    }
}
