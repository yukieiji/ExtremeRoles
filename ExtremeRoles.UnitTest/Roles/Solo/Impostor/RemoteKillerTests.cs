using System;
using AmongUs.GameOptions;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class RemoteKillerTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;

	public RemoteKillerTests()
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
		var role = new RemoteKiller();

		// Act
		InitializeRole(role);

		// Assert
		Assert.Equal(ExtremeRoleId.RemoteKiller, role.Core.Id);
		Assert.True(role.IsImpostor());
	}

	[Fact]
	public void RpcHandle_RobSuccess_AddsTargetToExecutionTargets()
	{
		// Arrange
		var role = new RemoteKiller();
		InitializeRole(role, 1);

		byte targetId = 2;
		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)RemoteKiller.RemoteKillerRpc.RobSuccess)
			.Returns((byte)1)
			.Returns(targetId);

		var readerObj = mockReader.Object;

		// Act
		RemoteKiller.RpcHandle(ref readerObj);

		// Assert
		string tag = role.GetRolePlayerNameTag(role, targetId);
		Assert.Contains("▼", tag);
	}

	[Fact]
	public void RpcHandle_PurgeStartAndCancel_TogglesPurgingState()
	{
		// Arrange
		var role = new RemoteKiller();
		InitializeRole(role, 1);

		byte targetId = 2;
		var startReaderMock = new Mock<MessageReader>();
		startReaderMock.SetupSequence(r => r.ReadByte())
			.Returns((byte)RemoteKiller.RemoteKillerRpc.PurgeStart)
			.Returns((byte)1)
			.Returns(targetId);

		var startReaderObj = startReaderMock.Object;

		// Act: PurgeStart
		RemoteKiller.RpcHandle(ref startReaderObj);

		// Assert
		Assert.True(role.IsPurging);
		Assert.NotNull(role.Status);

		// Act: PurgeCancel
		var cancelReaderMock = new Mock<MessageReader>();
		cancelReaderMock.SetupSequence(r => r.ReadByte())
			.Returns((byte)RemoteKiller.RemoteKillerRpc.PurgeCancel)
			.Returns((byte)1);

		var cancelReaderObj = cancelReaderMock.Object;
		RemoteKiller.RpcHandle(ref cancelReaderObj);

		// Assert
		Assert.False(role.IsPurging);
	}
}
