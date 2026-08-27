using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Implemented.Value;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption.Implemented.Value;

[Collection("UnityMock")]
public class FloatOptionValueTests
{
    public FloatOptionValueTests()
    {
        MockSetupHelper.SetupCommonMocks();
    }

    [Fact]
    public void Constructor_InitializesRangeAndDefaultIndexCorrectly()
    {
        // Arrange
        // min = 0.0, max = 5.0, step = 0.5 -> range [0.0, 0.5, 1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0, 4.5, 5.0] (11 items)
        var optionValue = new FloatOptionValue(1.5f, 0.0f, 5.0f, 0.5f);

        // Assert
        Assert.Equal(11, optionValue.Range);
        Assert.Equal("11", optionValue.ToString());
        Assert.Equal(3, optionValue.DefaultIndex);
        Assert.NotNull(optionValue.Meta);
    }

    [Fact]
    public void Selection_And_Value_And_StrValue_ReflectsChanges()
    {
        // Arrange
        var optionValue = new FloatOptionValue(1.5f, 0.0f, 5.0f, 0.5f);

        // Act & Assert (initial selection is 0)
        Assert.Equal(0, optionValue.Selection);
        Assert.Equal(0.0f, optionValue.Value);
        Assert.Equal("0", optionValue.StrValue);

        // Set selection to default index
        optionValue.Selection = optionValue.DefaultIndex;
        Assert.Equal(3, optionValue.Selection);
        Assert.Equal(1.5f, optionValue.Value);
        Assert.Equal("1.5", optionValue.StrValue);
    }

    [Fact]
    public void InnerRange_Setter_TransfersValueAndEvents()
    {
        // Arrange
        var optionValue = new FloatOptionValue(1.0f, 0.0f, 5.0f, 0.5f);
        optionValue.Selection = 2; // Value is 1.0f
        Assert.Equal(1.0f, optionValue.Value);

        int eventCallCount = 0;
        optionValue.OnValueChanged += () => eventCallCount++;
        // Initial subscription triggers 1 event
        Assert.Equal(1, eventCallCount);

        // Act: Replace InnerRange with a wider range [0.0 .. 10.0, step 0.5]
        var newRange = OptionRange<float>.Create(0.0f, 10.0f, 0.5f);
        optionValue.InnerRange = newRange;

        // Setting InnerRange re-selects index in new range, firing OnValueChanged
        Assert.Equal(2, eventCallCount);

        // Assert: Value 1.0f is retained (index 2 in new range)
        Assert.Equal(1.0f, optionValue.Value);
        Assert.Equal(21, optionValue.Range);

        // Verify events transferred to new InnerRange
        optionValue.Selection = 4; // Value 2.0f
        Assert.Equal(3, eventCallCount);
    }

    [Fact]
    public void OnValueChanged_SubscribesAndUnsubscribesSuccessfully()
    {
        // Arrange
        var optionValue = new FloatOptionValue(1.0f, 0.0f, 2.0f, 0.5f);
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
