using System;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.Solo.Crewmate.Loner;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Loner;

public class LonerStatusModelTests
{
	public LonerStatusModelTests()
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
	public void StressProgress_Reset_ResetsWaitTimerAndStressGage()
	{
		var option = new StressProgress.Option(
			ProgressOnTask: true,
			ProgressOnVentPlayer: true,
			ProgressOnMovingPlatPlayer: true);

		var progress = new StressProgress(5f, 0f, option);

		MockSetupHelper.SetupGameDataMock();

		var loner = new Mock<PlayerControl>(IntPtr.Zero);
		loner.SetupGet(p => p.PlayerId).Returns((byte)1);
		loner.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));

		progress.Update(loner.Object, 1.0f);

		progress.Reset();
		Assert.Equal(0.0f, progress.StressGage);
	}

	[Fact]
	public void StressProgress_Update_WhenInWaitTimer_DoesNotIncreaseOrDecreaseStress()
	{
		var option = new StressProgress.Option(
			ProgressOnTask: true,
			ProgressOnVentPlayer: true,
			ProgressOnMovingPlatPlayer: true);

		var progress = new StressProgress(5f, 10f, option); // waitTime = 10s

		var loner = new Mock<PlayerControl>(IntPtr.Zero);

		progress.Update(loner.Object, 1.0f); // waitTimer reduces from 10 to 9, no stress calculation

		Assert.Equal(0.0f, progress.StressGage);
	}

	[Fact]
	public void LonerStatusModel_DelegatesToStressProgress()
	{
		var option = new StressProgress.Option(
			ProgressOnTask: true,
			ProgressOnVentPlayer: true,
			ProgressOnMovingPlatPlayer: true);

		var model = new LonerStatusModel(5f, 0f, option);

		Assert.Equal(0.0f, model.StressGage);

		model.ResetStress();
		Assert.Equal(0.0f, model.StressGage);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		model.Update(mockPlayer.Object, 1.0f);
	}
}