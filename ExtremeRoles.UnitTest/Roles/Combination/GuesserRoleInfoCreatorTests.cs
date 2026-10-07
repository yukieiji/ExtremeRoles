using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination;
using Moq;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Combination;

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
		if (MockGameOptionsManagerget_InstanceHelper.Instance == null)
		{
			var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
			var mockGameOptionsManager = new Mock<GameOptionsManager>(IntPtr.Zero);
			mockGameOptionsManager.SetupGet(g => g.CurrentGameOptions).Returns(mockGameOptions.Object);

			var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
			mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockGameOptionsManager.Object);
			MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;
		}
	}

	[Fact]
	public void Create_CallsSubProvidersAndReturnsCombinedRoleInfoList()
	{
		// Arrange
		var vanillaMock = new Mock<IGuesserVanillaRoleProvider>();
		var normalMock = new Mock<IGuesserNormalRoleProvider>();
		var combMock = new Mock<IGuesserCombRoleProvider>();
		var optionLoader = new LiberalDefaultOptionLoader();

		var assignState = new GuesserNormalRoleAssignState();

		vanillaMock.Setup(v => v.AddVanillaRoles(
			It.IsAny<List<GuessBehaviour.RoleInfo>>(),
			It.IsAny<Dictionary<ExtremeRoleType, List<ExtremeRoleId>>>(),
			true,
			Guesser.DefaultGuessRole.Crewmate,
			It.IsAny<bool>(),
			It.IsAny<bool>()))
			.Callback<List<GuessBehaviour.RoleInfo>, Dictionary<ExtremeRoleType, List<ExtremeRoleId>>, bool, Guesser.DefaultGuessRole, bool, bool>(
				(list, dict, inc, def, lib, mil) =>
				{
					list.Add(new GuessBehaviour.RoleInfo { Id = (ExtremeRoleId)RoleTypes.Crewmate, Team = ExtremeRoleType.Crewmate });
				});

		normalMock.Setup(n => n.AddExRNormalRoles(
			It.IsAny<List<GuessBehaviour.RoleInfo>>(),
			It.IsAny<Dictionary<ExtremeRoleType, List<ExtremeRoleId>>>(),
			out assignState))
			.Callback((List<GuessBehaviour.RoleInfo> list, Dictionary<ExtremeRoleType, List<ExtremeRoleId>> dict, out GuesserNormalRoleAssignState state) =>
			{
				state = new GuesserNormalRoleAssignState();
				list.Add(new GuessBehaviour.RoleInfo { Id = ExtremeRoleId.Sheriff, Team = ExtremeRoleType.Crewmate });
			});

		combMock.Setup(c => c.AddExRCombRoles(
			It.IsAny<List<GuessBehaviour.RoleInfo>>(),
			It.IsAny<Dictionary<ExtremeRoleType, List<ExtremeRoleId>>>(),
			It.IsAny<GuesserNormalRoleAssignState>()))
			.Callback<List<GuessBehaviour.RoleInfo>, Dictionary<ExtremeRoleType, List<ExtremeRoleId>>, GuesserNormalRoleAssignState>(
				(list, dict, st) =>
				{
					list.Add(new GuessBehaviour.RoleInfo { Id = ExtremeRoleId.Lover, Team = ExtremeRoleType.Crewmate });
				});

		var creator = new GuesserRoleInfoCreator(
			vanillaMock.Object,
			normalMock.Object,
			combMock.Object,
			optionLoader);

		// Act
		var result = creator.Create(true, Guesser.DefaultGuessRole.Crewmate);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(3, result.Count);
		Assert.Equal((ExtremeRoleId)RoleTypes.Crewmate, result[0].Id);
		Assert.Equal(ExtremeRoleId.Sheriff, result[1].Id);
		Assert.Equal(ExtremeRoleId.Lover, result[2].Id);

		vanillaMock.Verify(v => v.AddVanillaRoles(
			It.IsAny<List<GuessBehaviour.RoleInfo>>(),
			It.IsAny<Dictionary<ExtremeRoleType, List<ExtremeRoleId>>>(),
			true,
			Guesser.DefaultGuessRole.Crewmate,
			It.IsAny<bool>(),
			It.IsAny<bool>()), Times.Once);

		vanillaMock.Verify(v => v.AddAmongUsRoles(
			It.IsAny<List<GuessBehaviour.RoleInfo>>(),
			It.IsAny<Dictionary<ExtremeRoleType, List<ExtremeRoleId>>>()), Times.Once);

		normalMock.Verify(n => n.AddExRNormalRoles(
			It.IsAny<List<GuessBehaviour.RoleInfo>>(),
			It.IsAny<Dictionary<ExtremeRoleType, List<ExtremeRoleId>>>(),
			out assignState), Times.Once);

		combMock.Verify(c => c.AddExRCombRoles(
			It.IsAny<List<GuessBehaviour.RoleInfo>>(),
			It.IsAny<Dictionary<ExtremeRoleType, List<ExtremeRoleId>>>(),
			It.IsAny<GuesserNormalRoleAssignState>()), Times.Once);
	}
}
