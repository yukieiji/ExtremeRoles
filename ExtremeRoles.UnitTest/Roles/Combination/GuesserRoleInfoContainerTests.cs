using System.Collections.Generic;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Combination;

public class GuesserRoleInfoContainerTests
{
	[Fact]
	public void Add_AddsRoleInfoToResult()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		container.Add(ExtremeRoleId.Sheriff, ExtremeRoleType.Crewmate, ExtremeRoleId.Null);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal(ExtremeRoleId.Sheriff, container.Result[0].Id);
		Assert.Equal(ExtremeRoleType.Crewmate, container.Result[0].Team);
		Assert.Equal(ExtremeRoleId.Null, container.Result[0].AnothorId);
	}

	[Fact]
	public void AddSeparatedRoleId_WhenTeamExists_AddsIdToSeparatedDict()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		container.AddSeparatedRoleId(ExtremeRoleType.Crewmate, ExtremeRoleId.Sheriff);

		// Assert
		Assert.Contains(ExtremeRoleId.Sheriff, container.SeparatedRoleId[ExtremeRoleType.Crewmate]);
	}

	[Fact]
	public void AddSeparatedRoleId_WhenTeamDoesNotExist_DoesNotThrow()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act & Assert
		container.AddSeparatedRoleId(ExtremeRoleType.Null, ExtremeRoleId.Sheriff);
	}

	[Fact]
	public void ListAdd_AddsRoleInfoForEveryItemInList()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		var subList = new List<ExtremeRoleId> { ExtremeRoleId.Lover, ExtremeRoleId.Jackal };

		// Act
		container.ListAdd(ExtremeRoleId.Servant, ExtremeRoleType.Neutral, subList);

		// Assert
		Assert.Equal(2, container.Result.Count);
		Assert.Equal(ExtremeRoleId.Servant, container.Result[0].Id);
		Assert.Equal(ExtremeRoleId.Lover, container.Result[0].AnothorId);
		Assert.Equal(ExtremeRoleId.Jackal, container.Result[1].AnothorId);
	}

	[Fact]
	public void ListAddTargetTeam_WhenTargetTeamExists_AddsFromTargetList()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		container.AddSeparatedRoleId(ExtremeRoleType.Crewmate, ExtremeRoleId.Sheriff);

		// Act
		container.ListAddTargetTeam(ExtremeRoleId.Lover, ExtremeRoleType.Crewmate, ExtremeRoleType.Crewmate);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal(ExtremeRoleId.Lover, container.Result[0].Id);
		Assert.Equal(ExtremeRoleId.Sheriff, container.Result[0].AnothorId);
	}

	[Fact]
	public void ListAddTargetTeam_WhenTargetTeamDoesNotExist_DoesNotAdd()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		container.ListAddTargetTeam(ExtremeRoleId.Lover, ExtremeRoleType.Crewmate, ExtremeRoleType.Null);

		// Assert
		Assert.Empty(container.Result);
	}
}
