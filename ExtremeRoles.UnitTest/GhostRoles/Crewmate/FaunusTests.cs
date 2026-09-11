using System;
using System.Collections.Generic;
using System.Reflection;
using ExtremeRoles.Extension.Manager;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Crewmate;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.UnitTest;
using Moq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Xunit;

namespace ExtremeRoles.UnitTest.GhostRoles.Crewmate;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class FaunusTests
{
    public FaunusTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
    }

    [Fact]
    public void GetRoleFilter_ReturnsEmptySet()
    {
        var faunus = new Faunus();
        HashSet<ExtremeRoleId> filter = faunus.GetRoleFilter();

        Assert.NotNull(filter);
        Assert.Empty(filter);
    }

    [Fact]
    public void Initialize_ResetsStateWithoutThrowing()
    {
        var faunus = new Faunus();
        faunus.Initialize();
    }

    [Fact]
    public void CreateSpecificOption_CreatesCountButtonOption()
    {
        using AutoParentSetOptionCategoryFactory factory = OptionCategoryAssembler.CreateAutoParentSetOptionCategory(
            2000,
            "FaunusTestOption",
            OptionTab.GhostCrewmateTab,
            Color.white);

        var faunus = new Faunus();
        faunus.CreateRoleSpecificOption(factory);
    }

    [Fact]
    public void MeetingHooks_ExecutesWithoutError()
    {
        var faunus = new Faunus();

        faunus.ResetOnMeetingStart();
        faunus.ResetOnMeetingEnd();
    }

    [Fact]
    public void CreateAbility_ConfiguresButtonAndLabel()
    {
        SetupHudManagerMock();

        var mockSprite = new Mock<Sprite>(IntPtr.Zero);
        LruCache<string, Sprite>.Add($"{ObjectPath.MaintainerRepair}115", mockSprite.Object);

        var mockLoader = new Mock<IOptionLoader>();
        mockLoader.Setup(l => l.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)).Returns(10f);
        mockLoader.Setup(l => l.GetValue<GhostRoleOption, bool>(GhostRoleOption.IsReportAbility)).Returns(false);

        var faunus = new Faunus();
        faunus.CreateAbility();

        Assert.NotNull(faunus.Button);
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
    }
}
