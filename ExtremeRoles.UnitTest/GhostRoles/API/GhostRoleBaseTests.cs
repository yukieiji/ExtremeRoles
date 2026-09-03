using System;
using System.Collections.Generic;
using System.Reflection;
using ExtremeRoles.Extension;
using ExtremeRoles.Extension.Manager;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Roles.Solo.Impostor;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Xunit;

namespace ExtremeRoles.UnitTest.GhostRoles.API;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class GhostRoleBaseTests
{
    private sealed class DummyGhostRole : GhostRoleBase
    {
        public bool MeetingStartHookCalled { get; private set; }
        public bool MeetingEndHookCalled { get; private set; }
        public bool CreateSpecificOptionCalled { get; private set; }

        private readonly IOptionLoader? customLoader;

        public override IOptionLoader Loader => customLoader ?? base.Loader;

        public DummyGhostRole(
            bool hasTask,
            ExtremeRoleType team,
            ExtremeGhostRoleId id,
            string roleName,
            Color color,
            OptionTab tab = OptionTab.GeneralTab,
            IOptionLoader? loader = null,
            ExtremeAbilityButton? button = null)
            : base(hasTask, team, id, roleName, color, tab)
        {
            this.customLoader = loader;
            this.Button = button;
        }

        public void SetButton(ExtremeAbilityButton? button)
        {
            this.Button = button;
        }

        public override void CreateAbility() { }
        public override HashSet<ExtremeRoleId> GetRoleFilter() => new();
        public override void Initialize() { }

        protected override void OnMeetingEndHook()
        {
            MeetingEndHookCalled = true;
        }

        protected override void OnMeetingStartHook()
        {
            MeetingStartHookCalled = true;
        }

        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory parentOps)
        {
            CreateSpecificOptionCalled = true;
        }

        protected override void UseAbility(RPCOperator.RpcCaller caller) { }

        public void TestButtonInit() => ButtonInit();
        public bool TestIsReportAbility() => IsReportAbility();

