using System;
using System.Collections.Generic;
using System.Reflection;

using Moq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Xunit;

using ExtremeRoles.Extension.Manager;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Impostor;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.UnitTest.GhostRoles.Impostor;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class VentgeistTests
{
    public VentgeistTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
    }

    [Fact]
    public void GetRoleFilter_ReturnsEmptySet()
    {
        var ventgeist = new Ventgeist();

        HashSet<ExtremeRoleId> filter = ventgeist.GetRoleFilter();

        Assert.NotNull(filter);
        Assert.Empty(filter);
    }

    [Fact]
    public void Initialize_LoadsOptionsFromLoader()
    {
        var mockLoader = new Mock<IOptionLoader>();
        mockLoader.Setup(l => l.GetValue<Ventgeist.Option, float>(Ventgeist.Option.Range)).Returns(1.5f);

        var ventgeist = new DummyVentgeist(mockLoader.Object);

        ventgeist.Initialize();

        mockLoader.Verify(l => l.GetValue<Ventgeist.Option, float>(Ventgeist.Option.Range), Times.Once);
    }

    [Fact]
    public void CreateSpecificOption_CreatesVentgeistOptions()
    {
        using AutoParentSetOptionCategoryFactory factory = OptionCategoryAssembler.CreateAutoParentSetOptionCategory(
            2013,
            "VentgeistTestOption",
            OptionTab.GhostImpostorTab,
            Color.white);

        var ventgeist = new Ventgeist();

        ventgeist.CreateRoleSpecificOption(factory);
    }

    [Fact]
    public void MeetingHooks_ExecutesWithoutError()
    {
        var mockLoader = new Mock<IOptionLoader>();
        mockLoader.Setup(l => l.GetValue<Ventgeist.Option, float>(Ventgeist.Option.Range)).Returns(1.5f);

        var ventgeist = new Ventgeist();

        ventgeist.ResetOnMeetingStart();
        ventgeist.ResetOnMeetingEnd();
    }

    [Fact]
    public void CreateAbility_ConfiguresButtonAndLabel()
    {
        SetupHudManagerMock();

        var mockLoader = new Mock<IOptionLoader>();
        mockLoader.Setup(l => l.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)).Returns(10f);
        mockLoader.Setup(l => l.GetValue<GhostRoleOption, bool>(GhostRoleOption.IsReportAbility)).Returns(false);

        var ventgeist = new Ventgeist();

        ventgeist.CreateAbility();

        Assert.NotNull(ventgeist.Button);
    }

    private sealed class DummyVentgeist : GhostRoleBase
    {
        private readonly IOptionLoader loader;

        public override IOptionLoader Loader => loader;

        public DummyVentgeist(IOptionLoader loader) : base(
            false,
            ExtremeRoleType.Impostor,
            ExtremeGhostRoleId.Ventgeist,
            ExtremeGhostRoleId.Ventgeist.ToString(),
            Palette.ImpostorRed)
        {
            this.loader = loader;
        }

        public override void CreateAbility() { }
        public override HashSet<ExtremeRoleId> GetRoleFilter() => new();

        public override void Initialize()
        {
            float range = this.Loader.GetValue<Ventgeist.Option, float>(Ventgeist.Option.Range);
        }

        protected override void OnMeetingEndHook() { }
        protected override void OnMeetingStartHook() { }
        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) { }
        protected override void UseAbility(RPCOperator.RpcCaller caller) { }
    }

    private static void SetupHudManagerMock()
    {
        if (MockVector3get_oneHelper.Instance == null)
        {
            var mockOne = new Mock<MockVector3get_oneHelper>();
            mockOne.Setup(x => x.Invoke()).Returns(new Vector3(1f, 1f, 1f));
            MockVector3get_oneHelper.Instance = mockOne.Object;
        }

        var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
        mockGameObject.Setup(g => g.SetActive(It.IsAny<bool>()));

        var mockGridArrange = new Mock<GridArrange>(IntPtr.Zero);
        mockGridArrange.Setup(g => g.ArrangeChilds());

        var mockParentGameObject = new Mock<GameObject>(IntPtr.Zero);
        mockParentGameObject.Setup(g => g.GetComponent<GridArrange>()).Returns(mockGridArrange.Object);

        var mockParentTransform = new Mock<Transform>(IntPtr.Zero);
        mockParentTransform.SetupGet(t => t.gameObject).Returns(mockParentGameObject.Object);

        typeof(HudManagerExtension)
            .GetField("cachedArrange", BindingFlags.NonPublic | BindingFlags.Static)?
            .SetValue(null, mockGridArrange.Object);

        var mockTransform = new Mock<Transform>(IntPtr.Zero);
        mockTransform.SetupGet(t => t.parent).Returns(mockParentTransform.Object);
        mockTransform.Setup(t => t.FindChild(It.IsAny<string>())).Returns((Transform)null!);

        var mockMaterial = new Mock<Material>(IntPtr.Zero);
        mockMaterial.Setup(m => m.SetFloat(It.IsAny<string>(), It.IsAny<float>()));

        var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
        mockSpriteRenderer.SetupProperty(s => s.sprite);
        mockSpriteRenderer.SetupProperty(s => s.color);
        mockSpriteRenderer.SetupProperty(s => s.enabled);
        mockSpriteRenderer.SetupGet(s => s.material).Returns(mockMaterial.Object);

        var mockVentButton = new Mock<VentButton>(IntPtr.Zero);
        mockVentButton.SetupGet(v => v.graphic).Returns(mockSpriteRenderer.Object);

        var mockLabelText = new Mock<TextMeshPro>(IntPtr.Zero);
        mockLabelText.SetupProperty(t => t.color);
        mockLabelText.SetupProperty(t => t.fontMaterial);
        mockLabelText.SetupProperty(t => t.text);
        mockLabelText.SetupGet(t => t.transform).Returns(mockTransform.Object);

        var mockCoolText = new Mock<TextMeshPro>(IntPtr.Zero);
        mockCoolText.SetupProperty(t => t.color);
        mockCoolText.SetupProperty(t => t.enableWordWrapping);
        mockCoolText.SetupProperty(t => t.text);
        mockCoolText.SetupGet(t => t.gameObject).Returns(mockGameObject.Object);
        mockCoolText.SetupGet(t => t.transform).Returns(mockTransform.Object);

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

        var mockHud = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
        mockHud.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);
        mockHud.SetupGet(h => h.UseButton).Returns(mockUseButton.Object);
        mockHud.SetupGet(h => h.ImpostorVentButton).Returns(mockVentButton.Object);
    }
}
