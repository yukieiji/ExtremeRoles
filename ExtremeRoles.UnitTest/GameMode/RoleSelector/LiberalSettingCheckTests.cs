using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomOption.Interfaces;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode.RoleSelector;

public class LiberalSettingCheckTests
{
    [Fact]
    public void Parent_ReturnsNull()
    {
        // Arrange
        var mockOption = new Mock<IOption>();
        var check = new LiberalSettingCheck(mockOption.Object, 2);

        // Act
        var parent = check.Parent;

        // Assert
        Assert.Null(parent);
    }

    [Theory]
    [InlineData(3, 2, true)]
    [InlineData(2, 2, true)]
    [InlineData(1, 2, false)]
    public void IsActive_ValueComparison_ReturnsExpected(int optionValue, int num, bool expected)
    {
        // Arrange
        var mockOption = new Mock<IOption>();
        mockOption.Setup(o => o.Value<int>()).Returns(optionValue);

        var check = new LiberalSettingCheck(mockOption.Object, num);

        // Act
        var isActive = check.IsActive;

        // Assert
        Assert.Equal(expected, isActive);
    }
}
