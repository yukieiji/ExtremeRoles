using ExtremeRoles.Module.SystemType;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class GameProgressSystemTests
{
	[Fact]
	public void IsRoleSetUpEnd_And_Is_Check_None_WhenNotSetup()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		var system = ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);

		GameProgressSystem.Current = GameProgressSystem.Progress.None;

		Assert.False(GameProgressSystem.IsRoleSetUpEnd);
		Assert.True(GameProgressSystem.Is(GameProgressSystem.Progress.None));
	}

	[Fact]
	public void SetProgress_And_CheckStates()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		var system = ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);

		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
		Assert.True(GameProgressSystem.Is(GameProgressSystem.Progress.RoleSetUpEnd));

		GameProgressSystem.Current = GameProgressSystem.Progress.IntroStart;
		Assert.True(GameProgressSystem.Is(GameProgressSystem.Progress.IntroStart));

		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpStart;
		Assert.True(GameProgressSystem.Is(GameProgressSystem.Progress.RoleSetUpStart));

		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpReady;
		Assert.True(GameProgressSystem.Is(GameProgressSystem.Progress.RoleSetUpReady));

		GameProgressSystem.Current = GameProgressSystem.Progress.IntroEnd;
		Assert.True(GameProgressSystem.Is(GameProgressSystem.Progress.IntroEnd));

		GameProgressSystem.Current = GameProgressSystem.Progress.PreTask;
		Assert.True(GameProgressSystem.Is(GameProgressSystem.Progress.PreTask));

		GameProgressSystem.Current = GameProgressSystem.Progress.Meeting;
		Assert.True(GameProgressSystem.Is(GameProgressSystem.Progress.Meeting));

		GameProgressSystem.Current = GameProgressSystem.Progress.Exiled;
		Assert.True(GameProgressSystem.Is(GameProgressSystem.Progress.Exiled));

		Assert.False(GameProgressSystem.Is((GameProgressSystem.Progress)999));
	}

	[Fact]
	public void Reset_And_UpdateSystem_DoNotThrow()
	{
		var system = new GameProgressSystem();
		system.Reset(ResetTiming.MeetingStart, null);
		system.UpdateSystem(null!, null!);
	}
}
