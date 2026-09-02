using System;
using System.Collections;
using System.Reflection;
using ExtremeRoles.Module;
using Moq;
using UnityEngine;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Module;



[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class FullScreenFlasherTests
{
    public FullScreenFlasherTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();

        var mockActionImplicit = new Mock<Il2CppSystem.MockActionop_ImplicitHelper<float>>();
        mockActionImplicit.Setup(x => x.Invoke(It.IsAny<Action<float>>()))
            .Returns((Action<float> act) => act != null ? new Il2CppSystem.Action<float>(IntPtr.Zero) : null!);
        Il2CppSystem.MockActionop_ImplicitHelper<float>.Instance = mockActionImplicit.Object;
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(-0.5f)]
    public void Constructor_InvalidFadeInTime_ThrowsArgumentOutOfRangeException(float fadeInTime)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FullScreenFlasher(Color.red, 1.0f, fadeInTime, 0.5f, 0.0f));
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(-0.5f)]
    public void Constructor_InvalidFadeOutTime_ThrowsArgumentOutOfRangeException(float fadeOutTime)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FullScreenFlasher(Color.red, 1.0f, 0.5f, fadeOutTime, 0.0f));
    }

    [Fact]
    public void Constructor_InvalidHoldTime_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FullScreenFlasher(Color.red, 1.0f, 0.5f, 0.5f, -0.1f));
    }

    [Fact]
    public void Constructor_ValidParameters_InitializesSuccessfully()
    {
        var flasher = new FullScreenFlasher(Color.blue, 0.8f, 0.5f, 0.5f, 0.2f);
        Assert.NotNull(flasher);
    }

    [Fact]
    public void Flash_WhenHudManagerInstanceIsNull_DoesNotThrow()
    {
        var mockSingleton = new Mock<MockDestroyableSingletonget_InstanceHelper<HudManager>>();
        mockSingleton.Setup(x => x.Invoke()).Returns((HudManager)null!);
        MockDestroyableSingletonget_InstanceHelper<HudManager>.Instance = mockSingleton.Object;

        var flasher = new FullScreenFlasher(Color.red);
        flasher.Flash();
    }

    [Fact]
    public void Flash_WhenHudManagerInstanceIsNotNull_CreatesRendererAndStartsCoroutine()
    {
        MockSetupHelper.SetupUnityCommonMocks();

        var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

        var mockTransform = new Mock<Transform>(IntPtr.Zero);
        var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
        var mockRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);

        mockRenderer.SetupGet(r => r.transform).Returns(mockTransform.Object);
        mockRenderer.SetupGet(r => r.gameObject).Returns(mockGameObject.Object);
        mockRenderer.SetupProperty(r => r.enabled);

        hudMock.SetupGet(h => h.FullScreen).Returns(mockRenderer.Object);
        hudMock.SetupGet(h => h.transform).Returns(mockTransform.Object);

        float passedDuration = 0f;
        var mockLerpHelper = new Mock<MockEffectsLerpHelper>();
        mockLerpHelper.Setup(x => x.Invoke(It.IsAny<float>(), It.IsAny<Il2CppSystem.Action<float>>()))
            .Callback<float, Il2CppSystem.Action<float>>((d, _) => passedDuration = d)
            .Returns((Il2CppSystem.Collections.IEnumerator)null!);
        MockEffectsLerpHelper.Instance = mockLerpHelper.Object;

        var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
        mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => mockRenderer.Object);
        MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

        var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
        mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => mockRenderer.Object);
        MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

        var flasher = new FullScreenFlasher(Color.red, 1.0f, 0.5f, 0.5f, 0.5f);

        flasher.Flash();

        mockGameObject.Verify(g => g.SetActive(true), Times.Once());
        Assert.True(mockRenderer.Object.enabled);
        Assert.Equal(1.5f, passedDuration);

        hudMock.Verify(h => h.StartCoroutine(It.IsAny<Il2CppSystem.Collections.IEnumerator>()), Times.Once());

        // Test Flash with override color
        flasher.Flash(Color.green);
        hudMock.Verify(h => h.StartCoroutine(It.IsAny<Il2CppSystem.Collections.IEnumerator>()), Times.Exactly(2));
    }

    [Fact]
    public void CreateLerpAction_ExecutesTimelinePhasesCorrectly()
    {
        MockSetupHelper.SetupUnityCommonMocks();

        var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

        var mockTransform = new Mock<Transform>(IntPtr.Zero);
        var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
        var mockRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);

        mockRenderer.SetupGet(r => r.transform).Returns(mockTransform.Object);
        mockRenderer.SetupGet(r => r.gameObject).Returns(mockGameObject.Object);
        mockRenderer.SetupProperty(r => r.enabled, true);

        hudMock.SetupGet(h => h.FullScreen).Returns(mockRenderer.Object);
        hudMock.SetupGet(h => h.transform).Returns(mockTransform.Object);

        var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
        mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => mockRenderer.Object);
        MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

        var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
        mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => mockRenderer.Object);
        MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

        var mockLerpHelper = new Mock<MockEffectsLerpHelper>();
        mockLerpHelper.Setup(x => x.Invoke(It.IsAny<float>(), It.IsAny<Il2CppSystem.Action<float>>()))
            .Returns((Il2CppSystem.Collections.IEnumerator)null!);
        MockEffectsLerpHelper.Instance = mockLerpHelper.Object;

        var flasher = new FullScreenFlasher(Color.red, 1.0f, 0.5f, 0.5f, 0.5f);
        flasher.Flash();

        var createLerpActionMethod = typeof(FullScreenFlasher)
            .GetMethod("createLerpAction", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(createLerpActionMethod);

        var action = createLerpActionMethod.Invoke(flasher, new object[] { Color.red }) as Action<float>;
        Assert.NotNull(action);

        // FadeIn phase
        action(0.2f);

        // Hold phase
        action(0.5f);

        // FadeOut phase
        action(0.8f);

        // End phase: disables renderer
        Assert.True(mockRenderer.Object.enabled);
        action(1.0f);
        Assert.False(mockRenderer.Object.enabled);
    }

    [Fact]
    public void Hide_WhenRendererIsNull_DoesNotThrow()
    {
        var flasher = new FullScreenFlasher(Color.red);
        flasher.Hide();
    }

    [Fact]
    public void Hide_WhenRendererIsNotNull_DisablesRenderer()
    {
        MockSetupHelper.SetupUnityCommonMocks();

        var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

        var mockTransform = new Mock<Transform>(IntPtr.Zero);
        var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
        var mockRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);

        mockRenderer.SetupGet(r => r.transform).Returns(mockTransform.Object);
        mockRenderer.SetupGet(r => r.gameObject).Returns(mockGameObject.Object);
        mockRenderer.SetupProperty(r => r.enabled);

        hudMock.SetupGet(h => h.FullScreen).Returns(mockRenderer.Object);
        hudMock.SetupGet(h => h.transform).Returns(mockTransform.Object);

        var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
        mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => mockRenderer.Object);
        MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

        var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
        mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => mockRenderer.Object);
        MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

        var mockLerpHelper = new Mock<MockEffectsLerpHelper>();
        mockLerpHelper.Setup(x => x.Invoke(It.IsAny<float>(), It.IsAny<Il2CppSystem.Action<float>>()))
            .Returns((Il2CppSystem.Collections.IEnumerator)null!);
        MockEffectsLerpHelper.Instance = mockLerpHelper.Object;

        var flasher = new FullScreenFlasher(Color.red);
        flasher.Flash();

        flasher.Hide();

        Assert.False(mockRenderer.Object.enabled);
    }

    [Fact]
    public void Reset_WhenRendererIsNull_DoesNotThrow()
    {
        var flasher = new FullScreenFlasher(Color.red);
        flasher.Reset();
    }

    [Fact]
    public void Reset_WhenRendererIsNotNull_DestroysGameObject()
    {
        MockSetupHelper.SetupUnityCommonMocks();

        var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

        var mockTransform = new Mock<Transform>(IntPtr.Zero);
        var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
        var mockRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);

        mockRenderer.SetupGet(r => r.transform).Returns(mockTransform.Object);
        mockRenderer.SetupGet(r => r.gameObject).Returns(mockGameObject.Object);

        hudMock.SetupGet(h => h.FullScreen).Returns(mockRenderer.Object);
        hudMock.SetupGet(h => h.transform).Returns(mockTransform.Object);

        var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
        mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => mockRenderer.Object);
        MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

        var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
        mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => mockRenderer.Object);
        MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

        var mockLerpHelper = new Mock<MockEffectsLerpHelper>();
        mockLerpHelper.Setup(x => x.Invoke(It.IsAny<float>(), It.IsAny<Il2CppSystem.Action<float>>()))
            .Returns((Il2CppSystem.Collections.IEnumerator)null!);
        MockEffectsLerpHelper.Instance = mockLerpHelper.Object;

        var mockDestroy = new Mock<MockObjectDestroyHelper2>();
        mockDestroy.Setup(d => d.Invoke(It.IsAny<UnityEngine.Object>()));
        MockObjectDestroyHelper2.Instance = mockDestroy.Object;

        var flasher = new FullScreenFlasher(Color.red);
        flasher.Flash();

        flasher.Reset();

        mockDestroy.Verify(d => d.Invoke(mockGameObject.Object), Times.Once());
    }

    [Fact]
    public void FullScreenRepeatFlasherWithAudio_Constructor_InitializesSuccessfully()
    {
        var flasher = new FullScreenRepeatFlasherWithAudio(null, Color.yellow, 1.5f);
        Assert.NotNull(flasher);
    }

    [Fact]
    public void FullScreenRepeatFlasherWithAudio_SetActive_WhenHudNull_DoesNothing()
    {
        var mockSingleton = new Mock<MockDestroyableSingletonget_InstanceHelper<HudManager>>();
        mockSingleton.Setup(x => x.Invoke()).Returns((HudManager)null!);
        MockDestroyableSingletonget_InstanceHelper<HudManager>.Instance = mockSingleton.Object;

        var flasher = new FullScreenRepeatFlasherWithAudio(null, Color.yellow);
        flasher.SetActive(true);
    }

    [Fact]
    public void FullScreenRepeatFlasherWithAudio_SetActive_False_StopsCoroutineAndDisablesFlush()
    {
        MockSetupHelper.SetupUnityCommonMocks();

        var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

        var mockTransform = new Mock<Transform>(IntPtr.Zero);
        var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
        var mockRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);

        mockRenderer.SetupGet(r => r.transform).Returns(mockTransform.Object);
        mockRenderer.SetupGet(r => r.gameObject).Returns(mockGameObject.Object);
        mockRenderer.SetupProperty(r => r.enabled);

        var mockCoroutine = new Mock<Coroutine>(IntPtr.Zero);

        var flasher = new FullScreenRepeatFlasherWithAudio(null, Color.yellow);

        // Set private fields coroutine and flush
        var coroutineField = typeof(FullScreenRepeatFlasherWithAudio)
            .GetField("coroutine", BindingFlags.NonPublic | BindingFlags.Instance);
        var flushField = typeof(FullScreenRepeatFlasherWithAudio)
            .GetField("flush", BindingFlags.NonPublic | BindingFlags.Instance);

        coroutineField?.SetValue(flasher, mockCoroutine.Object);
        flushField?.SetValue(flasher, mockRenderer.Object);

        flasher.SetActive(false);

        hudMock.Verify(h => h.StopCoroutine(mockCoroutine.Object), Times.Once());
        mockGameObject.Verify(g => g.SetActive(false), Times.Once());
        Assert.False(mockRenderer.Object.enabled);
        Assert.Null(coroutineField?.GetValue(flasher));
    }

    [Fact]
    public void FullScreenRepeatFlasherWithAudio_StartReactorFlush_ExecutesCoroutineSteps()
    {
        MockSetupHelper.SetupUnityCommonMocks();

        var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

        var mockSoundManager = new Mock<SoundManager>(IntPtr.Zero);
        var mockSoundInstanceHelper = new Mock<MockSoundManagerget_InstanceHelper>();
        mockSoundInstanceHelper.Setup(x => x.Invoke()).Returns(mockSoundManager.Object);
        MockSoundManagerget_InstanceHelper.Instance = mockSoundInstanceHelper.Object;

        var mockTransform = new Mock<Transform>(IntPtr.Zero);
        var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
        var mockRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);

        mockRenderer.SetupGet(r => r.transform).Returns(mockTransform.Object);
        mockRenderer.SetupGet(r => r.gameObject).Returns(mockGameObject.Object);
        mockRenderer.SetupProperty(r => r.enabled);
        mockRenderer.SetupProperty(r => r.color);

        hudMock.SetupGet(h => h.FullScreen).Returns(mockRenderer.Object);
        hudMock.SetupGet(h => h.transform).Returns(mockTransform.Object);

        var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
        mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => mockRenderer.Object);
        MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

        var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
        mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => mockRenderer.Object);
        MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

        var flasher = new FullScreenRepeatFlasherWithAudio(null, Color.yellow);

        var flushMethod = typeof(FullScreenRepeatFlasherWithAudio)
            .GetMethod("startReactorFlush", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(flushMethod);

        var coroutine = flushMethod.Invoke(flasher, null) as IEnumerator;
        Assert.NotNull(coroutine);

        // First iteration
        Assert.True(coroutine.MoveNext());
        mockGameObject.Verify(g => g.SetActive(true), Times.Once());
        mockSoundManager.Verify(s => s.PlaySound(It.IsAny<AudioClip>(), false, 1f, null), Times.Once());

        // Second iteration
        Assert.True(coroutine.MoveNext());
        mockSoundManager.Verify(s => s.PlaySound(It.IsAny<AudioClip>(), false, 1f, null), Times.Exactly(2));
    }
}
