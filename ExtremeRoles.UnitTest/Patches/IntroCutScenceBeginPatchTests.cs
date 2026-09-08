using System;
using System.Collections.Generic;
using System.Reflection;
using AmongUs.GameOptions;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.IntroRunner;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Patches;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using UnityEngine;
using Xunit;

using Il2CppIEnumerator = Il2CppSystem.Collections.IEnumerator;
using PlayerIl2CppList = Il2CppSystem.Collections.Generic.List<PlayerControl>;

namespace ExtremeRoles.UnitTest.Patches;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class IntroCutScenceBeginPatchTests : IDisposable
{
	private class DummySingleRole : SingleRoleBase
	{
		public DummySingleRole(RoleCore core, IStatusModel? status = null)
		{
			var coreField = typeof(SingleRoleBase).GetField("<Core>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
			coreField?.SetValue(this, core);

			if (status != null)
			{
				var statusField = typeof(SingleRoleBase).GetField("<Status>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
				statusField?.SetValue(this, status);
			}
		}

		protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) { }
		protected override void RoleSpecificInit() { }

		public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => "";
		public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => Color.white;
	}

	private sealed class DummyMultiAssignRole : MultiAssignRoleBase
	{
		public DummyMultiAssignRole(RoleArgs args, SingleRoleBase? anotherRole = null) : base(args)
		{
			if (anotherRole != null)
			{
				this.CanHasAnotherRole = true;
				SetAnotherRole(anotherRole);
			}
		}

		public override Color GetNameColor(bool isDead) => Color.white;
		public override string GetColoredRoleName(bool isDead = false) => "MultiAssignRole";
		public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => "";
		public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => Color.white;

		protected override void RoleSpecificInit() { }
		protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory parentOps) { }
	}

	private sealed class DummySpecialSetUpRole : DummySingleRole, IRoleSpecialSetUp
	{
		public bool IntroBeginSetUpCalled { get; private set; }

		public DummySpecialSetUpRole(RoleCore core) : base(core) { }

		public void IntroBeginSetUp()
		{
			IntroBeginSetUpCalled = true;
		}

		public void IntroEndSetUp() { }
	}

	private sealed class DummyAwakeRole : DummySingleRole, IRoleAwake<RoleTypes>
	{
		public bool IsAwake { get; set; }
		public RoleTypes AwakeRole => RoleTypes.Crewmate;
		public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

		public DummyAwakeRole(RoleCore core, bool isAwake) : base(core)
		{
			IsAwake = isAwake;
		}

		public string GetFakeOptionString() => "";
		public void Update(PlayerControl player) { }
	}

	public IntroCutScenceBeginPatchTests()
	{
		ResetState();
	}

	public void Dispose()
	{
		ResetState();
	}

	private static void ResetState()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupTimeHelpers();
		MockSetupHelper.SetupPlayerControlMocks();
		ExtremeRoleManager.GameRole.Clear();
		PlayerCache.RemovePlayerControl(_ => true);
		ExtremeGameModeManager.Create(GameModes.Normal);

