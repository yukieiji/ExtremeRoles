using System;
using AmongUs.GameOptions;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.Roles.Solo.Impostor.Retaliator;
using ExtremeRoles.Roles.Solo.Neutral;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class RetaliatorTests
{
    private readonly Mock<AmongUsClient> mockClient;
    private readonly Mock<PlayerControl> mockLocalPlayer;

    public RetaliatorTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        SetupTranslationControllerMock();
        MockSetupHelper.SetupConstantsHelpers();
        MockSetupHelper.SetupDestroyableSingletonMock<RoleManager>();

        mockClient = MockSetupHelper.SetupAmongUsClientMock();
        var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
        mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
            .Returns(mockWriter.Object);

        mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

        MockSetupHelper.SetupGameDataMock();
        MockSetupHelper.SetupGameOptionsManagerMock();
        MockSetupHelper.SetupOptionManager();

        if (ExtremeRolesPlugin.ShipState == null)
        {
            var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(
                nameof(ExtremeRolesPlugin.ShipState),
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            shipStateProp?.SetValue(null, new ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus());
        }
    }

    private static void SetupTranslationControllerMock()
    {
        var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
        mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>())).Returns("TestString");
        mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");
        mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>())).Returns("TestString");
        mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");
    }

    private static RetaliatorRole CreateAndInitRole(byte playerId = 1)
    {
        var role = new RetaliatorRole();
        role.CreateRoleAllOption();
        role.Initialize();
        ExtremeRoleManager.GameRole[playerId] = role;
        return role;
    }

    [Fact]
    public void StatusModel_Initialization_ValuesCorrect()
    {
        var status = new RetaliatorStatusModel(
            awakeKillCount: 2,
            maxAbilityUses: 3,
            maxTargetPerMeeting: 1,
            targetTeam: RetaliatorTargetTeam.CrewmateOnly,
            notifyTargetNextMeeting: true);

        Assert.False(status.IsAwake);
        Assert.Equal(2, status.AwakeKillCount);
        Assert.Equal(3, status.RemainingAbilityUses);
        Assert.Equal(1, status.MaxTargetPerMeeting);
        Assert.Equal(RetaliatorTargetTeam.CrewmateOnly, status.TargetTeam);
        Assert.True(status.NotifyTargetNextMeeting);
    }

    [Fact]
    public void StatusModel_TargetPlayer_DecrementsUsesAndIncrementsMeetingCount()
    {
        var status = new RetaliatorStatusModel(0, 3, 2, RetaliatorTargetTeam.CrewmateOnly, true);

        status.TargetPlayer(2);

        Assert.Equal(2, status.RemainingAbilityUses);
        Assert.Equal(1, status.CurrentMeetingTargetCount);
        Assert.True(status.IsTargeted(2));
    }

    [Fact]
    public void StatusModel_OnMeetingStart_RotatesTargetedToNotified()
    {
        var status = new RetaliatorStatusModel(0, 3, 2, RetaliatorTargetTeam.CrewmateOnly, true);
        status.TargetPlayer(2);

        status.OnMeetingStart();

        Assert.False(status.IsTargeted(2));
        Assert.Contains((byte)2, status.NotifiedTargetPlayerIds);
        Assert.Equal(0, status.CurrentMeetingTargetCount);
    }

    [Fact]
    public void AbilityHandler_TryRolePlayerKillTo_IncrementsKillCountAndAwakes()
    {
        var status = new RetaliatorStatusModel(2, 3, 1, RetaliatorTargetTeam.CrewmateOnly, true);
        var handler = new RetaliatorAbilityHandler(status);

        var killer = new Mock<PlayerControl>().Object;
        var target = new Mock<PlayerControl>().Object;

        Assert.False(status.IsAwake);

        handler.TryRolePlayerKillTo(killer, target);
        Assert.Equal(1, status.KillCount);
        Assert.False(status.IsAwake);

        handler.TryRolePlayerKillTo(killer, target);
        Assert.Equal(2, status.KillCount);
        Assert.True(status.IsAwake);
    }

    [Fact]
    public void AbilityHandler_TryKilledFrom_WhenTargetedMatchingTeam_BlocksKill()
    {
        var status = new RetaliatorStatusModel(0, 3, 1, RetaliatorTargetTeam.CrewmateOnly, true);
        status.TargetPlayer(2);
        var handler = new RetaliatorAbilityHandler(status);

        var crewmateRole = new Sheriff();
        crewmateRole.CreateRoleAllOption();
        crewmateRole.Initialize();
        ExtremeRoleManager.GameRole[2] = crewmateRole;

        var mockRetaliatorPlayer = new Mock<PlayerControl>();
        mockRetaliatorPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

        var mockAttackerPlayer = new Mock<PlayerControl>();
        mockAttackerPlayer.SetupGet(p => p.PlayerId).Returns((byte)2);

        bool result = handler.TryKilledFrom(mockRetaliatorPlayer.Object, mockAttackerPlayer.Object);

        Assert.False(result); // Blocked
    }

    [Fact]
    public void AbilityHandler_TryKilledFrom_WhenUntargeted_AllowsKill()
    {
        var status = new RetaliatorStatusModel(0, 3, 1, RetaliatorTargetTeam.CrewmateOnly, true);
        var handler = new RetaliatorAbilityHandler(status);

        var crewmateRole = new Sheriff();
        crewmateRole.CreateRoleAllOption();
        crewmateRole.Initialize();
        ExtremeRoleManager.GameRole[2] = crewmateRole;

        var mockRetaliatorPlayer = new Mock<PlayerControl>();
        mockRetaliatorPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

        var mockAttackerPlayer = new Mock<PlayerControl>();
        mockAttackerPlayer.SetupGet(p => p.PlayerId).Returns((byte)2);

        bool result = handler.TryKilledFrom(mockRetaliatorPlayer.Object, mockAttackerPlayer.Object);

        Assert.True(result); // Allowed
    }

    [Fact]
    public void RetaliatorRole_ResetOnMeetingStart_RotatesAndNotifiesTarget()
    {
        var role = CreateAndInitRole(1); // Player 1 is Retaliator
        role.TargetPlayer(2); // Player 2 is targeted

        // Simulate meeting start
        role.ResetOnMeetingStart();

        // Player 2 is now in NotifiedTargetPlayerIds
        var status = (RetaliatorStatusModel)role.Status!;
        Assert.Contains((byte)2, status.NotifiedTargetPlayerIds);
        Assert.False(status.IsTargeted(2));
    }

    [Fact]
    public void RetaliatorRole_Initialization_RegistersAsImpostor()
    {
        var role = CreateAndInitRole();

        Assert.Equal(ExtremeRoleId.Retaliator, role.Core.Id);
        Assert.True(role.IsImpostor());
    }

    [Fact]
    public void RetaliatorRole_GetRolePlayerNameTag_IncludesTagWhenTargeted()
    {
        var role = CreateAndInitRole();
        role.TargetPlayer(2);

        var crewmateRole = new Sheriff();
        crewmateRole.CreateRoleAllOption();
        crewmateRole.Initialize();

        string tag = role.GetRolePlayerNameTag(crewmateRole, 2);
        Assert.Contains("TestString", tag);

        string untargetedTag = role.GetRolePlayerNameTag(crewmateRole, 3);
        Assert.DoesNotContain("TestString", untargetedTag);
    }
}
