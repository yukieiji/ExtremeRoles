using System;
using AmongUs.GameOptions;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Crewmate;
using Moq;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.JailerTests;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class LawbreakerRoleTests
{
	private readonly Mock<AmongUsClient> clientMock;
	private readonly Mock<IOptionLoader> optionLoaderMock;

	public LawbreakerRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupPlayerControlMocks();
		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		clientMock = MockSetupHelper.SetupAmongUsClientMock();
		clientMock.SetupGet(c => c.GameState).Returns(InnerNet.InnerNetClient.GameStates.Started);

		optionLoaderMock = new Mock<IOptionLoader>();
	}

	[Fact]
	public void Constructor_WithKillEnabledAndHasOtherKillSettings_InitializesProperties()
	{
		var option = new Lawbreaker.Option(
			Kill: true,
			HasOtherKillCool: true,
			KillCool: 15.0f,
			HasOtherKillRange: true,
			KillRange: 2,
			Vent: true,
			Sab: true
		);

		var lawbreaker = new Lawbreaker(optionLoaderMock.Object, option);

		Assert.NotNull(lawbreaker);
		Assert.Equal(ExtremeRoleId.Lawbreaker, lawbreaker.Core.Id);
		Assert.True(lawbreaker.CanKill);
		Assert.True(lawbreaker.HasOtherKillCool);
		Assert.Equal(15.0f, lawbreaker.KillCoolTime);
		Assert.True(lawbreaker.HasOtherKillRange);
		Assert.Equal(2, lawbreaker.KillRange);
		Assert.True(lawbreaker.UseVent);
		Assert.True(lawbreaker.UseSabotage);
	}

	[Fact]
	public void Constructor_WithKillDisabled_SetsDefaultKillProperties()
	{
		var option = new Lawbreaker.Option(
			Kill: false,
			HasOtherKillCool: false,
			KillCool: 30.0f,
			HasOtherKillRange: false,
			KillRange: 1,
			Vent: false,
			Sab: false
		);

		var lawbreaker = new Lawbreaker(optionLoaderMock.Object, option);

		Assert.False(lawbreaker.CanKill);
		Assert.False(lawbreaker.UseVent);
		Assert.False(lawbreaker.UseSabotage);
	}

	[Theory]
	[InlineData(GameOverReason.ImpostorsByKill, true)]
	[InlineData(GameOverReason.ImpostorsBySabotage, true)]
	[InlineData(GameOverReason.CrewmatesByTask, false)]
	[InlineData(GameOverReason.CrewmatesByVote, false)]
	[InlineData(GameOverReason.CrewmateDisconnect, false)]
	public void ModifiedWinPlayer_AppliesWinnerBasedOnGameOverReason(GameOverReason reason, bool shouldAddWinner)
	{
		var option = new Lawbreaker.Option(true, true, 20f, false, 0, true, true);
		var lawbreaker = new Lawbreaker(optionLoaderMock.Object, option);

		var playerInfoMock = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		playerInfoMock.SetupGet(p => p.PlayerId).Returns((byte)1);

		var winnerContainer = new WinnerContainer();

		lawbreaker.ModifiedWinPlayer(playerInfoMock.Object, reason, winnerContainer);

		bool isWinnerAdded = winnerContainer.PlusedWinner.Contains(playerInfoMock.Object);
		Assert.Equal(shouldAddWinner, isWinnerAdded);
	}
}
