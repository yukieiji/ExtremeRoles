using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using Moq;
using UnityEngine;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection("UnityMock")]
public sealed class MonikaLoveTargetMeetingTests : IDisposable
{
	private sealed class DummyRole : SingleRoleBase
	{
		public DummyRole(RoleArgs args) : base(args) { }
		public override Color GetNameColor(bool isDead) => Color.white;
		public override string GetColoredRoleName(bool isDead = false) => "Dummy";
		public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => "";
		public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => Color.white;
		protected override void RoleSpecificInit() { }
		protected override void CreateSpecificOption(ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory parentOps) { }
	}

	public MonikaLoveTargetMeetingTests()
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
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		var manager = ExtremeSystemTypeManager.Instance;
		var flags = BindingFlags.NonPublic | BindingFlags.Instance;
		typeof(ExtremeSystemTypeManager).GetField("allSystems", flags)?.SetValue(manager, new Dictionary<ExtremeSystemType, IExtremeSystemType>());
		typeof(ExtremeSystemTypeManager).GetField("dirtableSystems", flags)?.SetValue(manager, new Dictionary<ExtremeSystemType, IDirtableSystemType>());
		typeof(ExtremeSystemTypeManager).GetField("sabotageSystem", flags)?.SetValue(manager, new List<ISabotageExtremeSystemType>());
		typeof(ExtremeSystemTypeManager).GetField("dirtySystem", flags)?.SetValue(manager, new List<ExtremeSystemType>());

		setupTranslationController();

		if (ExtremeRolesPlugin.ShipState == null)
		{
			var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(nameof(ExtremeRolesPlugin.ShipState), BindingFlags.Public | BindingFlags.Static);
			shipStateProp?.SetValue(null, new ExtremeShipStatus());
		}

