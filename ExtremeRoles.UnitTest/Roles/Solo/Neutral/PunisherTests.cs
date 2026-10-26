using System;
using AmongUs.GameOptions;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.Roles.Solo.Neutral;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Neutral;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class PunisherTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;

	public PunisherTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		SetupLobbyBehaviourMock();

		mockClient = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		MockSetupHelper.SetupGameDataMock();

		var mockMeetingHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHelper.Setup(h => h.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHelper.Object;

		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		if (ExtremeRolesPlugin.ShipState == null)
		{
			var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(nameof(ExtremeRolesPlugin.ShipState), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
			shipStateProp?.SetValue(null, new ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus());
		}

		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		var emptyTasks = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<NormalPlayerTask>(0);

		mockShipStatus.SetupGet(s => s.CommonTasks).Returns(emptyTasks);
		mockShipStatus.SetupGet(s => s.LongTasks).Returns(emptyTasks);
		mockShipStatus.SetupGet(s => s.ShortTasks).Returns(emptyTasks);

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShipStatus.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;
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

	[Fact]
	public void Initialize_RegistersOptionsAndProperties()
	{
		// Arrange
		var role = new Punisher();

		// Act
		InitializeRole(role);

		// Assert
		Assert.Equal(ExtremeRoleId.Punisher, role.Core.Id);
		Assert.True(role.IsNeutral());
	}

	[Fact]
	public void TryRolePlayerKillTo_TargetIsImpostor_SetsWinTrueAndReturnsTrue()
	{
		// Arrange
		var role = new Punisher();
		InitializeRole(role, 1);

		var targetRole = new SpecialImpostor();
		InitializeRole(targetRole, 2);

		var mockTargetPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockTargetPlayer.SetupGet(p => p.PlayerId).Returns((byte)2);

		// Act
		bool result = role.TryRolePlayerKillTo(mockLocalPlayer.Object, mockTargetPlayer.Object);
		role.OnEndKill();

		// Assert
		Assert.True(result);
		Assert.True(role.IsWin);
	}

	[Fact]
	public void TryRolePlayerKillTo_TargetIsNonImpostor_IncreasesKillCoolTimeAndReturnsTrue()
	{
		// Arrange
		var role = new Punisher();
		InitializeRole(role, 1);
		role.HasOtherKillCool = true;
		role.KillCoolTime = 20.0f;

		var targetRole = new SpecialCrew();
		InitializeRole(targetRole, 2);

		var mockTargetPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockTargetPlayer.SetupGet(p => p.PlayerId).Returns((byte)2);

		// Act
		bool result = role.TryRolePlayerKillTo(mockLocalPlayer.Object, mockTargetPlayer.Object);

		// Assert
		Assert.True(result);
		Assert.False(role.IsWin);
		Assert.True(role.KillCoolTime > 20.0f);
	}

	[Fact]
	public void OnEndKill_DoesNotThrow()
	{
		// Arrange
		var role = new Punisher();
		InitializeRole(role, 1);

		// Act
		role.OnStartKill();
		role.OnEndKill();

		// Assert
		Assert.False(role.IsWin);
	}

	[Fact]
	public void Update_WhenNullRolePlayer_DoesNotThrow()
	{
		// Arrange
		var role = new Punisher();
		InitializeRole(role, 1);

		// Act & Assert
		role.Update(null!);
	}

	[Fact]
	public void Update_WhenTaskCompleted_EnablesCanKillAndReducesKillTimer()
	{
		// Arrange
		var role = new Punisher();
		InitializeRole(role, 1);
		role.KillCoolTime = 30.0f;

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		var task1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		var task2 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);

		task1.SetupGet(t => t.Id).Returns(1u);
		task1.SetupGet(t => t.Complete).Returns(true);
		task2.SetupGet(t => t.Id).Returns(2u);
		task2.SetupGet(t => t.Complete).Returns(true);

		mockTasksList.SetupGet(l => l.Count).Returns(2);
		mockTasksList.SetupGet(l => l[0]).Returns(task1.Object);
		mockTasksList.SetupGet(l => l[1]).Returns(task2.Object);

		mockData.SetupGet(d => d.Object).Returns(mockLocalPlayer.Object);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Tasks).Returns(mockTasksList.Object);
		mockLocalPlayer.SetupGet(p => p.Data).Returns(mockData.Object);
		mockLocalPlayer.Object.killTimer = 15.0f;

		// Act
		role.Update(mockLocalPlayer.Object);

		// Assert
		Assert.True(role.CanKill);
		Assert.True(mockLocalPlayer.Object.killTimer < 15.0f);
	}
}
