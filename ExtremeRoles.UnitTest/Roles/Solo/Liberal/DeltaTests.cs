using System;
using System.Collections.Generic;
using System.Reflection;
using AmongUs.GameOptions;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Liberal;
using ExtremeRoles.Roles.Solo.Crewmate;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Liberal;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class DeltaTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;

	public DeltaTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		if (ExtremeRolesPlugin.ShipState == null)
		{
			var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(nameof(ExtremeRolesPlugin.ShipState), BindingFlags.Public | BindingFlags.Static);
			shipStateProp?.SetValue(null, new ExtremeShipStatus());
		}

		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		SetupLobbyBehaviourMock();

		mockClient = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		if (MockMessageWriterGetHelper.Instance == null)
		{
			var mockWriterGet = new Mock<MockMessageWriterGetHelper>();
			mockWriterGet.Setup(m => m.Invoke(It.IsAny<SendOption>())).Returns(mockWriter.Object);
			MockMessageWriterGetHelper.Instance = mockWriterGet.Object;
		}

		if (MockMessageReaderGetHelper.Instance == null)
		{
			var mockReaderHelper = new Mock<MockMessageReaderGetHelper>();
			var mockMsgReader = new Mock<MessageReader>();
			mockReaderHelper.Setup(m => m.Invoke(It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>>())).Returns(mockMsgReader.Object);
			MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;
		}

		if (InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance == null)
		{
			var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
			mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<MessageWriter>(), It.IsAny<InnerNet.InnerNetObject>()));
			InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;
		}

		mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		MockSetupHelper.SetupGameDataMock();

		var mockMeetingHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHelper.Setup(h => h.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHelper.Object;

		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();
	}

	private static void SetupLobbyBehaviourMock()
	{
		var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
		var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
		mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
		MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
	}

	private static void InitializeRole(Delta role, byte playerId = 1)
	{
		role.CreateRoleAllOption();
		role.Initialize();

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[playerId] = role;
	}

	[Fact]
	public void Initialize_RegistersDeltaRole()
	{
		// Arrange
		var role = new Delta();

		// Act
		InitializeRole(role);

		// Assert
		Assert.Equal("Dl", role.GetRoleTag());
		Assert.Equal(ExtremeRoleId.Delta, role.Core.Id);
	}

	[Fact]
	public void GetCompletedTaskCount_NullOrNoTasks_ReturnsZero()
	{
		// Assert
		Assert.Equal(0, Delta.GetCompletedTaskCount(null));
	}

	[Fact]
	public void GetCompletedTaskCount_WithTasks_ReturnsCorrectCount()
	{
		// Arrange
		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		var task1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		task1.SetupGet(t => t.Complete).Returns(true);
		var task2 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		task2.SetupGet(t => t.Complete).Returns(false);
		var task3 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		task3.SetupGet(t => t.Complete).Returns(true);

		mockTasksList.SetupGet(l => l.Count).Returns(3);
		mockTasksList.SetupGet(l => l[0]).Returns(task1.Object);
		mockTasksList.SetupGet(l => l[1]).Returns(task2.Object);
		mockTasksList.SetupGet(l => l[2]).Returns(task3.Object);

		var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockPlayerInfo.SetupGet(p => p.Tasks).Returns(mockTasksList.Object);

		// Act
		int completed = Delta.GetCompletedTaskCount(mockPlayerInfo.Object);

		// Assert
		Assert.Equal(2, completed);
	}

	[Fact]
	public void ExecuteAbility_WhenDeltaNotFound_ReturnsSafely()
	{
		// Arrange
		ExtremeRoleManager.GameRole.Clear();

		// Act & Assert - Should not throw
		Delta.ExecuteAbility(1, 2);
	}

	[Fact]
	public void ExecuteAbility_SameTeam_ExplodesDelta()
	{
		// Arrange
		var deltaRole = new Delta();
		InitializeRole(deltaRole, 1);

		var teammateRole = new Embezzle();
		teammateRole.CreateRoleAllOption();
		teammateRole.Initialize();
		ExtremeRoleManager.GameRole[2] = teammateRole;

		var deltaTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		var dTask1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		dTask1.SetupGet(t => t.Complete).Returns(true);
		deltaTasksList.SetupGet(l => l.Count).Returns(1);
		deltaTasksList.SetupGet(l => l[0]).Returns(dTask1.Object);

		var teammateTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		var tTask1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		tTask1.SetupGet(t => t.Complete).Returns(true);
		var tTask2 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		tTask2.SetupGet(t => t.Complete).Returns(true);
		teammateTasksList.SetupGet(l => l.Count).Returns(2);
		teammateTasksList.SetupGet(l => l[0]).Returns(tTask1.Object);
		teammateTasksList.SetupGet(l => l[1]).Returns(tTask2.Object);

		var deltaPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		deltaPlayerInfo.SetupGet(p => p.PlayerId).Returns((byte)1);
		deltaPlayerInfo.SetupGet(p => p.Tasks).Returns(deltaTasksList.Object);

		var teammatePlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		teammatePlayerInfo.SetupGet(p => p.PlayerId).Returns((byte)2);
		teammatePlayerInfo.SetupGet(p => p.Tasks).Returns(teammateTasksList.Object);

		var mockGameData = MockSetupHelper.SetupGameDataMock();
		mockGameData.Setup(g => g.GetPlayerById(1)).Returns(deltaPlayerInfo.Object);
		mockGameData.Setup(g => g.GetPlayerById(2)).Returns(teammatePlayerInfo.Object);

		// Act
		Delta.ExecuteAbility(1, 2);

		// Assert - RPC was called to murder delta
		this.mockClient.Verify(
			c => c.StartRpcImmediately(It.IsAny<uint>(), (byte)RPCOperator.Command.UncheckedMurderPlayer, It.IsAny<SendOption>(), It.IsAny<int>()),
			Times.AtLeastOnce());
	}

	[Fact]
	public void ExecuteAbility_DifferentTeamWithTaskDiff_AwardsMoney()
	{
		// Arrange
		var deltaRole = new Delta();
		InitializeRole(deltaRole, 1);

		var crewRole = new Sheriff();
		crewRole.CreateRoleAllOption();
		crewRole.Initialize();
		ExtremeRoleManager.GameRole[2] = crewRole;

		var deltaTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		var dTask1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		dTask1.SetupGet(t => t.Complete).Returns(true);
		deltaTasksList.SetupGet(l => l.Count).Returns(1);
		deltaTasksList.SetupGet(l => l[0]).Returns(dTask1.Object);

		var crewTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		var cTask1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		cTask1.SetupGet(t => t.Complete).Returns(true);
		var cTask2 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		cTask2.SetupGet(t => t.Complete).Returns(true);
		var cTask3 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		cTask3.SetupGet(t => t.Complete).Returns(true);
		crewTasksList.SetupGet(l => l.Count).Returns(3);
		crewTasksList.SetupGet(l => l[0]).Returns(cTask1.Object);
		crewTasksList.SetupGet(l => l[1]).Returns(cTask2.Object);
		crewTasksList.SetupGet(l => l[2]).Returns(cTask3.Object);

		var deltaPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		deltaPlayerInfo.SetupGet(p => p.PlayerId).Returns((byte)1);
		deltaPlayerInfo.SetupGet(p => p.Tasks).Returns(deltaTasksList.Object);

		var crewPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		crewPlayerInfo.SetupGet(p => p.PlayerId).Returns((byte)2);
		crewPlayerInfo.SetupGet(p => p.Tasks).Returns(crewTasksList.Object);

		var mockGameData = MockSetupHelper.SetupGameDataMock();
		mockGameData.Setup(g => g.GetPlayerById(1)).Returns(deltaPlayerInfo.Object);
		mockGameData.Setup(g => g.GetPlayerById(2)).Returns(crewPlayerInfo.Object);

		// Act
		Delta.ExecuteAbility(1, 2);

		// Assert - System RPC update sent for money bank
		this.mockClient.Verify(
			c => c.StartRpcImmediately(It.IsAny<uint>(), (byte)RPCOperator.Command.UpdateExtremeSystemType, It.IsAny<SendOption>(), It.IsAny<int>()),
			Times.AtLeastOnce());
	}
}
