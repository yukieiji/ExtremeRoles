using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Combination;

public class GuesserNormalRoleProviderTests
{
	[Fact]
	public void ProcessNormalRole_WhenGenericRole_AddsToResultAndSeparatedDict()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		var assignState = new GuesserNormalRoleAssignState();

		// Act
		GuesserNormalRoleProvider.ProcessNormalRole(container, ExtremeRoleId.Sheriff, ExtremeRoleType.Crewmate, assignState);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal(ExtremeRoleId.Sheriff, container.Result[0].Id);
		Assert.Equal(ExtremeRoleType.Crewmate, container.Result[0].Team);
		Assert.Contains(ExtremeRoleId.Sheriff, container.SeparatedRoleId[ExtremeRoleType.Crewmate]);
	}

	[Fact]
	public void ProcessNormalRole_WhenHypnotist_CallsProcessHypnotist()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		var assignState = new GuesserNormalRoleAssignState();

		// Act
		GuesserNormalRoleProvider.ProcessNormalRole(container, ExtremeRoleId.Hypnotist, ExtremeRoleType.Neutral, assignState);

		// Assert
		Assert.Equal(2, container.Result.Count);
		Assert.Equal(ExtremeRoleId.Hypnotist, container.Result[0].Id);
		Assert.Equal(ExtremeRoleId.Doll, container.Result[1].Id);
		Assert.Equal(ExtremeRoleType.Impostor, container.Result[1].Team);
		Assert.Contains(ExtremeRoleId.Doll, container.SeparatedRoleId[ExtremeRoleType.Neutral]);
	}

	[Fact]
	public void ProcessNormalRole_WhenQueen_SetsIsQueenOnAndDoesNotAddQueenDirectly()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		var assignState = new GuesserNormalRoleAssignState();

		// Act
		GuesserNormalRoleProvider.ProcessNormalRole(container, ExtremeRoleId.Queen, ExtremeRoleType.Neutral, assignState);

		// Assert
		Assert.Empty(container.Result);
		Assert.True(assignState.IsQueenOn);
	}

	[Fact]
	public void ProcessHypnotist_AddsDollAsImpostorAndNeutralSeparatedRole()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		GuesserNormalRoleProvider.ProcessHypnotist(container);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal(ExtremeRoleId.Doll, container.Result[0].Id);
		Assert.Equal(ExtremeRoleType.Impostor, container.Result[0].Team);
		Assert.Contains(ExtremeRoleId.Doll, container.SeparatedRoleId[ExtremeRoleType.Neutral]);
	}

	[Fact]
	public void ProcessJailerDirect_WhenMissingToDeadFalse_AddsYardbirdAndLawbreaker()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		GuesserNormalRoleProvider.ProcessJailerDirect(container, isMissingToDead: false);

		// Assert
		Assert.Equal(2, container.Result.Count);
		Assert.Equal(ExtremeRoleId.Yardbird, container.Result[0].Id);
		Assert.Equal(ExtremeRoleType.Crewmate, container.Result[0].Team);
		Assert.Equal(ExtremeRoleId.Lawbreaker, container.Result[1].Id);
		Assert.Equal(ExtremeRoleType.Neutral, container.Result[1].Team);
	}

	[Fact]
	public void ProcessJailerDirect_WhenMissingToDeadTrue_AddsOnlyYardbird()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		GuesserNormalRoleProvider.ProcessJailerDirect(container, isMissingToDead: true);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal(ExtremeRoleId.Yardbird, container.Result[0].Id);
		Assert.Equal(ExtremeRoleType.Crewmate, container.Result[0].Team);
	}

	[Fact]
	public void ProcessTucker_AddsChimeraAsNeutral()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		GuesserNormalRoleProvider.ProcessTucker(container);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal(ExtremeRoleId.Chimera, container.Result[0].Id);
		Assert.Equal(ExtremeRoleType.Neutral, container.Result[0].Team);
		Assert.Contains(ExtremeRoleId.Chimera, container.SeparatedRoleId[ExtremeRoleType.Neutral]);
	}

	[Fact]
	public void ProcessJackalAndSidekick_WhenJackalOnFalse_DoesNotAddJackalOrSidekick()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		var assignState = new GuesserNormalRoleAssignState { IsJackalOn = false };

		// Act
		GuesserNormalRoleProvider.ProcessJackalAndSidekick(container, assignState);

		// Assert
		Assert.Empty(container.Result);
	}

	[Fact]
	public void ProcessJackalAndSidekick_WhenJackalOnTrue_AddsJackalAndSidekick()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		var assignState = new GuesserNormalRoleAssignState { IsJackalOn = true };

		// Act
		GuesserNormalRoleProvider.ProcessJackalAndSidekick(container, assignState);

		// Assert
		Assert.Equal(2, container.Result.Count);
		Assert.Equal(ExtremeRoleId.Jackal, container.Result[0].Id);
		Assert.Equal(ExtremeRoleId.Sidekick, container.Result[1].Id);
		Assert.Contains(ExtremeRoleId.Jackal, container.SeparatedRoleId[ExtremeRoleType.Neutral]);
		Assert.Contains(ExtremeRoleId.Sidekick, container.SeparatedRoleId[ExtremeRoleType.Neutral]);
	}

	[Fact]
	public void ProcessQueenAndServant_WhenQueenOnFalse_DoesNotAddQueenOrServant()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		var assignState = new GuesserNormalRoleAssignState { IsQueenOn = false };

		// Act
		GuesserNormalRoleProvider.ProcessQueenAndServant(container, assignState);

		// Assert
		Assert.Empty(container.Result);
	}

	[Fact]
	public void IsInvestigatorOffice_ReturnsTrueOnlyForInvestigatorOfficeId()
	{
		// Act
		bool isInvestigator = GuesserNormalRoleProvider.IsInvestigatorOffice((byte)CombinationRoleType.InvestigatorOffice);
		bool isOther = GuesserNormalRoleProvider.IsInvestigatorOffice((byte)CombinationRoleType.Lover);

		// Assert
		Assert.True(isInvestigator);
		Assert.False(isOther);
	}
}
