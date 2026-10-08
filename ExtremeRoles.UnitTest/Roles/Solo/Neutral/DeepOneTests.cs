using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Roles.Solo.Neutral;
using Hazel;
using InnerNet;
using Moq;
using TMPro;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Neutral;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class DeepOneTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;
	private readonly Mock<MessageWriter> mockWriter;

	public DeepOneTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupPlayerVoteAreaMocks();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		SetupLobbyBehaviourMock();

		mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		if (Hazel.MockMessageWriterGetHelper.Instance == null)
		{
			var mockGet = new Mock<Hazel.MockMessageWriterGetHelper>();
			mockGet.Setup(g => g.Invoke(It.IsAny<SendOption>())).Returns(mockWriter.Object);
			Hazel.MockMessageWriterGetHelper.Instance = mockGet.Object;
		}

		if (InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance == null)
		{
			var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
			mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<MessageWriter>(), It.IsAny<InnerNetObject>()));
			InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;
		}

		mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockLocalData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockLocalData.SetupGet(d => d.IsDead).Returns(false);
		mockLocalData.SetupGet(d => d.Disconnected).Returns(false);
		mockLocalData.SetupGet(d => d.Object).Returns(mockLocalPlayer.Object);
		mockLocalPlayer.SetupGet(p => p.Data).Returns(mockLocalData.Object);

		MockSetupHelper.SetupGameDataMock();

		if (ExtremeRoles.GameMode.ExtremeGameModeManager.Instance == null)
		{
			ExtremeRoles.GameMode.ExtremeGameModeManager.Create(GameModes.Normal);
		}
		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		if (ExtremeRolesPlugin.ShipState == null)
		{
			var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(nameof(ExtremeRolesPlugin.ShipState), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
			shipStateProp?.SetValue(null, new ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus());
		}
	}

	private static void SetupLobbyBehaviourMock()
	{
		var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
		var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
		mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
		MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
	}

	private static void InitializeRole(SingleRoleBase role, byte playerId = 1)
	{
		role.CreateRoleAllOption();
		role.Initialize();

		ExtremeRoleManager.GameRole[playerId] = role;
	}

	#region DeepOneStatus Tests

	[Fact]
	public void DeepOneStatus_DefaultValues_AreAllFalse()
	{
		// Arrange
		var status = new DeepOneStatus(
			enableKillBlock: true,
			enableExileBlock: true,
			enableVentUnlock: true,
			enableMaxSpeed: true);

		// Act
		var isBlockKill = status.IsBlockKill;
		var isBlockExile = status.IsBlockExile;
		var useVent = status.UseVent;
		var isSpeedUp = status.IsSpeedUp;

		// Assert
		Assert.False(isBlockKill);
		Assert.False(isBlockExile);
		Assert.False(useVent);
		Assert.False(isSpeedUp);
	}

	[Theory]
	[InlineData(0, false, false, false, false)]
	[InlineData(1, true, false, false, false)]
	[InlineData(2, true, true, false, false)]
	[InlineData(3, true, true, true, false)]
	[InlineData(4, true, true, true, true)]
	[InlineData(5, true, true, true, true)]
	public void DeepOneStatus_UpdateStatus_TransitionsCorrectlyWhenEnabled(
		int frogCount, bool expectedKill, bool expectedExile, bool expectedVent, bool expectedSpeed)
	{
		// Arrange
		var status = new DeepOneStatus(
			enableKillBlock: true,
			enableExileBlock: true,
			enableVentUnlock: true,
			enableMaxSpeed: true);

		// Act
		status.UpdateStatus(frogCount);

		// Assert
		Assert.Equal(expectedKill, status.IsBlockKill);
		Assert.Equal(expectedExile, status.IsBlockExile);
		Assert.Equal(expectedVent, status.UseVent);
		Assert.Equal(expectedSpeed, status.IsSpeedUp);
	}

	[Fact]
	public void DeepOneStatus_UpdateStatus_RespectsDisabledOptions()
	{
		// Arrange
		var status = new DeepOneStatus(
			enableKillBlock: false,
			enableExileBlock: false,
			enableVentUnlock: false,
			enableMaxSpeed: false);

		// Act
		status.UpdateStatus(5);

		// Assert
		Assert.False(status.IsBlockKill);
		Assert.False(status.IsBlockExile);
		Assert.False(status.UseVent);
		Assert.False(status.IsSpeedUp);
	}

	[Fact]
	public void DeepOneStatus_UpdateStatus_ReturnsTrueWhenFrogCountDecreased()
	{
		// Arrange
		var status = new DeepOneStatus(true, true, true, true);
		status.UpdateStatus(3);

		// Act
		var resultDecreased = status.UpdateStatus(2);
		var resultIncreased = status.UpdateStatus(4);

		// Assert
		Assert.True(resultDecreased);
		Assert.False(resultIncreased);
	}

	[Fact]
	public void DeepOneStatus_ResetStatus_ResetsToNone()
	{
		// Arrange
		var status = new DeepOneStatus(true, true, true, true);
		status.UpdateStatus(5);

		// Act
		status.ResetStatus();

		// Assert
		Assert.False(status.IsBlockKill);
		Assert.False(status.IsBlockExile);
		Assert.False(status.UseVent);
		Assert.False(status.IsSpeedUp);
	}

	#endregion

	#region DeepOneAbilityHandler Tests

	[Fact]
	public void DeepOneAbilityHandler_IsBlockKillFrom_ReturnsStatusIsBlockKill()
	{
		// Arrange
		var system = new DeepOneFrogsControlSystem(5, false);
		var status = new DeepOneStatus(true, true, true, true);
		var handler = new DeepOneAbilityHandler(system, status);

		// Act before frog update
		var isBlockedBefore = handler.IsBlockKillFrom(2);
		var isValidKillBefore = handler.IsValidKillFromSource(2);
		var isValidAbilityBefore = handler.IsValidAbilitySource(2);

		status.UpdateStatus(1);

		// Act after frog update
		var isBlockedAfter = handler.IsBlockKillFrom(2);
		var isValidKillAfter = handler.IsValidKillFromSource(2);
		var isValidAbilityAfter = handler.IsValidAbilitySource(2);

		// Assert
		Assert.False(isBlockedBefore);
		Assert.True(isValidKillBefore);
		Assert.True(isValidAbilityBefore);

		Assert.True(isBlockedAfter);
		Assert.False(isValidKillAfter);
		Assert.False(isValidAbilityAfter);
	}

	#endregion

	#region DeepOne Role Tests

	[Fact]
	public void DeepOne_Initialize_RegistersRoleProperties()
	{
		// Arrange
		var deepOne = new DeepOne();

		// Act
		InitializeRole(deepOne, 1);

		// Assert
		Assert.Equal(ExtremeRoleId.DeepOne, deepOne.Core.Id);
		Assert.True(deepOne.IsNeutral());
		Assert.NotNull(deepOne.Status);
	}

	[Fact]
	public void DeepOne_UpdateFrogs_SetsWinStatusAndVent()
	{
		// Arrange
		var deepOne = new DeepOne();
		InitializeRole(deepOne, 1);

		// Act
		deepOne.UpdateFrogs(3);

		// Assert
		Assert.True(deepOne.UseVent);
		Assert.False(deepOne.IsWin);

		// Act for win threshold (default RequiredWinFrogs is 5)
		deepOne.UpdateFrogs(5);

		// Assert
		Assert.True(deepOne.IsWin);
	}

	[Fact]
	public void DeepOne_ModifiedVote_RemovesRolePlayerFromVoteResult_WhenExileNotBlocked()
	{
		// Arrange
		var deepOne = new DeepOne();
		InitializeRole(deepOne, 1);

		var voteTarget = new Dictionary<byte, byte> { { 2, 1 } };
		var voteResult = new Dictionary<byte, int> { { 1, 2 }, { 2, 1 } };

		// Act (Exile is NOT blocked when frog count is 0)
		deepOne.ModifiedVote(1, ref voteTarget, ref voteResult);

		// Assert
		Assert.False(voteResult.ContainsKey(1));
	}

	[Fact]
	public void DeepOne_ModifiedVote_DoesNotRemoveRolePlayer_WhenExileIsBlocked()
	{
		// Arrange
		var deepOne = new DeepOne();
		InitializeRole(deepOne, 1);
		deepOne.UpdateFrogs(2); // BlockExile threshold

		var voteTarget = new Dictionary<byte, byte> { { 2, 1 } };
		var voteResult = new Dictionary<byte, int> { { 1, 2 }, { 2, 1 } };

		// Act
		deepOne.ModifiedVote(1, ref voteTarget, ref voteResult);

		// Assert
		Assert.True(voteResult.ContainsKey(1));
	}

	[Fact]
	public void DeepOne_GetModdedVoteInfo_ReturnsNegativeVotes_WhenExileNotBlocked()
	{
		// Arrange
		var deepOne = new DeepOne();
		InitializeRole(deepOne, 1);

		var collector = new VoteInfoCollector();
		collector.AddTo(voter: 2, to: 1);

		var mockRolePlayer = new Mock<NetworkedPlayerInfo>();
		mockRolePlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		// Act
		var moddedVotes = deepOne.GetModdedVoteInfo(collector, mockRolePlayer.Object).ToList();

		// Assert
		Assert.Single(moddedVotes);
		Assert.Equal(-1, moddedVotes[0].Count);
	}

	[Fact]
	public void DeepOne_GetModdedVoteInfo_YieldsEmpty_WhenExileIsBlocked()
	{
		// Arrange
		var deepOne = new DeepOne();
		InitializeRole(deepOne, 1);
		deepOne.UpdateFrogs(2); // BlockExile threshold

		var collector = new VoteInfoCollector();
		collector.AddTo(voter: 2, to: 1);

		var mockRolePlayer = new Mock<NetworkedPlayerInfo>();
		mockRolePlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		// Act
		var moddedVotes = deepOne.GetModdedVoteInfo(collector, mockRolePlayer.Object).ToList();

		// Assert
		Assert.Empty(moddedVotes);
	}

	[Fact]
	public void DeepOne_AllReset_ResetsState()
	{
		// Arrange
		var deepOne = new DeepOne();
		InitializeRole(deepOne, 1);
		deepOne.UpdateFrogs(5);

		// Act
		deepOne.AllReset(mockLocalPlayer.Object);

		// Assert
		Assert.False(deepOne.IsWin);
		Assert.False(deepOne.UseVent);
	}

	[Fact]
	public void DeepOne_IsSameTeam_DelegatesToNeutralSameTeam()
	{
		// Arrange
		var deepOne = new DeepOne();
		var otherNeutral = new DeepOne();

		// Act
		var isSame = deepOne.IsSameTeam(otherNeutral);

		// Assert
		Assert.True(isSame);
	}

	#endregion

	#region DeepOneFrogsControlSystem & FrogBehavior Tests

	[Fact]
	public void DeepOneFrogsControlSystem_GetRequiredClicks_WithoutMultiplier_ReturnsBase()
	{
		// Arrange
		var system = new DeepOneFrogsControlSystem(baseRequiredClicks: 5, enableClickMultiplier: false);

		// Act
		var clicks = system.GetRequiredClicks(1);

		// Assert
		Assert.Equal(5, clicks);
	}

	[Fact]
	public void DeepOneFrogsControlSystem_GetRequiredClicks_WithMultiplier_CalculatesMultiplier()
	{
		// Arrange
		var system = new DeepOneFrogsControlSystem(baseRequiredClicks: 5, enableClickMultiplier: true);

		// Act
		var clicksNormal = system.GetRequiredClicks(1);

		// Assert
		Assert.Equal(5, clicksNormal);
	}

	[Fact]
	public void FrogBehavior_CanUse_ReturnsTrueWhenAlive()
	{
		// Arrange
		var mockText = new Mock<TextMeshPro>();
		var behavior = new FrogBehavior(frogId: 1, reqClicks: 3, textLabel: mockText.Object);

		var mockPlayer = new Mock<NetworkedPlayerInfo>();
		var mockPlayerControl = new Mock<PlayerControl>(IntPtr.Zero);

		mockPlayer.SetupGet(p => p.Object).Returns(mockPlayerControl.Object);
		mockPlayer.SetupGet(p => p.IsDead).Returns(false);
		mockPlayer.SetupGet(p => p.Disconnected).Returns(false);

		// Act
		var canUse = behavior.CanUse(mockPlayer.Object);

		// Assert
		Assert.True(canUse);
	}

	#endregion
}
