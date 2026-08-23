using System;
using ExtremeRoles.Module;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module;

public class CustomRegionTests
{
    [Fact]
    public void RegionStatus_PropertiesAndIsUpdate_ReturnsExpectedValues()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var status = new RegionStatus(RegionStatusEnum.Ok, now);

        // Assert
        Assert.Equal(RegionStatusEnum.Ok, status.Status);
        Assert.Equal(now, status.Time);
        Assert.False(status.IsUpdate());
    }

    [Fact]
    public void RegionStatus_IsUpdate_ReturnsTrueWhenOlderThanOneHour()
    {
        // Arrange
        var oldTime = DateTime.UtcNow.AddHours(-2);
        var status = new RegionStatus(RegionStatusEnum.Ng, oldTime);

        // Assert
        Assert.True(status.IsUpdate());
    }

    [Fact]
    public void RegionStatus_IsUpdate_ReturnsFalseWhenNewerThanOneHour()
    {
        // Arrange
        var recentTime = DateTime.UtcNow.AddMinutes(-30);
        var status = new RegionStatus(RegionStatusEnum.MayBeOk, recentTime);

        // Assert
        Assert.False(status.IsUpdate());
    }

    [Fact]
    public void Region_StructProperties_AreAssignedCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var status = new RegionStatus(RegionStatusEnum.Ok, now);
        var mockInfo = new Mock<IRegionInfo>(IntPtr.Zero);

        // Act
        var region = new Region(status, mockInfo.Object);

        // Assert
        Assert.Same(status, region.Status);
        Assert.Same(mockInfo.Object, region.Info);
    }

    [Fact]
    public void TryGetStatus_WhenRegionNotFound_ReturnsFalseAndNone()
    {
        // Act
        var result = CustomRegion.TryGetStatus("NonExistentRegionName_12345", out var status);

        // Assert
        Assert.False(result);
        Assert.Equal(RegionStatusEnum.None, status);
    }

    [Theory]
    [InlineData(RegionStatusEnum.None)]
    [InlineData(RegionStatusEnum.Ng)]
    [InlineData(RegionStatusEnum.MayBeOk)]
    [InlineData(RegionStatusEnum.Ok)]
    public void RegionStatusEnum_AllValuesDefined(RegionStatusEnum value)
    {
        Assert.True(Enum.IsDefined(typeof(RegionStatusEnum), value));
    }
}
