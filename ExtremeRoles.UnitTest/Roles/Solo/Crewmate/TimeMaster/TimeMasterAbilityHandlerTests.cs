using System;
using System.Reflection;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles.Solo.Crewmate.TimeMaster;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.TimeMaster;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class TimeMasterAbilityHandlerTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;
	private readonly TimeMasterHistory timeMasterHistory;

	public TimeMasterAbilityHandlerTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		mockClient = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		timeMasterHistory = new TimeMasterHistory(IntPtr.Zero);
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockGameObject.Setup(g => g.AddComponent<TimeMasterHistory>()).Returns(timeMasterHistory);
		mockLocalPlayer.SetupGet(p => p.gameObject).Returns(mockGameObject.Object);

		MockSetupHelper.SetupGameDataMock();
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
	public void TryKilledFrom_WhenIsRewindOn_ReturnsFalse()
	{
		// Arrange
		var status = new TimeMasterStatusModel(5.0f);
		var handler = new TimeMasterAbilityHandler(status);
		SetPrivateField(handler, "isRewindOn", true);

		var mockRolePlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var mockFromPlayer = new Mock<PlayerControl>(IntPtr.Zero);

		// Act
		bool result = handler.TryKilledFrom(mockRolePlayer.Object, mockFromPlayer.Object);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void TryKilledFrom_WhenIsShieldOn_TriggersRpcAndReturnsFalse()
	{
		// Arrange
		var status = new TimeMasterStatusModel(5.0f)
		{
			IsShieldOn = true
		};
		var handler = new TimeMasterAbilityHandler(status);
		timeMasterHistory.BlockAddHistory = true; // Prevent StartCoroutine call in unit test environment

		var mockRolePlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockRolePlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		var mockFromPlayer = new Mock<PlayerControl>(IntPtr.Zero);

		mockClient.Invocations.Clear();

		// Act
		bool result = handler.TryKilledFrom(mockRolePlayer.Object, mockFromPlayer.Object);

		// Assert
		Assert.False(result);
		mockClient.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()), Times.Once);
	}

	[Fact]
	public void TryKilledFrom_WhenShieldOffAndNotRewinding_ReturnsTrue()
	{
		// Arrange
		var status = new TimeMasterStatusModel(5.0f)
		{
			IsShieldOn = false
		};
		var handler = new TimeMasterAbilityHandler(status);

		var mockRolePlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var mockFromPlayer = new Mock<PlayerControl>(IntPtr.Zero);

		// Act
		bool result = handler.TryKilledFrom(mockRolePlayer.Object, mockFromPlayer.Object);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void StartRewind_WhenBlockAddHistoryIsTrue_ReturnsEarly()
	{
		// Arrange
		var status = new TimeMasterStatusModel(5.0f);
		var handler = new TimeMasterAbilityHandler(status);
		timeMasterHistory.BlockAddHistory = true;

		// Act
		handler.StartRewind(1);

		// Assert
		Assert.True(timeMasterHistory.BlockAddHistory);
	}

	[Fact]
	public void ResetMeeting_WhenMeetingHudExists_ResetsStatusUnblocksHistoryAndSpawnsPlayer()
	{
		// Arrange
		var status = new TimeMasterStatusModel(5.0f)
		{
			IsShieldOn = true
		};
		var handler = new TimeMasterAbilityHandler(status);
		SetPrivateField(handler, "isRewindOn", true);
		timeMasterHistory.BlockAddHistory = true;

		var mockMeetingHud = new Mock<MeetingHud>(IntPtr.Zero);
		var mockMeetingHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHelper.Setup(h => h.Invoke()).Returns(mockMeetingHud.Object);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHelper.Object;

		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShipStatus.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		// Act
		handler.ResetMeeting(1);

		// Assert
		Assert.False(status.IsShieldOn);
		Assert.False(timeMasterHistory.BlockAddHistory);
		bool isRewindOn = (bool)(GetPrivateField(handler, "isRewindOn") ?? true);
		Assert.False(isRewindOn);

		mockShipStatus.Verify(s => s.SpawnPlayer(It.IsAny<PlayerControl>(), It.IsAny<int>(), false), Times.Once);
	}

	[Fact]
	public void ResetMeeting_WhenMeetingHudIsNull_ResetsStatusAndUnblocksHistoryWithoutSpawnPlayer()
	{
		// Arrange
		var status = new TimeMasterStatusModel(5.0f)
		{
			IsShieldOn = true
		};
		var handler = new TimeMasterAbilityHandler(status);
		SetPrivateField(handler, "isRewindOn", true);
		timeMasterHistory.BlockAddHistory = true;

		var mockMeetingHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHelper.Setup(h => h.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHelper.Object;

		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShipStatus.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		// Act
		handler.ResetMeeting(1);

		// Assert
		Assert.False(status.IsShieldOn);
		Assert.False(timeMasterHistory.BlockAddHistory);
		bool isRewindOn = (bool)(GetPrivateField(handler, "isRewindOn") ?? true);
		Assert.False(isRewindOn);

		mockShipStatus.Verify(s => s.SpawnPlayer(It.IsAny<PlayerControl>(), It.IsAny<int>(), false), Times.Never);
	}
}
