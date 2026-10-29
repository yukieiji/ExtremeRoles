using System;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using Moq;
using UnityEngine;
using Xunit;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Crewmate;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class BurgermonSystemTests
{
	public BurgermonSystemTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void CalculateSpeedMod_ReturnsExpectedValues()
	{
		var system = new BurgermonSystem();

		// Fast
		system.SpeedMode = BurgermonSpeedMode.Fast;
		system.SpeedValue = 0.03f;
		Assert.Equal(1.0f, system.CalculateSpeedMod(0));
		Assert.Equal(1.03f, system.CalculateSpeedMod(1), 3);
		Assert.Equal(1.06f, system.CalculateSpeedMod(2), 3);

		// Slow
		system.SpeedMode = BurgermonSpeedMode.Slow;
		system.SpeedValue = 0.05f;
		Assert.Equal(0.95f, system.CalculateSpeedMod(1), 3);
		Assert.Equal(0.90f, system.CalculateSpeedMod(2), 3);

		// None
		system.SpeedMode = BurgermonSpeedMode.None;
		Assert.Equal(1.0f, system.CalculateSpeedMod(5));
	}

	[Fact]
	public void CalculateScaleFactor_ReturnsExpectedValues()
	{
		var system = new BurgermonSystem();

		// Big
		system.SizeMode = BurgermonSizeMode.Big;
		system.SizeValue = 0.25f;
		Assert.Equal(1.0f, system.CalculateScaleFactor(0));
		Assert.Equal(1.25f, system.CalculateScaleFactor(1), 3);
		Assert.Equal(1.50f, system.CalculateScaleFactor(2), 3);

		// Small
		system.SizeMode = BurgermonSizeMode.Small;
		system.SizeValue = 0.20f;
		Assert.Equal(0.80f, system.CalculateScaleFactor(1), 3);
		Assert.Equal(0.60f, system.CalculateScaleFactor(2), 3);

		// None
		system.SizeMode = BurgermonSizeMode.None;
		Assert.Equal(1.0f, system.CalculateScaleFactor(3));
	}

	[Fact]
	public void FeedBurger_IncrementsPendingCount_AndTransfersOnMeetingStart()
	{
		var system = new BurgermonSystem();
		byte targetId = 2;

		Assert.Equal(0, system.GetTotalFedCount(targetId));
		Assert.Equal(0, system.GetActiveFedCount(targetId));

		// Simulate RPC UpdateSystem
		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)BurgermonRpcOps.FeedBurger)
			.Returns(targetId);

		system.UpdateSystem(null!, mockReader.Object);

		Assert.Equal(1, system.GetTotalFedCount(targetId));
		Assert.Equal(0, system.GetActiveFedCount(targetId));

		// Reset on Meeting Start
		system.Reset(ResetTiming.MeetingStart);

		Assert.Equal(1, system.GetTotalFedCount(targetId));
		Assert.Equal(1, system.GetActiveFedCount(targetId));
	}

	[Fact]
	public void Reset_OnPlayer_ClearsFedCountsForPlayer()
	{
		var system = new BurgermonSystem();
		byte targetId = 2;

		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)BurgermonRpcOps.FeedBurger)
			.Returns(targetId);

		system.UpdateSystem(null!, mockReader.Object);
		system.Reset(ResetTiming.MeetingStart);

		Assert.Equal(1, system.GetActiveFedCount(targetId));

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(targetId);

		system.Reset(ResetTiming.OnPlayer, mockPlayer.Object);

		Assert.Equal(0, system.GetActiveFedCount(targetId));
	}
}

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class BurgermonRoleTests
{
	private readonly Mock<AmongUsClient> clientMock;

	public BurgermonRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		MockSetupHelper.SetupOptionManager();

		this.clientMock = MockSetupHelper.SetupAmongUsClientMock();
		SetLobbyMode(false);

		ExtremeGameModeManager.Create(GameModes.Normal);
		ExtremeRoleManager.GameRole.Clear();
	}

	private void SetLobbyMode(bool isLobby)
	{
		if (isLobby)
		{
			this.clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.NotJoined);
			var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
			var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
			mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
			MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
		}
		else
		{
			this.clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
			var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
			mockLobbyHelper.Setup(x => x.Invoke()).Returns((LobbyBehaviour)null!);
			MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
		}
	}

	[Fact]
	public void RoleSpecificInit_SetsSystemOptionsCorrectly()
	{
		var burgermon = new Burgermon();
		burgermon.CreateRoleAllOption();
		burgermon.Initialize();

		var system = BurgermonSystem.Get();

		Assert.Equal(BurgermonSpeedMode.Fast, system.SpeedMode);
		Assert.Equal(0.03f, system.SpeedValue, 3);
		Assert.Equal(BurgermonSizeMode.Big, system.SizeMode);
		Assert.Equal(0.25f, system.SizeValue, 3);
		Assert.True(system.KeepAfterDeath);
		Assert.True(system.AffectDeadBody);
	}

	[Fact]
	public void GetRolePlayerNameTag_WhenTargetHasFedBurgers_ReturnsFormattedTag()
	{
		var burgermon = new Burgermon();
		burgermon.CreateRoleAllOption();
		burgermon.Initialize();

		byte targetId = 3;
		var system = BurgermonSystem.Get();

		// Before feed
		string tagBefore = burgermon.GetRolePlayerNameTag(burgermon, targetId);
		Assert.Equal("", tagBefore);

		// Feed 2 burgers
		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)BurgermonRpcOps.FeedBurger)
			.Returns(targetId);
		system.UpdateSystem(null!, mockReader.Object);

		mockReader = new Mock<MessageReader>(IntPtr.Zero);
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)BurgermonRpcOps.FeedBurger)
			.Returns(targetId);
		system.UpdateSystem(null!, mockReader.Object);

		// After feed
		string tagAfter = burgermon.GetRolePlayerNameTag(burgermon, targetId);
		Assert.Contains("(2)", tagAfter);
		Assert.Contains("#B16423", tagAfter);
	}

	[Fact]
	public void TaskHandler_Update_ExecutesWithoutError()
	{
		var handler = new BurgermonTaskHandler();

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		var mockTasks = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		mockTasks.SetupGet(t => t.Count).Returns(0);
		mockData.SetupGet(d => d.Tasks).Returns(mockTasks.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		// Should return gracefully without throwing
		handler.Update(mockPlayer.Object);
		handler.ClearTask(mockPlayer.Object);
	}
}
