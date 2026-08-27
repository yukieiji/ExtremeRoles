using System.Collections.Generic;
using ExtremeRoles.Extension.Controller;
using ExtremeRoles.Module.CustomOption.Implemented.Value;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption.Implemented.Value;

[Collection("UnityMock")]
public class SelectionOptionValueTests
{
    private enum TestEnum
    {
        Alpha,
        Beta,
        Gamma
    }

    public SelectionOptionValueTests()
    {
        MockSetupHelper.SetupCommonMocks();
        var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
        mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
            .Returns((string id, string defaultStr, Il2CppReferenceArray<Il2CppSystem.Object> parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
        mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
            .Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
    }

    [Fact]
    public void Constructor_Array_SetsDefaultIndexAndValueCorrectly()
    {
        // Arrange
        string[] range = ["Option1", "Option2", "Option3"];
        var optionValue = new SelectionOptionValue(range, "Option2");

        // Act & Assert
        Assert.Equal(1, optionValue.DefaultIndex);
        Assert.Equal(3, optionValue.Range);
        Assert.Equal(0, optionValue.Value);
        Assert.Equal("Option1", optionValue.StrValue);

        optionValue.Selection = 1;
        Assert.Equal(1, optionValue.Value);
        Assert.Equal("Option2", optionValue.StrValue);
    }

    [Fact]
    public void Constructor_IEnumerable_SetsDefaultIndexCorrectly()
    {
        // Arrange
        IEnumerable<string> range = new List<string> { "A", "B", "C", "D" };
        var optionValue = new SelectionOptionValue(range, "C");

        // Act & Assert
        Assert.Equal(2, optionValue.DefaultIndex);
        Assert.Equal(4, optionValue.Range);
    }

    [Fact]
    public void CreateFromEnum_InitializesFromEnumValues()
    {
        // Act
        var optionValue = SelectionOptionValue.CreateFromEnum<TestEnum>();

        // Assert
        Assert.Equal(3, optionValue.Range);
        Assert.Equal(0, optionValue.Value);
        Assert.Equal("Alpha", optionValue.StrValue);

        optionValue.Selection = 1;
        Assert.Equal(1, optionValue.Value);
        Assert.Equal("Beta", optionValue.StrValue);

        optionValue.Selection = 2;
        Assert.Equal(2, optionValue.Value);
        Assert.Equal("Gamma", optionValue.StrValue);
    }
}