		ExtremeRoleManager.GameRole.Clear();
	}

	private static MonikaTrashSystem registerMonikaTrashSystem()
	{
		return ExtremeSystemTypeManager.Instance.CreateOrGet<MonikaTrashSystem>(
			ExtremeSystemType.MonikaTrashSystem,
			() => new MonikaTrashSystem(false));
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
	public void Constructor_WithoutMonikaSystem_ThrowsInvalidOperationException()
	{
		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => new MonikaLoveTargetMeeting());
	}

	[Fact]
	public void Constructor_WithMonikaSystem_InitializesProperties()
	{
		// Arrange
		registerMonikaTrashSystem();

		// Act
		var meeting = new MonikaLoveTargetMeeting();

		// Assert
		Assert.False(meeting.SkipButtonActive);
		Assert.Equal(byte.MaxValue, meeting.VoteTarget);
		Assert.Equal(VoteAreaState.None, meeting.GetVoteAreaState(null!));
		Assert.True(meeting.IsValidShowChatPlayer(null!));
		Assert.False(meeting.TryStartMeeting(1));
		Assert.False(meeting.TryGetGameEndReason(out var reason));
		Assert.Equal(RoleGameOverReason.UnKnown, reason);
	}

	[Fact]
	public void ValidPlayer_And_VoteTarget_And_CreateVoteResult_Flow()
	{
		// Arrange
		registerMonikaTrashSystem();
		var meeting = new MonikaLoveTargetMeeting();

		var mockGameData = MockSetupHelper.SetupGameDataMock();

		var mockPlayer1 = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockPlayer1.SetupGet(p => p.PlayerId).Returns((byte)1);
		mockPlayer1.SetupGet(p => p.PlayerName).Returns("Target1");
		mockPlayer1.SetupGet(p => p.IsDead).Returns(false);
		mockPlayer1.SetupGet(p => p.Disconnected).Returns(false);

		var mockPlayer2 = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockPlayer2.SetupGet(p => p.PlayerId).Returns((byte)2);
		mockPlayer2.SetupGet(p => p.PlayerName).Returns("Target2");
		mockPlayer2.SetupGet(p => p.IsDead).Returns(false);
		mockPlayer2.SetupGet(p => p.Disconnected).Returns(false);

		var mockMonikaPlayer = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockMonikaPlayer.SetupGet(p => p.PlayerId).Returns((byte)3);
		mockMonikaPlayer.SetupGet(p => p.PlayerName).Returns("Monika");
		mockMonikaPlayer.SetupGet(p => p.IsDead).Returns(false);
		mockMonikaPlayer.SetupGet(p => p.Disconnected).Returns(false);

		var monikaRole = new DummyRole(RoleArgs.BuildNeutral(ExtremeRoleId.Monika, Color.magenta, RolePropPresets.OptionalDefault));
		ExtremeRoleManager.GameRole[3] = monikaRole;

		var players = new List<NetworkedPlayerInfo?> { mockPlayer1.Object, mockPlayer2.Object, mockMonikaPlayer.Object };

		var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		mockList.SetupGet(l => l.Count).Returns(players.Count);
		mockList.Setup(l => l[It.IsAny<int>()]).Returns((int i) => players[i]!);

		mockGameData.SetupGet(g => g.AllPlayers).Returns(mockList.Object);
		mockGameData.Setup(g => g.GetPlayerById(3)).Returns(mockMonikaPlayer.Object);

		// Act - Populate targets via ValidPlayer
		var valid = meeting.ValidPlayer.ToList();

		// Assert ValidPlayer excludes Monika
		Assert.Equal(2, valid.Count);
		Assert.Contains((byte)1, valid);
		Assert.Contains((byte)2, valid);

		// Act - Set VoteTarget to 1 (Player 1 wins, Player 2 is another)
		meeting.VoteTarget = byte.MaxValue; // Should ignore byte.MaxValue
		Assert.Equal(byte.MaxValue, meeting.VoteTarget);

		meeting.VoteTarget = 1;

		// Assert VoteTarget, GameEndReason, and CreateVoteResult
		Assert.Equal(1, meeting.VoteTarget);
		Assert.True(meeting.TryGetGameEndReason(out var endReason));
		Assert.Equal(RoleGameOverReason.MonikaThisGameIsMine, endReason);

		var voteResult = meeting.CreateVoteResult(null!, 1);
		Assert.Equal(1, voteResult.VoteFor);
		Assert.Same(mockPlayer2.Object, voteResult.ExiledTarget);

		// ExiledInfo test
		var exiledInfo = meeting.CreateExiledInfo(3);
		Assert.True(exiledInfo.IsShowPlayer);
		Assert.Equal("MonikaMeetingExiled", exiledInfo.Text);
	}

	[Fact]
	public void CreateExiledInfo_WhenMonikaPlayerNotFound_ReturnsUnknownInfo()
	{
		// Arrange
		registerMonikaTrashSystem();
		var meeting = new MonikaLoveTargetMeeting();

		var mockGameData = MockSetupHelper.SetupGameDataMock();
		mockGameData.Setup(g => g.GetPlayerById(99)).Returns((NetworkedPlayerInfo)null!);

		// Act
		var info = meeting.CreateExiledInfo(99);

		// Assert
		Assert.True(info.IsShowPlayer);
		Assert.Equal("UNKNOWN MEETING PLAYER!!!", info.Text);
	}

	[Fact]
	public void GetTitle_ReturnsCorrectKeysForCallerAndTargetAndOther()
	{
		// Arrange
		registerMonikaTrashSystem();
		var meeting = new MonikaLoveTargetMeeting();

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		// Caller is LocalPlayer (1)
		Assert.Equal("MonikaMeetingSelectLover", meeting.GetTitle(1));

		// Caller is OtherPlayer (2)
		Assert.Equal("MonikaMeetingOther", meeting.GetTitle(2));
	}

	[Fact]
	public void IsDefaultForegroundForDead_ChecksLocalPlayerId()
	{
		// Arrange
		registerMonikaTrashSystem();
		var meeting = new MonikaLoveTargetMeeting();

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		// Act & Assert
		Assert.False(meeting.IsDefaultForegroundForDead(null!, 1));
		Assert.True(meeting.IsDefaultForegroundForDead(null!, 2));
	}

	[Fact]
	public void CanChatPlayer_ChecksPlayerValidity()
	{
		// Arrange
		registerMonikaTrashSystem();
		var meeting = new MonikaLoveTargetMeeting();

		var mockAlivePlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var mockAliveData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockAliveData.SetupGet(d => d.IsDead).Returns(false);
		mockAliveData.SetupGet(d => d.Disconnected).Returns(false);
		mockAliveData.SetupGet(d => d.Object).Returns(mockAlivePlayer.Object);
		mockAlivePlayer.SetupGet(p => p.Data).Returns(mockAliveData.Object);

		// Act & Assert
		Assert.True(meeting.CanChatPlayer(mockAlivePlayer.Object));
	}
}
