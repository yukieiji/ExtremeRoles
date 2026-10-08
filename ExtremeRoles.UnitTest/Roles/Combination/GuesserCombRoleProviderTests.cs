using System.Collections.Generic;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Roles.Combination.InvestigatorOffice;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Combination;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class GuesserCombRoleProviderTests
{
	public GuesserCombRoleProviderTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupPaletteHelpers();
	}

	[Fact]
	public void ProcessFlexibleMultiAssign_WhenAssignImpFalse_AddsSameTeamTargets()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		container.AddSeparatedRoleId(ExtremeRoleType.Crewmate, ExtremeRoleId.Sheriff);

		// Act
		GuesserCombRoleProvider.ProcessFlexibleMultiAssign(
			container,
			ExtremeRoleId.Lover,
			ExtremeRoleType.Crewmate,
			isAssignImp: false);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal(ExtremeRoleId.Lover, container.Result[0].Id);
		Assert.Equal(ExtremeRoleId.Sheriff, container.Result[0].AnothorId);
		Assert.Equal(ExtremeRoleType.Crewmate, container.Result[0].Team);
	}

	[Fact]
	public void ProcessFlexibleMultiAssign_WhenAssignImpTrue_AddsCrewmateAndImpostorTargets()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		container.AddSeparatedRoleId(ExtremeRoleType.Crewmate, ExtremeRoleId.Sheriff);
		container.AddSeparatedRoleId(ExtremeRoleType.Impostor, ExtremeRoleId.Assassin);

		// Act
		GuesserCombRoleProvider.ProcessFlexibleMultiAssign(
			container,
			ExtremeRoleId.Lover,
			ExtremeRoleType.Crewmate,
			isAssignImp: true);

		// Assert
		Assert.Equal(2, container.Result.Count);
		Assert.Equal(ExtremeRoleId.Sheriff, container.Result[0].AnothorId);
		Assert.Equal(ExtremeRoleType.Crewmate, container.Result[0].Team);
		Assert.Equal(ExtremeRoleId.Assassin, container.Result[1].AnothorId);
		Assert.Equal(ExtremeRoleType.Impostor, container.Result[1].Team);
	}

	[Fact]
	public void SingleAssignCombRole_WhenInvestigatorOffice_AddsInvestigatorApprentice()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		GuesserCombRoleProvider.ProcessSingleAssignCombRole(
			container,
			(byte)CombinationRoleType.InvestigatorOffice,
			new InvestigatorOfficeManager());

		// Assert
		Assert.Equal(3, container.Result.Count);
		Assert.Equal(ExtremeRoleId.Investigator, container.Result[0].Id);
		Assert.Equal(ExtremeRoleId.Assistant, container.Result[1].Id);
		Assert.Equal(ExtremeRoleId.InvestigatorApprentice, container.Result[2].Id);
		Assert.Equal(ExtremeRoleType.Crewmate, container.Result[2].Team);
	}

	[Fact]
	public void IsInvestigatorOffice_ReturnsTrueOnlyForInvestigatorOfficeId()
	{
		// Act
		bool isInvestigator = GuesserCombRoleProvider.IsInvestigatorOffice((byte)CombinationRoleType.InvestigatorOffice);
		bool isOther = GuesserCombRoleProvider.IsInvestigatorOffice((byte)CombinationRoleType.Lover);

		// Assert
		Assert.True(isInvestigator);
		Assert.False(isOther);
	}

	[Fact]
	public void ProcessMultiAssignCombRole_WhenInvestigatorOffice_AddsInvestigatorApprentice()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		container.AddSeparatedRoleId(ExtremeRoleType.Crewmate, ExtremeRoleId.Sheriff);

		// Act
		GuesserCombRoleProvider.ProcessMultiAssignCombRole(
			container,
			(byte)CombinationRoleType.InvestigatorOffice,
			new InvestigatorOfficeManager());

		// Assert
		Assert.Equal(3, container.Result.Count);
		Assert.Equal(ExtremeRoleId.Investigator, container.Result[0].Id);
		Assert.Equal(ExtremeRoleId.Assistant, container.Result[1].Id);
		Assert.Equal(ExtremeRoleId.InvestigatorApprentice, container.Result[2].Id);
	}

	[Fact]
	public void ProcessFlexibleCombRole_WhenJackalOnAndLover_AddsJackalLoverSidekick()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		var flexMng = new LoverManager();
		var assignState = new GuesserNormalRoleAssignState
		{
			IsJackalOn = true,
			IsJackalForceReplaceLover = false,
		};

		// Act
		GuesserCombRoleProvider.ProcessFlexibleCombRole(
			container,
			flexMng,
			multiAssign: false,
			assignState: assignState);

		// Assert
		Assert.Equal(2, container.Result.Count);
		Assert.Equal(ExtremeRoleId.Lover, container.Result[0].Id);
		Assert.Equal(ExtremeRoleId.Lover, container.Result[1].Id);
		Assert.Equal(ExtremeRoleId.Sidekick, container.Result[1].AnothorId);
		Assert.Equal(ExtremeRoleType.Neutral, container.Result[1].Team);
	}
}
