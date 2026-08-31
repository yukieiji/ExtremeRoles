using System.Reflection;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Neutral;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;


[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class NeutralSpecialWinCheckerTests
{
    public NeutralSpecialWinCheckerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
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

        var mockClient = new Mock<AmongUsClient>();
        var mockHelper = new Mock<MockAmongUsClientget_InstanceHelper>();
        mockHelper.Setup(h => h.Invoke()).Returns(mockClient.Object);
        MockAmongUsClientget_InstanceHelper.Instance = mockHelper.Object;

        var mockWriter = new Mock<Hazel.MessageWriter>(System.IntPtr.Zero);
        mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<Hazel.SendOption>(), It.IsAny<int>())).Returns(mockWriter.Object);

        var mockLocalPlayer = new Mock<PlayerControl>();
        mockLocalPlayer.SetupGet(p => p.NetId).Returns(1u);
        var mockPlayerHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        MockPlayerControlget_LocalPlayerHelper.Instance = mockPlayerHelper.Object;
        mockPlayerHelper.Setup(x => x.Invoke()).Returns(mockLocalPlayer.Object);
    }

    [Fact]
    public void TryCheckGameEnd_NoNeutralWinRoles_ReturnsFalse()
    {
        ExtremeRoleManager.GameRole.Clear();
        NeutralSpecialWinChecker checker = new NeutralSpecialWinChecker();

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }

    [Fact]
    public void TryCheckGameEnd_NeutralRoleIsWin_ReturnsTrueWithRoleReason()
    {
        ExtremeRoleManager.GameRole.Clear();
        Jester jester = new Jester();
        jester.IsWin = true;

        ExtremeRoleManager.GameRole[1] = jester;

        NeutralSpecialWinChecker checker = new NeutralSpecialWinChecker();

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.True(result);
        Assert.Equal((GameOverReason)RoleGameOverReason.JesterMeetingFavorite, reason);
    }
}
