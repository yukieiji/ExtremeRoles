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
	public void CalculateSpeedMod_PositiveAndNegative_ReturnsExpectedValues()
	{
		// Positive speed (e.g. +3% per burger)
		var system1 = new BurgermonSystem(0.03f, 0.25f, true, true);
		Assert.Equal(1.0f, system1.CalculateSpeedMod(0));
		Assert.Equal(1.03f, system1.CalculateSpeedMod(1), 3);
		Assert.Equal(1.06f, system1.CalculateSpeedMod(2), 3);

		// Negative speed (e.g. -5% per burger)
		var system2 = new BurgermonSystem(-0.05f, 0.25f, true, true);
		Assert.Equal(0.95f, system2.CalculateSpeedMod(1), 3);
		Assert.Equal(0.90f, system2.CalculateSpeedMod(2), 3);

		// Zero speed change
		var system3 = new BurgermonSystem(0.0f, 0.25f, true, true);
		Assert.Equal(1.0f, system3.CalculateSpeedMod(5));
	}

	[Fact]
	public void CalculateScaleFactor_PositiveAndNegative_ReturnsExpectedValues()
	{
		// Positive size (e.g. +25% per burger)
		var system1 = new BurgermonSystem(0.03f, 0.25f, true, true);
		Assert.Equal(1.0f, system1.CalculateScaleFactor(0));
		Assert.Equal(1.25f, system1.CalculateScaleFactor(1), 3);
		Assert.Equal(1.50f, system1.CalculateScaleFactor(2), 3);

		// Negative size (e.g. -20% per burger)
		var system2 = new BurgermonSystem(0.03f, -0.20f, true, true);
		Assert.Equal(0.80f, system2.CalculateScaleFactor(1), 3);
		Assert.Equal(0.60f, system2.CalculateScaleFactor(2), 3);

		// Zero size change
		var system3 = new BurgermonSystem(0.03f, 0.0f, true, true);
		Assert.Equal(1.0f, system3.CalculateScaleFactor(3));
	}

	[Fact]
	public void FeedBurger_IncrementsPendingCount_AndTransfersOnMeetingStart()
	{
		var system = new BurgermonSystem();
		byte targetId = 2;

		Assert.Equal(0, system.GetTotalFedCount(targetId));
		Assert.Equal(0, system.GetActiveFedCount(targetId));

		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)BurgermonRpcOps.FeedBurger)
			.Returns(targetId);

		system.UpdateSystem(null!, mockReader.Object);

		Assert.Equal(1, system.GetTotalFedCount(targetId));
		Assert.Equal(0, system.GetActiveFedCount(targetId));

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

		Assert.Equal(0.03f, system.SpeedValue, 3);
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

		string tagBefore = burgermon.GetRolePlayerNameTag(burgermon, targetId);
		Assert.Equal("", tagBefore);

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

		handler.Update(mockPlayer.Object);
		handler.ClearTask(mockPlayer.Object);
	}
}
