using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomMonoBehaviour;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class TimeMasterHistoryTests
{
	private readonly Mock<PlayerControl> mockLocalPlayer;

	public TimeMasterHistoryTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

		var mockExileHelper = new Mock<MockExileControllerget_InstanceHelper>();
		mockExileHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
		MockExileControllerget_InstanceHelper.Instance = mockExileHelper.Object;

		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		mockShipStatus.SetupGet(s => s.enabled).Returns(true);
		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(x => x.Invoke()).Returns(mockShipStatus.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		var gameProgressSystem = new GameProgressSystem();
		var allSystemsField = typeof(ExtremeSystemTypeManager).GetField("allSystems", BindingFlags.NonPublic | BindingFlags.Instance);
		var dict = (Dictionary<ExtremeSystemType, IExtremeSystemType>)allSystemsField?.GetValue(ExtremeSystemTypeManager.Instance)!;
		dict[ExtremeSystemType.GameProgress] = gameProgressSystem;

		mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		mockTransform.SetupProperty(t => t.position, new Vector3(1.0f, 2.0f, 0.0f));
		mockLocalPlayer.SetupGet(p => p.transform).Returns(mockTransform.Object);
		mockLocalPlayer.SetupGet(p => p.CanMove).Returns(true);
		mockLocalPlayer.SetupGet(p => p.inVent).Returns(false);
		mockLocalPlayer.SetupGet(p => p.inMovingPlat).Returns(false);
		mockLocalPlayer.SetupGet(p => p.onLadder).Returns(false);
	}

	private static void SetTaskPhase()
	{
		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;
	}

	[Fact]
	public void Initialize_SetsSizeAndInit()
	{
		// Arrange
		var history = new TimeMasterHistory(IntPtr.Zero);
		float historySeconds = 5.0f;

		// Act
		history.Initialize(historySeconds);

		// Assert
		int expectedSize = (int)Mathf.Round(historySeconds / Time.fixedDeltaTime);
		Assert.Equal(expectedSize, history.Size);
	}

	[Fact]
	public void Awake_ResetsState()
	{
		// Arrange
		var history = new TimeMasterHistory(IntPtr.Zero);
		history.Initialize(5.0f);
		history.BlockAddHistory = true;

		// Act
		history.Awake();

		// Assert
		Assert.Equal(0, history.Size);
		Assert.False(history.BlockAddHistory);
		Assert.Empty(history.GetAllHistory());
	}

	[Fact]
	public void Clear_ResetsState()
	{
		// Arrange
		var history = new TimeMasterHistory(IntPtr.Zero);
		history.Initialize(5.0f);
		history.BlockAddHistory = true;

		// Act
		history.Clear();

		// Assert
		Assert.Equal(0, history.Size);
		Assert.False(history.BlockAddHistory);
		Assert.Empty(history.GetAllHistory());
	}

	[Fact]
	public void ResetAfterRewind_ClearsHistoryAndUnblocks()
	{
		// Arrange
		var history = new TimeMasterHistory(IntPtr.Zero);
		history.Initialize(5.0f);
		history.BlockAddHistory = true;

		// Act
		history.ResetAfterRewind();

		// Assert
		Assert.False(history.BlockAddHistory);
		Assert.Empty(history.GetAllHistory());
	}

	[Fact]
	public void FixedUpdate_WhenNotInitialized_DoesNotAddHistory()
	{
		// Arrange
		var history = new TimeMasterHistory(IntPtr.Zero);
		// Not initialized
		SetTaskPhase();

		// Act
		history.FixedUpdate();

		// Assert
		Assert.Empty(history.GetAllHistory());
	}

	[Fact]
	public void FixedUpdate_WhenBlockAddHistoryIsTrue_DoesNotAddHistory()
	{
		// Arrange
		var history = new TimeMasterHistory(IntPtr.Zero);
		history.Initialize(5.0f);
		history.BlockAddHistory = true;
		SetTaskPhase();

		// Act
		history.FixedUpdate();

		// Assert
		Assert.Empty(history.GetAllHistory());
	}

	[Fact]
	public void FixedUpdate_WhenNotInTaskPhase_DoesNotAddHistory()
	{
		// Arrange
		var history = new TimeMasterHistory(IntPtr.Zero);
		history.Initialize(5.0f);
		GameProgressSystem.Current = GameProgressSystem.Progress.Meeting;

		// Act
		history.FixedUpdate();

		// Assert
		Assert.Empty(history.GetAllHistory());
	}

	[Fact]
	public void FixedUpdate_WhenValidTaskPhaseAndInitialized_EnqueuesHistory()
	{
		// Arrange
		var history = new TimeMasterHistory(IntPtr.Zero);
		history.Initialize(5.0f);

		// Ensure task phase
		SetTaskPhase();

		// Act
		history.FixedUpdate();

		// Assert
		var historyList = history.GetAllHistory().ToList();
		Assert.Single(historyList);

		var record = historyList[0];
		Assert.Equal(new Vector3(1.0f, 2.0f, 0.0f), record.Pos);
		Assert.True(record.CanMove);
		Assert.False(record.InVent);
		Assert.False(record.IsUsed);
	}

	[Fact]
	public void FixedUpdate_WhenOverflowing_DequeuesOldestHistory()
	{
		// Arrange
		var history = new TimeMasterHistory(IntPtr.Zero);
		history.Initialize(Time.fixedDeltaTime * 2); // Size = 2
		SetTaskPhase();

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		mockLocalPlayer.SetupGet(p => p.transform).Returns(mockTransform.Object);

		// Act
		mockTransform.SetupProperty(t => t.position, new Vector3(1.0f, 0f, 0f));
		history.FixedUpdate();

		mockTransform.SetupProperty(t => t.position, new Vector3(2.0f, 0f, 0f));
		history.FixedUpdate();

		mockTransform.SetupProperty(t => t.position, new Vector3(3.0f, 0f, 0f));
		history.FixedUpdate();

		mockTransform.SetupProperty(t => t.position, new Vector3(4.0f, 0f, 0f));
		history.FixedUpdate();

		// Assert
		var historyList = history.GetAllHistory().ToList();
		Assert.Equal(3, historyList.Count);
		// Dequeued (1.0f, 0f, 0f). History has (4.0, 3.0, 2.0).
		Assert.Equal(new Vector3(4.0f, 0f, 0f), historyList[0].Pos);
		Assert.Equal(new Vector3(3.0f, 0f, 0f), historyList[1].Pos);
		Assert.Equal(new Vector3(2.0f, 0f, 0f), historyList[2].Pos);
	}
}
