using System;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented.Value;
using ExtremeRoles.Module.CustomOption.Interfaces;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption;

public enum TestCategoryKey
{
    OptionOne = 1,
    OptionTwo = 2,
    OptionThree = 3,
    OptionFour = 4,
    OptionFive = 5,
    OptionSix = 6,
    OptionSeven = 7,
    OptionEight = 8,
    OptionNine = 9,
}

[Collection("UnityMock")]
public class OptionFactoryTests
{
    public OptionFactoryTests()
    {
        MockSetupHelper.SetupCommonMocks();
    }

    [Fact]
    public void ValueHolderAssembler_CreateMethods_ShouldReturnExpectedHolders()
    {
        var boolVal = ValueHolderAssembler.CreateBoolValue(true);
        Assert.NotNull(boolVal);

        var floatVal = ValueHolderAssembler.CreateFloatValue(1.0f, 0f, 2f, 0.5f);
        Assert.Equal(2, floatVal.DefaultIndex);

        var intVal = ValueHolderAssembler.CreateIntValue(5, 0, 10, 1);
        Assert.Equal(5, intVal.DefaultIndex);

        var dynInt1 = ValueHolderAssembler.CreateDynamicIntValue(5, 0, 1);
        Assert.Equal(5, dynInt1.InnerRange.Max);

        var dynInt2 = ValueHolderAssembler.CreateDynamicIntValue(1, 0, 5);
        Assert.Equal(5, dynInt2.InnerRange.Max);

        var dynInt3 = ValueHolderAssembler.CreateDynamicIntValue(1, 0, 1, tempMax: 20);
        Assert.Equal(20, dynInt3.InnerRange.Max);

        var dynFloat1 = ValueHolderAssembler.CreateDynamicFloatValue(5.0f, 0f, 1.0f);
        Assert.Equal(5.0f, dynFloat1.InnerRange.Max);

        var dynFloat2 = ValueHolderAssembler.CreateDynamicFloatValue(1.0f, 0f, 5.0f);
        Assert.Equal(5.0f, dynFloat2.InnerRange.Max);

        var dynFloat3 = ValueHolderAssembler.CreateDynamicFloatValue(1.0f, 0f, 1.0f, tempMax: 20.0f);
        Assert.Equal(20.0f, dynFloat3.InnerRange.Max);
    }

    [Fact]
    public void OptionCategoryFactory_CreateAndAddOptions_ShouldWorkCorrectly()
    {
        IOption registeredChild = null!;
        IOption registeredParent = null!;
        OptionCategory registeredCategory = null!;

        Action<IOption, IOption> childRegister = (parent, child) =>
        {
            registeredParent = parent;
            registeredChild = child;
        };

        Action<OptionTab, OptionCategory> categoryRegister = (tab, cat) =>
        {
            registeredCategory = cat;
        };

        using (var factory = new OptionCategoryFactory(
            "TestCategory", 100, childRegister, categoryRegister, OptionTab.GeneralTab))
        {
            factory.IdOffset = 10;
            factory.OptionPrefix = "Prefix_";

            var boolOpt = factory.CreateBoolOption(TestCategoryKey.OptionOne, true);
            Assert.NotNull(boolOpt);
            Assert.Same(boolOpt, factory.Get(11));

            var floatOpt = factory.CreateFloatOption(TestCategoryKey.OptionTwo, 1.0f, 0f, 5.0f, 0.5f);
            Assert.NotNull(floatOpt);

            var intOpt = factory.CreateIntOption(TestCategoryKey.OptionThree, 5, 0, 10, 1);
            Assert.NotNull(intOpt);

            var floatDynOpt = factory.CreateFloatDynamicMaxOption(
                TestCategoryKey.OptionFour, 1.0f, 0f, 0.5f, checkValueOption: floatOpt, tempMaxValue: 10f);
            Assert.NotNull(floatDynOpt);

            floatOpt.Selection = 2;

            var intDynOpt = factory.CreateIntDynamicMaxOption(
                TestCategoryKey.OptionFive, 2, 0, 1, checkValueOption: intOpt, tempMaxValue: 10);
            Assert.NotNull(intDynOpt);

            intOpt.Selection = 3;

            var selectOpt = factory.CreateSelectionOption(TestCategoryKey.OptionSix, new[] { "A", "B" });
            Assert.NotNull(selectOpt);

            var selectEnumOpt = factory.CreateSelectionOption<TestCategoryKey, TestEnum>(TestCategoryKey.OptionSeven);
            Assert.NotNull(selectEnumOpt);

            var mockParent = new Mock<IOption>();
            var mockActivator = new Mock<IOptionActivator>();
            mockActivator.SetupGet(a => a.Parent).Returns(mockParent.Object);

            var customOpt = factory.CreateOption(TestCategoryKey.OptionEight, new BoolOptionValue(false), mockActivator.Object);
            Assert.Same(mockParent.Object, registeredParent);
            Assert.Same(customOpt, registeredChild);
        }

        Assert.NotNull(registeredCategory);
        Assert.Equal("TestCategory", registeredCategory.Name);
    }