		if (MockObjectDontDestroyOnLoadHelper.Instance == null)
		{
			var mockDontDestroy = new Mock<MockObjectDontDestroyOnLoadHelper>();
			mockDontDestroy.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>()));
			MockObjectDontDestroyOnLoadHelper.Instance = mockDontDestroy.Object;
		}

		SetupInstantiateMocks();
	}

	private static void SetupInstantiateMocks()
	{
		var m5 = new Mock<MockObjectInstantiateHelper5>();
		m5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object orig, Transform parent) => orig);
		MockObjectInstantiateHelper5.Instance = m5.Object;

		var m10 = new Mock<MockObjectInstantiateHelper10>();
		m10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object orig, Transform parent) => orig);
		MockObjectInstantiateHelper10.Instance = m10.Object;
	}

	private static void SetupInstantiateReturn(UnityEngine.Object returnObj)
	{
		var asm = typeof(MockObjectInstantiateHelper5).Assembly;
		string[] helperNames = new[]
		{
			"MockObjectInstantiateHelper",
			"MockObjectInstantiateHelper2",
			"MockObjectInstantiateHelper3",
			"MockObjectInstantiateHelper4",
			"MockObjectInstantiateHelper5",
			"MockObjectInstantiateHelper6",
			"MockObjectInstantiateHelper7",
			"MockObjectInstantiateHelper8",
			"MockObjectInstantiateHelper9",
			"MockObjectInstantiateHelper10",
			"MockObjectInstantiateHelper11"
		};

		foreach (var name in helperNames)
		{
			var t = asm.GetType(name);
			if (t == null) continue;

			var prop = t.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
			if (prop == null) continue;

			try
			{
				var mockType = typeof(Mock<>).MakeGenericType(t);
				var mock = (Mock)Activator.CreateInstance(mockType)!;
				prop.SetValue(null, mock.Object);
			}
			catch { }
		}

		var m5 = new Mock<MockObjectInstantiateHelper5>();
		m5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object orig, Transform parent) => returnObj);
		MockObjectInstantiateHelper5.Instance = m5.Object;

		var m10 = new Mock<MockObjectInstantiateHelper10>();
		m10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object orig, Transform parent) => returnObj);
		MockObjectInstantiateHelper10.Instance = m10.Object;
	}

	#region IntroCutScenceBeginPatch Tests

	[Fact]
	public void BeginImpostorPrefix_WhenGameContextNotAvailable_DoesNotThrow()
	{
		var logger = new Mock<IModLogger>();
		var runtime = new Mock<IGameRuntime>();
		IGameContext? context = null;
		runtime.Setup(r => r.TryGetGameContext(out context)).Returns(false);

		var patch = new IntroCutScenceBeginPatch(logger.Object, runtime.Object);
		var mockIntro = new Mock<IntroCutscene>(IntPtr.Zero);
		var mockTeamList = new Mock<PlayerIl2CppList>(IntPtr.Zero);
		var teamList = mockTeamList.Object;

		patch.BeginImpostorPrefix(mockIntro.Object, ref teamList);
	}

	[Fact]
	public void BeginCrewmatePrefix_WhenGameContextNotAvailable_DoesNotThrow()
	{
		var logger = new Mock<IModLogger>();
		var runtime = new Mock<IGameRuntime>();
		IGameContext? context = null;
		runtime.Setup(r => r.TryGetGameContext(out context)).Returns(false);

		var patch = new IntroCutScenceBeginPatch(logger.Object, runtime.Object);
		var mockIntro = new Mock<IntroCutscene>(IntPtr.Zero);
		var mockTeamList = new Mock<PlayerIl2CppList>(IntPtr.Zero);
		var teamList = mockTeamList.Object;

		patch.BeginCrewmatePrefix(mockIntro.Object, ref teamList);
	}

	[Fact]
	public void BeginImpostorPostfix_WhenGameContextNotAvailable_DoesNotThrow()
	{
		var logger = new Mock<IModLogger>();
		var runtime = new Mock<IGameRuntime>();
		IGameContext? context = null;
		runtime.Setup(r => r.TryGetGameContext(out context)).Returns(false);

		var patch = new IntroCutScenceBeginPatch(logger.Object, runtime.Object);
		var mockIntro = new Mock<IntroCutscene>(IntPtr.Zero);

		patch.BeginImpostorPostfix(mockIntro.Object);
	}

	[Fact]
	public void BeginCrewmatePostfix_WhenGameContextNotAvailable_DoesNotThrow()
	{
		var logger = new Mock<IModLogger>();
		var runtime = new Mock<IGameRuntime>();
		IGameContext? context = null;
		runtime.Setup(r => r.TryGetGameContext(out context)).Returns(false);

		var patch = new IntroCutScenceBeginPatch(logger.Object, runtime.Object);
		var mockIntro = new Mock<IntroCutscene>(IntPtr.Zero);

		patch.BeginCrewmatePostfix(mockIntro.Object);
	}

	[Fact]
	public void BeginImpostorPrefix_WhenGameContextAvailable_AddsFakeTeam()
	{
		var logger = new Mock<IModLogger>();
		var runtime = new Mock<IGameRuntime>();

		var localPlayer = MockSetupHelper.SetupPlayerControlMocks();
		localPlayer.SetupGet(p => p.PlayerId).Returns((byte)0);

		var fakePlayer = new Mock<PlayerControl>(IntPtr.Zero);
		fakePlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
		mockLocalHelper.Setup(h => h.Invoke()).Returns(localPlayer.Object);
		MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

		// Setup role with IRoleFakeIntro status
		var mockStatus = new Mock<IRoleFakeIntro>();
		mockStatus.SetupGet(s => s.FakeTeam).Returns(ExtremeRoleType.Impostor);

		var fakeRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white), mockStatus.As<IStatusModel>().Object);
		var localRole = new DummySingleRole(RoleCore.BuildImpostor(ExtremeRoleId.Bait));

		ExtremeRoleManager.GameRole[(byte)0] = localRole;
		ExtremeRoleManager.GameRole[(byte)1] = fakeRole;

		PlayerCache.AddPlayerControl(localPlayer.Object);
		PlayerCache.AddPlayerControl(fakePlayer.Object);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(localRole);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		IGameContext? context = mockContext.Object;
		runtime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayerPrefab = new Mock<PoolablePlayer>(IntPtr.Zero);
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockPlayerPrefab.SetupGet(p => p.gameObject).Returns(mockGameObject.Object);

		SetupInstantiateReturn(mockPlayerPrefab.Object);

		var patch = new IntroCutScenceBeginPatch(logger.Object, runtime.Object);
		var mockIntro = new Mock<IntroCutscene>(IntPtr.Zero);
		mockIntro.SetupGet(i => i.PlayerPrefab).Returns(mockPlayerPrefab.Object);

		var mockTeamList = new Mock<PlayerIl2CppList>(IntPtr.Zero);
		var teamList = mockTeamList.Object;

		patch.BeginImpostorPrefix(mockIntro.Object, ref teamList);

		mockTeamList.Verify(t => t.Add(fakePlayer.Object), Times.Once);
		logger.Verify(l => l.LogTrace(It.Is<string>(s => s.Contains("Add fake Impostor: 1"))), Times.Once);
	}

	[Fact]
	public void BeginImpostorPrefix_WhenMultiAssignRoleAnotherRoleIsFakeIntro_AddsFakeTeam()
	{
		var logger = new Mock<IModLogger>();
		var runtime = new Mock<IGameRuntime>();

		var localPlayer = MockSetupHelper.SetupPlayerControlMocks();
		localPlayer.SetupGet(p => p.PlayerId).Returns((byte)0);

		var fakePlayer = new Mock<PlayerControl>(IntPtr.Zero);
		fakePlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
		mockLocalHelper.Setup(h => h.Invoke()).Returns(localPlayer.Object);
		MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

		var mockStatus = new Mock<IRoleFakeIntro>();
		mockStatus.SetupGet(s => s.FakeTeam).Returns(ExtremeRoleType.Impostor);

		var fakeAnotherRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white), mockStatus.As<IStatusModel>().Object);
		var fakeMultiRole = new DummyMultiAssignRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Lover, Color.white), fakeAnotherRole);

		var localRole = new DummySingleRole(RoleCore.BuildImpostor(ExtremeRoleId.Bait));

		ExtremeRoleManager.GameRole[(byte)0] = localRole;
		ExtremeRoleManager.GameRole[(byte)1] = fakeMultiRole;

		PlayerCache.AddPlayerControl(localPlayer.Object);
		PlayerCache.AddPlayerControl(fakePlayer.Object);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(localRole);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		IGameContext? context = mockContext.Object;
		runtime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockPlayerPrefab = new Mock<PoolablePlayer>(IntPtr.Zero);
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockPlayerPrefab.SetupGet(p => p.gameObject).Returns(mockGameObject.Object);

		SetupInstantiateReturn(mockPlayerPrefab.Object);

		var patch = new IntroCutScenceBeginPatch(logger.Object, runtime.Object);
		var mockIntro = new Mock<IntroCutscene>(IntPtr.Zero);
		mockIntro.SetupGet(i => i.PlayerPrefab).Returns(mockPlayerPrefab.Object);

		var mockTeamList = new Mock<PlayerIl2CppList>(IntPtr.Zero);
		var teamList = mockTeamList.Object;

		patch.BeginImpostorPrefix(mockIntro.Object, ref teamList);

		mockTeamList.Verify(t => t.Add(fakePlayer.Object), Times.Once);
		logger.Verify(l => l.LogTrace(It.Is<string>(s => s.Contains("Add fake Impostor: 1"))), Times.Once);
	}

	[Fact]
	public void CommonBeginPostfix_WhenRoleIsNeutralAndAwake_SetsNeutralTextAndColor()
	{
		var logger = new Mock<IModLogger>();
		var runtime = new Mock<IGameRuntime>();

		var neutralRole = new DummyAwakeRole(RoleCore.BuildNeutral(ExtremeRoleId.Yandere, Color.yellow), isAwake: true);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(neutralRole);
		IRoleAwake<RoleTypes>? awakeRole = neutralRole;
		IRoleAwake<RoleTypes>? subAwakeRole = null;
		mockRoleContainer.Setup(r => r.GetInterfaceCastedLocalRole<IRoleAwake<RoleTypes>>())
			.Returns((awakeRole, subAwakeRole));

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		IGameContext? context = mockContext.Object;
		runtime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var patch = new IntroCutScenceBeginPatch(logger.Object, runtime.Object);

		var mockIntro = new Mock<IntroCutscene>(IntPtr.Zero);
		var mockTeamTitle = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		var mockImpostorText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		var mockBackgroundBar = new Mock<MeshRenderer>(IntPtr.Zero);
		var mockMaterial = new Mock<Material>(IntPtr.Zero);

		mockBackgroundBar.SetupGet(b => b.material).Returns(mockMaterial.Object);
		mockIntro.SetupGet(i => i.TeamTitle).Returns(mockTeamTitle.Object);
		mockIntro.SetupGet(i => i.ImpostorText).Returns(mockImpostorText.Object);
		mockIntro.SetupGet(i => i.BackgroundBar).Returns(mockBackgroundBar.Object);

		patch.CommonBeginPostfix(mockIntro.Object);

		mockImpostorText.VerifySet(t => t.text = It.IsAny<string>(), Times.Once);
		mockTeamTitle.VerifySet(t => t.text = It.IsAny<string>(), Times.Once);
	}

	[Fact]
	public void CommonBeginPostfix_WhenRoleIsNeutralAndNotAwake_DoesNotSetText()
	{
		var logger = new Mock<IModLogger>();
		var runtime = new Mock<IGameRuntime>();

		var neutralRole = new DummyAwakeRole(RoleCore.BuildNeutral(ExtremeRoleId.Yandere, Color.yellow), isAwake: false);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(neutralRole);
		IRoleAwake<RoleTypes>? awakeRole = neutralRole;
		IRoleAwake<RoleTypes>? subAwakeRole = null;
		mockRoleContainer.Setup(r => r.GetInterfaceCastedLocalRole<IRoleAwake<RoleTypes>>())
			.Returns((awakeRole, subAwakeRole));

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		IGameContext? context = mockContext.Object;
		runtime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var patch = new IntroCutScenceBeginPatch(logger.Object, runtime.Object);

		var mockIntro = new Mock<IntroCutscene>(IntPtr.Zero);
		var mockTeamTitle = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		mockIntro.SetupGet(i => i.TeamTitle).Returns(mockTeamTitle.Object);

		patch.CommonBeginPostfix(mockIntro.Object);

		mockTeamTitle.VerifySet(t => t.text = It.IsAny<string>(), Times.Never);
	}

	[Fact]
	public void CommonBeginPostfix_WhenRoleIsXion_SetsXionBlueTextAndColor()
	{
		var logger = new Mock<IModLogger>();
		var runtime = new Mock<IGameRuntime>();

		var xionRole = new DummySingleRole(new RoleCore(ExtremeRoleId.Xion, ExtremeRoleType.Null, ColorPalette.XionBlue));

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(xionRole);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		IGameContext? context = mockContext.Object;
		runtime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var patch = new IntroCutScenceBeginPatch(logger.Object, runtime.Object);

		var mockIntro = new Mock<IntroCutscene>(IntPtr.Zero);
		var mockTeamTitle = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		var mockImpostorText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		var mockBackgroundBar = new Mock<MeshRenderer>(IntPtr.Zero);
		var mockMaterial = new Mock<Material>(IntPtr.Zero);

		mockBackgroundBar.SetupGet(b => b.material).Returns(mockMaterial.Object);
		mockIntro.SetupGet(i => i.TeamTitle).Returns(mockTeamTitle.Object);
		mockIntro.SetupGet(i => i.ImpostorText).Returns(mockImpostorText.Object);
		mockIntro.SetupGet(i => i.BackgroundBar).Returns(mockBackgroundBar.Object);

		patch.CommonBeginPostfix(mockIntro.Object);

		mockImpostorText.VerifySet(t => t.text = It.IsAny<string>(), Times.Once);
		mockTeamTitle.VerifySet(t => t.text = It.IsAny<string>(), Times.Once);
	}

	[Fact]
	public void CommonBeginPostfix_WhenRoleIsLiberal_SetsLiberalTextAndColor()
	{
		var logger = new Mock<IModLogger>();
		var runtime = new Mock<IGameRuntime>();

		var liberalRole = new DummySingleRole(RoleCore.BuildLiberal(ExtremeRoleId.Leader));

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(liberalRole);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		IGameContext? context = mockContext.Object;
		runtime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var patch = new IntroCutScenceBeginPatch(logger.Object, runtime.Object);

		var mockIntro = new Mock<IntroCutscene>(IntPtr.Zero);
		var mockTeamTitle = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		var mockImpostorText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		var mockBackgroundBar = new Mock<MeshRenderer>(IntPtr.Zero);
		var mockMaterial = new Mock<Material>(IntPtr.Zero);

		mockBackgroundBar.SetupGet(b => b.material).Returns(mockMaterial.Object);
		mockIntro.SetupGet(i => i.TeamTitle).Returns(mockTeamTitle.Object);
		mockIntro.SetupGet(i => i.ImpostorText).Returns(mockImpostorText.Object);
		mockIntro.SetupGet(i => i.BackgroundBar).Returns(mockBackgroundBar.Object);

		patch.CommonBeginPostfix(mockIntro.Object);

		mockImpostorText.VerifySet(t => t.text = It.IsAny<string>(), Times.Once);
		mockTeamTitle.VerifySet(t => t.text = It.IsAny<string>(), Times.Once);
	}

	[Fact]
	public void CommonBeginPostfix_CallsSpecialSetUp_WhenRoleImplementsIRoleSpecialSetUp()
	{
		var logger = new Mock<IModLogger>();
		var runtime = new Mock<IGameRuntime>();

		var specialRole = new DummySpecialSetUpRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white));

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(specialRole);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		IGameContext? context = mockContext.Object;
		runtime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var patch = new IntroCutScenceBeginPatch(logger.Object, runtime.Object);
		var mockIntro = new Mock<IntroCutscene>(IntPtr.Zero);

		patch.CommonBeginPostfix(mockIntro.Object);

		Assert.True(specialRole.IntroBeginSetUpCalled);
	}

	[Fact]
	public void CommonBeginPostfix_CallsSpecialSetUpOnMultiAssignAnotherRole()
	{
		var logger = new Mock<IModLogger>();
		var runtime = new Mock<IGameRuntime>();

		var specialAnotherRole = new DummySpecialSetUpRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white));
		var multiRole = new DummyMultiAssignRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Lover, Color.white), specialAnotherRole);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(multiRole);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		IGameContext? context = mockContext.Object;
		runtime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var patch = new IntroCutScenceBeginPatch(logger.Object, runtime.Object);
		var mockIntro = new Mock<IntroCutscene>(IntPtr.Zero);

		patch.CommonBeginPostfix(mockIntro.Object);

		Assert.True(specialAnotherRole.IntroBeginSetUpCalled);
	}

	[Fact]
	public void SetupIntroTeamIcons_WhenRoleIsLiberal_PopulatesTeamWithLiberalPlayers()
	{
		var logger = new Mock<IModLogger>();
		var runtime = new Mock<IGameRuntime>();

		var liberalRole = new DummySingleRole(RoleCore.BuildLiberal(ExtremeRoleId.Leader));

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(liberalRole);

		var p1 = new Mock<PlayerControl>(IntPtr.Zero);
		p1.SetupGet(p => p.PlayerId).Returns((byte)1);
		var p2 = new Mock<PlayerControl>(IntPtr.Zero);
		p2.SetupGet(p => p.PlayerId).Returns((byte)2);

		PlayerCache.AddPlayerControl(p1.Object);
		PlayerCache.AddPlayerControl(p2.Object);

		var liberalRole1 = new DummySingleRole(RoleCore.BuildLiberal(ExtremeRoleId.Leader));
		var otherRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white));

		SingleRoleBase outRole1 = liberalRole1;
		SingleRoleBase outRole2 = otherRole;
		mockRoleContainer.Setup(r => r.TryGetRole((byte)1, out outRole1)).Returns(true);
		mockRoleContainer.Setup(r => r.TryGetRole((byte)2, out outRole2)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var patch = new IntroCutScenceBeginPatch(logger.Object, runtime.Object);
		var mockTeamList = new Mock<PlayerIl2CppList>(IntPtr.Zero);
		var teamList = mockTeamList.Object;

		patch.setupIntroTeamIcons(mockContext.Object, ref teamList);

		mockTeamList.Verify(t => t.Clear(), Times.Once);
		mockTeamList.Verify(t => t.Add(p1.Object), Times.Once);
		mockTeamList.Verify(t => t.Add(p2.Object), Times.Never);
	}

	[Fact]
	public void SetupIntroTeamIcons_WhenRoleIsNeutralAndNotAwake_ReturnsEarlyWithoutModifyingTeam()
	{
		var logger = new Mock<IModLogger>();
		var runtime = new Mock<IGameRuntime>();

		var neutralRole = new DummyAwakeRole(RoleCore.BuildNeutral(ExtremeRoleId.Yandere, Color.yellow), isAwake: false);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(neutralRole);
		IRoleAwake<RoleTypes>? awakeRole = neutralRole;
		IRoleAwake<RoleTypes>? subAwakeRole = null;
		mockRoleContainer.Setup(r => r.GetInterfaceCastedLocalRole<IRoleAwake<RoleTypes>>())
			.Returns((awakeRole, subAwakeRole));

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var patch = new IntroCutScenceBeginPatch(logger.Object, runtime.Object);
		var mockTeamList = new Mock<PlayerIl2CppList>(IntPtr.Zero);
		var teamList = mockTeamList.Object;

		patch.setupIntroTeamIcons(mockContext.Object, ref teamList);

		Assert.Same(mockTeamList.Object, teamList);
		mockTeamList.Verify(t => t.Clear(), Times.Never);
	}

	#endregion

	#region IntroCutScenceCoBeginPatchBody Tests

	[Fact]
	public void CoBeginPrefix_WhenNoIntroRunner_StartsRuntimeAndReturnsTrue()
	{
		ExtremeGameModeManager.Create(GameModes.None);

		var progress = new Mock<IGameProgress>();
		var runtime = new Mock<IGameRuntime>();

		var patchBody = new IntroCutScenceCoBeginPatchBody(progress.Object, runtime.Object);
		var mockIntro = new Mock<IntroCutscene>(IntPtr.Zero);

		Il2CppIEnumerator? dummyResult = null;
		bool ret = patchBody.CoBeginPrefix(mockIntro.Object, ref dummyResult!);

		Assert.True(ret);
		runtime.Verify(r => r.Start(), Times.Once);
		progress.VerifySet(p => p.Current = GameProgressSystem.Progress.IntroStart, Times.Once);
	}

	#endregion

	#region IntroCutScenceShowRolePatchBody Tests

	[Fact]
	public void ShowRolePrefix_WhenGameContextNotAvailable_ReturnsTrue()
	{
		var runtime = new Mock<IGameRuntime>();
		IGameContext? context = null;
		runtime.Setup(r => r.TryGetGameContext(out context)).Returns(false);

		var patchBody = new IntroCutScenceShowRolePatchBody(runtime.Object);
		var mockIntro = new Mock<IntroCutscene>(IntPtr.Zero);
		Il2CppIEnumerator? result = null;

		bool ret = patchBody.ShowRolePrefix(mockIntro.Object, ref result!);

		Assert.True(ret);
	}

	[Fact]
	public void ShowRolePrefix_WhenRoleIsVanilla_ReturnsTrue()
	{
		var runtime = new Mock<IGameRuntime>();

		var vanillaRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.VanillaRole, Color.white));

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(vanillaRole);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		IGameContext? context = mockContext.Object;
		runtime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var patchBody = new IntroCutScenceShowRolePatchBody(runtime.Object);
		var mockIntro = new Mock<IntroCutscene>(IntPtr.Zero);
		Il2CppIEnumerator? result = null;

		bool ret = patchBody.ShowRolePrefix(mockIntro.Object, ref result!);

		Assert.True(ret);
	}

	[Fact]
	public void ShowRolePrefix_WhenAwakeVanillaRoleNotAwake_ReturnsTrue()
	{
		var runtime = new Mock<IGameRuntime>();

		var vanillaAwakeRole = new DummyAwakeRole(RoleCore.BuildCrewmate(ExtremeRoleId.VanillaRole, Color.white), isAwake: false);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(vanillaAwakeRole);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		IGameContext? context = mockContext.Object;
		runtime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var patchBody = new IntroCutScenceShowRolePatchBody(runtime.Object);
		var mockIntro = new Mock<IntroCutscene>(IntPtr.Zero);
		Il2CppIEnumerator? result = null;

		bool ret = patchBody.ShowRolePrefix(mockIntro.Object, ref result!);

		Assert.True(ret);
	}

	#endregion
}
