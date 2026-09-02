using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomOption.Interfaces;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode;

public class LiberalSettingCheckTests
{
    [Fact]
    public void Parent_ReturnsNull()
    {
        var mockOption = new Mock<IOption>();
        var check = new LiberalSettingCheck(mockOption.Object, 1);

        Assert.Null(check.Parent);
    }

    [Theory]
    [InlineData(2, 1, true)]
    [InlineData(1, 1, true)]
    [InlineData(0, 1, false)]
    public void IsActive_ReturnsTrueWhenMaxOptionValueIsGreaterThanOrEqualToNum(int optionValue, int targetNum, bool expected)
    {
        var mockOption = new Mock<IOption>();
        mockOption.Setup(o => o.Value<int>()).Returns(optionValue);

        var check = new LiberalSettingCheck(mockOption.Object, targetNum);

        Assert.Equal(expected, check.IsActive);
    }
}