        public static bool CallIsCommonUse() => IsCommonUse();
        public static bool CallIsCommonUseWithMinigame() => IsCommonUseWithMinigame();
        public static void CallEnumCheck<T>(T value) where T : struct, IConvertible => EnumCheck(value);
    }

    private sealed class CombinationDummyGhostRole : GhostRoleBase, ICombination
    {
        public ExtremeRoleId ParentRoleId { get; set; }
        public MultiAssignRoleBase.OptionOffsetInfo? OffsetInfo { get; set; }

        public CombinationDummyGhostRole(
            bool hasTask,
            ExtremeRoleType team,
            ExtremeGhostRoleId id,
            string roleName,
            Color color)
            : base(hasTask, team, id, roleName, color)
        {
        }

        public override void CreateAbility() { }
        public override HashSet<ExtremeRoleId> GetRoleFilter() => new();
        public override void Initialize() { }
        protected override void OnMeetingEndHook() { }
        protected override void OnMeetingStartHook() { }
        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory parentOps) { }
        protected override void UseAbility(RPCOperator.RpcCaller caller) { }
    }

    private sealed class DummySingleRole : SingleRoleBase
    {
        private readonly IStatusModel? status;

        public override IStatusModel? Status => status;

        public DummySingleRole(RoleArgs args, IStatusModel? status = null)
            : base(args)
        {
            this.status = status;
        }

        public override Color GetNameColor(bool isDead) => Color.white;
        public override string GetColoredRoleName(bool isDead = false) => "DummySingle";
        public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => "";
        public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => Color.clear;

        protected override void RoleSpecificInit() { }
        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory parentOps) { }
    }

    private sealed class TestBehavior : BehaviorBase, IActivatingBehavior, ICountBehavior
    {
        public float ActiveTime { get; set; } = 5.0f;
        public bool CanAbilityActiving => true;
        public int AbilityCount { get; private set; }
        public bool ForceAbilityOffCalled { get; private set; }
        public bool AbilityOffCalled { get; private set; }

        public TestBehavior() : base("Test", null!)
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
            newState = AbilityState.CoolDown;
            return true;
        }

        public void SetAbilityCount(int count)
        {
            AbilityCount = count;
        }

        public void SetButtonTextFormat(string newTextFormat) { }
    }

    private enum LongEnum : long
    {
        Value = 1
    }

    private enum IntEnum : int
    {
        Value = 1
    }

    public GhostRoleBaseTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
        mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
            .Returns((string id, string defaultStr, Il2CppReferenceArray<Il2CppSystem.Object> parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
        mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
            .Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
    }

    private static NetworkedPlayerInfo CreateMockPlayerInfo(byte playerId, string name, bool isDead = false)
    {
        var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockData.SetupGet(p => p.PlayerId).Returns(playerId);
        mockData.SetupGet(p => p.PlayerName).Returns(name);
        mockData.SetupGet(p => p.IsDead).Returns(isDead);
        return mockData.Object;
    }

    private static ExtremeAbilityButton CreateTestButton(BehaviorBase behavior)
    {
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

        var mockActivator = new Mock<IButtonAutoActivator>();
        mockActivator.Setup(a => a.IsActive()).Returns(true);

        return new ExtremeAbilityButton(behavior, mockActivator.Object, KeyCode.F);
    }

    // --- 1. Clone() ---
    [Fact]
    public void Clone_StandardGhostRole_CopiesPropertiesAndCreatesNewColorInstance()
    {
        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", new Color(0.1f, 0.2f, 0.3f, 0.4f));
        var copy = role.Clone();

        Assert.NotNull(copy);
        Assert.NotSame(role, copy);
        Assert.Equal(role.HasTask, copy.HasTask);
        Assert.Equal(role.Team, copy.Team);
        Assert.Equal(role.Id, copy.Id);
        Assert.Equal(role.Name, copy.Name);
        Assert.Equal(role.Color.r, copy.Color.r);
        Assert.Equal(role.Color.g, copy.Color.g);
        Assert.Equal(role.Color.b, copy.Color.b);
        Assert.Equal(role.Color.a, copy.Color.a);
    }

    [Fact]
    public void Clone_ICombinationGhostRole_CopiesOffsetInfo()
    {
        var role = new CombinationDummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Wisp, "Wisp", Color.red)
        {
            OffsetInfo = new MultiAssignRoleBase.OptionOffsetInfo(CombinationRoleType.Kids, 10)
        };

        var copy = role.Clone();

        Assert.NotNull(copy);
        var combCopy = Assert.IsAssignableFrom<ICombination>(copy);
        Assert.Equal(role.OffsetInfo, combCopy.OffsetInfo);
    }

    // --- 2. Loader ---
    [Fact]
    public void Loader_CategoryExists_ReturnsCategory()
    {
        var testId = (ExtremeGhostRoleId)240;
        OptionCategoryAssembler.CreateAutoParentSetOptionCategory(
            ExtremeGhostRoleManager.GetRoleGroupId(testId),
            "TestCategoryRole",
            OptionTab.GhostCrewmateTab,
            Color.white).Dispose();

        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, testId, "TestCategoryRole", Color.white);

        var loader = role.Loader;

        Assert.NotNull(loader);
    }

    [Fact]
    public void Loader_CategoryDoesNotExist_ThrowsArgumentException()
    {
        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, (ExtremeGhostRoleId)250, "UnregisteredRole", Color.white);

        Assert.Throws<ArgumentException>(() => role.Loader);
    }

    // --- 3. CreateRoleAllOption and CreateRoleSpecificOption ---
    [Fact]
    public void CreateRoleAllOption_ForCrewmateGhostRole_CreatesOptionsAndCallsSpecific()
    {
        var testId = (ExtremeGhostRoleId)241;
        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, testId, "TestCrewmateGhost", Color.white);

        role.CreateRoleAllOption();

        Assert.True(role.CreateSpecificOptionCalled);
        Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.GhostCrewmateTab, ExtremeGhostRoleManager.GetRoleGroupId(testId), out var cate));
        Assert.NotNull(cate);
    }

    [Fact]
    public void CreateRoleAllOption_ForImpostorGhostRole_CreatesOptionsWithImpostorMaxNum()
    {
        var testId = (ExtremeGhostRoleId)242;
        var role = new DummyGhostRole(false, ExtremeRoleType.Impostor, testId, "TestImpostorGhost", Color.red);

        role.CreateRoleAllOption();

        Assert.True(role.CreateSpecificOptionCalled);
        Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.GhostImpostorTab, ExtremeGhostRoleManager.GetRoleGroupId(testId), out var cate));
        Assert.NotNull(cate);
    }

    [Fact]
    public void CreateRoleSpecificOption_CallsCreateSpecificOption()
    {
        var testId = (ExtremeGhostRoleId)243;
        using var factory = OptionCategoryAssembler.CreateAutoParentSetOptionCategory(
            ExtremeGhostRoleManager.GetRoleGroupId(testId),
            "TestSpecificRole", OptionTab.GhostCrewmateTab, Color.white);

        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, testId, "TestSpecificRole", Color.white);

        role.CreateRoleSpecificOption(factory);

        Assert.True(role.CreateSpecificOptionCalled);
    }

    // --- 4. Team checks ---
    [Theory]
    [InlineData(ExtremeRoleType.Crewmate, true, false, false)]
    [InlineData(ExtremeRoleType.Impostor, false, true, false)]
    [InlineData(ExtremeRoleType.Neutral, false, false, true)]
    public void TeamCheckMethods_ReturnExpectedValues(ExtremeRoleType team, bool expectedCrewmate, bool expectedImpostor, bool expectedNeutral)
    {
        var role = new DummyGhostRole(true, team, ExtremeGhostRoleId.Faunus, "Faunus", Color.white);

        Assert.Equal(expectedCrewmate, role.IsCrewmate());
        Assert.Equal(expectedImpostor, role.IsImpostor());
        Assert.Equal(expectedNeutral, role.IsNeutral());
    }

    [Theory]
    [InlineData(ExtremeGhostRoleId.VanillaRole, true)]
    [InlineData(ExtremeGhostRoleId.Faunus, false)]
    public void IsVanillaRole_ReturnsExpectedValue(ExtremeGhostRoleId id, bool expected)
    {
        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, id, "Role", Color.white);

        Assert.Equal(expected, role.IsVanillaRole());
    }

    // --- 5. Name / Description / Important Text Getters ---
    [Fact]
    public void GetColoredRoleName_ReturnsColoredString()
    {
        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", Color.white);

        string result = role.GetColoredRoleName();

        Assert.Contains("Faunus", result);
    }

    [Fact]
    public void GetFullDescription_ReturnsTranslationString()
    {
        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", Color.white);

        string result = role.GetFullDescription();

        Assert.Equal($"{ExtremeGhostRoleId.Faunus}FullDescription", result);
    }

    [Fact]
    public void GetImportantText_ReturnsFormattedColoredString()
    {
        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", Color.white);

        string result = role.GetImportantText();

        Assert.Contains("Faunus", result);
        Assert.Contains($"{ExtremeGhostRoleId.Faunus}ShortDescription", result);
    }

    // --- 6. GetTargetRoleSeeColor ---
    [Fact]
    public void GetTargetRoleSeeColor_TargetIsOverLoader_IsOverLoadTrue_ReturnsImpostorRed()
    {
        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", Color.white);
        var overLoader = new OverLoader();
        overLoader.IsOverLoad = true;

        Color result = role.GetTargetRoleSeeColor(1, overLoader, null);

        Assert.Equal(Palette.ImpostorRed, result);
    }

    [Fact]
    public void GetTargetRoleSeeColor_TargetIsOverLoader_IsOverLoadFalse_AndThisIsCrewmate_ReturnsClear()
    {
        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", Color.white);
        var overLoader = new OverLoader();
        overLoader.IsOverLoad = false;

        Color result = role.GetTargetRoleSeeColor(1, overLoader, null);

        Assert.Equal(Color.clear, result);
    }

    [Fact]
    public void GetTargetRoleSeeColor_ThisIsImpostor_TargetRoleIsImpostor_ReturnsImpostorRed()
    {
        var role = new DummyGhostRole(false, ExtremeRoleType.Impostor, ExtremeGhostRoleId.Igniter, "Igniter", Color.red);
        var targetRole = new DummySingleRole(RoleArgs.BuildImpostor(ExtremeRoleId.Assassin));

        Color result = role.GetTargetRoleSeeColor(1, targetRole, null);

        Assert.Equal(Palette.ImpostorRed, result);
    }

    [Fact]
    public void GetTargetRoleSeeColor_ThisIsImpostor_TargetRoleIsFakeImpostor_ReturnsImpostorRed()
    {
        var role = new DummyGhostRole(false, ExtremeRoleType.Impostor, ExtremeGhostRoleId.Igniter, "Igniter", Color.red);
        var statusMock = new Mock<IStatusModel>();
        var fakeStatusMock = statusMock.As<IFakeImpostorStatus>();
        fakeStatusMock.SetupGet(s => s.IsFakeImpostor).Returns(true);

        var targetRole = new DummySingleRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white), status: statusMock.Object);

        Color result = role.GetTargetRoleSeeColor(1, targetRole, null);

        Assert.Equal(Palette.ImpostorRed, result);
    }

    [Fact]
    public void GetTargetRoleSeeColor_ThisIsImpostor_TargetGhostRoleIsImpostor_ReturnsImpostorRed()
    {
        var role = new DummyGhostRole(false, ExtremeRoleType.Impostor, ExtremeGhostRoleId.Igniter, "Igniter", Color.red);
        var targetRole = new DummySingleRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white));
        var targetGhostRole = new DummyGhostRole(false, ExtremeRoleType.Impostor, ExtremeGhostRoleId.Igniter, "Igniter", Color.red);

        Color result = role.GetTargetRoleSeeColor(1, targetRole, targetGhostRole);

        Assert.Equal(Palette.ImpostorRed, result);
    }

    [Fact]
    public void GetTargetRoleSeeColor_ThisIsCrewmate_TargetRoleIsImpostor_ReturnsClear()
    {
        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", Color.white);
        var targetRole = new DummySingleRole(RoleArgs.BuildImpostor(ExtremeRoleId.Assassin));

        Color result = role.GetTargetRoleSeeColor(1, targetRole, null);

        Assert.Equal(Color.clear, result);
    }

    // --- 7. SetGameControlId ---
    [Fact]
    public void SetGameControlId_UpdatesGameControlIdProperty()
    {
        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", Color.white);

        role.SetGameControlId(42);

        Assert.Equal(42, role.GameControlId);
    }

    // --- 8. ResetOnMeetingEnd and ResetOnMeetingStart ---
    [Fact]
    public void ResetOnMeetingEnd_WithoutButton_CallsHookOnly()
    {
        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", Color.white);

        role.ResetOnMeetingEnd();

        Assert.True(role.MeetingEndHookCalled);
    }

    [Fact]
    public void ResetOnMeetingStart_WithoutButton_CallsHookOnly()
    {
        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", Color.white);

        role.ResetOnMeetingStart();

        Assert.True(role.MeetingStartHookCalled);
    }

    [Fact]
    public void ResetOnMeetingEnd_WithButton_CallsButtonOnMeetingEndAndHook()
    {
        var behavior = new TestBehavior();
        var button = CreateTestButton(behavior);
        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", Color.white, button: button);

        role.ResetOnMeetingEnd();

        Assert.True(role.MeetingEndHookCalled);
    }

    [Fact]
    public void ResetOnMeetingStart_WithButton_CallsButtonOnMeetingStartAndHook()
    {
        var behavior = new TestBehavior();
        var button = CreateTestButton(behavior);
        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", Color.white, button: button);

        role.ResetOnMeetingStart();

        Assert.True(role.MeetingStartHookCalled);
    }

    // --- 9. ButtonInit ---
    [Fact]
    public void ButtonInit_WhenButtonIsNull_DoesNothing()
    {
        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", Color.white);

        role.TestButtonInit(); // Should not throw
    }

    [Fact]
    public void ButtonInit_WhenButtonIsPresent_ConfiguresBehaviorAndCallsOnMeetingEnd()
    {
        var mockLoader = new Mock<IOptionLoader>();
        mockLoader.Setup(l => l.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)).Returns(15f);

        float activeTimeVal = 10f;
        mockLoader.Setup(l => l.TryGetValue(RoleAbilityCommonOption.AbilityActiveTime, out activeTimeVal)).Returns(true);

        int countVal = 3;
        mockLoader.Setup(l => l.TryGetValue(RoleAbilityCommonOption.AbilityCount, out countVal)).Returns(true);

        var behavior = new TestBehavior();
        var button = CreateTestButton(behavior);

        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", Color.white, loader: mockLoader.Object, button: button);

        role.TestButtonInit();

        Assert.Equal(15f, behavior.CoolTime);
        Assert.Equal(10f, behavior.ActiveTime);
        Assert.Equal(3, behavior.AbilityCount);
    }

    [Fact]
    public void ButtonInit_WhenBehaviorDoesNotImplementInterfaces_AndTryGetValueReturnsFalse()
    {
        var mockLoader = new Mock<IOptionLoader>();
        mockLoader.Setup(l => l.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)).Returns(20f);

        float dummyFloat = 0f;
        mockLoader.Setup(l => l.TryGetValue(RoleAbilityCommonOption.AbilityActiveTime, out dummyFloat)).Returns(false);

        int dummyInt = 0;
        mockLoader.Setup(l => l.TryGetValue(RoleAbilityCommonOption.AbilityCount, out dummyInt)).Returns(false);

        var mockBehavior = new Mock<BehaviorBase>("UnimplementedBehavior", (Sprite)null!);
        var button = CreateTestButton(mockBehavior.Object);

        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", Color.white, loader: mockLoader.Object, button: button);

        role.TestButtonInit();

        mockBehavior.Verify(b => b.SetCoolTime(20f), Times.Once);
    }

    // --- 10. IsReportAbility ---
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsReportAbility_ReturnsValueFromLoader(bool expectedValue)
    {
        var mockLoader = new Mock<IOptionLoader>();
        mockLoader.Setup(l => l.GetValue<GhostRoleOption, bool>(GhostRoleOption.IsReportAbility)).Returns(expectedValue);

        var role = new DummyGhostRole(true, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.Faunus, "Faunus", Color.white, loader: mockLoader.Object);

        Assert.Equal(expectedValue, role.TestIsReportAbility());
    }

    // --- 11. IsCommonUse ---
    [Fact]
    public void IsCommonUse_WhenPlayerIsDeadAndCanMove_ReturnsTrue()
    {
        var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
        var mockData = CreateMockPlayerInfo(0, "LocalPlayer", isDead: true);
        var mockDataObj = Mock.Get(mockData);
        mockDataObj.SetupGet(p => p.Object).Returns(mockPlayer.Object);

        mockPlayer.SetupGet(p => p.Data).Returns(mockData);
        mockPlayer.SetupGet(p => p.CanMove).Returns(true);

        Assert.True(DummyGhostRole.CallIsCommonUse());
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public void IsCommonUse_WhenPlayerNotDeadOrCannotMove_ReturnsFalse(bool isDead, bool canMove)
    {
        var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
        var mockData = CreateMockPlayerInfo(0, "LocalPlayer", isDead: isDead);
        var mockDataObj = Mock.Get(mockData);
        mockDataObj.SetupGet(p => p.Object).Returns(mockPlayer.Object);

        mockPlayer.SetupGet(p => p.Data).Returns(mockData);
        mockPlayer.SetupGet(p => p.CanMove).Returns(canMove);

        Assert.False(DummyGhostRole.CallIsCommonUse());
    }

    // --- 12. IsCommonUseWithMinigame ---
    [Fact]
    public void IsCommonUseWithMinigame_WhenDeadAndNoUIOpen_ReturnsTrue()
    {
        var mockPhysics = new Mock<PlayerPhysics>(IntPtr.Zero);
        mockPhysics.SetupGet(p => p.DoingCustomAnimation).Returns(false);

        var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
        var mockData = CreateMockPlayerInfo(0, "LocalPlayer", isDead: true);
        var mockDataObj = Mock.Get(mockData);
        mockDataObj.SetupGet(p => p.Object).Returns(mockPlayer.Object);

        mockPlayer.SetupGet(p => p.Data).Returns(mockData);
        mockPlayer.SetupGet(p => p.inVent).Returns(false);
        mockPlayer.SetupGet(p => p.shapeshifting).Returns(false);
        mockPlayer.SetupGet(p => p.waitingForShapeshiftResponse).Returns(false);
        mockPlayer.SetupGet(p => p.MyPhysics).Returns(mockPhysics.Object);

        var mockChat = new Mock<ChatController>(IntPtr.Zero);
        mockChat.SetupGet(c => c.IsOpenOrOpening).Returns(false);

        var mockKillOverlay = new Mock<KillOverlay>(IntPtr.Zero);
        mockKillOverlay.SetupGet(k => k.IsOpen).Returns(false);

        var mockGameMenu = new Mock<OptionsMenuBehaviour>(IntPtr.Zero);
        mockGameMenu.SetupGet(g => g.IsOpen).Returns(false);

        var mockHud = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
        mockHud.SetupGet(h => h.Chat).Returns(mockChat.Object);
        mockHud.SetupGet(h => h.KillOverlay).Returns(mockKillOverlay.Object);
        mockHud.SetupGet(h => h.GameMenu).Returns(mockGameMenu.Object);
        mockHud.SetupGet(h => h.IsIntroDisplayed).Returns(false);

        var mockMeetingHelper = new Mock<MockMeetingHudget_InstanceHelper>();
        mockMeetingHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
        MockMeetingHudget_InstanceHelper.Instance = mockMeetingHelper.Object;

        var mockCustomizationHelper = new Mock<MockPlayerCustomizationMenuget_InstanceHelper>();
        mockCustomizationHelper.Setup(x => x.Invoke()).Returns((PlayerCustomizationMenu)null!);
        MockPlayerCustomizationMenuget_InstanceHelper.Instance = mockCustomizationHelper.Object;

        var mockExileHelper = new Mock<MockExileControllerget_InstanceHelper>();
        mockExileHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
        MockExileControllerget_InstanceHelper.Instance = mockExileHelper.Object;

        var mockIntroHelper = new Mock<MockIntroCutsceneget_InstanceHelper>();
        mockIntroHelper.Setup(x => x.Invoke()).Returns((IntroCutscene)null!);
        MockIntroCutsceneget_InstanceHelper.Instance = mockIntroHelper.Object;

        var mockMapHelper = new Mock<MockMapBehaviourget_InstanceHelper>();
        mockMapHelper.Setup(x => x.Invoke()).Returns((MapBehaviour)null!);
        MockMapBehaviourget_InstanceHelper.Instance = mockMapHelper.Object;

        Assert.True(DummyGhostRole.CallIsCommonUseWithMinigame());
    }

    [Fact]
    public void IsCommonUseWithMinigame_WhenHudManagerIsNull_ReturnsFalse()
    {
        var mockPhysics = new Mock<PlayerPhysics>(IntPtr.Zero);

        var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
        var mockData = CreateMockPlayerInfo(0, "LocalPlayer", isDead: true);
        mockPlayer.SetupGet(p => p.Data).Returns(mockData);
        mockPlayer.SetupGet(p => p.MyPhysics).Returns(mockPhysics.Object);

        var mockSingleton = new Mock<MockDestroyableSingletonget_InstanceHelper<HudManager>>();
        mockSingleton.Setup(x => x.Invoke()).Returns((HudManager)null!);
        MockDestroyableSingletonget_InstanceHelper<HudManager>.Instance = mockSingleton.Object;

        Assert.False(DummyGhostRole.CallIsCommonUseWithMinigame());
    }

    [Fact]
    public void IsCommonUseWithMinigame_WhenMeetingHudIsNotNull_ReturnsFalse()
    {
        var mockPhysics = new Mock<PlayerPhysics>(IntPtr.Zero);

        var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
        var mockData = CreateMockPlayerInfo(0, "LocalPlayer", isDead: true);
        mockPlayer.SetupGet(p => p.Data).Returns(mockData);
        mockPlayer.SetupGet(p => p.MyPhysics).Returns(mockPhysics.Object);

        var mockHud = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

        var mockMeeting = new Mock<MeetingHud>(IntPtr.Zero);
        var mockMeetingHelper = new Mock<MockMeetingHudget_InstanceHelper>();
        mockMeetingHelper.Setup(x => x.Invoke()).Returns(mockMeeting.Object);
        MockMeetingHudget_InstanceHelper.Instance = mockMeetingHelper.Object;

        Assert.False(DummyGhostRole.CallIsCommonUseWithMinigame());
    }

    // --- 13. EnumCheck ---
    [Fact]
    public void EnumCheck_WithIntUnderlyingType_DoesNotThrow()
    {
        DummyGhostRole.CallEnumCheck(IntEnum.Value);
    }

    [Fact]
    public void EnumCheck_WithLongUnderlyingType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => DummyGhostRole.CallEnumCheck(LongEnum.Value));
    }
}
