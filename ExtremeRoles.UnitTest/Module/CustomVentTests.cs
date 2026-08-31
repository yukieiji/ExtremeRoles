using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ExtremeRoles.Module;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module;

[Collection("UnityMock")]
public class CustomVentTests
{
    public CustomVentTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    private static Vent CreateMockVent(int id)
    {
        var mockVent = new Mock<Vent>(IntPtr.Zero);
        mockVent.SetupGet(v => v.Id).Returns(id);
        return mockVent.Object;
    }

    [Fact]
    public void CustomVent_InitialState_IsEmpty()
    {
        var customVent = new CustomVent();

        Assert.False(customVent.Contains(1));
        Assert.False(customVent.TryGet(CustomVent.Type.Mery, out var ventList));
        Assert.Null(ventList);
    }

    [Fact]
    public void Add_RegistersVentAndId()
    {
        var customVent = new CustomVent();
        var vent = CreateMockVent(10);

        customVent.Add(CustomVent.Type.Mery, vent);

        Assert.True(customVent.Contains(10));
        Assert.False(customVent.Contains(999));
        Assert.True(customVent.TryGet(CustomVent.Type.Mery, out var ventList));
        Assert.NotNull(ventList);
        Assert.Single(ventList);
        Assert.Same(vent, ventList[0]);
    }

    [Fact]
    public void Add_WithCustomSpriteSize_CreatesArrayOfSpecifiedSize()
    {
        var customVent = new CustomVent();
        var vent = CreateMockVent(5);

        customVent.Add(CustomVent.Type.Mery, vent, spriteSize: 10);

        var ventAnimField = typeof(CustomVent).GetField("ventAnimation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var ventAnimDict = ventAnimField?.GetValue(customVent) as Dictionary<CustomVent.Type, Sprite?[]>;

        Assert.NotNull(ventAnimDict);
        Assert.True(ventAnimDict.ContainsKey(CustomVent.Type.Mery));
        Assert.Equal(10, ventAnimDict[CustomVent.Type.Mery].Length);
    }

    [Fact]
    public void Add_MultipleVentsToSameType_AppendsToList()
    {
        var customVent = new CustomVent();
        var vent1 = CreateMockVent(1);
        var vent2 = CreateMockVent(2);

        customVent.Add(CustomVent.Type.Mery, vent1);
        customVent.Add(CustomVent.Type.Mery, vent2);

        Assert.True(customVent.Contains(1));
        Assert.True(customVent.Contains(2));
        Assert.True(customVent.TryGet(CustomVent.Type.Mery, out var ventList));
        Assert.NotNull(ventList);
        Assert.Equal(2, ventList.Count);
        Assert.Same(vent1, ventList[0]);
        Assert.Same(vent2, ventList[1]);
    }

    [Fact]
    public void TryGet_UnregisteredType_ReturnsFalse()
    {
        var customVent = new CustomVent();

        bool result = customVent.TryGet((CustomVent.Type)999, out var ventList);

        Assert.False(result);
        Assert.Null(ventList);
    }

    [Fact]
    public void GetSprite_UnregisteredVentId_ReturnsNull()
    {
        var customVent = new CustomVent();

        Sprite? sprite = customVent.GetSprite(1, 0);

        Assert.Null(sprite);
    }

    [Fact]
    public void GetSprite_InvalidCustomVentType_ReturnsNull()
    {
        var customVent = new CustomVent();
        var vent = CreateMockVent(5);

        var invalidType = (CustomVent.Type)999;
        customVent.Add(invalidType, vent);

        Assert.True(customVent.Contains(5));

        Sprite? sprite = customVent.GetSprite(5, 0);

        Assert.Null(sprite);
    }

    [Fact]
    public void GetSprite_WhenVentAnimationDictMissingType_ReturnsNull()
    {
        var customVent = new CustomVent();
        var vent = CreateMockVent(7);

        customVent.Add(CustomVent.Type.Mery, vent);

        // Remove type from ventAnimation dict manually to test missing animation array
        var ventAnimField = typeof(CustomVent).GetField("ventAnimation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var ventAnimDict = ventAnimField?.GetValue(customVent) as Dictionary<CustomVent.Type, Sprite?[]>;
        ventAnimDict?.Remove(CustomVent.Type.Mery);

        Sprite? sprite = customVent.GetSprite(7, 0);

        Assert.Null(sprite);
    }

    [Fact]
    public void GetSprite_WhenCachedSpriteExists_ReturnsCachedSprite()
    {
        var customVent = new CustomVent();
        var vent = CreateMockVent(42);

        customVent.Add(CustomVent.Type.Mery, vent);

        var dummySprite = new Mock<Sprite>(IntPtr.Zero).Object;
        var ventAnimField = typeof(CustomVent).GetField("ventAnimation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var ventAnimDict = ventAnimField?.GetValue(customVent) as Dictionary<CustomVent.Type, Sprite?[]>;
        Assert.NotNull(ventAnimDict);
        Assert.True(ventAnimDict.ContainsKey(CustomVent.Type.Mery));
        ventAnimDict[CustomVent.Type.Mery][0] = dummySprite;

        Sprite? sprite = customVent.GetSprite(42, 0);

        Assert.Same(dummySprite, sprite);
    }
}
