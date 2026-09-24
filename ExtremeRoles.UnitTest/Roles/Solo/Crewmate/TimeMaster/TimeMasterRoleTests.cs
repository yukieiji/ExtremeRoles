using System;
using System.Reflection;
using AmongUs.GameOptions;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate.TimeMaster;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.TimeMaster;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class TimeMasterRoleTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;
	private readonly TimeMasterHistory timeMasterHistory;

	public TimeMasterRoleTests()
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

		var mockHistory = new Mock<TimeMasterHistory>(IntPtr.Zero);
		timeMasterHistory = mockHistory.Object;
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockGameObject.Setup(g => g.AddComponent<TimeMasterHistory>()).Returns(timeMasterHistory);
		mockLocalPlayer.SetupGet(p => p.gameObject).Returns(mockGameObject.Object);

		MockSetupHelper.SetupGameDataMock();

		var mockMeetingHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHelper.Setup(h => h.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHelper.Object;

		if (ClientOption.Instance == null)
		{
			OptionCreator.Create();
		}
	}

	private static void SetupLobbyBehaviourMock()
	{
		var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
		var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
		mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
		MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
	}

	private static void InitializeRole(TimeMasterRole role, byte playerId = 1)
	{
		role.CreateRoleAllOption();
		var initMethod = typeof(TimeMasterRole).GetMethod("RoleSpecificInit", BindingFlags.NonPublic | BindingFlags.Instance);
		initMethod?.Invoke(role, null);

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[playerId] = role;
	}

	private static void SetPrivateField(object target, string fieldName, object value)
	{
		var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
		field?.SetValue(target, value);
	}

	private static object? GetPrivateField(object target, string fieldName)
	{
		var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
		return field?.GetValue(target);
	}

	[Fact]
	public void Constructor_InitializesCorrectly()
	{
		// Act
		var role = new TimeMasterRole();

		// Assert
		Assert.NotNull(role);
		Assert.Equal(ExtremeRoleId.TimeMaster, role.Core.Id);
		Assert.Null(role.Status);
	}

	[Fact]
	public void RoleSpecificInit_InitializesStatusAndAbilityClass()
	{
		// Arrange
		var role = new TimeMasterRole();

		// Act
		InitializeRole(role);

		// Assert
		Assert.NotNull(role.Status);
		Assert.IsType<TimeMasterStatusModel>(role.Status);
		Assert.NotNull(role.AbilityClass);
		Assert.IsType<TimeMasterAbilityHandler>(role.AbilityClass);
	}

	[Fact]
	public void UseAbility_TriggersRpcAndSetsShieldOn()
	{
		// Arrange
		var role = new TimeMasterRole();
		InitializeRole(role, 1);
		var status = (TimeMasterStatusModel)role.Status!;
		mockClient.Invocations.Clear();

		// Act
		bool result = role.UseAbility();

		// Assert
		Assert.True(result);
		Assert.True(status.IsShieldOn);
		mockClient.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()), Times.Once);
	}

	[Fact]
	public void CleanUp_TriggersRpcAndSetsShieldOff()
	{
		// Arrange
		var role = new TimeMasterRole();
		InitializeRole(role, 1);
		var status = (TimeMasterStatusModel)role.Status!;
		status.IsShieldOn = true;
		mockClient.Invocations.Clear();

		// Act
		role.CleanUp();

		// Assert
		Assert.False(status.IsShieldOn);
		mockClient.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()), Times.Once);
	}

	[Fact]
	public void ResetOnMeetingStart_TriggersRpcAndResetsMeeting()
	{
		// Arrange
		var role = new TimeMasterRole();
		InitializeRole(role, 1);
		var status = (TimeMasterStatusModel)role.Status!;
		status.IsShieldOn = true;
		mockClient.Invocations.Clear();

		// Act
		role.ResetOnMeetingStart();

		// Assert
		Assert.False(status.IsShieldOn);
		mockClient.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()), Times.Once);
	}

	[Fact]
	public void IsAbilityUse_ReturnsBoolean()
	{
		// Arrange
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockLocalPlayer.Object);
		mockLocalPlayer.SetupGet(p => p.Data).Returns(mockData.Object);
		mockLocalPlayer.SetupGet(p => p.CanMove).Returns(true);

		var role = new TimeMasterRole();

		// Act
		bool result = role.IsAbilityUse();

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Ability_ShieldOn_Ops_SetsShieldOn()
	{
		// Arrange
		byte playerId = 1;
		var role = new TimeMasterRole();
		InitializeRole(role, playerId);
		var status = (TimeMasterStatusModel)role.Status!;

		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		int callCount = 0;
		mockReader.Setup(r => r.ReadByte()).Returns(() =>
		{
			callCount++;
			return callCount == 1 ? playerId : (byte)TimeMasterRole.TimeMasterOps.ShieldOn;
		});

		var reader = mockReader.Object;

		// Act
		TimeMasterRole.Ability(ref reader);

		// Assert
		Assert.True(status.IsShieldOn);
	}

	[Fact]
	public void Ability_ShieldOff_Ops_SetsShieldOff()
	{
		// Arrange
		byte playerId = 1;
		var role = new TimeMasterRole();
		InitializeRole(role, playerId);
		var status = (TimeMasterStatusModel)role.Status!;
		status.IsShieldOn = true;

		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		int callCount = 0;
		mockReader.Setup(r => r.ReadByte()).Returns(() =>
		{
			callCount++;
			return callCount == 1 ? playerId : (byte)TimeMasterRole.TimeMasterOps.ShieldOff;
		});

		var reader = mockReader.Object;

		// Act
		TimeMasterRole.Ability(ref reader);

		// Assert
		Assert.False(status.IsShieldOn);
	}

	[Fact]
	public void Ability_ResetMeeting_Ops_CallsResetMeetingOnAbilityHandler()
	{
		// Arrange
		byte playerId = 1;
		var role = new TimeMasterRole();
		InitializeRole(role, playerId);
		var status = (TimeMasterStatusModel)role.Status!;
		status.IsShieldOn = true;

		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		int callCount = 0;
		mockReader.Setup(r => r.ReadByte()).Returns(() =>
		{
			callCount++;
			return callCount == 1 ? playerId : (byte)TimeMasterRole.TimeMasterOps.ResetMeeting;
		});

		var reader = mockReader.Object;

		// Act
		TimeMasterRole.Ability(ref reader);

		// Assert
		Assert.False(status.IsShieldOn);
	}

	[Fact]
	public void Ability_RewindTime_Ops_CallsStartRewindOnAbilityHandler()
	{
		// Arrange
		byte playerId = 1;
		var role = new TimeMasterRole();
		InitializeRole(role, playerId);
		timeMasterHistory.BlockAddHistory = true;

		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		int callCount = 0;
		mockReader.Setup(r => r.ReadByte()).Returns(() =>
		{
			callCount++;
			return callCount == 1 ? playerId : (byte)TimeMasterRole.TimeMasterOps.RewindTime;
		});

		var reader = mockReader.Object;

		// Act
		TimeMasterRole.Ability(ref reader);

		// Assert
		Assert.True(timeMasterHistory.BlockAddHistory);
	}

	[Fact]
	public void Ability_WhenRoleNotFound_DoesNotModifyState()
	{
		// Arrange
		ExtremeRoleManager.GameRole.Clear();

		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		int callCount = 0;
		mockReader.Setup(r => r.ReadByte()).Returns(() =>
		{
			callCount++;
			return callCount == 1 ? (byte)99 : (byte)TimeMasterRole.TimeMasterOps.ShieldOn;
		});

		var reader = mockReader.Object;

		// Act
		TimeMasterRole.Ability(ref reader);

		// Assert
		mockReader.Verify(r => r.ReadByte(), Times.Exactly(2));
		Assert.False(ExtremeRoleManager.GameRole.ContainsKey(99));
	}

	[Fact]
	public void CreateSpecificOption_CreatesExpectedOptions()
	{
		// Arrange
		int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.TimeMaster);
		using AutoParentSetOptionCategoryFactory factory = OptionCategoryAssembler.CreateAutoParentSetOptionCategory(
			groupId,
			"TimeMasterTestCategory",
			OptionTab.CrewmateTab,
			Color.white);

		var role = new TimeMasterRole();

		// Act
		var createOptionMethod = typeof(TimeMasterRole).GetMethod("CreateSpecificOption", BindingFlags.NonPublic | BindingFlags.Instance);
		createOptionMethod?.Invoke(role, new object[] { factory });

		// Assert
		Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
		Assert.NotNull(category);
	}
}
