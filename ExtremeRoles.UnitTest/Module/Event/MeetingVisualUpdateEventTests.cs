using ExtremeRoles.UnitTest.Mocks;
using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.Event;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Visual;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using TMPro;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Event;

public class MeetingVisualUpdateEventTests : SerialTestBase, IClassFixture<SerialFixture>, IClassFixture<UnityCommonMock>
{
    private sealed class DummySingleRole : SingleRoleBase
    {
        private readonly Color color;
        private readonly string roleName;
        private readonly string tag;
        private readonly IVisual? visual;

        public override IVisual? Visual => visual;

        public DummySingleRole(RoleArgs args, Color? color = null, string roleName = "TestRole", string tag = "", IVisual? visual = null)
            : base(args)
        {
            this.color = color ?? Color.white;
            this.roleName = roleName;
            this.tag = tag;
            this.visual = visual;
        }

        public override Color GetNameColor(bool isDead) => color;
        public override string GetColoredRoleName(bool isDead = false) => roleName;
        public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => tag;
        public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => color;

        protected override void RoleSpecificInit() { }
        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory parentOps) { }
    }

    private sealed class DummyRoleVisual : IVisual, ILookedTag
    {
        private readonly string tag;
        public DummyRoleVisual(string tag) { this.tag = tag; }
        public string GetLookedToThisTag(byte playerId) => tag;
    }

    private sealed class DummyGhostRole : GhostRoleBase
    {
        public DummyGhostRole(Color color, string name = "DummyGhost")
            : base(false, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.VanillaRole, name, color) { }

        public override void CreateAbility() { }
        public override HashSet<ExtremeRoleId> GetRoleFilter() => [];
        public override void Initialize() { }
        protected override void OnMeetingEndHook() { }
        protected override void OnMeetingStartHook() { }
        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory parentOps) { }
        protected override void UseAbility(RPCOperator.RpcCaller caller) { }
    }

    public MeetingVisualUpdateEventTests(SerialFixture fixture, UnityCommonMock unityCommonMock)
        : base(fixture, unityCommonMock.OperatorsMock, unityCommonMock.Vector2Mock, unityCommonMock.ColorMock, unityCommonMock.MathfMock, unityCommonMock.PaletteMock, unityCommonMock.GameOptionsManagerMock, unityCommonMock.CompatModManagerMock, unityCommonMock.TimeMock)
    {
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
        if (ClientOption.Instance == null)
        {
            ClientOption.Create();
        }

        var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
        mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
            .Returns((string id, string defaultStr, Il2CppReferenceArray<Il2CppSystem.Object> parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
        mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
            .Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);

        SetupVector3Mocks();
        SetupColorOperators();
    }

    private static void SetupColorOperators()
    {
        var mockColorEq = new Mock<MockColorop_EqualityHelper>();
        mockColorEq.Setup(x => x.Invoke(It.IsAny<Color>(), It.IsAny<Color>()))
            .Returns((Color a, Color b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a);
        MockColorop_EqualityHelper.Instance = mockColorEq.Object;

        var mockColorIneq = new Mock<MockColorop_InequalityHelper>();
        mockColorIneq.Setup(x => x.Invoke(It.IsAny<Color>(), It.IsAny<Color>()))
            .Returns((Color a, Color b) => a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a);
        MockColorop_InequalityHelper.Instance = mockColorIneq.Object;

        var mockColorAdd = new Mock<MockColorop_AdditionHelper>();
        mockColorAdd.Setup(x => x.Invoke(It.IsAny<Color>(), It.IsAny<Color>()))
            .Returns((Color a, Color b) => new Color(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a));
        MockColorop_AdditionHelper.Instance = mockColorAdd.Object;

        var mockColorDiv = new Mock<MockColorop_DivisionHelper>();
        mockColorDiv.Setup(x => x.Invoke(It.IsAny<Color>(), It.IsAny<float>()))
            .Returns((Color a, float b) => new Color(a.r / b, a.g / b, a.b / b, a.a / b));
        MockColorop_DivisionHelper.Instance = mockColorDiv.Object;

        var mockToHtmlStringRGBA = new Mock<MockColorUtilityToHtmlStringRGBAHelper>();
        mockToHtmlStringRGBA.Setup(x => x.Invoke(It.IsAny<Color>())).Returns("FFFFFF");
        MockColorUtilityToHtmlStringRGBAHelper.Instance = mockToHtmlStringRGBA.Object;
    }

    private static void SetupInstantiateFor(UnityEngine.Object source, UnityEngine.Object result)
    {
        var m5 = new Mock<MockObjectInstantiateHelper5>();
        m5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>())).Returns(result);
        MockObjectInstantiateHelper5.Instance = m5.Object;

        var m10 = new Mock<MockObjectInstantiateHelper10>();
        m10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>())).Returns(result);
        MockObjectInstantiateHelper10.Instance = m10.Object;
    }

    private static void SetupVector3Mocks()
    {
        var mockDown = new Mock<MockVector3get_downHelper>();
        mockDown.Setup(x => x.Invoke()).Returns(new Vector3(0f, -1f, 0f));
        MockVector3get_downHelper.Instance = mockDown.Object;

        var mockLeft = new Mock<MockVector3get_leftHelper>();
        mockLeft.Setup(x => x.Invoke()).Returns(new Vector3(-1f, 0f, 0f));
        MockVector3get_leftHelper.Instance = mockLeft.Object;

        var mockZero = new Mock<MockVector3get_zeroHelper>();
        mockZero.Setup(x => x.Invoke()).Returns(new Vector3(0f, 0f, 0f));
        MockVector3get_zeroHelper.Instance = mockZero.Object;

        var mockAdd = new Mock<MockVector3op_AdditionHelper>();
        mockAdd.Setup(x => x.Invoke(It.IsAny<Vector3>(), It.IsAny<Vector3>()))
            .Returns((Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z));
        MockVector3op_AdditionHelper.Instance = mockAdd.Object;

        var mockMult = new Mock<MockVector3op_MultiplyHelper>();
        mockMult.Setup(x => x.Invoke(It.IsAny<Vector3>(), It.IsAny<float>()))
            .Returns((Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d));
        MockVector3op_MultiplyHelper.Instance = mockMult.Object;
    }

    private static NetworkedPlayerInfo CreateMockPlayerInfo(byte playerId, string name, bool isDead = false, bool isImpostor = false, RoleTypes roleType = RoleTypes.Crewmate)
    {
        var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockData.SetupGet(p => p.PlayerId).Returns(playerId);
        mockData.SetupGet(p => p.PlayerName).Returns(name);
        mockData.SetupGet(p => p.IsDead).Returns(isDead);

        var mockRole = new Mock<RoleBehaviour>(IntPtr.Zero);
        mockRole.SetupGet(r => r.IsImpostor).Returns(isImpostor);
        mockRole.SetupGet(r => r.Role).Returns(roleType);
        mockData.SetupGet(p => p.Role).Returns(mockRole.Object);

        return mockData.Object;
    }

    private static (PlayerVoteArea pva, Mock<TextMeshPro> nameText, Mock<TextMeshPro> infoText) CreateMockPlayerVoteArea()
    {
        var mockPva = new Mock<PlayerVoteArea>(IntPtr.Zero);

        var mockNameText = new Mock<TextMeshPro>(IntPtr.Zero);
        mockNameText.SetupProperty(t => t.text, "OriginalName");
        mockNameText.SetupProperty(t => t.color, Color.white);
        mockNameText.SetupProperty(t => t.fontSize, 10f);

        var mockInfoText = new Mock<TextMeshPro>(IntPtr.Zero);
        mockInfoText.SetupProperty(t => t.text, "InfoText");
        mockInfoText.SetupProperty(t => t.color, Color.white);
        mockInfoText.SetupProperty(t => t.fontSize, 6.3f);

        var mockNameTransform = new Mock<Transform>(IntPtr.Zero);
        mockNameTransform.SetupProperty(t => t.localPosition, new Vector3(0f, 0f, 0f));
        mockNameText.SetupGet(t => t.transform).Returns(mockNameTransform.Object);

        var mockInfoTransform = new Mock<Transform>(IntPtr.Zero);
        mockInfoTransform.SetupProperty(t => t.localPosition, new Vector3(0f, 0f, 0f));
        mockInfoText.SetupGet(t => t.transform).Returns(mockInfoTransform.Object);

        var mockNameGameObject = new Mock<GameObject>(IntPtr.Zero);
        mockNameGameObject.Setup(g => g.SetActive(It.IsAny<bool>()));
        mockNameGameObject.SetupProperty(g => g.name, "NameGO");
        mockNameText.SetupGet(t => t.gameObject).Returns(mockNameGameObject.Object);

        var mockInfoGameObject = new Mock<GameObject>(IntPtr.Zero);
        mockInfoGameObject.Setup(g => g.SetActive(It.IsAny<bool>()));
        mockInfoGameObject.SetupProperty(g => g.name, "VoteAreaInfo");
        mockInfoText.SetupGet(t => t.gameObject).Returns(mockInfoGameObject.Object);

        mockPva.SetupGet(p => p.NameText).Returns(mockNameText.Object);

        SetupInstantiateFor(mockNameText.Object, mockInfoText.Object);

        return (mockPva.Object, mockNameText, mockInfoText);
    }

    [Fact]
    public void LocalPlayerMeetingVisualUpdateEvent_Invoke_ReturnsFalse_WhenMeetingHudInstanceIsNull()
    {
        var mockLocalData = CreateMockPlayerInfo(0, "LocalPlayer");
        var mockLocalPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockLocalPlayer.SetupGet(p => p.Data).Returns(mockLocalData);

        var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        mockLocalHelper.Setup(x => x.Invoke()).Returns(mockLocalPlayer.Object);
        MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

        var (pva, _, _) = CreateMockPlayerVoteArea();
        var status = new MeetingStatus(pva, false);
        var ev = new LocalPlayerMeetingVisualUpdateEvent(status);

        var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
        mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
        MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

        bool result = ev.Invoke();

        Assert.False(result);
    }

    [Fact]
    public void LocalPlayerMeetingVisualUpdateEvent_Invoke_UpdatesVisuals_WhenMeetingHudExists()
    {
        var mockLocalData = CreateMockPlayerInfo(0, "LocalPlayer");
        var mockLocalPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockLocalPlayer.SetupGet(p => p.Data).Returns(mockLocalData);

        var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        mockLocalHelper.Setup(x => x.Invoke()).Returns(mockLocalPlayer.Object);
        MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

        var (pva, mockNameText, mockInfoText) = CreateMockPlayerVoteArea();
        var status = new MeetingStatus(pva, false);
        var ev = new LocalPlayerMeetingVisualUpdateEvent(status);

        var mockMeetingHud = new Mock<MeetingHud>(IntPtr.Zero);
        mockMeetingHud.SetupGet(m => m.state).Returns(MeetingHud.MeetingStates.Discussion);
        var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
        mockMeetingHudHelper.Setup(x => x.Invoke()).Returns(mockMeetingHud.Object);
        MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

        var localRole = new DummySingleRole(
            RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white),
            Color.red, "Crewmate", "[Tag]");

        ExtremeRoleManager.GameRole.Clear();
        ExtremeRoleManager.GameRole[0] = localRole;
        ExtremeGhostRoleManager.GameRole.Clear();

        bool result = ev.Invoke();

        Assert.True(result);
        Assert.Equal("LocalPlayer[Tag]", mockNameText.Object.text);
        Assert.Contains("Crewmate", mockInfoText.Object.text);
    }

    [Fact]
    public void LocalPlayerMeetingVisualUpdateEvent_Invoke_HandlesGhostRoleAndCommsActive()
    {
        var mockLocalData = CreateMockPlayerInfo(0, "LocalPlayer");
        var mockLocalPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockLocalPlayer.SetupGet(p => p.Data).Returns(mockLocalData);

        var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        mockLocalHelper.Setup(x => x.Invoke()).Returns(mockLocalPlayer.Object);
        MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

        var (pva, _, mockInfoText) = CreateMockPlayerVoteArea();
        var status = new MeetingStatus(pva, isCommActive: true);
        var ev = new LocalPlayerMeetingVisualUpdateEvent(status);

        var mockMeetingHud = new Mock<MeetingHud>(IntPtr.Zero);
        mockMeetingHud.SetupGet(m => m.state).Returns(MeetingHud.MeetingStates.Discussion);
        var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
        mockMeetingHudHelper.Setup(x => x.Invoke()).Returns(mockMeetingHud.Object);
        MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

        var localRole = new DummySingleRole(
            RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white),
            Color.white, "Crewmate", "");

        var ghostRole = new DummyGhostRole(Color.yellow, "Phantom");

        ExtremeRoleManager.GameRole.Clear();
        ExtremeRoleManager.GameRole[0] = localRole;
        ExtremeGhostRoleManager.GameRole.Clear();
        ExtremeGhostRoleManager.GameRole[0] = ghostRole;

        bool result = ev.Invoke();

        Assert.True(result);
        Assert.Contains("Phantom", mockInfoText.Object.text);
        Assert.Contains("Crewmate", mockInfoText.Object.text);
    }

    [Fact]
    public void LocalPlayerMeetingVisualUpdateEvent_Invoke_InResultsState_SetsEmptyInfoText()
    {
        var mockLocalData = CreateMockPlayerInfo(0, "LocalPlayer");
        var mockLocalPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockLocalPlayer.SetupGet(p => p.Data).Returns(mockLocalData);

        var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        mockLocalHelper.Setup(x => x.Invoke()).Returns(mockLocalPlayer.Object);
        MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

        var (pva, _, mockInfoText) = CreateMockPlayerVoteArea();
        var status = new MeetingStatus(pva, isCommActive: false);
        var ev = new LocalPlayerMeetingVisualUpdateEvent(status);

        var mockMeetingHud = new Mock<MeetingHud>(IntPtr.Zero);
        mockMeetingHud.SetupGet(m => m.state).Returns(MeetingHud.MeetingStates.Results);
        var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
        mockMeetingHudHelper.Setup(x => x.Invoke()).Returns(mockMeetingHud.Object);
        MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

        var localRole = new DummySingleRole(
            RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white),
            Palette.ClearWhite, "Crewmate", "");

        ExtremeRoleManager.GameRole.Clear();
        ExtremeRoleManager.GameRole[0] = localRole;
        ExtremeGhostRoleManager.GameRole.Clear();

        bool result = ev.Invoke();

        Assert.True(result);
        Assert.Equal("", mockInfoText.Object.text);
    }

    [Fact]
    public void OtherPlayerMeetingVisualUpdateEvent_Invoke_ReturnsFalse_WhenTargetRoleNotFound()
    {
        var mockLocalData = CreateMockPlayerInfo(0, "LocalPlayer");
        var mockLocalPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockLocalPlayer.SetupGet(p => p.Data).Returns(mockLocalData);

        var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        mockLocalHelper.Setup(x => x.Invoke()).Returns(mockLocalPlayer.Object);
        MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

        var (pva, _, _) = CreateMockPlayerVoteArea();
        var status = new MeetingStatus(pva, false);
        var targetData = CreateMockPlayerInfo(1, "TargetPlayer");

        var ev = new OtherPlayerMeetingVisualUpdateEvent(targetData, status);

        var mockMeetingHud = new Mock<MeetingHud>(IntPtr.Zero);
        var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
        mockMeetingHudHelper.Setup(x => x.Invoke()).Returns(mockMeetingHud.Object);
        MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

        ExtremeRoleManager.GameRole.Clear(); // Target (id 1) not in GameRole

        bool result = ev.Invoke();

        Assert.False(result);
    }

    [Fact]
    public void OtherPlayerMeetingVisualUpdateEvent_Invoke_UpdatesVisuals_WhenTargetRoleExists()
    {
        var mockLocalData = CreateMockPlayerInfo(0, "LocalPlayer", isDead: true);
        var mockLocalPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockLocalPlayer.SetupGet(p => p.Data).Returns(mockLocalData);

        var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        mockLocalHelper.Setup(x => x.Invoke()).Returns(mockLocalPlayer.Object);
        MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

        var (pva, mockNameText, _) = CreateMockPlayerVoteArea();
        var status = new MeetingStatus(pva, false);
        var targetData = CreateMockPlayerInfo(1, "TargetPlayer");

        var ev = new OtherPlayerMeetingVisualUpdateEvent(targetData, status);

        var mockMeetingHud = new Mock<MeetingHud>(IntPtr.Zero);
        mockMeetingHud.SetupGet(m => m.state).Returns(MeetingHud.MeetingStates.Discussion);
        var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
        mockMeetingHudHelper.Setup(x => x.Invoke()).Returns(mockMeetingHud.Object);
        MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

        var localRole = new DummySingleRole(
            RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white),
            Palette.ClearWhite, "Sheriff", "");

        var targetRole = new DummySingleRole(
            RoleArgs.BuildImpostor(ExtremeRoleId.Assassin),
            Color.blue, "Impostor", "");

        ExtremeRoleManager.GameRole.Clear();
        ExtremeRoleManager.GameRole[0] = localRole;
        ExtremeRoleManager.GameRole[1] = targetRole;
        ExtremeGhostRoleManager.GameRole.Clear();

        bool result = ev.Invoke();

        Assert.True(result);
        Assert.Equal("TargetPlayer", mockNameText.Object.text);
    }

    [Fact]
    public void OtherPlayerMeetingVisualUpdateEvent_Invoke_HandlesLookedTag()
    {
        var mockLocalData = CreateMockPlayerInfo(0, "LocalPlayer", isDead: true);
        var mockLocalPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockLocalPlayer.SetupGet(p => p.Data).Returns(mockLocalData);

        var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        mockLocalHelper.Setup(x => x.Invoke()).Returns(mockLocalPlayer.Object);
        MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

        var (pva, mockNameText, _) = CreateMockPlayerVoteArea();
        var status = new MeetingStatus(pva, false);
        var targetData = CreateMockPlayerInfo(1, "TargetPlayer");

        var ev = new OtherPlayerMeetingVisualUpdateEvent(targetData, status);

        var mockMeetingHud = new Mock<MeetingHud>(IntPtr.Zero);
        mockMeetingHud.SetupGet(m => m.state).Returns(MeetingHud.MeetingStates.Discussion);
        var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
        mockMeetingHudHelper.Setup(x => x.Invoke()).Returns(mockMeetingHud.Object);
        MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

        var localRole = new DummySingleRole(
            RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white),
            Color.cyan, "Sheriff", "[LocalTag]");

        var targetRole = new DummySingleRole(
            RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white),
            Color.blue, "TargetRole", "", new DummyRoleVisual("[LookedTag]"));

        ExtremeRoleManager.GameRole.Clear();
        ExtremeRoleManager.GameRole[0] = localRole;
        ExtremeRoleManager.GameRole[1] = targetRole;
        ExtremeGhostRoleManager.GameRole.Clear();

        bool result = ev.Invoke();

        Assert.True(result);
        Assert.Equal("TargetPlayer[LocalTag][LookedTag]", mockNameText.Object.text);
    }

    [Fact]
    public void OtherPlayerMeetingVisualUpdateEvent_Invoke_BlockCondition_GuardianAngel()
    {
        var mockLocalData = CreateMockPlayerInfo(0, "LocalPlayer", isDead: true, roleType: RoleTypes.GuardianAngel);
        var mockLocalPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockLocalPlayer.SetupGet(p => p.Data).Returns(mockLocalData);

        var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        mockLocalHelper.Setup(x => x.Invoke()).Returns(mockLocalPlayer.Object);
        MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

        var (pva, _, mockInfoText) = CreateMockPlayerVoteArea();
        var status = new MeetingStatus(pva, false);
        var targetData = CreateMockPlayerInfo(1, "TargetPlayer");

        var ev = new OtherPlayerMeetingVisualUpdateEvent(targetData, status);

        var mockMeetingHud = new Mock<MeetingHud>(IntPtr.Zero);
        mockMeetingHud.SetupGet(m => m.state).Returns(MeetingHud.MeetingStates.Discussion);
        var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
        mockMeetingHudHelper.Setup(x => x.Invoke()).Returns(mockMeetingHud.Object);
        MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

        var localRole = new DummySingleRole(
            RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white),
            Color.cyan, "Sheriff", "");

        var targetRole = new DummySingleRole(
            RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white),
            Color.blue, "TargetRole", "");

        ExtremeRoleManager.GameRole.Clear();
        ExtremeRoleManager.GameRole[0] = localRole;
        ExtremeRoleManager.GameRole[1] = targetRole;

        bool result = ev.Invoke();

        Assert.True(result);
        // Meeting info is hidden when blockCondition is true
        Assert.False(mockInfoText.Object.gameObject.activeSelf);
    }
}