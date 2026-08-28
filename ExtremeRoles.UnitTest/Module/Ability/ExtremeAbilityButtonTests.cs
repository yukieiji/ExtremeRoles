using ExtremeRoles.UnitTest.Mocks;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Moq;
using Xunit;
using ExtremeRoles.Extension;
using ExtremeRoles.Extension.Manager;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;

namespace ExtremeRoles.UnitTest.Module.Ability;

public class ExtremeAbilityButtonTests : SerialTestBase, IClassFixture<SerialFixture>, IClassFixture<UnityCommonMock>
{
    public ExtremeAbilityButtonTests(SerialFixture fixture, UnityCommonMock unityCommonMock)
        : base(fixture, unityCommonMock.OperatorsMock, unityCommonMock.Vector2Mock, unityCommonMock.ColorMock, unityCommonMock.MathfMock, unityCommonMock.PaletteMock, unityCommonMock.GameOptionsManagerMock, unityCommonMock.CompatModManagerMock, unityCommonMock.TimeMock, new LoggerMock())
    {
        MockSetupHelper.SetupMockExtremeRolePlugin();
    }

    private sealed class TestBehavior : BehaviorBase
    {
        public bool IsUseResult { get; set; } = true;
        public bool ForceAbilityOffCalled { get; private set; }
        public bool AbilityOffCalled { get; private set; }
        public AbilityState NextStateOnUse { get; set; } = AbilityState.CoolDown;
        public bool TryUseResult { get; set; } = true;
        public AbilityState StateReturnedOnUpdate { get; set; } = AbilityState.CoolDown;

        public TestBehavior() : base("Test", null!)
        {
            SetCoolTime(10.0f);
        }

        public override void Initialize(ActionButton button) { }
        public override void ForceAbilityOff() => ForceAbilityOffCalled = true;
        public override void AbilityOff() => AbilityOffCalled = true;
        public override bool IsUse() => IsUseResult;
        public override AbilityState Update(AbilityState curState) => StateReturnedOnUpdate == AbilityState.CoolDown ? curState : StateReturnedOnUpdate;
        public override bool TryUseAbility(float timer, AbilityState curState, out AbilityState newState)
        {
            newState = NextStateOnUse;
            return TryUseResult;
        }
    }

    private sealed class TestChargingBehavior : BehaviorBase, IChargingBehavior
    {
        public float ChargeGage { get; set; }
        public float ChargeTime { get; set; } = 3.0f;
        public bool IsCharging { get; set; } = true;
        public bool ForceAbilityOffCalled { get; private set; }

        public TestChargingBehavior() : base("ChargingTest", null!)
        {
            SetCoolTime(10.0f);
        }

        public override void Initialize(ActionButton button) { }
        public override void ForceAbilityOff() => ForceAbilityOffCalled = true;
        public override void AbilityOff() { }
        public override bool IsUse() => true;
        public override AbilityState Update(AbilityState curState) => curState;
        public override bool TryUseAbility(float timer, AbilityState curState, out AbilityState newState)
        {
            newState = AbilityState.Charging;
            return true;
        }
    }

    private sealed class TestActivatingBehavior : BehaviorBase, IActivatingBehavior
    {
        public float ActiveTime { get; set; } = 5.0f;
        public bool CanAbilityActiving { get; set; } = true;
        public bool ForceAbilityOffCalled { get; private set; }
        public bool AbilityOffCalled { get; private set; }

        public TestActivatingBehavior() : base("ActivatingTest", null!)
        {
            SetCoolTime(10.0f);
        }

        public override void Initialize(ActionButton button) { }
        public override void ForceAbilityOff() => ForceAbilityOffCalled = true;
        public override void AbilityOff() => AbilityOffCalled = true;
        public override bool IsUse() => true;
        public override AbilityState Update(AbilityState curState) => curState;
        public override bool TryUseAbility(float timer, AbilityState curState, out AbilityState newState)
        {
            newState = AbilityState.Activating;
            return true;
        }
    }

    private sealed class TestReclickActivatingBehavior : BehaviorBase, IActivatingBehavior, IReclickBehavior
    {
        public float ActiveTime { get; set; } = 5.0f;
        public bool CanAbilityActiving { get; set; } = true;

        public TestReclickActivatingBehavior() : base("ReclickTest", null!)
        {
            SetCoolTime(10.0f);
        }

