using System;

using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.CheckPoint;
using ExtremeRoles.Performance.Il2Cpp;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.CheckPoint;

[Collection("UnityMock")]
public class RoleAssignCheckpointTests : IDisposable
{
	public RoleAssignCheckpointTests()
	{
		ResetState();
	}

	public void Dispose()
	{
		ResetState();
	}

	private static void ResetState()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		var mockIntroHelper = new Mock<MockIntroCutsceneget_InstanceHelper>();
		mockIntroHelper.Setup(x => x.Invoke()).Returns((IntroCutscene)null!);
		MockIntroCutsceneget_InstanceHelper.Instance = mockIntroHelper.Object;
	}

	[Fact]
	public void AddCheckPoint_AddsPlayerIdToCheckedPlayerSet()
	{
		// Arrange
		var checkpoint = new RoleAssignCheckPoint();

		// Act
		checkpoint.AddCheckPoint(10);
		checkpoint.AddCheckPoint(20);

		// Assert
		Assert.Equal(2, checkpoint.CheckedPlayer.Count);
		Assert.Contains((byte)10, checkpoint.CheckedPlayer);
		Assert.Contains((byte)20, checkpoint.CheckedPlayer);
	}

	[Fact]
	public void HandleChecked_UpdatesGameProgressToRoleSetUpReady()
	{
		// Arrange
		var checkpoint = new RoleAssignCheckPoint();
		ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);
		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpStart;

		// Act
		checkpoint.HandleChecked();

		// Assert
		Assert.True(GameProgressSystem.Is(GameProgressSystem.Progress.RoleSetUpReady));
	}
}
