using AmongUs.GameOptions;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination.Guesser;
using Xunit;

using GuesserRole = ExtremeRoles.Roles.Combination.Guesser.Guesser;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Combination.Guesser;

public class GuesserVanillaRoleProviderTests
{
	[Fact]
	public void ShouldSkipVanillaRole_ReturnsTrueForDefaultRolesAndGhosts()
	{
		// Act
		bool skipCrewmate = GuesserVanillaRoleProvider.ShouldSkipVanillaRole(RoleTypes.Crewmate);
		bool skipImpostor = GuesserVanillaRoleProvider.ShouldSkipVanillaRole(RoleTypes.Impostor);
		bool skipGuardian = GuesserVanillaRoleProvider.ShouldSkipVanillaRole(RoleTypes.GuardianAngel);
		bool skipCrewGhost = GuesserVanillaRoleProvider.ShouldSkipVanillaRole(RoleTypes.CrewmateGhost);
		bool skipImpGhost = GuesserVanillaRoleProvider.ShouldSkipVanillaRole(RoleTypes.ImpostorGhost);
		bool skipSpiritGuide = GuesserVanillaRoleProvider.ShouldSkipVanillaRole(RoleTypes.SpiritGuide);
		bool skipScientist = GuesserVanillaRoleProvider.ShouldSkipVanillaRole(RoleTypes.Scientist);

		// Assert
		Assert.True(skipCrewmate);
		Assert.True(skipImpostor);
		Assert.True(skipGuardian);
		Assert.True(skipCrewGhost);
		Assert.True(skipImpGhost);
		Assert.True(skipSpiritGuide);
		Assert.False(skipScientist);
	}

	[Fact]
	public void ProcessAmongUsRole_WhenCrewmateAdditionalRole_AddsWithCrewmateTeam()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		GuesserVanillaRoleProvider.ProcessAmongUsRole(container, RoleTypes.Scientist);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal((ExtremeRoleId)RoleTypes.Scientist, container.Result[0].Id);
		Assert.Equal(ExtremeRoleType.Crewmate, container.Result[0].Team);
		Assert.Contains((ExtremeRoleId)RoleTypes.Scientist, container.SeparatedRoleId[ExtremeRoleType.Crewmate]);
	}

	[Fact]
	public void ProcessAmongUsRole_WhenImpostorAdditionalRole_AddsWithImpostorTeam()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		GuesserVanillaRoleProvider.ProcessAmongUsRole(container, RoleTypes.Shapeshifter);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal((ExtremeRoleId)RoleTypes.Shapeshifter, container.Result[0].Id);
		Assert.Equal(ExtremeRoleType.Impostor, container.Result[0].Team);
		Assert.Contains((ExtremeRoleId)RoleTypes.Shapeshifter, container.SeparatedRoleId[ExtremeRoleType.Impostor]);
	}

	[Fact]
	public void ProcessAmongUsRole_WhenUnknownRole_AddsWithNullTeam()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		GuesserVanillaRoleProvider.ProcessAmongUsRole(container, (RoleTypes)9999);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal((ExtremeRoleId)9999, container.Result[0].Id);
		Assert.Equal(ExtremeRoleType.Null, container.Result[0].Team);
	}

	[Fact]
	public void AddCrewmateDefaultRole_AddsCrewmateWhenFlagIsSet()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		GuesserVanillaRoleProvider.AddCrewmateDefaultRole(container, GuesserRole.DefaultGuessRole.Crewmate);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal((ExtremeRoleId)RoleTypes.Crewmate, container.Result[0].Id);
	}

	[Fact]
	public void AddCrewmateDefaultRole_DoesNotAddWhenFlagNotSet()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		GuesserVanillaRoleProvider.AddCrewmateDefaultRole(container, GuesserRole.DefaultGuessRole.None);

		// Assert
		Assert.Empty(container.Result);
	}

	[Fact]
	public void AddImpostorDefaultRole_AddsImpostorWhenFlagIsSet()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		GuesserVanillaRoleProvider.AddImpostorDefaultRole(container, GuesserRole.DefaultGuessRole.Impostor);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal((ExtremeRoleId)RoleTypes.Impostor, container.Result[0].Id);
	}

	[Fact]
	public void AddDoveDefaultRole_AddsDoveWhenFlagIsSet()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		GuesserVanillaRoleProvider.AddDoveDefaultRole(container, GuesserRole.DefaultGuessRole.Dove);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal(ExtremeRoleId.Dove, container.Result[0].Id);
		Assert.Equal(ExtremeRoleType.Liberal, container.Result[0].Team);
	}

	[Fact]
	public void AddMilitantDefaultRole_AddsMilitantWhenMilitantOnAndFlagIsSet()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		GuesserVanillaRoleProvider.AddMilitantDefaultRole(container, GuesserRole.DefaultGuessRole.Militant, militantOn: true);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal(ExtremeRoleId.Militant, container.Result[0].Id);
	}

	[Fact]
	public void AddMilitantDefaultRole_DoesNotAddWhenMilitantOff()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();

		// Act
		GuesserVanillaRoleProvider.AddMilitantDefaultRole(container, GuesserRole.DefaultGuessRole.Militant, militantOn: false);

		// Assert
		Assert.Empty(container.Result);
	}

	[Fact]
	public void AddVanillaRoles_WhenIncludeNoneRoleFalse_DoesNotAddAnyRole()
	{
		// Arrange
		var provider = new GuesserVanillaRoleProvider();
		var container = new GuesserRoleInfoContainer();

		// Act
		provider.AddVanillaRoles(container, includeNoneRole: false, GuesserRole.DefaultGuessRole.Crewmate, liberalOn: true, militantOn: true);

		// Assert
		Assert.Empty(container.Result);
	}

	[Fact]
	public void AddVanillaRoles_WhenIncludeNoneRoleTrue_AddsConfiguredRoles()
	{
		// Arrange
		var provider = new GuesserVanillaRoleProvider();
		var container = new GuesserRoleInfoContainer();

		// Act
		provider.AddVanillaRoles(
			container,
			includeNoneRole: true,
			defaultRole: GuesserRole.DefaultGuessRole.Crewmate | GuesserRole.DefaultGuessRole.Impostor | GuesserRole.DefaultGuessRole.Dove | GuesserRole.DefaultGuessRole.Militant,
			liberalOn: true,
			militantOn: true);

		// Assert
		Assert.Equal(4, container.Result.Count);
	}

	[Fact]
	public void AddVanillaRoles_WhenLiberalOff_DoesNotAddLiberalRoles()
	{
		// Arrange
		var provider = new GuesserVanillaRoleProvider();
		var container = new GuesserRoleInfoContainer();

		// Act
		provider.AddVanillaRoles(
			container,
			includeNoneRole: true,
			defaultRole: GuesserRole.DefaultGuessRole.Crewmate | GuesserRole.DefaultGuessRole.Dove,
			liberalOn: false,
			militantOn: true);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal((ExtremeRoleId)RoleTypes.Crewmate, container.Result[0].Id);
	}
}
