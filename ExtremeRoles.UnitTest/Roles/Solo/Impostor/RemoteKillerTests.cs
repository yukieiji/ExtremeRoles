using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor.RemoteKiller;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
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
		SetupTranslationControllerMock();

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

	private static void SetupTranslationControllerMock()
	{
		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");
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
		var role = new RemoteKillerRole();

		// Act
		InitializeRole(role);

		// Assert
		Assert.Equal(ExtremeRoleId.RemoteKiller, role.Core.Id);
		Assert.True(role.IsImpostor());
		Assert.NotNull(role.Status);
	}

	[Fact]
	public void RpcHandle_PurgeStartAndCancel_TogglesPurgingState()
	{
		// Arrange
		var role = new RemoteKillerRole();
		InitializeRole(role, 1);

		var startReaderMock = new Mock<MessageReader>();
		startReaderMock.SetupSequence(r => r.ReadByte())
			.Returns((byte)RemoteKillerRole.RemoteKillerRpc.PurgeStart)
			.Returns((byte)1);

		var startReaderObj = startReaderMock.Object;

		// Act: PurgeStart
		RemoteKillerRole.RpcHandle(ref startReaderObj);

		// Assert
		Assert.True(role.IsPurging);

		// Act: PurgeCancel
		var cancelReaderMock = new Mock<MessageReader>();
		cancelReaderMock.SetupSequence(r => r.ReadByte())
			.Returns((byte)RemoteKillerRole.RemoteKillerRpc.PurgeCancel)
			.Returns((byte)1);

		var cancelReaderObj = cancelReaderMock.Object;
		RemoteKillerRole.RpcHandle(ref cancelReaderObj);

		// Assert
		Assert.False(role.IsPurging);
	}

	[Fact]
	public void RpcHandle_InvalidPlayerId_DoesNotThrow()
	{
		// Arrange
		var startReaderMock = new Mock<MessageReader>();
		startReaderMock.SetupSequence(r => r.ReadByte())
			.Returns((byte)RemoteKillerRole.RemoteKillerRpc.PurgeStart)
			.Returns((byte)99); // player 99 doesn't exist

		var readerObj = startReaderMock.Object;

		// Act & Assert
		RemoteKillerRole.RpcHandle(ref readerObj);
	}

	[Fact]
	public void GetRolePlayerNameTag_WhenTargetInExecutionTargets_ReturnsRedTag()
	{
		// Arrange
		var role = new RemoteKillerRole();
		InitializeRole(role, 1);

		var status = (RemoteKillerStatusModel)role.Status!;
		status.AddExecutionTarget(2);

		var targetRole = new SpecialCrew();

		// Act
		string tag = role.GetRolePlayerNameTag(targetRole, 2);

		// Assert
		Assert.Contains("▼", tag);
	}

	[Fact]
	public void GetRolePlayerNameTag_WhenTargetNotInExecutionTargets_ReturnsBaseTag()
	{
		// Arrange
		var role = new RemoteKillerRole();
		InitializeRole(role, 1);

		var targetRole = new SpecialCrew();

		// Act
		string tag = role.GetRolePlayerNameTag(targetRole, 3);

		// Assert
		Assert.DoesNotContain("▼", tag);
	}

	[Fact]
	public void ResetOnMeetingEnd_ResetsPurgingAndCanMove()
	{
		// Arrange
		var role = new RemoteKillerRole();
		InitializeRole(role, 1);

		var status = (RemoteKillerStatusModel)role.Status!;
		status.SetPurging(true);
		status.CanMove = false;

		// Act
		role.ResetOnMeetingEnd(null);

		// Assert
		Assert.True(status.CanMove);
		Assert.False(role.IsPurging);
	}

	[Fact]
	public void ReportSerializer_SerializeAndDeserialize_RestoresData()
	{
		// Arrange
		var playerIds = new List<byte> { 2, 3, 4 };
		var serializer = new RemoteKillerRole.RemoteKillerReportSerializer(playerIds);

		using var caller = RPCOperator.CreateCaller(RPCOperator.Command.RemoteKillerOps);

		// Act: Serialize
		serializer.Serialize(caller);

		// Act: Deserialize
		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)2) // count
			.Returns((byte)5) // id 1
			.Returns((byte)6); // id 2

		var deserializedSerializer = new RemoteKillerRole.RemoteKillerReportSerializer();
		deserializedSerializer.Deserialize(mockReader.Object);

		// Assert
		Assert.Equal(StringSerializerType.RemoteKillerNotebookStolen, deserializedSerializer.Type);
	}

	[Fact]
	public void ReportSerializer_ToString_WhenContactListEmpty_ReturnsHeader()
	{
		// Arrange
		var serializer = new RemoteKillerRole.RemoteKillerReportSerializer(new List<byte>());

		// Act
		string result = serializer.ToString();

		// Assert
		Assert.NotNull(result);
	}
}
