using System;
using System.Collections.Generic;
using System.Linq;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Crewmate;
using Moq;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection("UnityMock")]
public sealed class CEOForceMeetingTests : IDisposable
{
	public CEOForceMeetingTests()
	{
		resetState();
	}

	public void Dispose()
	{
		resetState();
	}

	private static void resetState()
	{
		PlayerCache.RemovePlayerControl(_ => true);
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupLogger();
		MockSetupHelper.SetupGameDataMock();
		MockSetupHelper.SetupPlayerControlMocks();

		setupTranslationController();
		setupObjectImplicitHelpers();
	}

	private static void setupTranslationController()
	{
		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation
			.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
		mockTranslation
			.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object>>()))
			.Returns((string id, string defaultStr, Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object> parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
	}

	private static void setupObjectImplicitHelpers()
	{
		Il2CppSystem.MockObjectop_ImplicitHelper.Instance ??= new Mock<Il2CppSystem.MockObjectop_ImplicitHelper>().Object;
		Il2CppSystem.MockObjectop_ImplicitHelper2.Instance ??= new Mock<Il2CppSystem.MockObjectop_ImplicitHelper2>().Object;
		Il2CppSystem.MockObjectop_ImplicitHelper3.Instance ??= new Mock<Il2CppSystem.MockObjectop_ImplicitHelper3>().Object;
		Il2CppSystem.MockObjectop_ImplicitHelper4.Instance ??= new Mock<Il2CppSystem.MockObjectop_ImplicitHelper4>().Object;
		Il2CppSystem.MockObjectop_ImplicitHelper5.Instance ??= new Mock<Il2CppSystem.MockObjectop_ImplicitHelper5>().Object;
		Il2CppSystem.MockObjectop_ImplicitHelper6.Instance ??= new Mock<Il2CppSystem.MockObjectop_ImplicitHelper6>().Object;
		Il2CppSystem.MockObjectop_ImplicitHelper7.Instance ??= new Mock<Il2CppSystem.MockObjectop_ImplicitHelper7>().Object;
		Il2CppSystem.MockObjectop_ImplicitHelper8.Instance ??= new Mock<Il2CppSystem.MockObjectop_ImplicitHelper8>().Object;
		Il2CppSystem.MockObjectop_ImplicitHelper9.Instance ??= new Mock<Il2CppSystem.MockObjectop_ImplicitHelper9>().Object;
		Il2CppSystem.MockObjectop_ImplicitHelper10.Instance ??= new Mock<Il2CppSystem.MockObjectop_ImplicitHelper10>().Object;
		Il2CppSystem.MockObjectop_ImplicitHelper11.Instance ??= new Mock<Il2CppSystem.MockObjectop_ImplicitHelper11>().Object;
		Il2CppSystem.MockObjectop_ImplicitHelper12.Instance ??= new Mock<Il2CppSystem.MockObjectop_ImplicitHelper12>().Object;
		Il2CppSystem.MockObjectop_ImplicitHelper13.Instance ??= new Mock<Il2CppSystem.MockObjectop_ImplicitHelper13>().Object;
	}

	[Fact]
	public void PropertiesAndDefaults_AreCorrect()
	{
		// Arrange
		var meeting = new CEOForceMeeting();

		// Act
		meeting.VoteTarget = 5;

		// Assert
		Assert.True(meeting.SkipButtonActive);
		Assert.Equal(5, meeting.VoteTarget);
		Assert.Equal(VoteAreaState.None, meeting.GetVoteAreaState(null!));
		Assert.False(meeting.TryGetGameEndReason(out var reason));
		Assert.Equal(RoleGameOverReason.UnKnown, reason);
	}

	[Fact]
	public void ValidPlayer_FiltersDeadAndDisconnectedAndNullPlayers()
	{
		// Arrange
		var meeting = new CEOForceMeeting();

		var mockGameData = MockSetupHelper.SetupGameDataMock();
		var mockAlive = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockAlive.SetupGet(p => p.PlayerId).Returns((byte)1);
		mockAlive.SetupGet(p => p.IsDead).Returns(false);
		mockAlive.SetupGet(p => p.Disconnected).Returns(false);

		var mockDead = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockDead.SetupGet(p => p.PlayerId).Returns((byte)2);
		mockDead.SetupGet(p => p.IsDead).Returns(true);
		mockDead.SetupGet(p => p.Disconnected).Returns(false);

		var mockDisconnected = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockDisconnected.SetupGet(p => p.PlayerId).Returns((byte)3);
		mockDisconnected.SetupGet(p => p.IsDead).Returns(false);
		mockDisconnected.SetupGet(p => p.Disconnected).Returns(true);

		var players = new List<NetworkedPlayerInfo?> { mockAlive.Object, mockDead.Object, mockDisconnected.Object, null };

		var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		mockList.SetupGet(l => l.Count).Returns(players.Count);
		mockList.Setup(l => l[It.IsAny<int>()]).Returns((int i) => players[i]!);

		mockGameData.SetupGet(g => g.AllPlayers).Returns(mockList.Object);

		// Act
		var validPlayers = meeting.ValidPlayer.ToList();

		// Assert
		Assert.Single(validPlayers);
		Assert.Contains((byte)1, validPlayers);
	}

	[Fact]
	public void CreateExiledInfo_WhenTargetNull_ReturnsSkipInfo()
	{
		// Arrange
		var meeting = new CEOForceMeeting { VoteTarget = 99 };
		var mockGameData = MockSetupHelper.SetupGameDataMock();
		mockGameData.Setup(g => g.GetPlayerById(99)).Returns((NetworkedPlayerInfo)null!);

		// Act
		var info = meeting.CreateExiledInfo(1);

		// Assert
		Assert.False(info.IsShowPlayer);
		Assert.Equal("CEOMeetingSkip", info.Text);
	}

	[Fact]
	public void CreateExiledInfo_WhenTargetExists_ReturnsSelectInfo()
	{
		// Arrange
		var meeting = new CEOForceMeeting { VoteTarget = 1 };
		var mockGameData = MockSetupHelper.SetupGameDataMock();
		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTarget.SetupGet(p => p.PlayerName).Returns("CEO_Target");
		mockGameData.Setup(g => g.GetPlayerById(1)).Returns(mockTarget.Object);

		// Act
		var info = meeting.CreateExiledInfo(1);

		// Assert
		Assert.True(info.IsShowPlayer);
		Assert.Equal("CEOMeetingSelect", info.Text);
	}

	[Fact]
	public void CreateVoteResult_ReturnsTargetInfo()
	{
		// Arrange
		var meeting = new CEOForceMeeting();
		var mockGameData = MockSetupHelper.SetupGameDataMock();
		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockGameData.Setup(g => g.GetPlayerById(1)).Returns(mockTarget.Object);

		// Act
		var result = meeting.CreateVoteResult(null!, 1);

		// Assert
		Assert.Equal(1, result.VoteFor);
		Assert.Same(mockTarget.Object, result.ExiledTarget);
	}

	[Fact]
	public void GetTitle_And_IsDefaultForegroundForDead_CheckLocalPlayerId()
	{
		// Arrange
		var meeting = new CEOForceMeeting();
		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		// Act & Assert
		Assert.Equal("CEOMeetingCEO", meeting.GetTitle(1));
		Assert.Equal("CEOMeetingOther", meeting.GetTitle(2));

		Assert.False(meeting.IsDefaultForegroundForDead(null!, 1));
		Assert.True(meeting.IsDefaultForegroundForDead(null!, 2));
	}

	[Fact]
	public void CanChatPlayer_And_IsValidShowChatPlayer_CheckPlayerIsAlive()
	{
		// Arrange
		var meeting = new CEOForceMeeting();

		var mockAlivePlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var mockAliveData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockAliveData.SetupGet(d => d.IsDead).Returns(false);
		mockAliveData.SetupGet(d => d.Disconnected).Returns(false);
		mockAliveData.SetupGet(d => d.Object).Returns(mockAlivePlayer.Object);
		mockAlivePlayer.SetupGet(p => p.Data).Returns(mockAliveData.Object);

		var mockDeadPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var mockDeadData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockDeadData.SetupGet(d => d.IsDead).Returns(true);
		mockDeadData.SetupGet(d => d.Object).Returns(mockDeadPlayer.Object);
		mockDeadPlayer.SetupGet(p => p.Data).Returns(mockDeadData.Object);

		// Act & Assert
		Assert.True(meeting.CanChatPlayer(mockAlivePlayer.Object));
		Assert.False(meeting.CanChatPlayer(mockDeadPlayer.Object));

		Assert.True(meeting.IsValidShowChatPlayer(mockAlivePlayer.Object));
		Assert.False(meeting.IsValidShowChatPlayer(mockDeadPlayer.Object));
	}

	[Fact]
	public void TryStartMeeting_WhenRoleNotCEO_ReturnsFalse()
	{
		// Arrange
		var meeting = new CEOForceMeeting();

		// Act
		bool result = meeting.TryStartMeeting(10);

		// Assert
		Assert.False(result);
	}
}
