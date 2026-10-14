using System;
using AmongUs.GameOptions;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.Roles.Solo.Impostor.Retaliator;
using ExtremeRoles.Extension.Player;
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
        MockSetupHelper.SetupObjectImplicitHelpers();
        MockSetupHelper.SetupPlayerVoteAreaMocks();
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
    public void StatusModel_TargetPlayer_DecrementsUsesAndIncrementsMeetingCount()
    {
        // Arrange
        var status = new RetaliatorStatusModel(0, 3, 2, RetaliatorTargetTeam.CrewmateOnly, true);

        // Act
        status.TargetPlayer(2);

        // Assert
        Assert.Equal(2, status.RemainingAbilityUses);
        Assert.Equal(1, status.CurrentMeetingTargetCount);
        Assert.True(status.IsTargeted(2));
    }

    [Fact]
    public void AbilityHandler_TryRolePlayerKillTo_WhenPreAwake_IncrementsKillCountAndAwakesWhenThresholdReached()
    {
        // Arrange
        var status = new RetaliatorStatusModel(2, 3, 1, RetaliatorTargetTeam.CrewmateOnly, true);
        var handler = new RetaliatorAbilityHandler(status);
        var killer = new Mock<PlayerControl>().Object;
        var target = new Mock<PlayerControl>().Object;

        // Act 1
        handler.TryRolePlayerKillTo(killer, target);

        // Assert 1
        Assert.Equal(1, status.AwakeKillCount);
        Assert.False(status.IsAwake);

        // Act 2
        handler.TryRolePlayerKillTo(killer, target);

        // Assert 2
        Assert.Equal(0, status.AwakeKillCount);
        Assert.True(status.IsAwake);
    }

    [Fact]
    public void AbilityHandler_TryKilledFrom_WhenTargetedMatchingTeam_BlocksKill()
    {
        // Arrange
        var status = new RetaliatorStatusModel(0, 3, 1, RetaliatorTargetTeam.CrewmateOnly, true);
        status.TargetPlayer(2);
        var handler = new RetaliatorAbilityHandler(status);

        var crewmateRole = new Sheriff();
        crewmateRole.CreateRoleAllOption();
        crewmateRole.Initialize();
        ExtremeRoleManager.GameRole[2] = crewmateRole;

        var mockRetaliatorPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockRetaliatorPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
        var mockRetaliatorData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockRetaliatorData.SetupGet(d => d.PlayerId).Returns((byte)1);
        mockRetaliatorData.SetupGet(d => d.Disconnected).Returns(false);
        mockRetaliatorData.SetupGet(d => d.Object).Returns(mockRetaliatorPlayer.Object);
        mockRetaliatorPlayer.SetupGet(p => p.Data).Returns(mockRetaliatorData.Object);

        var mockAttackerPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockAttackerPlayer.SetupGet(p => p.PlayerId).Returns((byte)2);
        var mockAttackerData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockAttackerData.SetupGet(d => d.PlayerId).Returns((byte)2);
        mockAttackerData.SetupGet(d => d.Disconnected).Returns(false);
        mockAttackerData.SetupGet(d => d.Object).Returns(mockAttackerPlayer.Object);
        mockAttackerPlayer.SetupGet(p => p.Data).Returns(mockAttackerData.Object);

        ExtremeRoles.Performance.PlayerCache.AddPlayerControl(mockRetaliatorPlayer.Object);
        ExtremeRoles.Performance.PlayerCache.AddPlayerControl(mockAttackerPlayer.Object);

        // Act
        bool result = handler.TryKilledFrom(mockRetaliatorPlayer.Object, mockAttackerPlayer.Object);

        // Assert
        Assert.False(result); // Kill blocked
    }

    [Fact]
    public void AbilityHandler_TryKilledFrom_WhenTargetedNonMatchingTeam_AllowsKill()
    {
        // Arrange: Retaliator target team is CrewmateOnly, but attacker is Impostor
        var status = new RetaliatorStatusModel(0, 3, 1, RetaliatorTargetTeam.CrewmateOnly, true);
        status.TargetPlayer(2);
        var handler = new RetaliatorAbilityHandler(status);

        var impostorRole = new YoYo();
        impostorRole.CreateRoleAllOption();
        impostorRole.Initialize();
        ExtremeRoleManager.GameRole[2] = impostorRole;

        var mockRetaliatorPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockRetaliatorPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
        var mockRetaliatorData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockRetaliatorData.SetupGet(d => d.PlayerId).Returns((byte)1);
        mockRetaliatorData.SetupGet(d => d.Disconnected).Returns(false);
        mockRetaliatorData.SetupGet(d => d.Object).Returns(mockRetaliatorPlayer.Object);
        mockRetaliatorPlayer.SetupGet(p => p.Data).Returns(mockRetaliatorData.Object);

        var mockAttackerPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockAttackerPlayer.SetupGet(p => p.PlayerId).Returns((byte)2);
        var mockAttackerData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockAttackerData.SetupGet(d => d.PlayerId).Returns((byte)2);
        mockAttackerData.SetupGet(d => d.Disconnected).Returns(false);
        mockAttackerData.SetupGet(d => d.Object).Returns(mockAttackerPlayer.Object);
        mockAttackerPlayer.SetupGet(p => p.Data).Returns(mockAttackerData.Object);

        ExtremeRoles.Performance.PlayerCache.AddPlayerControl(mockRetaliatorPlayer.Object);
        ExtremeRoles.Performance.PlayerCache.AddPlayerControl(mockAttackerPlayer.Object);

        // Act
        bool result = handler.TryKilledFrom(mockRetaliatorPlayer.Object, mockAttackerPlayer.Object);

        // Assert
        Assert.True(result); // Kill allowed because team did not match
    }

    [Fact]
    public void RetaliatorRole_IsBlockMeetingButtonAbility_WhenAwake_AllowsValidTargetRegardlessOfTeam()
    {
        // Arrange: Player 1 (Local) is Retaliator, Player 2 is target
        var targetRole = new YoYo(); // Impostor role
        targetRole.CreateRoleAllOption();
        targetRole.Initialize();

        var role = CreateAndInitRole(1);
        ExtremeRoleManager.GameRole[0] = targetRole;
        ExtremeRoleManager.GameRole[2] = targetRole;

        // Awaken Retaliator
        ((ITryKillTo)role.AbilityClass!).TryRolePlayerKillTo(null!, null!);

        var mockPva = new Mock<PlayerVoteArea>(IntPtr.Zero);
        mockPva.SetupGet(p => p.PlayerId).Returns((byte)2);

        // Act
        bool isBlocked = role.IsBlockMeetingButtonAbility(mockPva.Object);

		// Assert
		Assert.False(isBlocked); // Allowed in meeting regardless of target team (no team leak)
	}

    [Fact]
    public void RetaliatorRole_IsBlockMeetingButtonAbility_WhenTargetingSelf_BlocksAbility()
    {
        // Arrange: Player 1 (Local) is Retaliator
        var role = CreateAndInitRole(1);

        var mockPva = new Mock<PlayerVoteArea>(IntPtr.Zero);
        mockPva.SetupGet(p => p.PlayerId).Returns((byte)1); // Self

        // Act
        bool isBlocked = role.IsBlockMeetingButtonAbility(mockPva.Object);

        // Assert
        Assert.True(isBlocked); // Blocked for self
    }

    [Fact]
    public void RetaliatorRole_GetRolePlayerNameTag_WhenTargeted_ReturnsTagContainingString()
    {
        // Arrange
        var role = CreateAndInitRole(1);
        role.TargetPlayer(2);

        var crewmateRole = new Sheriff();
        crewmateRole.CreateRoleAllOption();
        crewmateRole.Initialize();

        // Act
        string tag = role.GetRolePlayerNameTag(crewmateRole, 2);
        string untargetedTag = role.GetRolePlayerNameTag(crewmateRole, 3);

        // Assert
        Assert.Contains("TestString", tag);
        Assert.DoesNotContain("TestString", untargetedTag);
    }
}
