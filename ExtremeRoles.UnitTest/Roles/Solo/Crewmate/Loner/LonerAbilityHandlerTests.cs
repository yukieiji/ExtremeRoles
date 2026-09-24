using System;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.Solo.Crewmate.Loner;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Loner;

public class LonerAbilityHandlerTests
{
	public LonerAbilityHandlerTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		SetupMinigameMock(null);
	}

	private static void SetupMinigameMock(Minigame? minigame)
	{
		var mockMinigameHelper = new Mock<MockMinigameget_InstanceHelper>();
		mockMinigameHelper.Setup(x => x.Invoke()).Returns(minigame!);
		MockMinigameget_InstanceHelper.Instance = mockMinigameHelper.Object;
	}

	[Fact]
	public void ArrowController_Hide_DoesNotThrow()
	{
		MockSetupHelper.SetupGameDataMock();
		var controller = new ArrowController(2, true);

		controller.Hide();
	}

	[Fact]
	public void ArrowController_Update_WhenArrowNumZero_DoesNotThrow()
	{
		MockSetupHelper.SetupGameDataMock();
		var controller = new ArrowController(0, true);

		var loner = new Mock<PlayerControl>(IntPtr.Zero);
		controller.Update(loner.Object);
	}

	[Fact]
	public void LonerAbilityHandler_Update_WhenNotInTaskPhase_ResetsStress()
	{
		MockSetupHelper.SetupGameDataMock();

		var option = new StressProgress.Option(true, true, true);
		var status = new LonerStatusModel(5f, 0f, option);
		var handler = new LonerAbilityHandler(100f, 2, true, status);

		var loner = new Mock<PlayerControl>(IntPtr.Zero);

		GameProgressSystem.Current = GameProgressSystem.Progress.Meeting;

		handler.Update(loner.Object);

		Assert.Equal(0.0f, status.StressGage);
	}

	[Fact]
	public void LonerAbilityHandler_Reset_HidesArrowWithoutThrowing()
	{
		MockSetupHelper.SetupGameDataMock();

		var option = new StressProgress.Option(true, true, true);
		var status = new LonerStatusModel(5f, 0f, option);
		var handler = new LonerAbilityHandler(100f, 2, true, status);

		handler.Reset();
	}
}