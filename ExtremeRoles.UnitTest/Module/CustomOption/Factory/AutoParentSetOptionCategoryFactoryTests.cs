using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption.Factory;

public class AutoParentSetOptionCategoryFactoryTests
{
	public AutoParentSetOptionCategoryFactoryTests()
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
		Opt1 = 1,
		Opt2 = 2,
		Opt3 = 3,
		Opt4 = 4,
		Opt5 = 5,
		Opt6 = 6,
		Opt7 = 7,
		Opt8 = 8
	}

	public enum TestSelectionEnum
	{
		Choice1,
		Choice2
	}

	[Fact]
	public void Constructor_WithoutParent_SetsActivatorNullInitially_AndSetsFirstCreatedOptionAsParentForSubsequentOptions()
	{
		using var baseFactory = new OptionCategoryFactory(
			"ParentCategory",
			4000,
			(p, c) => { },
			(t, c) => { }
		);
		using var autoParentFactory = new AutoParentSetOptionCategoryFactory(baseFactory);

		Assert.Null(autoParentFactory.Activator);

		var firstOpt = autoParentFactory.CreateBoolOption(TestOptionEnum.Opt1, true);
		Assert.NotNull(autoParentFactory.Activator);
		Assert.IsType<ParentActive>(autoParentFactory.Activator);
		Assert.Same(firstOpt, autoParentFactory.Activator.Parent);

		var secondOpt = autoParentFactory.CreateBoolOption(TestOptionEnum.Opt2, false);
		Assert.NotNull(secondOpt.Activator);
		Assert.Same(firstOpt, secondOpt.Activator.Parent);
	}

	[Fact]
	public void Constructor_WithParent_UsesProvidedParentAsActivatorFromStart()
	{
		using var baseFactory = new OptionCategoryFactory(
			"ParentCategory",
			4001,
			(p, c) => { },
			(t, c) => { }
		);

		var externalParentOpt = baseFactory.CreateBoolOption(TestOptionEnum.Opt1, true);

		using var autoParentFactory = new AutoParentSetOptionCategoryFactory(baseFactory, externalParentOpt);

		Assert.NotNull(autoParentFactory.Activator);
		Assert.Same(externalParentOpt, autoParentFactory.Activator.Parent);

		var childOpt = autoParentFactory.CreateBoolOption(TestOptionEnum.Opt2, false);
		Assert.NotNull(childOpt.Activator);
		Assert.Same(externalParentOpt, childOpt.Activator.Parent);
	}

	[Fact]
	public void Setters_ForIdOffsetAndPrefix_DelegatesToInternalFactory()
	{
		using var baseFactory = new OptionCategoryFactory(
			"BaseName",
			4002,
			(p, c) => { },
			(t, c) => { }
		);
		using var autoParentFactory = new AutoParentSetOptionCategoryFactory(baseFactory)
		{
			IdOffset = 10,
			OptionPrefix = "CustomPrefix"
		};

		var opt = autoParentFactory.CreateBoolOption(TestOptionEnum.Opt1, true);

		Assert.Equal(11, opt.Info.Id);
		Assert.Equal("CustomPrefixOpt1", opt.Info.Name);
	}

	[Fact]
	public void CreateAllOptionTypes_AutomaticallySetsFirstOptionAsParentAndCreatesSubsequentOptions()
	{
		using var baseFactory = new OptionCategoryFactory(
			"ParentCategory",
			4003,
			(p, c) => { },
			(t, c) => { }
		);
		using var autoParentFactory = new AutoParentSetOptionCategoryFactory(baseFactory);

		var floatOpt = autoParentFactory.CreateFloatOption(TestOptionEnum.Opt1, 1.0f, 0.0f, 5.0f, 0.5f);
		var dynFloatOpt = autoParentFactory.CreateFloatDynamicMaxOption(TestOptionEnum.Opt2, 1.0f, 0.0f, 0.5f, floatOpt);
		var intOpt = autoParentFactory.CreateIntOption(TestOptionEnum.Opt3, 2, 0, 10, 1);
		var dynIntOpt = autoParentFactory.CreateIntDynamicMaxOption(TestOptionEnum.Opt4, 2, 0, 1, intOpt);
		var selectArrOpt = autoParentFactory.CreateSelectionOption(TestOptionEnum.Opt5, ["A", "B"]);
		var selectEnumOpt = autoParentFactory.CreateSelectionOption<TestOptionEnum, TestSelectionEnum>(TestOptionEnum.Opt6);
		var genericOpt = autoParentFactory.CreateOption(TestOptionEnum.Opt7, ValueHolderAssembler.CreateBoolValue(true));
		var pctOpt = autoParentFactory.Create0To100Percentage10StepOption(TestOptionEnum.Opt8, defaultGage: 30);

		Assert.Same(floatOpt, autoParentFactory.Get(floatOpt.Info.Id));
		Assert.Same(floatOpt, dynFloatOpt.Activator?.Parent);
		Assert.Same(floatOpt, intOpt.Activator?.Parent);
		Assert.Same(floatOpt, dynIntOpt.Activator?.Parent);
		Assert.Same(floatOpt, selectArrOpt.Activator?.Parent);
		Assert.Same(floatOpt, selectEnumOpt.Activator?.Parent);
		Assert.Same(floatOpt, genericOpt.Activator?.Parent);
		Assert.Same(floatOpt, pctOpt.Activator?.Parent);
	}
}
