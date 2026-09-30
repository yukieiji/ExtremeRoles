using System;
using System.Collections.Generic;
using ExtremeRoles.Roles.Solo.Crewmate.Loner;
using MonoMod.RuntimeDetour;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Loner;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class LonerStatusModelTests : IDisposable
{
	private delegate float Vector2MagOrig(ref Vector2 self);
	private delegate float Vector2MagHook(Vector2MagOrig orig, ref Vector2 self);

	private delegate void Vector2CtorOrig(ref Vector2 self, float x, float y);
	private delegate void Vector2CtorHook(Vector2CtorOrig orig, ref Vector2 self, float x, float y);

	private readonly Hook magHook;
	private readonly Hook ctorHook;

	public LonerStatusModelTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();

		var mockMinigameHelper = new Mock<MockMinigameget_InstanceHelper>();
		mockMinigameHelper.Setup(h => h.Invoke()).Returns((Minigame)null!);
		MockMinigameget_InstanceHelper.Instance = mockMinigameHelper.Object;

		var mockSub = new Mock<MockVector2op_SubtractionHelper>();
		mockSub.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>()))
			.Returns((Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y));
		MockVector2op_SubtractionHelper.Instance = mockSub.Object;

		var magTarget = typeof(Vector2).GetProperty("magnitude")!.GetGetMethod()!;
		var magHookDelegate = new Vector2MagHook(getMagnitudeHook);
		this.magHook = new Hook(magTarget, magHookDelegate);

		var ctorTarget = typeof(Vector2).GetConstructor(new[] { typeof(float), typeof(float) })!;
		var ctorHookDelegate = new Vector2CtorHook(getCtorHook);
		this.ctorHook = new Hook(ctorTarget, ctorHookDelegate);
	}

	public void Dispose()
	{
		this.magHook.Dispose();
		this.ctorHook.Dispose();
	}

	private static float getMagnitudeHook(Vector2MagOrig orig, ref Vector2 self)
	{
		return (float)Math.Sqrt(self.x * self.x + self.y * self.y);
	}

	private static void getCtorHook(Vector2CtorOrig orig, ref Vector2 self, float x, float y)
	{
		self.x = x;
		self.y = y;
	}

	private static Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>> CreateMockPlayerList(List<NetworkedPlayerInfo?> players)
	{
		var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		mockList.SetupGet(l => l.Count).Returns(players.Count);
		mockList.Setup(l => l[It.IsAny<int>()]).Returns((int i) => players[i]!);
		return mockList;
	}

	[Fact]
	public void Constructor_InitializesStressGageToZero()
	{
		// Arrange
		var option = new StressProgress.Option(false, true, false);

		// Act
		var statusModel = new LonerStatusModel(2.5f, 10f, option);

		// Assert
		Assert.Equal(0f, statusModel.StressGage);
	}

	[Fact]
	public void Update_WhenInWaitTime_DoesNotIncreaseStress()
	{
		// Arrange
		var option = new StressProgress.Option(false, true, false);
		var statusModel = new LonerStatusModel(2.5f, 10f, option);

		var mockRolePlayer = MockSetupHelper.SetupPlayerControlMocks();

		// Act
		statusModel.Update(mockRolePlayer.Object, 1.0f);

		// Assert
		Assert.Equal(0f, statusModel.StressGage);
	}

	[Fact]
	public void Update_WhenPlayerNearby_IncreasesStressGage()
	{
		// Arrange
		var option = new StressProgress.Option(true, true, true);
		var statusModel = new LonerStatusModel(2.5f, -1.0f, option);

		var mockRolePlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockRolePlayer.SetupGet(p => p.PlayerId).Returns((byte)0);
		mockRolePlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));

		var mockRoleData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockRoleData.SetupGet(d => d.IsDead).Returns(false);
		mockRoleData.SetupGet(d => d.Disconnected).Returns(false);
		mockRoleData.SetupGet(d => d.Object).Returns(mockRolePlayer.Object);
		mockRoleData.SetupGet(d => d.PlayerId).Returns((byte)0);
		mockRolePlayer.SetupGet(p => p.Data).Returns(mockRoleData.Object);

		var mockOtherPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockOtherPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		mockOtherPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(1f, 0f));

		var mockOtherData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockOtherData.SetupGet(d => d.IsDead).Returns(false);
		mockOtherData.SetupGet(d => d.Disconnected).Returns(false);
		mockOtherData.SetupGet(d => d.Object).Returns(mockOtherPlayer.Object);
		mockOtherData.SetupGet(d => d.PlayerId).Returns((byte)1);
		mockOtherPlayer.SetupGet(p => p.Data).Returns(mockOtherData.Object);

		var playersList = new List<NetworkedPlayerInfo?> { mockRoleData.Object, mockOtherData.Object };

		var mockGameData = MockSetupHelper.SetupGameDataMock();
		mockGameData.SetupGet(g => g.AllPlayers).Returns(CreateMockPlayerList(playersList).Object);

		// Act
		statusModel.Update(mockRolePlayer.Object, 1.0f);

		// Assert
		Assert.True(statusModel.StressGage > 0f);
	}

	[Fact]
	public void Update_WhenNoPlayerNearby_DecreasesStressGage()
	{
		// Arrange
		var option = new StressProgress.Option(true, true, true);
		var statusModel = new LonerStatusModel(2.5f, -1.0f, option);

		var mockRolePlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockRolePlayer.SetupGet(p => p.PlayerId).Returns((byte)0);
		mockRolePlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));

		var mockRoleData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockRoleData.SetupGet(d => d.IsDead).Returns(false);
		mockRoleData.SetupGet(d => d.Disconnected).Returns(false);
		mockRoleData.SetupGet(d => d.Object).Returns(mockRolePlayer.Object);
		mockRoleData.SetupGet(d => d.PlayerId).Returns((byte)0);
		mockRolePlayer.SetupGet(p => p.Data).Returns(mockRoleData.Object);

		var mockOtherPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockOtherPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		mockOtherPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(1f, 0f));

		var mockOtherData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockOtherData.SetupGet(d => d.IsDead).Returns(false);
		mockOtherData.SetupGet(d => d.Disconnected).Returns(false);
		mockOtherData.SetupGet(d => d.Object).Returns(mockOtherPlayer.Object);
		mockOtherData.SetupGet(d => d.PlayerId).Returns((byte)1);
		mockOtherPlayer.SetupGet(p => p.Data).Returns(mockOtherData.Object);

		var playersListNearby = new List<NetworkedPlayerInfo?> { mockRoleData.Object, mockOtherData.Object };
		var mockGameData = MockSetupHelper.SetupGameDataMock();
		mockGameData.SetupGet(g => g.AllPlayers).Returns(CreateMockPlayerList(playersListNearby).Object);

		// Increase stress first
		statusModel.Update(mockRolePlayer.Object, 2.0f);
		float stressAfterIncrease = statusModel.StressGage;

		// Move other player far away
		mockOtherPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(100f, 0f));

		// Act
		statusModel.Update(mockRolePlayer.Object, 1.0f);

		// Assert
		Assert.True(statusModel.StressGage < stressAfterIncrease);
	}

	[Fact]
	public void Update_WhenInMinigameAndProgressOnTaskFalse_DoesNotUpdateStress()
	{
		// Arrange
		var mockMinigame = new Mock<Minigame>(IntPtr.Zero);
		var mockMinigameHelper = new Mock<MockMinigameget_InstanceHelper>();
		mockMinigameHelper.Setup(h => h.Invoke()).Returns(mockMinigame.Object);
		MockMinigameget_InstanceHelper.Instance = mockMinigameHelper.Object;

		var option = new StressProgress.Option(false, true, true);
		var statusModel = new LonerStatusModel(2.5f, -1.0f, option);

		var mockRolePlayer = MockSetupHelper.SetupPlayerControlMocks();

		// Act
		statusModel.Update(mockRolePlayer.Object, 1.0f);

		// Assert
		Assert.Equal(0f, statusModel.StressGage);

		// Reset minigame mock
		var mockResetMinigame = new Mock<MockMinigameget_InstanceHelper>();
		mockResetMinigame.Setup(h => h.Invoke()).Returns((Minigame)null!);
		MockMinigameget_InstanceHelper.Instance = mockResetMinigame.Object;
	}

	[Fact]
	public void ResetStress_ResetsGageToZero()
	{
		// Arrange
		var option = new StressProgress.Option(true, true, true);
		var statusModel = new LonerStatusModel(2.5f, -1.0f, option);

		var mockRolePlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockRolePlayer.SetupGet(p => p.PlayerId).Returns((byte)0);
		mockRolePlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));

		var mockRoleData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockRoleData.SetupGet(d => d.IsDead).Returns(false);
		mockRoleData.SetupGet(d => d.Disconnected).Returns(false);
		mockRoleData.SetupGet(d => d.Object).Returns(mockRolePlayer.Object);
		mockRoleData.SetupGet(d => d.PlayerId).Returns((byte)0);
		mockRolePlayer.SetupGet(p => p.Data).Returns(mockRoleData.Object);

		var mockOtherPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockOtherPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		mockOtherPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(1f, 0f));

		var mockOtherData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockOtherData.SetupGet(d => d.IsDead).Returns(false);
		mockOtherData.SetupGet(d => d.Disconnected).Returns(false);
		mockOtherData.SetupGet(d => d.Object).Returns(mockOtherPlayer.Object);
		mockOtherData.SetupGet(d => d.PlayerId).Returns((byte)1);
		mockOtherPlayer.SetupGet(p => p.Data).Returns(mockOtherData.Object);

		var playersList = new List<NetworkedPlayerInfo?> { mockRoleData.Object, mockOtherData.Object };
		var mockGameData = MockSetupHelper.SetupGameDataMock();
		mockGameData.SetupGet(g => g.AllPlayers).Returns(CreateMockPlayerList(playersList).Object);

		statusModel.Update(mockRolePlayer.Object, 2.0f);
		Assert.True(statusModel.StressGage > 0f);

		// Act
		statusModel.ResetStress();

		// Assert
		Assert.Equal(0f, statusModel.StressGage);
	}
}
