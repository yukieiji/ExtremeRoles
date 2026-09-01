using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption.Factory;

public class OptionCategoryAssemblerTests
{
	public OptionCategoryAssemblerTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupAmongUsClientMock();
		MockSetupHelper.SetupLobbyMock();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupLogger();
	}

	public enum TestOptionEnum
	{
		CategoryOption = 99950
	}

	[Fact]
	public void CreateOptionCategory_WithInt_ReturnsOptionCategoryFactory()
	{
		using var factory = OptionCategoryAssembler.CreateOptionCategory(99910, "TestCategory", OptionTab.CrewmateTab, Color.red);

		Assert.NotNull(factory);
		Assert.IsType<OptionCategoryFactory>(factory);
		Assert.Equal("TestCategory", factory.Name);
		Assert.Equal(OptionTab.CrewmateTab, factory.Tab);
	}

	[Fact]
	public void CreateOptionCategory_WithEnum_ReturnsOptionCategoryFactory()
	{
		using var factory = OptionCategoryAssembler.CreateOptionCategory(TestOptionEnum.CategoryOption, OptionTab.ImpostorTab);

		Assert.NotNull(factory);
		Assert.IsType<OptionCategoryFactory>(factory);
		Assert.Equal("CategoryOption", factory.Name);
		Assert.Equal(OptionTab.ImpostorTab, factory.Tab);
	}

	[Fact]
	public void CreateSequentialOptionCategory_ReturnsSequentialOptionCategoryFactory()
	{
		using var factory = OptionCategoryAssembler.CreateSequentialOptionCategory(99920, "SeqCategory", OptionTab.GeneralTab);

		Assert.NotNull(factory);
		Assert.IsType<SequentialOptionCategoryFactory>(factory);
		Assert.Equal("SeqCategory", factory.Name);
	}

	[Fact]
	public void CreateAutoParentSetOptionCategory_WithInt_ReturnsAutoParentSetOptionCategoryFactory()
	{
		using var factory = OptionCategoryAssembler.CreateAutoParentSetOptionCategory(99930, "AutoCategory", OptionTab.CrewmateTab);

		Assert.NotNull(factory);
		Assert.IsType<AutoParentSetOptionCategoryFactory>(factory);
	}

	[Fact]
	public void CreateAutoParentSetOptionCategory_WithEnum_ReturnsAutoParentSetOptionCategoryFactory()
	{
		using var factory = OptionCategoryAssembler.CreateAutoParentSetOptionCategory(TestOptionEnum.CategoryOption, OptionTab.CrewmateTab);

		Assert.NotNull(factory);
		Assert.IsType<AutoParentSetOptionCategoryFactory>(factory);
	}
}
