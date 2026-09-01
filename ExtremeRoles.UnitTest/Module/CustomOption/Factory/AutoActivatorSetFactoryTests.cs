using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption.Factory;

public class AutoActivatorSetFactoryTests
{
	public AutoActivatorSetFactoryTests()
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
		ValA,
		ValB
	}

	[Fact]
	public void Activator_WhenSetOnFactory_UsesDefaultActivatorWhenNonePassed()
	{
		using var baseFactory = new OptionCategoryFactory(
			"AutoCategory",
			3000,
			(p, c) => { },
			(t, c) => { }
		);
		using var autoFactory = new AutoActivatorSetFactory(baseFactory);

		var parentOpt = baseFactory.CreateBoolOption(TestOptionEnum.Opt1, true);
		var parentActivator = new ParentActive(parentOpt);

		autoFactory.Activator = parentActivator;

		var childOpt = autoFactory.CreateBoolOption(TestOptionEnum.Opt2, false);

		Assert.Same(parentActivator, childOpt.Activator);
	}

	[Fact]
	public void Activator_WhenPassedExplicitly_OverridesFactoryDefaultActivator()
	{
		using var baseFactory = new OptionCategoryFactory(
			"AutoCategory",
			3001,
			(p, c) => { },
			(t, c) => { }
		);
		using var autoFactory = new AutoActivatorSetFactory(baseFactory);

		var parentOpt1 = baseFactory.CreateBoolOption(TestOptionEnum.Opt1, true);
		var parentOpt2 = baseFactory.CreateBoolOption(TestOptionEnum.Opt2, true);

		var defaultActivator = new ParentActive(parentOpt1);
		var explicitActivator = new ParentActive(parentOpt2);

		autoFactory.Activator = defaultActivator;

		var childOpt = autoFactory.CreateBoolOption(TestOptionEnum.Opt3, false, activator: explicitActivator);

		Assert.Same(explicitActivator, childOpt.Activator);
	}

	[Fact]
	public void Setters_ForIdOffsetAndPrefix_DelegatesToInternalFactory()
	{
		using var baseFactory = new OptionCategoryFactory(
			"BaseName",
			3002,
			(p, c) => { },
			(t, c) => { }
		);
		using var autoFactory = new AutoActivatorSetFactory(baseFactory)
		{
			IdOffset = 50,
			OptionPrefix = "NewPrefix"
		};

		var opt = autoFactory.CreateBoolOption(TestOptionEnum.Opt1, true);

		Assert.Equal(51, opt.Info.Id);
		Assert.Equal("NewPrefixOpt1", opt.Info.Name);
	}

	[Fact]
	public void CreateAllOptionTypes_DelegatesCorrectlyAndReturnsValidOptions()
	{
		using var baseFactory = new OptionCategoryFactory(
			"AutoCategory",
			3003,
			(p, c) => { },
			(t, c) => { }
		);
		using var autoFactory = new AutoActivatorSetFactory(baseFactory);

		var floatOpt = autoFactory.CreateFloatOption(TestOptionEnum.Opt1, 1.0f, 0.0f, 5.0f, 0.5f);
		var dynFloatOpt = autoFactory.CreateFloatDynamicMaxOption(TestOptionEnum.Opt2, 1.0f, 0.0f, 0.5f, floatOpt);
		var intOpt = autoFactory.CreateIntOption(TestOptionEnum.Opt3, 2, 0, 10, 1);
		var dynIntOpt = autoFactory.CreateIntDynamicMaxOption(TestOptionEnum.Opt4, 2, 0, 1, intOpt);
		var selectArrOpt = autoFactory.CreateSelectionOption(TestOptionEnum.Opt5, ["X", "Y"]);
		var selectEnumOpt = autoFactory.CreateSelectionOption<TestOptionEnum, TestSelectionEnum>(TestOptionEnum.Opt6);
		var genericOpt = autoFactory.CreateOption(TestOptionEnum.Opt7, ValueHolderAssembler.CreateBoolValue(true));
		var pctOpt = autoFactory.Create0To100Percentage10StepOption(TestOptionEnum.Opt8, defaultGage: 50);

		Assert.NotNull(floatOpt);
		Assert.NotNull(dynFloatOpt);
		Assert.NotNull(intOpt);
		Assert.NotNull(dynIntOpt);
		Assert.NotNull(selectArrOpt);
		Assert.NotNull(selectEnumOpt);
		Assert.NotNull(genericOpt);
		Assert.NotNull(pctOpt);

		Assert.Same(floatOpt, autoFactory.Get(floatOpt.Info.Id));
	}
}
