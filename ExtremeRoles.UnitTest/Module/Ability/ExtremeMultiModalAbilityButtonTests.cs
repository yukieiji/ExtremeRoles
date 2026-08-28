using ExtremeRoles.UnitTest.Mocks;
using System;
using System.Collections.Generic;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.Interface;
using Moq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Ability;

public class ExtremeMultiModalAbilityButtonTests : SerialTestBase, IClassFixture<UnityCommonMock>
{
    private Mock<IButtonAutoActivator> mockActivator;
    private Mock<IGameObjectFactory> mockGOFactory = null!;
    private Mock<ISpriteLoader> mockSpriteLoader = null!;
    private TestBehavior behavior1;
    private TestBehavior behavior2;
    private TestBehavior behavior3;
    private Mock<GameObject> mockMultiAbilityGO = null!;

    public ExtremeMultiModalAbilityButtonTests(SerialFixture fixture, UnityCommonMock unityCommonMock)
        : base(fixture, unityCommonMock.OperatorsMock, unityCommonMock.Vector2Mock, unityCommonMock.ColorMock, unityCommonMock.MathfMock, unityCommonMock.TimeMock, new PaletteMock(), new GameOptionsManagerMock(), new CompatModManagerMock())
    {

        var mockVectorOne = new Mock<MockVector3get_oneHelper>();
        mockVectorOne.Setup(x => x.Invoke()).Returns(new Vector3(1f, 1f, 1f));
        MockVector3get_oneHelper.Instance = mockVectorOne.Object;

        var mockVectorMultiply = new Mock<MockVector3op_MultiplyHelper>();
        mockVectorMultiply.Setup(x => x.Invoke(It.IsAny<Vector3>(), It.IsAny<float>()))
            .Returns((Vector3 v, float f) => new Vector3(v.x * f, v.y * f, v.z * f));
        MockVector3op_MultiplyHelper.Instance = mockVectorMultiply.Object;

        var mockVectorAdd = new Mock<MockVector3op_AdditionHelper>();
        mockVectorAdd.Setup(x => x.Invoke(It.IsAny<Vector3>(), It.IsAny<Vector3>()))
            .Returns((Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z));
        MockVector3op_AdditionHelper.Instance = mockVectorAdd.Object;

        mockActivator = new Mock<IButtonAutoActivator>();
        mockActivator.Setup(a => a.IsActive()).Returns(true);

        behavior1 = new TestBehavior(10.0f);
        behavior2 = new TestBehavior(15.0f);
        behavior3 = new TestBehavior(5.0f);

        setupMocksForButtons();
    }

    private void setupMocksForButtons()
    {
        var mockHudManager = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

        var mockKillButton = new Mock<KillButton>(IntPtr.Zero);
        var mockPassiveButton = new Mock<PassiveButton>(IntPtr.Zero);
        var mockGraphic = new Mock<SpriteRenderer>(IntPtr.Zero);
        var mockText = new Mock<TextMeshPro>(IntPtr.Zero);
        var mockTextGO = new Mock<GameObject>(IntPtr.Zero);
        var mockTextTransform = new Mock<Transform>(IntPtr.Zero);

        mockTextGO.SetupGet(g => g.transform).Returns(mockTextTransform.Object);
        mockText.SetupGet(t => t.gameObject).Returns(mockTextGO.Object);
        mockText.SetupGet(t => t.transform).Returns(mockTextTransform.Object);

        var mockMaterial = new Mock<Material>(IntPtr.Zero);
        var mockTransform = new Mock<Transform>(IntPtr.Zero);
        var mockParentTransform = new Mock<Transform>(IntPtr.Zero);
        var mockButtonGO = new Mock<GameObject>(IntPtr.Zero);

        var mockOnClick = new Mock<Button.ButtonClickedEvent>(IntPtr.Zero);
        var mockPersistentCalls = new Mock<PersistentCallGroup>(IntPtr.Zero);
        mockOnClick.SetupGet(c => c.m_PersistentCalls).Returns(mockPersistentCalls.Object);

        var mockActionHelper = new Mock<MockUnityActionop_ImplicitHelper>();
        mockActionHelper.Setup(x => x.Invoke(It.IsAny<Action>())).Returns(new UnityAction(IntPtr.Zero));
        MockUnityActionop_ImplicitHelper.Instance = mockActionHelper.Object;

        var mockGridArrangeComp = new Mock<GridArrange>(IntPtr.Zero);
        mockGridArrangeComp.Setup(g => g.ArrangeChilds());

        mockMaterial.Setup(m => m.SetFloat(It.IsAny<string>(), It.IsAny<float>()));
        mockText.SetupGet(t => t.fontMaterial).Returns(mockMaterial.Object);
        mockGraphic.SetupGet(g => g.material).Returns(mockMaterial.Object);

        mockButtonGO.SetupGet(g => g.transform).Returns(mockTransform.Object);
        mockButtonGO.Setup(g => g.GetComponent<GridArrange>()).Returns(mockGridArrangeComp.Object);

        mockParentTransform.SetupGet(p => p.gameObject).Returns(mockButtonGO.Object);

        mockKillButton.SetupGet(b => b.gameObject).Returns(mockButtonGO.Object);
        mockKillButton.SetupGet(b => b.transform).Returns(mockTransform.Object);
        mockKillButton.SetupGet(b => b.graphic).Returns(mockGraphic.Object);
        mockKillButton.SetupGet(b => b.buttonLabelText).Returns(mockText.Object);
        mockKillButton.SetupGet(b => b.cooldownTimerText).Returns(mockText.Object);
        mockKillButton.Setup(b => b.GetComponent<PassiveButton>()).Returns(mockPassiveButton.Object);

        mockPassiveButton.SetupGet(p => p.OnClick).Returns(mockOnClick.Object);
        mockTransform.SetupGet(t => t.parent).Returns(mockParentTransform.Object);
        mockTransform.Setup(t => t.FindChild(It.IsAny<string>())).Returns((Transform)null!);

        var mockUseButton = new Mock<UseButton>(IntPtr.Zero);
        mockUseButton.SetupGet(b => b.buttonLabelText).Returns(mockText.Object);
        mockUseButton.SetupGet(b => b.transform).Returns(mockTransform.Object);

        mockHudManager.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);
        mockHudManager.SetupGet(h => h.UseButton).Returns(mockUseButton.Object);

