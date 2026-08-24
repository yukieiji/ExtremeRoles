using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Moq;
using TMPro;
using UnityEngine;
using Xunit;
using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

namespace ExtremeRoles.UnitTest.Module;

[Collection("UnityMock")]
public class PlayerReviverTests
{
    public PlayerReviverTests()
    {
        MockSetupHelper.SetupCommonMocks();
    }

    private static PlayerControl CreateMockPlayerControl(byte playerId = 1)
    {
        var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);
        return mockPlayer.Object;
    }

    private static TextMeshPro CreateMockTextMeshPro()
    {
        var mockText = new Mock<TextMeshPro>(IntPtr.Zero);
        var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
        mockGameObject.Setup(g => g.SetActive(It.IsAny<bool>()));
        mockText.SetupGet(t => t.gameObject).Returns(mockGameObject.Object);
        return mockText.Object;
    }

    private static object CreateReviveToken(
        float resurrectTime,
        TextMeshPro resurrectText,
        PlayerControl rolePlayer,
        Action<PlayerControl> onReviveCompleted,
        Action onDispose)
    {
        var nestedTypes = typeof(PlayerReviver).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public);
        Type? reviveTokenType = null;
        foreach (var type in nestedTypes)
        {
            if (type.Name.Contains("ReviveToken"))
            {
                reviveTokenType = type;
                break;
            }
        }

        Assert.NotNull(reviveTokenType);

        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        object token = Activator.CreateInstance(reviveTokenType, flags | BindingFlags.Public, null, new object?[] { resurrectTime, resurrectText, rolePlayer, onReviveCompleted, onDispose }, null)!;
        reviveTokenType.GetField("resurrectTimer", flags)?.SetValue(token, resurrectTime);
        reviveTokenType.GetField("maxTime", flags)?.SetValue(token, resurrectTime);
        reviveTokenType.GetField("resurrectText", flags)?.SetValue(token, resurrectText);
        reviveTokenType.GetField("rolePlayer", flags)?.SetValue(token, rolePlayer);
        reviveTokenType.GetField("onReviveCompleted", flags)?.SetValue(token, onReviveCompleted);
        reviveTokenType.GetField("onDispose", flags)?.SetValue(token, onDispose);

        return token;
    }

    [Fact]
    public void InitialState_IsRevivingIsFalse()
    {
        var reviver = new PlayerReviver(5.0f);
        Assert.False(reviver.IsReviving);
    }

    [Fact]
    public void Release_WhenReviving_SetsIsRevivingToFalse()
    {
        var reviver = new PlayerReviver(5.0f);
        var text = CreateMockTextMeshPro();
        var player = CreateMockPlayerControl();

        object token = CreateReviveToken(
            5.0f,
            text,
            player,
            (_) => { },
            () => reviver.Release());

        typeof(PlayerReviver)
            .GetField("token", BindingFlags.NonPublic | BindingFlags.Instance)?
            .SetValue(reviver, token);

        Assert.True(reviver.IsReviving);

        reviver.Release();

        Assert.False(reviver.IsReviving);
    }

    [Fact]
    public void Reset_ResetsResurrectTimerAndHidesText()
    {
        // Mock HudManager.Instance via uninitialized object and singleton helper
        var hudManager = (HudManager)RuntimeHelpers.GetUninitializedObject(typeof(HudManager));
        var mockSingleton = new Mock<MockDestroyableSingletonget_InstanceHelper<HudManager>>();
        mockSingleton.Setup(s => s.Invoke()).Returns(hudManager);
        var oldHudInstance = MockDestroyableSingletonget_InstanceHelper<HudManager>.Instance;
        MockDestroyableSingletonget_InstanceHelper<HudManager>.Instance = mockSingleton.Object;

        // Set OnemanMeetingSystemManager.IsActive to true
        var systemManager = (ExtremeSystemTypeManager)RuntimeHelpers.GetUninitializedObject(typeof(ExtremeSystemTypeManager));
        var allSystems = new System.Collections.Generic.Dictionary<ExtremeSystemType, ExtremeRoles.Module.Interface.IExtremeSystemType>();
        var onemanManager = (OnemanMeetingSystemManager)RuntimeHelpers.GetUninitializedObject(typeof(OnemanMeetingSystemManager));

        var mockMeeting = new Mock<IOnemanMeeting>();
        typeof(OnemanMeetingSystemManager)
            .GetField("meeting", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(onemanManager, mockMeeting.Object);

        allSystems[ExtremeSystemType.OnemanMeetingSystem] = onemanManager;

        typeof(ExtremeSystemTypeManager)
            .GetField("allSystems", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(systemManager, allSystems);

        var instanceField = typeof(ExtremeSystemTypeManager).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
        var oldInstance = instanceField?.GetValue(null);
        instanceField?.SetValue(null, systemManager);

        try
        {
            Assert.NotNull(HudManager.Instance);
            Assert.True(OnemanMeetingSystemManager.IsActive);

            var reviver = new PlayerReviver(5.0f);
            var textMock = new Mock<TextMeshPro>(IntPtr.Zero);
            var gameObjectMock = new Mock<GameObject>(IntPtr.Zero);

            textMock.SetupGet(t => t.gameObject).Returns(gameObjectMock.Object);

            var player = CreateMockPlayerControl();

            object token = CreateReviveToken(
                5.0f,
                textMock.Object,
                player,
                (_) => { },
                () => reviver.Release());

            var tokenType = token.GetType();
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            tokenType.GetField("resurrectTimer", flags)?.SetValue(token, 2.0f);

            var resetMethod = tokenType.GetMethod("Reset", flags | BindingFlags.Public);
            Assert.NotNull(resetMethod);

            resetMethod.Invoke(token, null);

            float currentTimer = (float)(tokenType.GetField("resurrectTimer", flags)?.GetValue(token) ?? 0f);
            Assert.Equal(5.0f, currentTimer);
            gameObjectMock.Verify(g => g.SetActive(false), Times.AtLeastOnce());
        }
        finally
        {
            instanceField?.SetValue(null, oldInstance);
            MockDestroyableSingletonget_InstanceHelper<HudManager>.Instance = oldHudInstance;
        }
    }

    [Fact]
    public void ReviveToken_ExecuteRevive_WhenNullRolePlayer_DoesNotThrow()
    {
        var textMock = new Mock<TextMeshPro>(IntPtr.Zero);
        var gameObjectMock = new Mock<GameObject>(IntPtr.Zero);
        textMock.SetupGet(t => t.gameObject).Returns(gameObjectMock.Object);

        bool callbackCalled = false;
        Action<PlayerControl> callback = (_) => { callbackCalled = true; };
        Action onDispose = () => { };

        object token = CreateReviveToken(
            0.0f,
            textMock.Object,
            null!,
            callback,
            onDispose);

        var executeReviveMethod = token.GetType().GetMethod("executeRevive", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(executeReviveMethod);

        executeReviveMethod.Invoke(token, null);
        Assert.False(callbackCalled);
    }
}