        public override void Initialize(ActionButton button) { }
        public override void ForceAbilityOff() { }
        public override void AbilityOff() { }
        public override bool IsUse() => true;
        public override AbilityState Update(AbilityState curState) => curState;
        public override bool TryUseAbility(float timer, AbilityState curState, out AbilityState newState)
        {
            newState = AbilityState.Activating;
            return true;
        }
    }

    private (ExtremeAbilityButton button, Mock<KillButton> mockKillButton, Mock<GameObject> mockGameObject, T behavior, Mock<IButtonAutoActivator> mockActivator) CreateTestButton<T>(
        T? behavior = null,
        Mock<IButtonAutoActivator>? mockActivator = null,
        KeyCode hotKey = KeyCode.F) where T : BehaviorBase
    {
        // Setup ExtremeSystemTypeManager instance safely via RuntimeHelpers
        var systemManager = (ExtremeSystemTypeManager)RuntimeHelpers.GetUninitializedObject(typeof(ExtremeSystemTypeManager));
        var allSystems = new Dictionary<ExtremeSystemType, IExtremeSystemType>();
        typeof(ExtremeSystemTypeManager)
            .GetField("allSystems", BindingFlags.NonPublic | BindingFlags.Instance)?
            .SetValue(systemManager, allSystems);
        typeof(ExtremeSystemTypeManager)
            .GetField("instance", BindingFlags.NonPublic | BindingFlags.Static)?
            .SetValue(null, systemManager);

        // Reset IntroCutscene.Instance helper to return null by default
        var mockIntroHelper = new Mock<MockIntroCutsceneget_InstanceHelper>();
        mockIntroHelper.Setup(x => x.Invoke()).Returns((IntroCutscene)null!);
        MockIntroCutsceneget_InstanceHelper.Instance = mockIntroHelper.Object;

        var mockGetKeyDown = new Mock<MockInputGetKeyDownHelper>();
        mockGetKeyDown.Setup(x => x.Invoke(It.IsAny<KeyCode>())).Returns(false);
        MockInputGetKeyDownHelper.Instance = mockGetKeyDown.Object;

        var mockGetKeyDownInt = new Mock<MockInputGetKeyDownIntHelper>();
        mockGetKeyDownInt.Setup(x => x.Invoke(It.IsAny<KeyCode>())).Returns(false);
        MockInputGetKeyDownIntHelper.Instance = mockGetKeyDownInt.Object;

        var mockObjectImplicitInt = new Mock<Il2CppSystem.MockObjectop_ImplicitHelper6>();
        mockObjectImplicitInt.Setup(x => x.Invoke(It.IsAny<int>())).Returns(new Mock<Il2CppSystem.Object>(IntPtr.Zero).Object);
        Il2CppSystem.MockObjectop_ImplicitHelper6.Instance = mockObjectImplicitInt.Object;

        var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
        mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("100%");

        var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

        var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
        mockGameObject.Setup(g => g.SetActive(It.IsAny<bool>()));

        var mockGridArrange = new Mock<GridArrange>(IntPtr.Zero);
        mockGridArrange.Setup(g => g.ArrangeChilds());

        var mockParentGameObject = new Mock<GameObject>(IntPtr.Zero);
        mockParentGameObject.Setup(g => g.GetComponent<GridArrange>()).Returns(mockGridArrange.Object);

        var mockParentTransform = new Mock<Transform>(IntPtr.Zero);
        mockParentTransform.Setup(t => t.gameObject).Returns(mockParentGameObject.Object);

        typeof(HudManagerExtension)
            .GetField("cachedArrange", BindingFlags.NonPublic | BindingFlags.Static)?
            .SetValue(null, mockGridArrange.Object);

        var mockTransform = new Mock<Transform>(IntPtr.Zero);
        mockTransform.Setup(t => t.parent).Returns(mockParentTransform.Object);
        mockTransform.Setup(t => t.FindChild(It.IsAny<string>())).Returns((Transform)null!);

        var mockMaterial = new Mock<Material>(IntPtr.Zero);
        mockMaterial.Setup(m => m.SetFloat(It.IsAny<string>(), It.IsAny<float>()));

        var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
        mockSpriteRenderer.SetupProperty(s => s.sprite);
        mockSpriteRenderer.SetupProperty(s => s.color);
        mockSpriteRenderer.SetupProperty(s => s.enabled);
        mockSpriteRenderer.SetupGet(s => s.material).Returns(mockMaterial.Object);

        var mockLabelText = new Mock<TextMeshPro>(IntPtr.Zero);
        mockLabelText.SetupProperty(t => t.color);
        mockLabelText.SetupProperty(t => t.fontMaterial);
        mockLabelText.SetupProperty(t => t.text);

        var mockCoolText = new Mock<TextMeshPro>(IntPtr.Zero);
        mockCoolText.SetupProperty(t => t.color);
        mockCoolText.SetupProperty(t => t.enableWordWrapping);
        mockCoolText.SetupProperty(t => t.text);
        mockCoolText.SetupGet(t => t.gameObject).Returns(mockGameObject.Object);

        var mockPersistentCallGroup = new Mock<PersistentCallGroup>(IntPtr.Zero);
        mockPersistentCallGroup.Setup(p => p.Clear());

        var mockOnClick = new Mock<UnityEngine.UI.Button.ButtonClickedEvent>(IntPtr.Zero);
        mockOnClick.Setup(e => e.RemoveAllListeners());
        mockOnClick.Setup(e => e.AddListener(It.IsAny<UnityAction>()));
        mockOnClick.SetupGet(e => e.m_PersistentCalls).Returns(mockPersistentCallGroup.Object);

        var mockPassiveButton = new Mock<PassiveButton>(IntPtr.Zero);
        mockPassiveButton.SetupGet(p => p.OnClick).Returns(mockOnClick.Object);

        var mockKillButton = new Mock<KillButton>(IntPtr.Zero);
        mockKillButton.SetupGet(b => b.transform).Returns(mockTransform.Object);
        mockKillButton.SetupGet(b => b.gameObject).Returns(mockGameObject.Object);
        mockKillButton.SetupGet(b => b.graphic).Returns(mockSpriteRenderer.Object);
        mockKillButton.SetupGet(b => b.buttonLabelText).Returns(mockLabelText.Object);
        mockKillButton.SetupGet(b => b.cooldownTimerText).Returns(mockCoolText.Object);
        mockKillButton.Setup(b => b.GetComponent<PassiveButton>()).Returns(mockPassiveButton.Object);
        mockKillButton.SetupGet(b => b.isActiveAndEnabled).Returns(true);
        mockKillButton.Setup(b => b.OverrideText(It.IsAny<string>()));
        mockKillButton.Setup(b => b.SetCoolDown(It.IsAny<float>(), It.IsAny<float>()));
        mockKillButton.Setup(b => b.SetCooldownFill(It.IsAny<float>()));

        var mockUseButton = new Mock<UseButton>(IntPtr.Zero);
        mockUseButton.SetupGet(b => b.buttonLabelText).Returns(mockLabelText.Object);
        mockUseButton.SetupGet(b => b.transform).Returns(mockTransform.Object);

        hudMock.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);
        hudMock.SetupGet(h => h.UseButton).Returns(mockUseButton.Object);

