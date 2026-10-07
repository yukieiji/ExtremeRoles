using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination;
using Moq;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Combination;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class GuesserSubProvidersTests
{
	public GuesserSubProvidersTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupPaletteHelpers();
		MockSetupHelper.SetupAmongUsClientMock();
		MockSetupHelper.SetupGameDataMock();
		MockSetupHelper.SetupOptionManager();
		SetupGameOptionsManagerMock();
	}

	private static void SetupGameOptionsManagerMock()
	{
		var mockRoleOptions = new Mock<IRoleOptionsCollection>(IntPtr.Zero);
		mockRoleOptions.Setup(r => r.GetChancePerGame(It.IsAny<RoleTypes>())).Returns(0);
		mockRoleOptions.Setup(r => r.GetChancePerGame(RoleTypes.Scientist)).Returns(100);

		var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
		mockGameOptions.SetupGet(g => g.RoleOptions).Returns(mockRoleOptions.Object);

		var mockGameOptionsManager = new Mock<GameOptionsManager>(IntPtr.Zero);
		mockGameOptionsManager.SetupGet(g => g.CurrentGameOptions).Returns(mockGameOptions.Object);

		var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
		mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockGameOptionsManager.Object);
		MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;
	}

	[Fact]
	public void AddVanillaRoles_WhenIncludeNoneRoleTrue_AddsConfiguredDefaultRoles()
	{
		// Arrange
		var provider = new GuesserVanillaRoleProvider();
		var result = new List<GuessBehaviour.RoleInfo>();
		var separated = new Dictionary<ExtremeRoleType, List<ExtremeRoleId>>
		{
			{ ExtremeRoleType.Crewmate, new List<ExtremeRoleId>() },
			{ ExtremeRoleType.Impostor, new List<ExtremeRoleId>() },
			{ ExtremeRoleType.Neutral, new List<ExtremeRoleId>() },
			{ ExtremeRoleType.Liberal, new List<ExtremeRoleId>() },
		};

		// Act
		provider.AddVanillaRoles(
			result,
			separated,
			includeNoneRole: true,
			defaultRole: Guesser.DefaultGuessRole.Crewmate | Guesser.DefaultGuessRole.Dove,
			liberalOn: true,
			militantOn: true);

		// Assert
		Assert.Equal(2, result.Count);
		Assert.Equal((ExtremeRoleId)RoleTypes.Crewmate, result[0].Id);
		Assert.Equal(ExtremeRoleId.Dove, result[1].Id);
	}

	[Fact]
	public void AddVanillaRoles_WhenIncludeNoneRoleFalse_DoesNotAddRoles()
	{
		// Arrange
		var provider = new GuesserVanillaRoleProvider();
		var result = new List<GuessBehaviour.RoleInfo>();
		var separated = new Dictionary<ExtremeRoleType, List<ExtremeRoleId>>
		{
			{ ExtremeRoleType.Crewmate, new List<ExtremeRoleId>() },
			{ ExtremeRoleType.Impostor, new List<ExtremeRoleId>() },
			{ ExtremeRoleType.Neutral, new List<ExtremeRoleId>() },
			{ ExtremeRoleType.Liberal, new List<ExtremeRoleId>() },
		};

		// Act
		provider.AddVanillaRoles(
			result,
			separated,
			includeNoneRole: false,
			defaultRole: Guesser.DefaultGuessRole.Crewmate,
			liberalOn: true,
			militantOn: true);

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public void AddAmongUsRoles_AddsRolesWithChanceGreaterThanZero()
	{
		// Arrange
		var provider = new GuesserVanillaRoleProvider();
		var result = new List<GuessBehaviour.RoleInfo>();
		var separated = new Dictionary<ExtremeRoleType, List<ExtremeRoleId>>
		{
			{ ExtremeRoleType.Crewmate, new List<ExtremeRoleId>() },
			{ ExtremeRoleType.Impostor, new List<ExtremeRoleId>() },
			{ ExtremeRoleType.Neutral, new List<ExtremeRoleId>() },
			{ ExtremeRoleType.Liberal, new List<ExtremeRoleId>() },
		};

		// Act
		provider.AddAmongUsRoles(result, separated);

		// Assert
		Assert.Single(result);
		Assert.Equal((ExtremeRoleId)RoleTypes.Scientist, result[0].Id);
		Assert.Contains((ExtremeRoleId)RoleTypes.Scientist, separated[ExtremeRoleType.Crewmate]);
	}
}
