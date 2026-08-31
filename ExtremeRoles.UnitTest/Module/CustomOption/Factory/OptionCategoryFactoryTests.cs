using System;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Interfaces;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption.Factory;

public class OptionCategoryFactoryTests
{
	public OptionCategoryFactoryTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupAmongUsClientMock();
		MockSetupHelper.SetupLobbyMock();
		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>(), It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object>>()))
			.Returns((StringNames name, Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object> args) => name.ToString());
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object>>()))
			.Returns((string key, string defaultStr, Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object> parts) => defaultStr);
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string key, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr);
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupLogger();
	}

	public enum TestOptionEnum
	{
		FirstOption = 1,
		SecondOption = 2
	}

	public enum TestSelectionEnum
	{
		ChoiceA,
		ChoiceB,
		ChoiceC
	}

	[Fact]
	public void CreateBoolOption_CreatesAndRegistersOption()
	{
		IOption registeredParent = null!;
		IOption registeredChild = null!;
		OptionCategory registeredCategory = null!;

		using var factory = new OptionCategoryFactory(
			"TestCategory",
			1000,
			(parent, child) => { registeredParent = parent; registeredChild = child; },
			(tab, category) => registeredCategory = category,
			OptionTab.GeneralTab
		);

		var opt = factory.CreateBoolOption(TestOptionEnum.FirstOption, true);

		Assert.NotNull(opt);
		Assert.Equal(1, opt.Info.Id);
		Assert.Equal("TestCategoryFirstOption", opt.Info.Name);
		Assert.True(opt.Value<bool>());
		Assert.Same(opt, factory.Get(1));
	}

	[Fact]
	public void CreateBoolOption_WithIgnorePrefix_FormatsNameCorrectly()
	{
		using var factory = new OptionCategoryFactory(
			"TestCategory",
			1001,
			(p, c) => { },
			(t, c) => { }
		);

		var opt = factory.CreateBoolOption(TestOptionEnum.FirstOption, false, ignorePrefix: true);

		Assert.Equal("|TestCategory|FirstOption", opt.Info.Name);
	}

	[Fact]
	public void CreateFloatOption_CreatesFloatOptionWithRange()
	{
		using var factory = new OptionCategoryFactory(
			"TestCategory",
			1002,
			(p, c) => { },
			(t, c) => { }
		);

		var opt = factory.CreateFloatOption(TestOptionEnum.FirstOption, 2.5f, 1.0f, 5.0f, 0.5f);

		Assert.NotNull(opt);
		Assert.Equal(2.5f, opt.Value<float>());
	}

	[Fact]
	public void CreateFloatDynamicMaxOption_UpdatesMaxWhenCheckValueOptionChanges()
	{
		using var factory = new OptionCategoryFactory(
			"TestCategory",
			1003,
			(p, c) => { },
			(t, c) => { }
		);

		var checkOpt = factory.CreateFloatOption(TestOptionEnum.FirstOption, 10.0f, 1.0f, 20.0f, 1.0f);
		var dynOpt = factory.CreateFloatDynamicMaxOption(TestOptionEnum.SecondOption, 5.0f, 1.0f, 1.0f, checkOpt);

		checkOpt.Selection = 14;

		dynOpt.Selection = 14;
		Assert.Equal(15.0f, dynOpt.Value<float>());
	}

	[Fact]
	public void CreateIntOption_CreatesIntOptionWithRange()
	{
		using var factory = new OptionCategoryFactory(
			"TestCategory",
			1004,
			(p, c) => { },
			(t, c) => { }
		);

		var opt = factory.CreateIntOption(TestOptionEnum.FirstOption, 5, 0, 10, 1);

		Assert.NotNull(opt);
		Assert.Equal(5, opt.Value<int>());
	}

	[Fact]
	public void CreateIntDynamicMaxOption_UpdatesMaxWhenCheckValueOptionChanges()
	{
		using var factory = new OptionCategoryFactory(
			"TestCategory",
			1005,
			(p, c) => { },
			(t, c) => { }
		);

		var checkOpt = factory.CreateIntOption(TestOptionEnum.FirstOption, 10, 1, 20, 1);
		var dynOpt = factory.CreateIntDynamicMaxOption(TestOptionEnum.SecondOption, 5, 1, 1, checkOpt);

		checkOpt.Selection = 17;

		dynOpt.Selection = 17;
		Assert.Equal(18, dynOpt.Value<int>());
	}

	[Fact]
	public void CreateSelectionOption_WithArray_CreatesSelectionOption()
	{
		using var factory = new OptionCategoryFactory(
			"TestCategory",
			1006,
			(p, c) => { },
			(t, c) => { }
		);

		string[] selections = ["Opt1", "Opt2", "Opt3"];
		var opt = factory.CreateSelectionOption(TestOptionEnum.FirstOption, selections);

		Assert.NotNull(opt);
		Assert.Equal(0, opt.Selection);
		Assert.Equal("Opt1", opt.TransedValue);
	}

	[Fact]
	public void CreateSelectionOption_WithEnum_CreatesSelectionOption()
	{
		using var factory = new OptionCategoryFactory(
			"TestCategory",
			1007,
			(p, c) => { },
			(t, c) => { }
		);

		var opt = factory.CreateSelectionOption<TestOptionEnum, TestSelectionEnum>(TestOptionEnum.FirstOption);

		Assert.NotNull(opt);
		Assert.Equal(0, opt.Selection);
		Assert.Equal("ChoiceA", opt.TransedValue);
	}

	[Fact]
	public void CreateOption_WithCustomValueHolderAndActivator_RegistersChildWithParent()
	{
		IOption registeredParent = null!;
		IOption registeredChild = null!;

		using var factory = new OptionCategoryFactory(
			"TestCategory",
			1008,
			(parent, child) => { registeredParent = parent; registeredChild = child; },
			(tab, category) => { }
		);

		var parentOpt = factory.CreateBoolOption(TestOptionEnum.FirstOption, true);
		var activator = new ParentActive(parentOpt);

		var childHolder = ValueHolderAssembler.CreateBoolValue(false);
		var childOpt = factory.CreateOption(TestOptionEnum.SecondOption, childHolder, activator);

		Assert.Same(parentOpt, registeredParent);
		Assert.Same(childOpt, registeredChild);
	}

	[Fact]
	public void GetOptionId_WithIdOffset_AppliesOffset()
	{
		using var factory = new OptionCategoryFactory(
			"TestCategory",
			1009,
			(p, c) => { },
			(t, c) => { }
		)
		{
			IdOffset = 1000
		};

		int id = factory.GetOptionId(TestOptionEnum.FirstOption);
		Assert.Equal(1001, id);
	}

	[Fact]
	public void GetOptionName_CleansOptionPrefix()
	{
		using var factory = new OptionCategoryFactory(
			"Test|Name<Tag>\\n",
			1010,
			(p, c) => { },
			(t, c) => { }
		);

		string name = factory.GetOptionName(TestOptionEnum.FirstOption);
		Assert.Equal("TestNameFirstOption", name);
	}

	[Fact]
	public void Dispose_RegistersOptionCategory()
	{
		OptionTab registeredTab = OptionTab.GeneralTab;
		OptionCategory registeredCategory = null!;

		var factory = new OptionCategoryFactory(
			"MyCategory",
			1011,
			(p, c) => { },
			(tab, category) => { registeredTab = tab; registeredCategory = category; },
			OptionTab.CrewmateTab
		);

		factory.CreateBoolOption(TestOptionEnum.FirstOption, true);
		factory.Dispose();

		Assert.Equal(OptionTab.CrewmateTab, registeredTab);
		Assert.NotNull(registeredCategory);
		Assert.Equal("MyCategory", registeredCategory.Name);
	}
}
