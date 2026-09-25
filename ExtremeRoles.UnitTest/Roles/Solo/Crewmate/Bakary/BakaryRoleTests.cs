using System;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Crewmate;
using Moq;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Bakary;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class BakaryRoleTests
{
	private readonly Mock<AmongUsClient> mockClient;

	public BakaryRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupOptionManager();
		MockSetupHelper.SetupGameOptionsManagerMock();

		mockClient = MockSetupHelper.SetupAmongUsClientMock();
	}

	[Fact]
	public void CreateRoleAllOption_CreatesExpectedOptions()
	{
		// Arrange
		int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Bakary);
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bakary();

		// Act
		role.CreateRoleAllOption();

		// Assert
		Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
		Assert.NotNull(category);
		Assert.True(role.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bakary.BakaryOption.ChangeCooking, out var changeCookingOpt));
		Assert.NotNull(changeCookingOpt);
		Assert.True(role.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bakary.BakaryOption.GoodBakeTime, out var goodTimeOpt));
		Assert.NotNull(goodTimeOpt);
		Assert.True(role.Loader.TryGet(ExtremeRoles.Roles.Solo.Crewmate.Bakary.BakaryOption.BadBakeTime, out var badTimeOpt));
		Assert.NotNull(badTimeOpt);
	}

	[Fact]
	public void Initialize_RegistersBakerySystemInExtremeSystemTypeManager()
	{
		// Arrange
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Bakary();
		role.CreateRoleAllOption();

		// Act
		role.Initialize();

		// Assert
		Assert.True(ExtremeSystemTypeManager.Instance.TryGet<BakerySystem>(ExtremeSystemType.BakeryReport, out var system));
		Assert.NotNull(system);
	}
}
