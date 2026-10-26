using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination.Guesser;
using Moq;
using Xunit;

using GuesserRole = ExtremeRoles.Roles.Combination.Guesser.Guesser;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Combination.Guesser;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class GuesserRoleInfoCreatorTests
{
	public GuesserRoleInfoCreatorTests()
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
		var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
		mockGameOptions.SetupGet(g => g.RoleOptions).Returns(mockRoleOptions.Object);

		var mockGameOptionsManager = new Mock<GameOptionsManager>(IntPtr.Zero);
		mockGameOptionsManager.SetupGet(g => g.CurrentGameOptions).Returns(mockGameOptions.Object);

		var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
		mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockGameOptionsManager.Object);
		MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;
	}

	[Fact]
	public void Create_CallsSubProvidersAndReturnsCombinedRoleInfoList()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		var vanillaMock = new Mock<IGuesserVanillaRoleProvider>();
		var normalMock = new Mock<IGuesserNormalRoleProvider>();
		var combMock = new Mock<IGuesserCombRoleProvider>();
		var optionLoader = new LiberalDefaultOptionLoader();

		var assignState = new GuesserNormalRoleAssignState();

		vanillaMock.Setup(v => v.AddVanillaRoles(
			container,
			true,
			GuesserRole.DefaultGuessRole.Crewmate,
			It.IsAny<bool>(),
			It.IsAny<bool>()))
			.Callback<IGuesserRoleInfoContainer, bool, GuesserRole.DefaultGuessRole, bool, bool>(
				(cnt, inc, def, lib, mil) =>
				{
					cnt.Add((ExtremeRoleId)RoleTypes.Crewmate, ExtremeRoleType.Crewmate);
				});

		normalMock.Setup(n => n.AddExRNormalRoles(
			container,
			out assignState))
			.Callback((IGuesserRoleInfoContainer cnt, out GuesserNormalRoleAssignState state) =>
			{
				state = new GuesserNormalRoleAssignState();
				cnt.Add(ExtremeRoleId.Sheriff, ExtremeRoleType.Crewmate);
			});

		combMock.Setup(c => c.AddExRCombRoles(
			container,
			It.IsAny<GuesserNormalRoleAssignState>()))
			.Callback<IGuesserRoleInfoContainer, GuesserNormalRoleAssignState>(
				(cnt, st) =>
				{
					cnt.Add(ExtremeRoleId.Lover, ExtremeRoleType.Crewmate);
				});

		var creator = new GuesserRoleInfoCreator(
			container,
			vanillaMock.Object,
			normalMock.Object,
			combMock.Object,
			optionLoader);

		// Act
		var result = creator.Create(true, GuesserRole.DefaultGuessRole.Crewmate);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(3, result.Count);
		Assert.Equal((ExtremeRoleId)RoleTypes.Crewmate, result[0].Id);
		Assert.Equal(ExtremeRoleId.Sheriff, result[1].Id);
		Assert.Equal(ExtremeRoleId.Lover, result[2].Id);

		vanillaMock.Verify(v => v.AddVanillaRoles(
			container,
			true,
			GuesserRole.DefaultGuessRole.Crewmate,
			It.IsAny<bool>(),
			It.IsAny<bool>()), Times.Once);

		vanillaMock.Verify(v => v.AddAmongUsRoles(container), Times.Once);

		normalMock.Verify(n => n.AddExRNormalRoles(
			container,
			out assignState), Times.Once);

		combMock.Verify(c => c.AddExRCombRoles(
			container,
			It.IsAny<GuesserNormalRoleAssignState>()), Times.Once);
	}
}
