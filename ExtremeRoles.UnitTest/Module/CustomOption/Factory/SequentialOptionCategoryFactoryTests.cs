using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption.Factory;

public class SequentialOptionCategoryFactoryTests
{
	public SequentialOptionCategoryFactoryTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupAmongUsClientMock();
		MockSetupHelper.SetupLobbyMock();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupLogger();
	}

	public enum TestSelectionEnum
	{
		Choice1,
		Choice2
	}

	[Fact]
	public void CreateOptions_IncrementsOffsetSequentially()
	{
		using var factory = new SequentialOptionCategoryFactory(
			"SeqCategory",
			2000,
			(p, c) => { },
			(t, c) => { }
		);

		Assert.Equal(0, factory.StartId);
		Assert.Equal(-1, factory.EndId);

		var mockFloatOpt = new Mock<IOption>();
		mockFloatOpt.Setup(o => o.Value<float>()).Returns(10.0f);

		var opt1 = factory.CreateBoolOption("BoolOpt", true);
		var opt2 = factory.CreateIntOption("IntOpt", 5, 0, 10, 1);
		var opt3 = factory.CreateFloatDynamicMaxOption("FloatDynOpt", 1.0f, 0.0f, 0.5f, mockFloatOpt.Object);
		var opt4 = factory.CreateIntDynamicMaxOption("IntDynOpt", 2, 0, 1, opt2);
		var opt5 = factory.CreateSelectionOption("SelectOpt", ["A", "B"]);
		var opt6 = factory.CreateSelectionOption<TestSelectionEnum>("EnumSelectOpt");

		Assert.Equal(0, opt1.Info.Id);
		Assert.Equal(1, opt2.Info.Id);
		Assert.Equal(2, opt3.Info.Id);
		Assert.Equal(3, opt4.Info.Id);
		Assert.Equal(4, opt5.Info.Id);
		Assert.Equal(5, opt6.Info.Id);

		Assert.Equal(0, factory.StartId);
		Assert.Equal(5, factory.EndId);
	}

	[Fact]
	public void CreateOptions_WithIdOffset_AppliesOffsetToIds()
	{
		using var factory = new SequentialOptionCategoryFactory(
			"SeqCategory",
			2001,
			(p, c) => { },
			(t, c) => { }
		)
		{
			IdOffset = 200
		};

		var opt1 = factory.CreateBoolOption("BoolOpt", false);
		var opt2 = factory.CreateIntOption("IntOpt", 10, 0, 20, 2);

		Assert.Equal(200, opt1.Info.Id);
		Assert.Equal(201, opt2.Info.Id);
	}

	[Fact]
	public void CreateOptions_FormatsNamesCorrectly()
	{
		using var factory = new SequentialOptionCategoryFactory(
			"SeqCategory",
			2002,
			(p, c) => { },
			(t, c) => { }
		);

		var optNormal = factory.CreateBoolOption("NormalOption", true);
		var optIgnorePrefix = factory.CreateBoolOption("IgnoredPrefixOption", true, ignorePrefix: true);

		Assert.Equal("SeqCategoryNormalOption", optNormal.Info.Name);
		Assert.Equal("|SeqCategory|IgnoredPrefixOption", optIgnorePrefix.Info.Name);
	}
}
