using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.InGameVisualUpdater;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Visual;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using TMPro;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module;

[Collection("UnityMock")]
public class InGameVisualUpdaterTests
{
	private sealed class DummySingleRole : SingleRoleBase
	{
		private readonly Color color;
		private readonly string roleName;
		private readonly string tag;
		private readonly IVisual? visual;
		private readonly bool blockInfo;

		public override IVisual? Visual => visual;

		public DummySingleRole(
			RoleArgs args,
			Color? color = null,
			string roleName = "TestRole",
			string tag = "",
			IVisual? visual = null,
			bool blockInfo = false)
			: base(args)
		{
			this.color = color ?? Color.white;
			this.roleName = roleName;
			this.tag = tag;
			this.visual = visual;
			this.blockInfo = blockInfo;
		}

		public override Color GetNameColor(bool isDead) => color;
		public override string GetColoredRoleName(bool isDead = false) => roleName;
		public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => tag;
		public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => color;
		public override bool IsBlockShowPlayingRoleInfo() => blockInfo;

		protected override void RoleSpecificInit() { }
		protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory parentOps) { }
	}

	private sealed class DummyUpdateRole : SingleRoleBase, IRoleUpdate
	{
		public bool WasUpdated { get; private set; }

		public DummyUpdateRole(RoleArgs args) : base(args) { }

		public void Update(PlayerControl player)
		{
			WasUpdated = true;
		}

		public override Color GetNameColor(bool isDead) => Color.white;
		public override string GetColoredRoleName(bool isDead = false) => "UpdateRole";
		public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => "";
		public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => Color.white;

		protected override void RoleSpecificInit() { }
		protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory parentOps) { }
	}

	private sealed class DummyMultiAssignRole : MultiAssignRoleBase
	{
		public bool OverrideCalled { get; private set; }

		public DummyMultiAssignRole(RoleArgs args) : base(args) { }

		public override void OverrideAnotherRoleSetting()
		{
			OverrideCalled = true;
		}

		public override Color GetNameColor(bool isDead) => Color.white;
		public override string GetColoredRoleName(bool isDead = false) => "MultiAssignRole";
		public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => "";
		public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => Color.white;

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
		private readonly Color seeColor;

		public DummyGhostRole(Color color, string name = "DummyGhost", Color? seeColor = null)
			: base(false, ExtremeRoleType.Crewmate, ExtremeGhostRoleId.VanillaRole, name, color)
		{
			this.seeColor = seeColor ?? Color.clear;
		}

		public override Color GetTargetRoleSeeColor(byte targetPlayerId, SingleRoleBase targetRole, GhostRoleBase? targetGhostRole) => seeColor;

		public override void CreateAbility() { }
		public override HashSet<ExtremeRoleId> GetRoleFilter() => [];
		public override void Initialize() { }
		protected override void OnMeetingEndHook() { }
		protected override void OnMeetingStartHook() { }
		protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory parentOps) { }
		protected override void UseAbility(RPCOperator.RpcCaller caller) { }
	}

	public InGameVisualUpdaterTests()
	{
		MockSetupHelper.SetupCommonMocks();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		if (ClientOption.Instance == null)
		{
			ClientOption.Create();
		}

		var mockHasTask = new Mock<MockPlayerTaskPlayerHasTaskOfTypeHelper>();
		mockHasTask.Setup(x => x.Invoke<IHudOverrideTask>(It.IsAny<PlayerControl>())).Returns(false);
		MockPlayerTaskPlayerHasTaskOfTypeHelper.Instance = mockHasTask.Object;

		var mockHud = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		var mockTaskPanel = new Mock<TaskPanelBehaviour>(IntPtr.Zero);
		var mockTabSprite = new Mock<SpriteRenderer>(IntPtr.Zero);
		var mockTabTransform = new Mock<Transform>(IntPtr.Zero);
		mockTabSprite.SetupGet(s => s.transform).Returns(mockTabTransform.Object);
		mockTaskPanel.SetupGet(f => f.tab).Returns(mockTabSprite.Object);
		mockHud.SetupGet(h => h.TaskPanel).Returns(mockTaskPanel.Object);

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
			.Returns((string id, string defaultStr, Il2CppReferenceArray<Il2CppSystem.Object> parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
			.Returns((StringNames id, Il2CppReferenceArray<Il2CppSystem.Object> parts) => id.ToString());

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
	}

	private static void SetupVector3Mocks()
	{
		var mockUp = new Mock<MockVector3get_upHelper>();
		mockUp.Setup(x => x.Invoke()).Returns(new Vector3(0f, 1f, 0f));
		MockVector3get_upHelper.Instance = mockUp.Object;

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

	private static void SetupInstantiateFor(UnityEngine.Object source, UnityEngine.Object result)
	{
		var m5 = new Mock<MockObjectInstantiateHelper5>();
		m5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>())).Returns(result);
		MockObjectInstantiateHelper5.Instance = m5.Object;

		var m10 = new Mock<MockObjectInstantiateHelper10>();
		m10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>())).Returns(result);
		MockObjectInstantiateHelper10.Instance = m10.Object;
	}

	private static (Mock<PlayerControl> playerMock, Mock<TextMeshPro> nameTextMock, Mock<TextMeshPro> infoTextMock, Mock<GameObject> infoGoMock) CreateMockPlayer(
		byte playerId = 0,
		string playerName = "TestPlayer",
		bool isDead = false,
		bool isImpostor = false,
		bool isVisible = true,
		RoleTypes roleType = RoleTypes.Crewmate,
		bool disconnected = true)
	{
		var playerMock = new Mock<PlayerControl>(IntPtr.Zero);
		playerMock.SetupGet(p => p.PlayerId).Returns(playerId);
		playerMock.SetupGet(p => p.Visible).Returns(isVisible);

		var tasksList = (Il2CppSystem.Collections.Generic.List<PlayerTask>)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(Il2CppSystem.Collections.Generic.List<PlayerTask>));
		IntPtr fakeListStructHandle = System.Runtime.InteropServices.Marshal.AllocHGlobal(64);
		unsafe
		{
			byte* ptr = (byte*)fakeListStructHandle;
			for (int i = 0; i < 64; i++) ptr[i] = 0;
		}
		typeof(Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)
			.GetField("<Pointer>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
			.SetValue(tasksList, fakeListStructHandle);
		playerMock.SetupGet(p => p.myTasks).Returns(tasksList);

		var outfitMock = new Mock<NetworkedPlayerInfo.PlayerOutfit>(IntPtr.Zero);
		outfitMock.SetupGet(o => o.PlayerName).Returns(playerName);
		playerMock.SetupGet(p => p.CurrentOutfit).Returns(outfitMock.Object);

		var dataMock = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		dataMock.SetupGet(d => d.PlayerId).Returns(playerId);
		dataMock.SetupGet(d => d.PlayerName).Returns(playerName);
		dataMock.SetupGet(d => d.IsDead).Returns(isDead);
		dataMock.SetupGet(d => d.Disconnected).Returns(disconnected);

		var roleBehaviourMock = new Mock<RoleBehaviour>(IntPtr.Zero);
		roleBehaviourMock.SetupGet(r => r.IsImpostor).Returns(isImpostor);
		roleBehaviourMock.SetupGet(r => r.Role).Returns(roleType);
		dataMock.SetupGet(d => d.Role).Returns(roleBehaviourMock.Object);

		playerMock.SetupGet(p => p.Data).Returns(dataMock.Object);

		var cosmeticsMock = new Mock<CosmeticsLayer>(IntPtr.Zero);
		var nameTextMock = new Mock<TextMeshPro>(IntPtr.Zero);
		nameTextMock.SetupProperty(t => t.text, playerName);
		nameTextMock.SetupProperty(t => t.color, Color.white);
		nameTextMock.SetupProperty(t => t.fontSize, 10f);

		var transformMock = new Mock<Transform>(IntPtr.Zero);
		transformMock.SetupProperty(t => t.localPosition, Vector3.zero);
		transformMock.SetupGet(t => t.parent).Returns((Transform)null!);
		nameTextMock.SetupGet(t => t.transform).Returns(transformMock.Object);

		cosmeticsMock.SetupGet(c => c.nameText).Returns(nameTextMock.Object);
		cosmeticsMock.Setup(c => c.SetName(It.IsAny<string>())).Callback<string>(s => nameTextMock.Object.text = s);
		cosmeticsMock.Setup(c => c.SetNameColor(It.IsAny<Color>())).Callback<Color>(c => nameTextMock.Object.color = c);

		playerMock.SetupGet(p => p.cosmetics).Returns(cosmeticsMock.Object);

		var infoTextMock = new Mock<TextMeshPro>(IntPtr.Zero);
		infoTextMock.SetupProperty(t => t.text, "");
		infoTextMock.SetupProperty(t => t.fontSize, 10f);

		var infoTransformMock = new Mock<Transform>(IntPtr.Zero);
		infoTransformMock.SetupProperty(t => t.localPosition, Vector3.zero);
		infoTextMock.SetupGet(t => t.transform).Returns(infoTransformMock.Object);

		var infoGoMock = new Mock<GameObject>(IntPtr.Zero);
		infoGoMock.Setup(g => g.SetActive(It.IsAny<bool>()));
		infoGoMock.SetupProperty(g => g.name, "");
		infoTextMock.SetupGet(t => t.gameObject).Returns(infoGoMock.Object);

		SetupInstantiateFor(nameTextMock.Object, infoTextMock.Object);

		return (playerMock, nameTextMock, infoTextMock, infoGoMock);
	}

	[Fact]
	public void LocalPlayerVisualUpdater_Update_ReturnsEarly_WhenPlayerInvalid()
	{
		var (playerMock, nameTextMock, _, _) = CreateMockPlayer();
		playerMock.SetupGet(p => p.Data).Returns((NetworkedPlayerInfo)null!);

		var updater = new LocalPlayerVisualUpdater(playerMock.Object);
		updater.Update();

		// SetName should not be called beyond initial setup
		Assert.Equal("TestPlayer", nameTextMock.Object.text);
	}

	[Fact]
	public void LocalPlayerVisualUpdater_Update_UpdatesNameAndColor_WhenValidPlayer()
	{
		var (playerMock, nameTextMock, infoTextMock, infoGoMock) = CreateMockPlayer(0, "LocalPlayer");
		var updater = new LocalPlayerVisualUpdater(playerMock.Object);

		var role = new DummySingleRole(
			RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white),
			Color.red, "SheriffRole", "[Sheriff]");

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[0] = role;
		ExtremeGhostRoleManager.GameRole.Clear();

		updater.Update();

		Assert.Equal("LocalPlayer[Sheriff]", nameTextMock.Object.text);
		Assert.Equal(Color.red, nameTextMock.Object.color);
		Assert.Equal("SheriffRole", infoTextMock.Object.text);
		Assert.Equal(ExtremeRoles.Module.InGameVisualUpdater.InGameVisualUpdaterBase.RoleInfoObjectName, infoTextMock.Object.gameObject.name);
		infoGoMock.Verify(g => g.SetActive(true), Times.AtLeastOnce());
	}

	[Fact]
	public void LocalPlayerVisualUpdater_Update_BlendsGhostRoleColorAndSetsGhostRoleName()
	{
		var (playerMock, nameTextMock, infoTextMock, _) = CreateMockPlayer(0, "LocalPlayer", isDead: true);
		var updater = new LocalPlayerVisualUpdater(playerMock.Object);

		var role = new DummySingleRole(
			RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white),
			new Color(1f, 0f, 0f, 1f), "SheriffRole", "");

		var ghostRole = new DummyGhostRole(new Color(0f, 1f, 0f, 1f), "GhostRole");

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[0] = role;
		ExtremeGhostRoleManager.GameRole.Clear();
		ExtremeGhostRoleManager.GameRole[0] = ghostRole;

		updater.Update();

		Assert.Equal(new Color(0.5f, 0.5f, 0f, 1f), nameTextMock.Object.color);
		Assert.Equal("<color=#FFFFFF>GhostRole</color>(SheriffRole)", infoTextMock.Object.text);
	}

	[Fact]
	public void LocalPlayerVisualUpdater_Update_CallsIRoleUpdateAndMultiAssignRole()
	{
		var (playerMock, _, _, _) = CreateMockPlayer(0, "LocalPlayer");
		var updater = new LocalPlayerVisualUpdater(playerMock.Object);

		var updateRole = new DummyUpdateRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white));
		var multiRole = new DummyMultiAssignRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white));
		multiRole.AnotherRole = updateRole;

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[0] = multiRole;
		ExtremeGhostRoleManager.GameRole.Clear();

		updater.Update();

		Assert.True(updateRole.WasUpdated);
		Assert.True(multiRole.OverrideCalled);
	}

	[Fact]
	public void LocalPlayerVisualUpdater_Update_DisablesInfo_WhenNotVisual()
	{
		var (playerMock, _, _, infoGoMock) = CreateMockPlayer(0, "LocalPlayer", isVisible: false);
		var updater = new LocalPlayerVisualUpdater(playerMock.Object);

		var role = new DummySingleRole(
			RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white),
			Color.white, "SheriffRole", "");

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[0] = role;
		ExtremeGhostRoleManager.GameRole.Clear();

		updater.Update();

		infoGoMock.Verify(g => g.SetActive(false), Times.AtLeastOnce());
	}

	[Fact]
	public void OtherPlayerVisualUpdater_Update_ReturnsEarly_WhenTargetRoleNotFound()
	{
		var (localMock, _, _, _) = CreateMockPlayer(0, "LocalPlayer");
		var (targetMock, targetNameText, _, _) = CreateMockPlayer(1, "TargetPlayer");

		var updater = new OtherPlayerVisualUpdater(localMock.Object, targetMock.Object);

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[0] = new DummySingleRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white));
		// Target role not in GameRole

		updater.Update();

		Assert.Equal("TargetPlayer", targetNameText.Object.text);
	}

	[Fact]
	public void OtherPlayerVisualUpdater_Update_UpdatesTargetVisuals_WhenGhostsSeeRoleActive()
	{
		var (localMock, _, _, _) = CreateMockPlayer(0, "LocalPlayer", isDead: true);
		var (targetMock, targetNameText, targetInfoText, targetInfoGoMock) = CreateMockPlayer(1, "TargetPlayer");

		ClientOption.Instance.GhostsSeeRole.Value = true;
		ClientOption.Instance.GhostsSeeTask.Value = true;

		var localRole = new DummySingleRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white), Color.white, "Sheriff");
		var targetRole = new DummySingleRole(RoleArgs.BuildImpostor(ExtremeRoleId.Assassin), Color.red, "Assassin", "[LookerTag]", new DummyRoleVisual("[LookedTag]"));

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[0] = localRole;
		ExtremeRoleManager.GameRole[1] = targetRole;
		ExtremeGhostRoleManager.GameRole.Clear();

		var updater = new OtherPlayerVisualUpdater(localMock.Object, targetMock.Object);
		updater.Update();

		Assert.Equal("TargetPlayer[LookedTag]", targetNameText.Object.text);
		Assert.Equal(Color.red, targetNameText.Object.color);
		Assert.Equal("Assassin", targetInfoText.Object.text);
		targetInfoGoMock.Verify(g => g.SetActive(true), Times.AtLeastOnce());
	}

	[Fact]
	public void OtherPlayerVisualUpdater_Update_PaintsGhostColor_WhenLocalGhostRoleExists()
	{
		var (localMock, _, _, _) = CreateMockPlayer(0, "LocalPlayer", isDead: false);
		var (targetMock, targetNameText, _, _) = CreateMockPlayer(1, "TargetPlayer");

		ClientOption.Instance.GhostsSeeRole.Value = false;

		var localRole = new DummySingleRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white), new Color(1f, 0f, 0f, 1f));
		var targetRole = new DummySingleRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white), Color.white);

		var localGhostRole = new DummyGhostRole(Color.white, "Ghost", seeColor: new Color(0f, 1f, 0f, 1f));

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[0] = localRole;
		ExtremeRoleManager.GameRole[1] = targetRole;
		ExtremeGhostRoleManager.GameRole.Clear();
		ExtremeGhostRoleManager.GameRole[0] = localGhostRole;

		var updater = new OtherPlayerVisualUpdater(localMock.Object, targetMock.Object);
		updater.Update();

		Assert.Equal(new Color(0.5f, 0.5f, 0f, 1f), targetNameText.Object.color);
	}

	[Fact]
	public void OtherPlayerVisualUpdater_Update_BlockCondition_GuardianAngel_HidesInfo()
	{
		var (localMock, _, _, _) = CreateMockPlayer(0, "LocalPlayer", isDead: true, roleType: RoleTypes.GuardianAngel);
		var (targetMock, _, _, targetInfoGoMock) = CreateMockPlayer(1, "TargetPlayer");

		ClientOption.Instance.GhostsSeeRole.Value = true;

		var localRole = new DummySingleRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white));
		var targetRole = new DummySingleRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white));

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[0] = localRole;
		ExtremeRoleManager.GameRole[1] = targetRole;

		var updater = new OtherPlayerVisualUpdater(localMock.Object, targetMock.Object);
		updater.Update();

		targetInfoGoMock.Verify(g => g.SetActive(false), Times.AtLeastOnce());
	}
}
