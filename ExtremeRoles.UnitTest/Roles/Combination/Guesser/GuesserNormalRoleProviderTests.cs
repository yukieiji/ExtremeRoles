using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination.Guesser;
using ExtremeRoles.Roles.Solo.Neutral.Jackal;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Combination.Guesser;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class GuesserNormalRoleProviderTests
{
	public GuesserNormalRoleProviderTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupPaletteHelpers();
		MockSetupHelper.SetupOptionManager();
	}

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
	public void ProcessNormalRole_WhenJackal_SetsJackalStateAndDoesNotAddJackalDirectly()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		var assignState = new GuesserNormalRoleAssignState();

		// Act
		GuesserNormalRoleProvider.ProcessNormalRole(container, ExtremeRoleId.Jackal, ExtremeRoleType.Neutral, assignState);

		// Assert
		Assert.Empty(container.Result);
		Assert.True(assignState.IsJackalOn);
	}

	[Fact]
	public void ProcessNormalRole_WhenQueen_SetsQueenStateAndDoesNotAddQueenDirectly()
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
	public void ProcessNormalRole_WhenJailer_CallsProcessJailer()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		var assignState = new GuesserNormalRoleAssignState();

		// Act
		GuesserNormalRoleProvider.ProcessNormalRole(container, ExtremeRoleId.Jailer, ExtremeRoleType.Crewmate, assignState);

		// Assert
		Assert.Equal(3, container.Result.Count);
		Assert.Equal(ExtremeRoleId.Jailer, container.Result[0].Id);
		Assert.Equal(ExtremeRoleId.Yardbird, container.Result[1].Id);
		Assert.Equal(ExtremeRoleId.Lawbreaker, container.Result[2].Id);
	}

	[Fact]
	public void ProcessNormalRole_WhenTucker_CallsProcessTucker()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		var assignState = new GuesserNormalRoleAssignState();

		// Act
		GuesserNormalRoleProvider.ProcessNormalRole(container, ExtremeRoleId.Tucker, ExtremeRoleType.Neutral, assignState);

		// Assert
		Assert.Equal(2, container.Result.Count);
		Assert.Equal(ExtremeRoleId.Tucker, container.Result[0].Id);
		Assert.Equal(ExtremeRoleId.Chimera, container.Result[1].Id);
	}

	[Fact]
	public void ProcessNormalRole_WhenDefaultRole_AddsGenericRoleOnly()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		var assignState = new GuesserNormalRoleAssignState();

		// Act
		GuesserNormalRoleProvider.ProcessNormalRole(container, ExtremeRoleId.Bait, ExtremeRoleType.Crewmate, assignState);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal(ExtremeRoleId.Bait, container.Result[0].Id);
	}

	[Fact]
	public void ProcessJackal_SetsJackalStateFromOptionManager()
	{
		// Arrange
		var assignState = new GuesserNormalRoleAssignState();

		// Act
		GuesserNormalRoleProvider.ProcessJackal(assignState);

		// Assert
		Assert.True(assignState.IsJackalOn);
		Assert.Equal(
			OptionManager.Instance.TryGetCategory(
				OptionTab.NeutralTab,
				ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Jackal),
				out var cate) && cate.GetValue<JackalRole.JackalOption, bool>(JackalRole.JackalOption.ForceReplaceLover),
			assignState.IsJackalForceReplaceLover);
	}

	[Fact]
	public void ProcessQueen_SetsQueenStateTrue()
	{
		// Arrange
		var assignState = new GuesserNormalRoleAssignState();

		// Act
		GuesserNormalRoleProvider.ProcessQueen(assignState);

		// Assert
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
	public void ProcessQueenAndServant_WhenQueenOnTrueAndNeutralCountLessThanTwo_AddsQueenWithoutServantAlone()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		var assignState = new GuesserNormalRoleAssignState { IsQueenOn = true };

		// Act
		GuesserNormalRoleProvider.ProcessQueenAndServant(container, assignState);

		// Assert
		Assert.Contains(container.Result, r => r.Id == ExtremeRoleId.Queen);
		Assert.DoesNotContain(container.Result, r => r.Id == ExtremeRoleId.Servant && r.AnothorId == ExtremeRoleId.Null);
	}

	[Fact]
	public void ProcessQueenAndServant_WhenQueenOnTrueAndNeutralCountMoreThanOne_AddsServantAlone()
	{
		// Arrange
		var container = new GuesserRoleInfoContainer();
		container.AddSeparatedRoleId(ExtremeRoleType.Neutral, ExtremeRoleId.Marlin);
		container.AddSeparatedRoleId(ExtremeRoleType.Neutral, ExtremeRoleId.Jackal);
		var assignState = new GuesserNormalRoleAssignState { IsQueenOn = true };

		// Act
		GuesserNormalRoleProvider.ProcessQueenAndServant(container, assignState);

		// Assert
		Assert.Contains(container.Result, r => r.Id == ExtremeRoleId.Queen);
		Assert.Contains(container.Result, r => r.Id == ExtremeRoleId.Servant && r.AnothorId == ExtremeRoleId.Null);
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

	[Fact]
	public void ProcessNormalRole_WhenDeepOneAndNoFrogs_AddsDeepOneToContainer()
	{
		// Arrange
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		var container = new GuesserRoleInfoContainer();
		var assignState = new GuesserNormalRoleAssignState();

		var system = new DeepOneFrogsControlSystem(5, false);
		ExtremeSystemTypeManager.Instance.TryAdd(ExtremeSystemType.DeepOneFrogsControlSystem, system);

		// Act
		GuesserNormalRoleProvider.ProcessNormalRole(container, ExtremeRoleId.DeepOne, ExtremeRoleType.Neutral, assignState);

		// Assert
		Assert.Single(container.Result);
		Assert.Equal(ExtremeRoleId.DeepOne, container.Result[0].Id);
		Assert.Equal(ExtremeRoleType.Neutral, container.Result[0].Team);
		Assert.Contains(ExtremeRoleId.DeepOne, container.SeparatedRoleId[ExtremeRoleType.Neutral]);
	}

	[Fact]
	public void ProcessNormalRole_WhenDeepOneAndFrogPlaced_DoesNotAddDeepOneToContainer()
	{
		// Arrange
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		var container = new GuesserRoleInfoContainer();
		var assignState = new GuesserNormalRoleAssignState();

		var system = new DeepOneFrogsControlSystem(5, false);
		system.AddFrogForTest(1);
		ExtremeSystemTypeManager.Instance.TryAdd(ExtremeSystemType.DeepOneFrogsControlSystem, system);

		// Act
		GuesserNormalRoleProvider.ProcessNormalRole(container, ExtremeRoleId.DeepOne, ExtremeRoleType.Neutral, assignState);

		// Assert
		Assert.Empty(container.Result);
		Assert.DoesNotContain(ExtremeRoleId.DeepOne, container.SeparatedRoleId[ExtremeRoleType.Neutral]);
	}
}
