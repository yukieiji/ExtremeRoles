using System;
using System.Reflection;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SpecialWinChecker;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SpecialWinChecker;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class MonikaAliveWinCheckerTests
{
    public MonikaAliveWinCheckerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupGameDataMock();
        SetupAmongUsClientAndShipState();
    }

    private static void SetupAmongUsClientAndShipState()
    {
        MockSetupHelper.SetupMockExtremeRolePlugin();
        if (ExtremeRolesPlugin.ShipState == null)
        {
            var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(nameof(ExtremeRolesPlugin.ShipState), BindingFlags.Public | BindingFlags.Static);
            shipStateProp?.SetValue(null, new ExtremeShipStatus());
        }

        var mockClient = MockSetupHelper.SetupAmongUsClientMock();
        var mockWriter = new Mock<Hazel.MessageWriter>(IntPtr.Zero);
        mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<Hazel.SendOption>(), It.IsAny<int>())).Returns(mockWriter.Object);

        var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.NetId).Returns(1u);
    }

    [Fact]
    public void Reason_ReturnsMonikaIamTheOnlyOne()
    {
        var checker = new MonikaAliveWinChecker();
        Assert.Equal(RoleGameOverReason.MonikaIamTheOnlyOne, checker.Reason);
    }

    [Fact]
    public void IsWin_AliveNumZeroOrGreaterThanOne_ReturnsFalse()
    {
        var checker = new MonikaAliveWinChecker();
        var mockStats = new Mock<IPlayerStatistics>();

        // aliveNum == 0
        Assert.False(checker.IsWin(mockStats.Object));

        // aliveNum > 1
        var mockRole = new Mock<SingleRoleBase>();
        checker.AddAliveRole(1, mockRole.Object);
        checker.AddAliveRole(2, mockRole.Object);
        Assert.False(checker.IsWin(mockStats.Object));
    }

    [Fact]
    public void IsWin_NoMonikaTrashSystem_ReturnsFalse()
    {
        var checker = new MonikaAliveWinChecker();
        var mockRole = new Mock<SingleRoleBase>();
        checker.AddAliveRole(1, mockRole.Object); // aliveNum = 1

        var mockStats = new Mock<IPlayerStatistics>();
        // MonikaTrashSystem is not registered in ExtremeSystemTypeManager
        Assert.False(checker.IsWin(mockStats.Object));
    }
}
