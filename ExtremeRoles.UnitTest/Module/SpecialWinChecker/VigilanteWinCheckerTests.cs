using System.Collections.Generic;
using System.Reflection;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SpecialWinChecker;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SpecialWinChecker;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class VigilanteWinCheckerTests
{
    private sealed class DummySingleRole : SingleRoleBase
    {
        public DummySingleRole(ExtremeRoleId roleId, ExtremeRoleType team = ExtremeRoleType.Neutral)
        {
            var core = new RoleCore(roleId, team, Color.white, roleId.ToString());
            var field = typeof(SingleRoleBase).GetField("<Core>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(this, core);
        }

        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) { }
        protected override void RoleSpecificInit() { }
    }

    public VigilanteWinCheckerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupGameDataMock();
    }

    [Fact]
    public void IsWin_HeroVillainVigilanteAlive_ReturnsTrue()
    {
        ExtremeRoleManager.GameRole.Clear();

        var mockHero = new DummySingleRole(ExtremeRoleId.Hero);
        var mockVillain = new DummySingleRole(ExtremeRoleId.Villain);
        var mockVigilante = new DummySingleRole(ExtremeRoleId.Vigilante);

        ExtremeRoleManager.GameRole[1] = mockHero;
        ExtremeRoleManager.GameRole[2] = mockVillain;
        ExtremeRoleManager.GameRole[3] = mockVigilante;

        var mockPlayer1 = new Moq.Mock<NetworkedPlayerInfo>(System.IntPtr.Zero);
        mockPlayer1.SetupGet(p => p.IsDead).Returns(false);
        var mockPlayer2 = new Moq.Mock<NetworkedPlayerInfo>(System.IntPtr.Zero);
        mockPlayer2.SetupGet(p => p.IsDead).Returns(false);
        var mockPlayer3 = new Moq.Mock<NetworkedPlayerInfo>(System.IntPtr.Zero);
        mockPlayer3.SetupGet(p => p.IsDead).Returns(false);

        var mockGameData = MockSetupHelper.SetupGameDataMock();
        mockGameData.Setup(g => g.GetPlayerById(1)).Returns(mockPlayer1.Object);
        mockGameData.Setup(g => g.GetPlayerById(2)).Returns(mockPlayer2.Object);
        mockGameData.Setup(g => g.GetPlayerById(3)).Returns(mockPlayer3.Object);

        var checker = new VigilanteWinChecker();
        var mockStats = new Moq.Mock<IPlayerStatistics>();

        Assert.True(checker.IsWin(mockStats.Object));
    }

    [Fact]
    public void IsWin_DeadHero_ReturnsFalse()
    {
        ExtremeRoleManager.GameRole.Clear();

        var mockHero = new DummySingleRole(ExtremeRoleId.Hero);
        var mockVillain = new DummySingleRole(ExtremeRoleId.Villain);
        var mockVigilante = new DummySingleRole(ExtremeRoleId.Vigilante);

        ExtremeRoleManager.GameRole[1] = mockHero;
        ExtremeRoleManager.GameRole[2] = mockVillain;
        ExtremeRoleManager.GameRole[3] = mockVigilante;

        var mockPlayer1 = new Moq.Mock<NetworkedPlayerInfo>(System.IntPtr.Zero);
        mockPlayer1.SetupGet(p => p.IsDead).Returns(true); // Hero is dead
        var mockPlayer2 = new Moq.Mock<NetworkedPlayerInfo>(System.IntPtr.Zero);
        mockPlayer2.SetupGet(p => p.IsDead).Returns(false);
        var mockPlayer3 = new Moq.Mock<NetworkedPlayerInfo>(System.IntPtr.Zero);
        mockPlayer3.SetupGet(p => p.IsDead).Returns(false);

        var mockGameData = MockSetupHelper.SetupGameDataMock();
        mockGameData.Setup(g => g.GetPlayerById(1)).Returns(mockPlayer1.Object);
        mockGameData.Setup(g => g.GetPlayerById(2)).Returns(mockPlayer2.Object);
        mockGameData.Setup(g => g.GetPlayerById(3)).Returns(mockPlayer3.Object);

        var checker = new VigilanteWinChecker();
        var mockStats = new Moq.Mock<IPlayerStatistics>();

        Assert.False(checker.IsWin(mockStats.Object));
    }

    [Fact]
    public void IsWin_OtherRoleAlive_ReturnsFalse()
    {
        ExtremeRoleManager.GameRole.Clear();

        var mockHero = new DummySingleRole(ExtremeRoleId.Hero);
        var mockVillain = new DummySingleRole(ExtremeRoleId.Villain);
        var mockVigilante = new DummySingleRole(ExtremeRoleId.Vigilante);
        var mockOther = new DummySingleRole(ExtremeRoleId.Sheriff);

        ExtremeRoleManager.GameRole[1] = mockHero;
        ExtremeRoleManager.GameRole[2] = mockVillain;
        ExtremeRoleManager.GameRole[3] = mockVigilante;
        ExtremeRoleManager.GameRole[4] = mockOther;

        var mockPlayer1 = new Moq.Mock<NetworkedPlayerInfo>(System.IntPtr.Zero);
        mockPlayer1.SetupGet(p => p.IsDead).Returns(false);
        var mockPlayer2 = new Moq.Mock<NetworkedPlayerInfo>(System.IntPtr.Zero);
        mockPlayer2.SetupGet(p => p.IsDead).Returns(false);
        var mockPlayer3 = new Moq.Mock<NetworkedPlayerInfo>(System.IntPtr.Zero);
        mockPlayer3.SetupGet(p => p.IsDead).Returns(false);
        var mockPlayer4 = new Moq.Mock<NetworkedPlayerInfo>(System.IntPtr.Zero);
        mockPlayer4.SetupGet(p => p.IsDead).Returns(false);

        var mockGameData = MockSetupHelper.SetupGameDataMock();
        mockGameData.Setup(g => g.GetPlayerById(1)).Returns(mockPlayer1.Object);
        mockGameData.Setup(g => g.GetPlayerById(2)).Returns(mockPlayer2.Object);
        mockGameData.Setup(g => g.GetPlayerById(3)).Returns(mockPlayer3.Object);
        mockGameData.Setup(g => g.GetPlayerById(4)).Returns(mockPlayer4.Object);

        var checker = new VigilanteWinChecker();
        var mockStats = new Moq.Mock<IPlayerStatistics>();

        Assert.False(checker.IsWin(mockStats.Object));
    }
}
