using ExtremeRoles.UnitTest.Mocks;
using System.Collections.Generic;
using ExtremeRoles.Helper;
using Xunit;

namespace ExtremeRoles.UnitTest.Helper;

public class OptionSplitterTests : SerialTestBase, IClassFixture<SerialFixture>
{
    public OptionSplitterTests(SerialFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void IsValidOption_NullOptionId_ShouldReturnTrue()
    {
        bool isValid = OptionSplitter.IsValidOption(null, 42);
        Assert.True(isValid);
    }

    [Fact]
    public void IsValidOption_EmptyOptionId_ShouldReturnTrue()
    {
        var emptySet = new HashSet<int>();
        bool isValid = OptionSplitter.IsValidOption(emptySet, 42);
        Assert.True(isValid);
    }

    [Fact]
    public void IsValidOption_OptionIdContainsId_ShouldReturnTrue()
    {
        var set = new HashSet<int> { 10, 20, 30 };
        bool isValid = OptionSplitter.IsValidOption(set, 20);
        Assert.True(isValid);
    }

    [Fact]
    public void IsValidOption_OptionIdDoesNotContainId_ShouldReturnFalse()
    {
        var set = new HashSet<int> { 10, 20, 30 };
        bool isValid = OptionSplitter.IsValidOption(set, 99);
        Assert.False(isValid);
    }
}