    [Fact]
    public void SequentialOptionCategoryFactory_SequentialIdAssignment_ShouldWorkCorrectly()
    {
        OptionCategory registeredCategory = null!;

        using (var seqFactory = new SequentialOptionCategoryFactory(
            "SeqCategory", 200, (p, c) => { }, (t, cat) => registeredCategory = cat, OptionTab.GeneralTab))
        {
            Assert.Equal(0, seqFactory.StartId);

            var opt1 = seqFactory.CreateBoolOption("SeqOpt1", true);
            var opt2 = seqFactory.CreateIntOption("SeqOpt2", 10, 0, 20, 1);
            var floatHolder = ValueHolderAssembler.CreateFloatValue(2.0f, 0f, 5.0f, 0.5f);
            var floatOpt = seqFactory.CreateOption(2, "SeqOpt3", OptionUnit.None, false, floatHolder);
            typeof(SequentialOptionCategoryFactory).GetProperty("Offset")?.SetValue(seqFactory, 3);

            var opt4 = seqFactory.CreateSelectionOption<TestEnum>("SeqOpt4");
            var floatDyn = seqFactory.CreateFloatDynamicMaxOption("SeqOpt5", 1.0f, 0f, 0.5f, floatOpt);
            var intDyn = seqFactory.CreateIntDynamicMaxOption("SeqOpt6", 5, 0, 1, opt2);

            Assert.Equal(0, opt1.Info.Id);
            Assert.Equal(1, opt2.Info.Id);
            Assert.Equal(2, floatOpt.Info.Id);
            Assert.Equal(3, opt4.Info.Id);
            Assert.Equal(4, floatDyn.Info.Id);
            Assert.Equal(5, intDyn.Info.Id);
            Assert.Equal(5, seqFactory.EndId);
        }

        Assert.NotNull(registeredCategory);
    }

    [Fact]
    public void AutoParentSetOptionCategoryFactory_ShouldAutoSetParentActivator()
    {
        OptionCategoryFactory baseFactory = new OptionCategoryFactory(
            "AutoParentCat", 300, (p, c) => { }, (t, cat) => { }, OptionTab.GeneralTab);

        using (var autoParentFactory = new AutoParentSetOptionCategoryFactory(baseFactory))
        {
            autoParentFactory.IdOffset = 5;
            autoParentFactory.OptionPrefix = "AutoPref_";

            var firstOpt = autoParentFactory.CreateBoolOption(TestCategoryKey.OptionOne, true);
            Assert.NotNull(firstOpt);
            Assert.NotNull(autoParentFactory.Activator);
            Assert.Same(firstOpt, autoParentFactory.Activator.Parent);

            var secondOpt = autoParentFactory.CreateFloatOption(TestCategoryKey.OptionTwo, 1f, 0f, 2f, 0.5f);
            Assert.Same(firstOpt, secondOpt.Activator.Parent);

            var thirdOpt = autoParentFactory.CreateFloatDynamicMaxOption(TestCategoryKey.OptionThree, 1f, 0f, 0.5f, secondOpt);
            Assert.NotNull(thirdOpt);

            var intOpt = autoParentFactory.CreateIntOption(TestCategoryKey.OptionFour, 1, 0, 5, 1);
            Assert.NotNull(intOpt);

            var intDynOpt = autoParentFactory.CreateIntDynamicMaxOption(TestCategoryKey.OptionFive, 1, 0, 1, intOpt);
            Assert.NotNull(intDynOpt);

            var selOpt = autoParentFactory.CreateSelectionOption(TestCategoryKey.OptionSix, new[] { "1" });
            Assert.NotNull(selOpt);

            var selEnumOpt = autoParentFactory.CreateSelectionOption<TestCategoryKey, TestEnum>(TestCategoryKey.OptionSeven);
            Assert.NotNull(selEnumOpt);

            var custOpt = autoParentFactory.CreateOption(TestCategoryKey.OptionEight, new BoolOptionValue(false));
            Assert.NotNull(custOpt);

            var percOpt = autoParentFactory.Create0To100Percentage10StepOption(TestCategoryKey.OptionNine);
            Assert.NotNull(percOpt);

            Assert.Same(firstOpt, autoParentFactory.Get(6));
        }
    }

    [Fact]
    public void AutoActivatorSetFactory_DirectTest_ShouldWorkCorrectly()
    {
        var baseFactory = new OptionCategoryFactory(
            "AutoActCat", 400, (p, c) => { }, (t, cat) => { }, OptionTab.GeneralTab);

        using var factory = new AutoActivatorSetFactory(baseFactory);
        factory.IdOffset = 2;
        factory.OptionPrefix = "Prefix_";

        var mockActivator = new Mock<IOptionActivator>();
        factory.Activator = mockActivator.Object;

        var opt1 = factory.CreateBoolOption(TestCategoryKey.OptionOne, true);
        Assert.Same(mockActivator.Object, opt1.Activator);

        var opt2 = factory.CreateFloatOption(TestCategoryKey.OptionTwo, 1f, 0f, 2f, 0.5f);
        Assert.Same(mockActivator.Object, opt2.Activator);

        var opt3 = factory.CreateFloatDynamicMaxOption(TestCategoryKey.OptionThree, 1f, 0f, 0.5f, opt2);
        Assert.NotNull(opt3);

        var opt4 = factory.CreateIntOption(TestCategoryKey.OptionFour, 1, 0, 5, 1);
        Assert.NotNull(opt4);

        var opt5 = factory.CreateIntDynamicMaxOption(TestCategoryKey.OptionFive, 1, 0, 1, opt4);
        Assert.NotNull(opt5);

        var opt6 = factory.CreateSelectionOption(TestCategoryKey.OptionSix, new[] { "A" });
        Assert.NotNull(opt6);

        var opt7 = factory.CreateSelectionOption<TestCategoryKey, TestEnum>(TestCategoryKey.OptionSeven);
        Assert.NotNull(opt7);

        var opt8 = factory.CreateOption(TestCategoryKey.OptionEight, new BoolOptionValue(false));
        Assert.NotNull(opt8);

        var percOpt = factory.Create0To100Percentage10StepOption(TestCategoryKey.OptionNine);
        Assert.NotNull(percOpt);

        Assert.Same(opt1, factory.Get(3)); // 1 + 2 = 3
    }
}