        var mockInstHelper5 = new Mock<MockObjectInstantiateHelper5>();
        mockInstHelper5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object orig, Transform parent) =>
            {
                if (orig is KillButton)
                {
                    return mockKillButton.Object;
                }
                if (orig is TextMeshPro)
                {
                    return mockText.Object;
                }
                return orig;
            });
        MockObjectInstantiateHelper5.Instance = mockInstHelper5.Object;

        var mockInstHelper10 = new Mock<MockObjectInstantiateHelper10>();
        mockInstHelper10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object orig, Transform parent) =>
            {
                if (orig is KillButton)
                {
                    return mockKillButton.Object;
                }
                if (orig is TextMeshPro)
                {
                    return mockText.Object;
                }
                return orig;
            });
        MockObjectInstantiateHelper10.Instance = mockInstHelper10.Object;

        var mockInstHelper7 = new Mock<MockObjectInstantiateHelper7>();
        mockInstHelper7.Setup(x => x.Invoke(It.IsAny<Material>()))
            .Returns(mockMaterial.Object);
        MockObjectInstantiateHelper7.Instance = mockInstHelper7.Object;

        var mockInstHelper = new Mock<MockObjectInstantiateHelper>();
        mockInstHelper.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Vector3>(), It.IsAny<Quaternion>()))
            .Returns(mockText.Object);
        MockObjectInstantiateHelper.Instance = mockInstHelper.Object;

        var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
        mockTranslation.Setup(t => t.GetString(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object>>()))
            .Returns("100%");
        mockTranslation.Setup(t => t.GetString(
            It.IsAny<StringNames>(),
            It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object>>()))
            .Returns("100%");

        mockMultiAbilityGO = new Mock<GameObject>(IntPtr.Zero);
        var mockMultiTransform = new Mock<Transform>(IntPtr.Zero);
        var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
        mockMultiAbilityGO.SetupGet(g => g.transform).Returns(mockMultiTransform.Object);
        mockMultiAbilityGO.Setup(g => g.AddComponent<SpriteRenderer>()).Returns(mockSpriteRenderer.Object);

        mockGOFactory = new Mock<IGameObjectFactory>();
        mockGOFactory.Setup(f => f.Create(It.IsAny<string>())).Returns(mockMultiAbilityGO.Object);

        var mockSprite = new Mock<Sprite>(IntPtr.Zero);
        mockSpriteLoader = new Mock<ISpriteLoader>();
        mockSpriteLoader.Setup(s => s.LoadSprite(It.IsAny<string>(), It.IsAny<string>())).Returns(mockSprite.Object);
    }

    [Fact]
    public void Add_AddsBehaviorAndUpdatesCount()
    {
        var behaviors = new List<BehaviorBase>
        {
            behavior1
        };

        var button = new ExtremeMultiModalAbilityButton(behaviors, mockActivator.Object, KeyCode.F, mockGOFactory.Object, mockSpriteLoader.Object);
        Assert.Equal(1, button.MultiModalAbilityNum);

        button.Add(behavior2);

        Assert.Equal(2, button.MultiModalAbilityNum);
        Assert.True(behavior2.UpdateCalled);
    }

    [Fact]
    public void Remove_ByIndex_RemovesBehavior()
    {
        var behaviors = new List<BehaviorBase>
        {
            behavior1,
            behavior2
        };

        var button = new ExtremeMultiModalAbilityButton(behaviors, mockActivator.Object, KeyCode.F, mockGOFactory.Object, mockSpriteLoader.Object);
        button.Remove(1);

        Assert.Equal(1, button.MultiModalAbilityNum);
    }

    [Fact]
    public void Remove_CurrentBehavior_SwitchesToNextAbility()
    {
        var behaviors = new List<BehaviorBase>
        {
            behavior1,
            behavior2
        };

        var button = new ExtremeMultiModalAbilityButton(behaviors, mockActivator.Object, KeyCode.F, mockGOFactory.Object, mockSpriteLoader.Object);
        Assert.Same(behavior1, button.Behavior);

        button.Remove(behavior1);

        Assert.Equal(1, button.MultiModalAbilityNum);
        Assert.Same(behavior2, button.Behavior);
    }

    [Fact]
    public void Remove_OnlyAbility_ThrowsIndexOutOfRangeException()
    {
        var behaviors = new List<BehaviorBase>
        {
            behavior1
        };

        var button = new ExtremeMultiModalAbilityButton(behaviors, mockActivator.Object, KeyCode.F, mockGOFactory.Object, mockSpriteLoader.Object);

        Assert.Throws<IndexOutOfRangeException>(() => button.Remove(0));
    }

    [Fact]
    public void ClearAndAnd_RemovesAllExceptCurrentAndAddsNewBehavior()
    {
        var behaviors = new List<BehaviorBase>
        {
            behavior1,
            behavior2
        };

        var button = new ExtremeMultiModalAbilityButton(behaviors, mockActivator.Object, KeyCode.F, mockGOFactory.Object, mockSpriteLoader.Object);
        button.ClearAndAnd(behavior3);

        Assert.Equal(1, button.MultiModalAbilityNum);
        Assert.Same(behavior3, button.Behavior);
    }

    [Fact]
    public void SwitchAbility_TriggersHideAndShowOnIHideLogic()
    {
        var b1 = new HideShowTestBehavior();
        var b2 = new HideShowTestBehavior();

        var behaviors = new List<BehaviorBase>
        {
            b1,
            b2
        };

        var button = new ExtremeMultiModalAbilityButton(behaviors, mockActivator.Object, KeyCode.F, mockGOFactory.Object, mockSpriteLoader.Object);

        button.Remove(b1); // Removing current behavior b1 switches to b2

        Assert.True(b1.HideCalled);
        Assert.True(b2.ShowCalled);
        Assert.Same(b2, button.Behavior);
    }

    [Fact]
    public void SwitchAbility_CoolTimeDiff_AddsTimerOffset()
    {
        var behaviors = new List<BehaviorBase>
        {
            behavior1, // CT: 10
            behavior2  // CT: 15
        };

        var button = new ExtremeMultiModalAbilityButton(behaviors, mockActivator.Object, KeyCode.F, mockGOFactory.Object, mockSpriteLoader.Object);
        button.OnMeetingEnd(); // Reset CT
        Assert.Equal(10.0f, button.Timer);

        button.Remove(behavior1); // Switch to behavior2 (CT: 15, diff: +5)

        Assert.Equal(15.0f, button.Timer);
    }

    private sealed class TestBehavior : BehaviorBase
    {
        public bool UpdateCalled { get; private set; }

        public TestBehavior(float coolTime) : base(new ButtonGraphic("Test", null!))
        {
            SetCoolTime(coolTime);
        }

        public override void Initialize(ActionButton button) { }
        public override void ForceAbilityOff() { }
        public override void AbilityOff() { }

        public override bool TryUseAbility(float timer, AbilityState curState, out AbilityState newState)
        {
            newState = AbilityState.CoolDown;
            return true;
        }

        public override bool IsUse() => true;

        public override AbilityState Update(AbilityState curState)
        {
            UpdateCalled = true;
            return AbilityState.None;
        }
    }

    private sealed class HideShowTestBehavior : BehaviorBase, IHideLogic
    {
        public bool HideCalled { get; private set; }
        public bool ShowCalled { get; private set; }

        public HideShowTestBehavior() : base(new ButtonGraphic("Test", null!)) { }

        public override void Initialize(ActionButton button) { }
        public override void ForceAbilityOff() { }
        public override void AbilityOff() { }

        public override bool TryUseAbility(float timer, AbilityState curState, out AbilityState newState)
        {
            newState = AbilityState.CoolDown;
            return true;
        }

        public override bool IsUse() => true;

        public override AbilityState Update(AbilityState curState) => AbilityState.None;

        public void Hide() => HideCalled = true;

        public void Show() => ShowCalled = true;
    }
}