        var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
        mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => original);
        MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

        var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
        mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => original);
        MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

        var mockUnityActionImplicit = new Mock<MockUnityActionop_ImplicitHelper>();
        mockUnityActionImplicit.Setup(x => x.Invoke(It.IsAny<Action>()))
            .Returns((Action action) => action != null ? new UnityAction(IntPtr.Zero) : null!);
        MockUnityActionop_ImplicitHelper.Instance = mockUnityActionImplicit.Object;

        behavior ??= (T)(BehaviorBase)new TestBehavior();

        if (mockActivator == null)
        {
            mockActivator = new Mock<IButtonAutoActivator>();
            mockActivator.Setup(a => a.IsActive()).Returns(true);
        }

        var button = new ExtremeAbilityButton(behavior, mockActivator.Object, hotKey);
        return (button, mockKillButton, mockGameObject, behavior, mockActivator);
    }

    private static void SetPrivateState(ExtremeAbilityButton button, AbilityState state)
    {
        typeof(ExtremeAbilityButton)
            .GetProperty(nameof(ExtremeAbilityButton.State), BindingFlags.Public | BindingFlags.Instance)?
            .SetValue(button, state);
    }

    private static void SetPrivateTimer(ExtremeAbilityButton button, float timer)
    {
        typeof(ExtremeAbilityButton)
            .GetProperty(nameof(ExtremeAbilityButton.Timer), BindingFlags.Public | BindingFlags.Instance)?
            .SetValue(button, timer);
    }

    private static void InvokeOnClick(ExtremeAbilityButton button)
    {
        typeof(ExtremeAbilityButton)
            .GetMethod("onClick", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(button, null);
    }

    private static void InvokeAddTimerOffset(ExtremeAbilityButton button, float offset)
    {
        typeof(ExtremeAbilityButton)
            .GetMethod("AddTimerOffset", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(button, new object[] { offset });
    }

    [Fact]
    public void Constructor_InitializesStateToCoolDown()
    {
        var (button, mockKillButton, _, behavior, _) = CreateTestButton<TestBehavior>();

        Assert.Equal(AbilityState.CoolDown, button.State);
        Assert.Equal(10.0f, button.Timer);
        Assert.NotNull(button.Transform);
        Assert.Same(behavior, button.Behavior);
    }

    [Fact]
    public void OnMeetingStart_ForcesAbilityOff_And_HidesButton()
    {
        var (button, mockKillButton, mockGameObject, behavior, _) = CreateTestButton<TestBehavior>();

        button.OnMeetingStart();

        Assert.True(behavior.ForceAbilityOffCalled);
        mockGameObject.Verify(g => g.SetActive(false), Times.AtLeastOnce());
    }

    [Fact]
    public void OnMeetingEnd_ResetsStatusToCoolDown_And_ShowsButton()
    {
        var (button, mockKillButton, mockGameObject, behavior, _) = CreateTestButton<TestBehavior>();

        button.OnMeetingEnd();

        Assert.Equal(AbilityState.CoolDown, button.State);
        Assert.Equal(behavior.CoolTime, button.Timer);
        mockGameObject.Verify(g => g.SetActive(true), Times.AtLeastOnce());
    }

    [Fact]
    public void SetButtonShow_TogglesVisibility()
    {
        var (button, mockKillButton, mockGameObject, _, _) = CreateTestButton<TestBehavior>();

        button.SetButtonShow(false);
        mockGameObject.Verify(g => g.SetActive(false), Times.AtLeastOnce());

        button.SetButtonShow(true);
        mockGameObject.Verify(g => g.SetActive(true), Times.AtLeastOnce());
    }

    [Fact]
    public void SetLabelToCrewmate_UpdatesFontMaterial()
    {
        var (button, mockKillButton, _, _, _) = CreateTestButton<TestBehavior>();

        var mockDestroy = new Mock<MockObjectDestroyHelper2>();
        mockDestroy.Setup(d => d.Invoke(It.IsAny<UnityEngine.Object>()));
        MockObjectDestroyHelper2.Instance = mockDestroy.Object;

        button.SetLabelToCrewmate();

        mockDestroy.Verify(d => d.Invoke(It.IsAny<UnityEngine.Object>()), Times.Once());
    }

    [Fact]
    public void Update_WhenIntroCutsceneActive_DoesNothing()
    {
        var (button, mockKillButton, _, behavior, _) = CreateTestButton<TestBehavior>();

        var mockIntroCutscene = new Mock<IntroCutscene>(IntPtr.Zero);
        var mockIntroHelper = new Mock<MockIntroCutsceneget_InstanceHelper>();
        mockIntroHelper.Setup(x => x.Invoke()).Returns(mockIntroCutscene.Object);
        MockIntroCutsceneget_InstanceHelper.Instance = mockIntroHelper.Object;

        SetPrivateState(button, AbilityState.CoolDown);
        SetPrivateTimer(button, 5.0f);

        button.Update();

        Assert.Equal(5.0f, button.Timer);
    }

    [Fact]
    public void Update_WhenButtonLocked_ExecutesBlockedUpdate()
    {
        var (button, mockKillButton, _, behavior, _) = CreateTestButton<TestBehavior>();

        var systemManager = ExtremeSystemTypeManager.Instance;
        var allSystems = (Dictionary<ExtremeSystemType, IExtremeSystemType>)typeof(ExtremeSystemTypeManager)
            .GetField("allSystems", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(systemManager)!;

        var lockSystem = new ButtonLockSystem(ExtremeSystemType.AbilityButtonLockSystem);
        allSystems[ExtremeSystemType.AbilityButtonLockSystem] = lockSystem;

        lockSystem.Lock(1);

        SetPrivateState(button, AbilityState.Activating);

        button.Update();

        Assert.True(behavior.ForceAbilityOffCalled);
        Assert.Equal(AbilityState.Ready, button.State);

        lockSystem.UnLock(1);
    }

    [Fact]
    public void Update_CoolDownState_DecrementsTimer_And_TransitionsToReadyWhenExpired()
    {
        var (button, mockKillButton, _, behavior, _) = CreateTestButton<TestBehavior>();

        SetPrivateState(button, AbilityState.CoolDown);
        SetPrivateTimer(button, -0.5f);

        button.Update();

        Assert.Equal(AbilityState.Ready, button.State);
    }

    [Fact]
    public void Update_ReadyState_SetsTimerToZero()
    {
        var (button, mockKillButton, _, behavior, _) = CreateTestButton<TestBehavior>();

        SetPrivateState(button, AbilityState.Ready);
        SetPrivateTimer(button, 5.0f);

        button.Update();

        Assert.Equal(0.0f, button.Timer);
    }

    [Fact]
    public void Update_ChargingState_ThrowsArgException_WhenBehaviorNotIChargingBehavior()
    {
        var (button, mockKillButton, _, behavior, _) = CreateTestButton<TestBehavior>();

        SetPrivateState(button, AbilityState.Charging);

        Assert.Throws<ArgumentException>(() => button.Update());
    }

    [Fact]
    public void Update_ChargingState_NormalAndFailureScenarios()
    {
        var chargingBehavior = new TestChargingBehavior();
        var (button, mockKillButton, _, _, _) = CreateTestButton(behavior: chargingBehavior);

        SetPrivateState(button, AbilityState.Charging);
        SetPrivateTimer(button, 1.5f);

        button.Update();

        Assert.Equal(0.5f, chargingBehavior.ChargeGage);

        chargingBehavior.IsCharging = false;
        button.Update();

        Assert.Equal(0.0f, chargingBehavior.ChargeGage);
        Assert.True(chargingBehavior.ForceAbilityOffCalled);
        Assert.Equal(AbilityState.Ready, button.State);

        SetPrivateState(button, AbilityState.Charging);
        SetPrivateTimer(button, chargingBehavior.ChargeTime + 3.0f);
        chargingBehavior.IsCharging = true;

        button.Update();

        Assert.Equal(AbilityState.Charging, button.State);
    }

    [Fact]
    public void Update_ActivatingState_ThrowsArgException_WhenBehaviorNotIActivatingBehavior()
    {
        var (button, mockKillButton, _, behavior, _) = CreateTestButton<TestBehavior>();

        SetPrivateState(button, AbilityState.Activating);

        Assert.Throws<ArgumentException>(() => button.Update());
    }

    [Fact]
    public void Update_ActivatingState_NormalAndExpirationScenarios()
    {
        var activatingBehavior = new TestActivatingBehavior();
        var (button, mockKillButton, _, _, _) = CreateTestButton(behavior: activatingBehavior);

        SetPrivateState(button, AbilityState.Activating);
        SetPrivateTimer(button, 2.0f);

        button.Update();
        Assert.Equal(AbilityState.Activating, button.State);

        activatingBehavior.CanAbilityActiving = false;
        button.Update();

        Assert.True(activatingBehavior.ForceAbilityOffCalled);
        Assert.Equal(AbilityState.Ready, button.State);

        activatingBehavior.CanAbilityActiving = true;
        SetPrivateState(button, AbilityState.Activating);
        SetPrivateTimer(button, -0.1f);

        button.Update();

        Assert.True(activatingBehavior.AbilityOffCalled);
        Assert.Equal(AbilityState.CoolDown, button.State);
    }

    [Fact]
    public void OnClick_ExecutesTryUseAbility_And_HandlesCoolDownTransition()
    {
        var (button, mockKillButton, _, behavior, _) = CreateTestButton<TestBehavior>();
        behavior.IsUseResult = true;
        behavior.NextStateOnUse = AbilityState.CoolDown;

        InvokeOnClick(button);

        Assert.True(behavior.AbilityOffCalled);
        Assert.Equal(AbilityState.CoolDown, button.State);
    }

    [Fact]
    public void AddTimerOffset_ModifiesTimer()
    {
        var (button, mockKillButton, _, behavior, _) = CreateTestButton<TestBehavior>();

        SetPrivateState(button, AbilityState.Ready);
        InvokeAddTimerOffset(button, 3.0f);

        Assert.Equal(AbilityState.CoolDown, button.State);
        Assert.Equal(3.0f, button.Timer);

        InvokeAddTimerOffset(button, 2.0f);
        Assert.Equal(5.0f, button.Timer);
    }
}