using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Implemented.Value;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption.Implemented.Value;

[Collection("UnityMock")]
public class IntOptionValueTests
{
    public IntOptionValueTests()
    {

    }

    [Fact]
    public void Constructor_InitializesRangeAndDefaultIndexCorrectly()
    {
        // Arrange
        // min = 0, max = 50, step = 5 -> range [0, 5, 10, 15, ..., 50] (11 items)
        var optionValue = new IntOptionValue(10, 0, 50, 5);

        // Assert
        Assert.Equal(11, optionValue.Range);
        Assert.Equal("11", optionValue.ToString());
        Assert.Equal(2, optionValue.DefaultIndex);
        Assert.NotNull(optionValue.Meta);
    }

    [Fact]
    public void Selection_And_Value_And_StrValue_ReflectsChanges()
    {
        // Arrange
        var optionValue = new IntOptionValue(10, 0, 50, 5);

        // Act & Assert (initial selection is 0)
        Assert.Equal(0, optionValue.Selection);
        Assert.Equal(0, optionValue.Value);
        Assert.Equal("0", optionValue.StrValue);

        // Set selection to default index
        optionValue.Selection = optionValue.DefaultIndex;
        Assert.Equal(2, optionValue.Selection);
        Assert.Equal(10, optionValue.Value);
        Assert.Equal("10", optionValue.StrValue);
    }

    [Fact]
    public void InnerRange_Setter_TransfersValueAndEvents()
    {
        // Arrange
        var optionValue = new IntOptionValue(10, 0, 50, 5);
        optionValue.Selection = 2; // Value is 10
        Assert.Equal(10, optionValue.Value);

        int eventCallCount = 0;
        optionValue.OnValueChanged += () => eventCallCount++;
        // Initial subscription triggers 1 event
        Assert.Equal(1, eventCallCount);

        // Act: Replace InnerRange with a wider range [0 .. 100, step 5]
        var newRange = OptionRange<int>.Create(0, 100, 5);
        optionValue.InnerRange = newRange;

        // Setting InnerRange re-selects index in new range, firing OnValueChanged
        Assert.Equal(2, eventCallCount);

        // Assert: Value 10 is retained (index 2 in new range)
        Assert.Equal(10, optionValue.Value);
        Assert.Equal(21, optionValue.Range);

        // Verify events transferred to new InnerRange
        optionValue.Selection = 4; // Value 20
        Assert.Equal(3, eventCallCount);
    }

    [Fact]
    public void OnValueChanged_SubscribesAndUnsubscribesSuccessfully()
    {
        // Arrange
        var optionValue = new IntOptionValue(10, 0, 20, 5);
        int callCount = 0;
        System.Action listener = () => callCount++;

        // Act & Assert: Subscribe
        optionValue.OnValueChanged += listener;
        Assert.Equal(1, callCount); // Immediate invocation on subscribe in OptionRange

        optionValue.Selection = 1;
        Assert.Equal(2, callCount);

        // Unsubscribe
        optionValue.OnValueChanged -= listener;
        optionValue.Selection = 2;
        Assert.Equal(2, callCount);
    }
}
