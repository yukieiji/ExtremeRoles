using ExtremeRoles.Module.CustomOption.Implemented.Value;
using ExtremeRoles.Extension.Controller;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption.Implemented.Value;

[Collection("UnityMock")]
public class BoolOptionValueTests
{
    public BoolOptionValueTests()
    {

        var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
        mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
            .Returns((string id, string defaultStr, Il2CppReferenceArray<Il2CppSystem.Object> parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
        mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
            .Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
    }

    [Fact]
    public void DefaultIndex_WhenDefaultIsTrue_ReturnsOne()
    {
        // Arrange
        var optionValue = new BoolOptionValue(true);

        // Act
        int defaultIndex = optionValue.DefaultIndex;

        // Assert
        Assert.Equal(1, defaultIndex);
    }

    [Fact]
    public void DefaultIndex_WhenDefaultIsFalse_ReturnsZero()
    {
        // Arrange
        var optionValue = new BoolOptionValue(false);

        // Act
        int defaultIndex = optionValue.DefaultIndex;

        // Assert
        Assert.Equal(0, defaultIndex);
    }

    [Fact]
    public void Value_WhenSelectionIsZero_ReturnsFalse()
    {
        // Arrange
        var optionValue = new BoolOptionValue(true);

        // Act
        optionValue.Selection = 0;

        // Assert
        Assert.False(optionValue.Value);
        Assert.Equal(0, optionValue.Selection);
    }

    [Fact]
    public void Value_WhenSelectionIsOne_ReturnsTrue()
    {
        // Arrange
        var optionValue = new BoolOptionValue(false);

        // Act
        optionValue.Selection = 1;

        // Assert
        Assert.True(optionValue.Value);
        Assert.Equal(1, optionValue.Selection);
    }

    [Fact]
    public void StrValue_ReturnsTranslatedValue()
    {
        // Arrange
        var optionValue = new BoolOptionValue(true);

        // Act & Assert
        optionValue.Selection = 0;
        Assert.Equal("optionOff", optionValue.StrValue);

        optionValue.Selection = 1;
        Assert.Equal("optionOn", optionValue.StrValue);
    }

    [Fact]
    public void OnValueChanged_FiresWhenSelectionChanges()
    {
        // Arrange
        var optionValue = new BoolOptionValue(false);
        int callCount = 0;

        // Act
        optionValue.OnValueChanged += () => callCount++;

        // Assert: Event fires once immediately on subscription in OptionRange
        Assert.Equal(1, callCount);

        optionValue.Selection = 1;
        Assert.Equal(2, callCount);
    }
}
