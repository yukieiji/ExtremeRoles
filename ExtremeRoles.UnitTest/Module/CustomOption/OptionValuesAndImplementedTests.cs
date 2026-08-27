using System;
using System.Collections.Generic;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Implemented.Value;
using ExtremeRoles.Module.CustomOption.Interfaces;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption;

public enum TestEnum
{
    FirstOption,
    SecondOption,
}

[Collection("UnityMock")]
public class OptionValuesAndImplementedTests
{
    public OptionValuesAndImplementedTests()
    {
        MockSetupHelper.SetupCommonMocks();
    }

    [Fact]
    public void BoolOptionValue_ShouldReturnCorrectDefaultAndValue()
    {
        var boolValTrue = new BoolOptionValue(true);
        var boolValFalse = new BoolOptionValue(false);

        Assert.Equal(1, boolValTrue.DefaultIndex);
        Assert.Equal(0, boolValFalse.DefaultIndex);

        boolValTrue.Selection = 0;
        Assert.False(boolValTrue.Value);

        boolValTrue.Selection = 1;
        Assert.True(boolValTrue.Value);
    }

    [Fact]
    public void IntOptionValue_ShouldBehaveCorrectly()
    {
        var intVal = new IntOptionValue(10, 0, 20, 5);

        Assert.Equal(2, intVal.DefaultIndex);

        Assert.Equal(0, intVal.Value);
        intVal.Selection = 2;
        Assert.Equal(10, intVal.Value);
        Assert.Equal("10", intVal.StrValue);
        Assert.Equal("5", intVal.ToString());

        var newRange = OptionRange<int>.Create(0, 30, 10);
        intVal.InnerRange = newRange;
        Assert.Equal(1, intVal.Selection);
    }

    [Fact]
    public void FloatOptionValue_ShouldBehaveCorrectly()
    {
        var floatVal = new FloatOptionValue(1.5f, 1.0f, 2.0f, 0.5f);

        Assert.Equal(1, floatVal.DefaultIndex);

        Assert.Equal(1.0f, floatVal.Value);
        floatVal.Selection = 1;
        Assert.Equal(1.5f, floatVal.Value);
        Assert.Equal("1.5", floatVal.StrValue);
        Assert.Equal("3", floatVal.ToString());

        var newRange = OptionRange<float>.Create(1.0f, 3.0f, 1.0f);
        floatVal.InnerRange = newRange;
        Assert.Equal(0, floatVal.Selection);
    }

    [Fact]
    public void SelectionOptionValue_ShouldBehaveCorrectly()
    {
        var selectionVal = new SelectionOptionValue(new[] { "A", "B", "C" }, "B");

        Assert.Equal(1, selectionVal.DefaultIndex);
        Assert.Equal(0, selectionVal.Value);

        selectionVal.Selection = 2;
        Assert.Equal(2, selectionVal.Value);

        var enumVal = SelectionOptionValue.CreateFromEnum<TestEnum>();
        Assert.Equal(2, enumVal.Range);
    }

    [Fact]
    public void OptionRange_Operations_ShouldWorkCorrectly()
    {
        var range = new OptionRange<int>(new[] { 10, 20, 30 });
        int eventCallCount = 0;
        range.OnValueChanged += () => eventCallCount++;

        Assert.Equal(1, eventCallCount);

        range.Selection = 1;
        Assert.Equal(20, range.RangedValue);
        Assert.Equal(10, range.Min);
        Assert.Equal(30, range.Max);
        Assert.Equal(3, range.Range);
        Assert.Equal(2, eventCallCount);

        range.Selection = 5;
        Assert.Equal(30, range.RangedValue);

        Assert.Equal(1, range.GetIndex(20));
        Assert.Equal(0, range.GetIndex(999));

        Assert.NotNull(range.ToString());

        var enumStrings = new List<string>(OptionRange<string>.GetEnumString<TestEnum>());
        Assert.Contains("FirstOption", enumStrings);
        Assert.Contains("SecondOption", enumStrings);
    }

    [Fact]
    public void MetaData_Values_ShouldReflectTypes()
    {
        var intMeta = new MetaData<int>(new[] { 1, 2, 3 });
        Assert.Equal("Int32", intMeta.Type);
        Assert.Equal(3, intMeta.Values.Length);
        Assert.Equal(1, intMeta.Values[0]);

        var strMeta = new MetaData<string>(new[] { "hello" });
        Assert.Equal("String", strMeta.Type);
        Assert.Equal("hello", strMeta.Values[0]);
    }

    [Fact]
    public void DefaultOptionActivators_ShouldEvaluatedCorrectly()
    {
        var always = new AlwaysActive();
        Assert.Null(always.Parent);
        Assert.True(always.IsActive);

        var mockOption = new Mock<IOption>();
        mockOption.SetupGet(x => x.IsChangeDefault).Returns(true);
        mockOption.SetupGet(x => x.IsActive).Returns(true);

        var parentActive = new ParentActive(mockOption.Object);
        Assert.Same(mockOption.Object, parentActive.Parent);
        Assert.True(parentActive.IsActive);

        var invertActive = new InvertActive(mockOption.Object);
        Assert.Same(mockOption.Object, invertActive.Parent);
        Assert.False(invertActive.IsActive);

        var mockAct1 = new Mock<IOptionActivator>();
        var mockAct2 = new Mock<IOptionActivator>();
        mockAct1.SetupGet(x => x.Parent).Returns(mockOption.Object);
        mockAct1.SetupGet(x => x.IsActive).Returns(true);
        mockAct2.SetupGet(x => x.IsActive).Returns(false);

        var multiActive = new MultiActive(mockAct1.Object, mockAct2.Object);
        Assert.Same(mockOption.Object, multiActive.Parent);
        Assert.False(multiActive.IsActive);

        var orActive = new OrActive(mockAct1.Object, mockAct2.Object);
        Assert.Same(mockOption.Object, orActive.Parent);
        Assert.True(orActive.IsActive);
    }

    [Fact]
    public void OptionInfo_And_PresetOptionInfo_Properties_ShouldReturnExpectedValues()
    {
        var presetInfo = new PresetOptionInfo(1, "PresetName");
        Assert.Equal(1, presetInfo.Id);
        Assert.Equal("PresetName", presetInfo.Name);
        Assert.Equal("PresetName", presetInfo.CodeRemovedName);
        Assert.Equal(OptionUnit.Preset.ToString(), presetInfo.Format);
        Assert.False(presetInfo.IsHidden);

        var optInfo = new OptionInfo(10, "<color=red>TestName</color>", OptionUnit.Second, true);
        Assert.Equal(10, optInfo.Id);
        Assert.Equal("<color=red>TestName</color>", optInfo.Name);
        Assert.Equal("TestName", optInfo.CodeRemovedName);
        Assert.Equal(OptionUnit.Second.ToString(), optInfo.Format);
        Assert.True(optInfo.IsHidden);
        Assert.Contains("TestName", optInfo.ToString());
    }
}
