using System;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using Moq;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection("UnityMock")]
public sealed class AssassinAssassinateTargetMeetingTests : IDisposable
{
	private sealed class DummyRole : SingleRoleBase
	{
		public DummyRole(RoleArgs args) : base(args) { }
		public override UnityEngine.Color GetNameColor(bool isDead) => UnityEngine.Color.white;
		public override string GetColoredRoleName(bool isDead = false) => "Dummy";
		public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => "";
		public override UnityEngine.Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => UnityEngine.Color.white;
		protected override void RoleSpecificInit() { }
		protected override void CreateSpecificOption(ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory parentOps) { }
	}

	public AssassinAssassinateTargetMeetingTests()
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
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupPlayerVoteAreaMocks();

		setupTranslationController();

		ExtremeRoleManager.GameRole.Clear();
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

	[Fact]
	public void PropertiesAndDefaults_AreCorrect()
	{
		// Arrange
		var meeting = new AssassinAssassinateTargetMeeting();

		// Assert
		Assert.False(meeting.SkipButtonActive);
		Assert.Equal(byte.MaxValue, meeting.VoteTarget);

		Assert.False(meeting.TryGetGameEndReason(out var reason));
		Assert.Equal(RoleGameOverReason.UnKnown, reason);
	}

	[Fact]
	public void VoteTarget_WhenSetToNonMarlin_FailsAssassination()
	{
		// Arrange
		var meeting = new AssassinAssassinateTargetMeeting();

		// Act
		meeting.VoteTarget = 5;

		// Assert
		Assert.Equal(5, meeting.VoteTarget);
		Assert.False(meeting.TryGetGameEndReason(out var reason));
		Assert.Equal(RoleGameOverReason.UnKnown, reason);
	}

	[Fact]
	public void VoteTarget_WhenSetToMarlin_SucceedsAssassination()
	{
		// Arrange
		var meeting = new AssassinAssassinateTargetMeeting();
		var marlinRole = new DummyRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Marlin, UnityEngine.Color.blue));
		ExtremeRoleManager.GameRole[5] = marlinRole;

		var mockGameData = MockSetupHelper.SetupGameDataMock();
		var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTarget.SetupGet(p => p.PlayerName).Returns("MarlinPlayer");
		mockGameData.Setup(g => g.GetPlayerById(5)).Returns(mockTarget.Object);

		// Act
		meeting.VoteTarget = 5;

		// Assert
		Assert.Equal(5, meeting.VoteTarget);
		Assert.True(meeting.TryGetGameEndReason(out var reason));
		Assert.Equal(RoleGameOverReason.AssassinationMarin, reason);

		var info = meeting.CreateExiledInfo(1);
		Assert.False(info.IsShowPlayer);
		Assert.Contains("MarlinPlayer", info.Text);
	}

	[Fact]
	public void CreateExiledInfo_WhenTargetPlayerNull_ReturnsUnknown()
	{
		// Arrange
		var meeting = new AssassinAssassinateTargetMeeting { VoteTarget = 99 };
		var mockGameData = MockSetupHelper.SetupGameDataMock();
		mockGameData.Setup(g => g.GetPlayerById(99)).Returns((NetworkedPlayerInfo)null!);

		// Act
		var info = meeting.CreateExiledInfo(1);

		// Assert
		Assert.True(info.IsShowPlayer);
		Assert.Equal("UNKNOWN TARGET PLAYER!!!", info.Text);
	}

	[Fact]
	public void CreateVoteResult_WithSpecificTarget_ReturnsTargetWithNullExiled()
	{
		// Arrange
		var meeting = new AssassinAssassinateTargetMeeting();

		// Act
		var result = meeting.CreateVoteResult(null!, 3);

		// Assert
		Assert.Equal(3, result.VoteFor);
		Assert.Null(result.ExiledTarget);
	}

	[Fact]
	public void GetTitle_And_IsDefaultForegroundForDead_BehaveAsExpected()
	{
		// Arrange
		var meeting = new AssassinAssassinateTargetMeeting();
		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		// Act & Assert
		Assert.Equal("whoIsMarine", meeting.GetTitle(1));
		Assert.False(meeting.IsDefaultForegroundForDead(null!, 1));
		Assert.True(meeting.IsDefaultForegroundForDead(null!, 2));
	}

	[Fact]
	public void GetVoteAreaState_ChecksDisconnectedAndDead()
	{
		// Arrange
		var meeting = new AssassinAssassinateTargetMeeting();

		var mockAlive = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockAlive.SetupGet(p => p.IsDead).Returns(false);
		mockAlive.SetupGet(p => p.Disconnected).Returns(false);

		var mockDead = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockDead.SetupGet(p => p.IsDead).Returns(true);
		mockDead.SetupGet(p => p.Disconnected).Returns(false);

		var mockDisconnected = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockDisconnected.SetupGet(p => p.IsDead).Returns(false);
		mockDisconnected.SetupGet(p => p.Disconnected).Returns(true);

		// Act & Assert
		Assert.Equal(VoteAreaState.None, meeting.GetVoteAreaState(mockAlive.Object));
		Assert.Equal(VoteAreaState.XMark, meeting.GetVoteAreaState(mockDead.Object));
		Assert.Equal(VoteAreaState.XMark, meeting.GetVoteAreaState(mockDisconnected.Object));
	}

	[Fact]
	public void CanChatPlayer_WhenPlayerIsImpostor_ReturnsTrue()
	{
		// Arrange
		var meeting = new AssassinAssassinateTargetMeeting();
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)5);

		var impostorRole = new DummyRole(RoleArgs.BuildImpostor(ExtremeRoleId.Assassin));
		ExtremeRoleManager.GameRole[5] = impostorRole;

		// Act
		bool canChat = meeting.CanChatPlayer(mockPlayer.Object);

		// Assert
		Assert.True(canChat);
	}

	[Fact]
	public void IsValidShowChatPlayer_WhenBothAreImpostors_ReturnsTrue()
	{
		// Arrange
		var meeting = new AssassinAssassinateTargetMeeting();

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockSource = new Mock<PlayerControl>(IntPtr.Zero);
		mockSource.SetupGet(p => p.PlayerId).Returns((byte)2);

		var localRole = new DummyRole(RoleArgs.BuildImpostor(ExtremeRoleId.Assassin));
		var sourceRole = new DummyRole(RoleArgs.BuildImpostor(ExtremeRoleId.BountyHunter));

		ExtremeRoleManager.GameRole[1] = localRole;
		ExtremeRoleManager.GameRole[2] = sourceRole;

		// Act
		bool result = meeting.IsValidShowChatPlayer(mockSource.Object);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void TryStartMeeting_WhenNotAssassin_ReturnsFalse()
	{
		// Arrange
		var meeting = new AssassinAssassinateTargetMeeting();

		// Act
		bool result = meeting.TryStartMeeting(1);

		// Assert
		Assert.False(result);
	}
}
