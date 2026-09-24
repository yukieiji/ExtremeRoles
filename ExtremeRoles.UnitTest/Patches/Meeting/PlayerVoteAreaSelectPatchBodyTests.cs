using System;
using AmongUs.GameOptions;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Patches.Meeting;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Roles.Solo;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Patches.Meeting;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class PlayerVoteAreaSelectPatchBodyTests : IDisposable
{
	private sealed class DummySingleRole : SingleRoleBase
	{
		public DummySingleRole(RoleCore core)
			: base(new RoleArgs(core, RoleProp.None))
		{
		}

		protected override void CreateSpecificOption(ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory factory) { }
		protected override void RoleSpecificInit() { }

		public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => "";
		public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => Color.white;
	}

	private sealed class DummyButtonRole : SingleRoleBase, IRoleMeetingButtonAbility
	{
		private readonly bool _isBlock;

		public DummyButtonRole(RoleCore core, bool isBlock = false)
			: base(new RoleArgs(core, RoleProp.None))
		{
			_isBlock = isBlock;
		}

		public Sprite AbilityImage => null!;
		public Action CreateAbilityAction(PlayerVoteArea area) => () => { };
		public bool IsBlockMeetingButtonAbility(PlayerVoteArea area) => _isBlock;
		public void ButtonMod(PlayerVoteArea area, UiElement element) { }

		protected override void CreateSpecificOption(ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory factory) { }
		protected override void RoleSpecificInit() { }

		public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => "";
		public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => Color.white;
	}

	private sealed class DummyMultiButtonRole : MultiAssignRoleBase, IRoleMeetingButtonAbility
	{
		public DummyMultiButtonRole(RoleCore core, SingleRoleBase anotherRole)
			: base(new RoleArgs(core, RoleProp.None))
		{
			this.CanHasAnotherRole = true;
			this.SetAnotherRole(anotherRole);
		}

		public Sprite AbilityImage => null!;
		public Action CreateAbilityAction(PlayerVoteArea area) => () => { };
		public bool IsBlockMeetingButtonAbility(PlayerVoteArea area) => false;
		public void ButtonMod(PlayerVoteArea area, UiElement element) { }

		protected override void CreateSpecificOption(ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory factory) { }
		protected override void RoleSpecificInit() { }

		public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => "";
		public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => Color.white;
	}

	public PlayerVoteAreaSelectPatchBodyTests()
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
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupLogger();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupPlayerVoteAreaMocks();

		var mockActionImplicit = new Mock<Il2CppSystem.MockActionop_ImplicitHelper<float>>();
		mockActionImplicit.Setup(x => x.Invoke(It.IsAny<Action<float>>()))
			.Returns((Action<float> act) => act != null ? new Il2CppSystem.Action<float>(IntPtr.Zero) : null!);
		Il2CppSystem.MockActionop_ImplicitHelper<float>.Instance = mockActionImplicit.Object;

		var mockLerpHelper = new Mock<MockEffectsLerpHelper>();
		MockEffectsLerpHelper.Instance = mockLerpHelper.Object;

		var mockAllHelper = new Mock<MockEffectsAllHelper>();
		mockAllHelper.Setup(x => x.Invoke(It.Is<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Collections.IEnumerator>>(_ => true)))
			.Returns((Il2CppSystem.Collections.IEnumerator)null!);
		MockEffectsAllHelper.Instance = mockAllHelper.Object;

		var mockAllHelper2 = new Mock<MockEffectsAllHelper2>();
		mockAllHelper2.Setup(x => x.Invoke(It.Is<Il2CppSystem.Collections.IEnumerator[]>(_ => true)))
			.Returns((Il2CppSystem.Collections.IEnumerator)null!);
		MockEffectsAllHelper2.Instance = mockAllHelper2.Object;

		MockPlayerControlget_LocalPlayerHelper.Instance = null;
		PlayerCache.RemovePlayerControl(_ => true);

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();

		if (MockPlayerVoteAreaget_SkippedVoteHelper.Instance == null)
		{
			var mockSkipped = new Mock<MockPlayerVoteAreaget_SkippedVoteHelper>();
			mockSkipped.Setup(h => h.Invoke()).Returns((byte)251);
			MockPlayerVoteAreaget_SkippedVoteHelper.Instance = mockSkipped.Object;
		}

		SetupGameOptionsManagerMock();

		if (ExtremeGameModeManager.Instance == null)
		{
			ExtremeGameModeManager.Create(GameModes.Normal);
		}

		var oneman = OnemanMeetingSystemManager.CreateOrGet();
		oneman.Reset(ResetTiming.MeetingEnd);

		if (ExtremeSystemTypeManager.Instance.TryGet<MonikaTrashSystem>(ExtremeSystemType.MonikaTrashSystem, out var currentMonika))
		{
			var rClear = new Mock<MessageReader>();
			rClear.SetupSequence(r => r.ReadByte())
				.Returns((byte)MonikaTrashSystem.Ops.ClearTrash);
			currentMonika.UpdateSystem(null!, rClear.Object);
		}
	}

	private static void SetupGameOptionsManagerMock()
	{
		var mockOptions = new Mock<IGameOptions>(IntPtr.Zero);
		var mockNormalOptions = new Mock<NormalGameOptionsV11>(IntPtr.Zero);

		var mockOptionsMgr = new Mock<GameOptionsManager>(IntPtr.Zero);
		mockOptionsMgr.SetupGet(m => m.CurrentGameOptions).Returns(mockOptions.Object);
		mockOptionsMgr.SetupGet(m => m.currentGameOptions).Returns(mockOptions.Object);
		mockOptionsMgr.SetupGet(m => m.currentNormalGameOptions).Returns(mockNormalOptions.Object);

		var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
		mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockOptionsMgr.Object);
		MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;
	}

	private static (PlayerVoteArea pva, Mock<MeetingHud> mockParent, Mock<GameObject> mockButtonsGameObject) CreateMockPlayerVoteArea(
		byte playerId = 1,
		bool voteComplete = false,
		bool parentSelectResult = true,
		bool amDead = false)
	{
		var mockPva = new Mock<PlayerVoteArea>(IntPtr.Zero);
		var mockParent = new Mock<MeetingHud>(IntPtr.Zero);
		var mockButtonsGameObject = new Mock<GameObject>(IntPtr.Zero);
		var mockButtonsTransform = new Mock<Transform>(IntPtr.Zero);

		mockButtonsGameObject.SetupGet(b => b.transform).Returns(mockButtonsTransform.Object);
		mockButtonsTransform.SetupGet(t => t.gameObject).Returns(mockButtonsGameObject.Object);

		var mockCancelBtn = new Mock<UiElement>(IntPtr.Zero);
		var mockConfirmBtn = new Mock<UiElement>(IntPtr.Zero);
		var mockConfirmTransform = new Mock<Transform>(IntPtr.Zero);

		var mockPassiveBtn = new Mock<PassiveButton>(IntPtr.Zero);
		var mockOnClick = new Mock<UnityEngine.UI.Button.ButtonClickedEvent>(IntPtr.Zero);
		var mockPersistentCalls = new Mock<UnityEngine.Events.PersistentCallGroup>(IntPtr.Zero);
		mockOnClick.SetupGet(o => o.m_PersistentCalls).Returns(mockPersistentCalls.Object);
		mockPassiveBtn.SetupGet(p => p.OnClick).Returns(mockOnClick.Object);
		mockCancelBtn.Setup(c => c.GetComponent<PassiveButton>()).Returns(mockPassiveBtn.Object);

		mockCancelBtn.SetupGet(c => c.name).Returns("CancelButton");
		mockConfirmBtn.SetupGet(c => c.name).Returns("ConfirmButton");
		mockConfirmBtn.SetupGet(c => c.transform).Returns(mockConfirmTransform.Object);

		mockPva.SetupGet(p => p.PlayerId).Returns(playerId);
		mockPva.SetupGet(p => p.AmDead).Returns(amDead);
		mockPva.SetupGet(p => p.VoteComplete).Returns(voteComplete);
		mockPva.SetupGet(p => p.Parent).Returns(mockParent.Object);
		mockPva.SetupGet(p => p.Buttons).Returns(mockButtonsGameObject.Object);
		mockPva.SetupGet(p => p.CancelButton).Returns(mockCancelBtn.Object);
		mockPva.SetupGet(p => p.ConfirmButton).Returns(mockConfirmBtn.Object);

		mockParent.Setup(p => p.Select(It.IsAny<int>())).Returns(parentSelectResult);

		var mockCacheBtn = new Mock<UiElement>(IntPtr.Zero);
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		var mockCacheTransform = new Mock<Transform>(IntPtr.Zero);

		mockCacheBtn.SetupGet(c => c.gameObject).Returns(mockGameObject.Object);
		mockCacheBtn.SetupGet(c => c.transform).Returns(mockCacheTransform.Object);
		mockCacheBtn.Setup(c => c.GetComponent<PassiveButton>()).Returns(mockPassiveBtn.Object);

		var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
		mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns(mockCacheBtn.Object);
		MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

		var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
		mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns(mockCacheBtn.Object);
		MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

		return (mockPva.Object, mockParent, mockButtonsGameObject);
	}

	[Fact]
	public void Prefix_WhenIsNotGameNow_ReturnsTrue()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(false);

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);
		var (pva, _, _) = CreateMockPlayerVoteArea();

		// Act
		bool result = patchBody.Prefix(pva);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenTryGetGameContextReturnsFalse_ReturnsTrue()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);
		IGameContext? ctx = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(false);

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);
		var (pva, _, _) = CreateMockPlayerVoteArea();

		// Act
		bool result = patchBody.Prefix(pva);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenVoteComplete_ReturnsFalse()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var role = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white));
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(role);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);
		var (pva, _, _) = CreateMockPlayerVoteArea(voteComplete: true);

		// Act
		bool result = patchBody.Prefix(pva);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenParentIsNull_ReturnsFalse()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var role = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white));
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(role);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);
		var mockPva = new Mock<PlayerVoteArea>(IntPtr.Zero);
		var mockCancelBtn = new Mock<UiElement>(IntPtr.Zero);
		var mockConfirmBtn = new Mock<UiElement>(IntPtr.Zero);
		mockCancelBtn.SetupGet(c => c.name).Returns("CancelButton");
		mockConfirmBtn.SetupGet(c => c.name).Returns("ConfirmButton");
		mockPva.SetupGet(p => p.CancelButton).Returns(mockCancelBtn.Object);
		mockPva.SetupGet(p => p.ConfirmButton).Returns(mockConfirmBtn.Object);
		mockPva.SetupGet(p => p.Parent).Returns((MeetingHud)null!);

		// Act
		bool result = patchBody.Prefix(mockPva.Object);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenParentSelectReturnsFalse_ReturnsFalse()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var role = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white));
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(role);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);
		var (pva, _, _) = CreateMockPlayerVoteArea(parentSelectResult: false);

		// Act
		bool result = patchBody.Prefix(pva);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenButtonEnumerableIsNull_ReturnsTrue()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		var role = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white));
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(role);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);
		var (pva, _, _) = CreateMockPlayerVoteArea(playerId: 1);

		// Act
		bool result = patchBody.Prefix(pva);

		// Assert
		Assert.True(result);
	}


	[Fact]
	public void Prefix_WhenValidButtonEnumerable_ActivatesButtonsAndOpensOverlayAndReturnsFalse()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockUiElementList = new Mock<Il2CppSystem.Collections.Generic.List<UiElement>>(IntPtr.Zero);
		mockObjectProvider.Setup(p => p.GetList<UiElement>()).Returns(mockUiElementList.Object);
		var mockLogger = new Mock<IModLogger>();

		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var role = new DummyButtonRole(RoleCore.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white));
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(role);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var mockControllerManager = new Mock<ControllerManager>(IntPtr.Zero);
		var mockControllerManagerHelper = new Mock<MockControllerManagerget_InstanceHelper>();
		mockControllerManagerHelper.Setup(h => h.Invoke()).Returns(mockControllerManager.Object);
		MockControllerManagerget_InstanceHelper.Instance = mockControllerManagerHelper.Object;

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);
		var (pva, _, mockButtonsGameObject) = CreateMockPlayerVoteArea(playerId: 1);

		// Act
		bool result = patchBody.Prefix(pva);

		// Assert
		Assert.False(result);
		mockButtonsGameObject.Verify(b => b.SetActive(true), Times.Once);
		mockControllerManager.Verify(c => c.OpenOverlayMenu(
			It.IsAny<string>(),
			It.IsAny<UiElement>(),
			It.IsAny<UiElement>(),
			It.IsAny<Il2CppSystem.Collections.Generic.List<UiElement>>(),
			false), Times.Once);
	}

	[Fact]
	public void TryGetMeetingButton_WhenLocalPlayerIsNull_ReturnsTrue()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		var mockLocalPlayerHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
		mockLocalPlayerHelper.Setup(h => h.Invoke()).Returns((PlayerControl)null!);
		MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalPlayerHelper.Object;

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);
		var (pva, _, _) = CreateMockPlayerVoteArea(playerId: 1);
		var mockContext = new Mock<IGameContext>();

		// Act
		bool result = patchBody.TryGetMeetingButton(pva, mockContext.Object, out var buttonEnumerable);

		// Assert
		Assert.True(result);
		Assert.Null(buttonEnumerable);
	}

	[Fact]
	public void TryGetMeetingButton_WhenOnemanMeetingSystemActive_ReturnsIsVotor()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		var localPlayer = MockSetupHelper.SetupPlayerControlMocks();
		localPlayer.SetupGet(p => p.PlayerId).Returns(0);

		var mockCaller = new Mock<PlayerControl>(IntPtr.Zero);
		mockCaller.SetupGet(p => p.PlayerId).Returns(0);
		var mockReporter = new Mock<PlayerControl>(IntPtr.Zero);
		mockReporter.SetupGet(p => p.PlayerId).Returns(1);

		var oneman = OnemanMeetingSystemManager.CreateOrGet();
		oneman.Start(mockCaller.Object, OnemanMeetingSystemManager.Type.CEO, mockReporter.Object);

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);
		var (pva, _, _) = CreateMockPlayerVoteArea(playerId: 1);
		var mockContext = new Mock<IGameContext>();

		// Act
		bool result = patchBody.TryGetMeetingButton(pva, mockContext.Object, out var buttonEnumerable);

		// Assert
		Assert.True(result);
		Assert.NotNull(buttonEnumerable);
	}

	[Fact]
	public void TryGetMeetingButton_WhenOnemanMeetingActiveAndNotCaller_ReturnsFalse()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		var localPlayer = MockSetupHelper.SetupPlayerControlMocks();
		localPlayer.SetupGet(p => p.PlayerId).Returns(1); // Not the caller (caller is 0)

		var mockCaller = new Mock<PlayerControl>(IntPtr.Zero);
		mockCaller.SetupGet(p => p.PlayerId).Returns(0);
		var mockReporter = new Mock<PlayerControl>(IntPtr.Zero);
		mockReporter.SetupGet(p => p.PlayerId).Returns(2);

		var oneman = OnemanMeetingSystemManager.CreateOrGet();
		oneman.Start(mockCaller.Object, OnemanMeetingSystemManager.Type.CEO, mockReporter.Object);

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);
		var (pva, _, _) = CreateMockPlayerVoteArea(playerId: 1);
		var mockContext = new Mock<IGameContext>();

		// Act
		bool result = patchBody.TryGetMeetingButton(pva, mockContext.Object, out var buttonEnumerable);

		// Assert
		Assert.False(result);
		Assert.NotNull(buttonEnumerable);
	}

	[Fact]
	public void TryGetMeetingButton_WhenMonikaTrashInvalidPlayer_ReturnsFalseAndNullResult()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		var localPlayer = MockSetupHelper.SetupPlayerControlMocks();
		localPlayer.SetupGet(p => p.PlayerId).Returns(0);

		var monikaSystem = new MonikaTrashSystem(false);
		ExtremeSystemTypeManager.Instance.TryAdd(ExtremeSystemType.MonikaTrashSystem, monikaSystem);

		var rAdd = new Mock<MessageReader>();
		rAdd.SetupSequence(r => r.ReadByte())
			.Returns((byte)MonikaTrashSystem.Ops.AddTrash)
			.Returns((byte)0);
		monikaSystem.UpdateSystem(null!, rAdd.Object);

		var role = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white));
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(role);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);
		var (pva, _, _) = CreateMockPlayerVoteArea(playerId: 1);

		// Act
		bool result = patchBody.TryGetMeetingButton(pva, mockContext.Object, out var buttonEnumerable);

		// Assert
		Assert.False(result);
		Assert.Null(buttonEnumerable);
	}

	[Fact]
	public void TryGetMeetingButton_WhenSingleRoleButtonAbility_ReturnsFirstRowButton()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		var role = new DummyButtonRole(RoleCore.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white));
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(role);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);
		var (pva, _, _) = CreateMockPlayerVoteArea(playerId: 1);

		// Act
		bool result = patchBody.TryGetMeetingButton(pva, mockContext.Object, out var buttonEnumerable);

		// Assert
		Assert.True(result);
		Assert.NotNull(buttonEnumerable);
	}

	[Fact]
	public void TryGetMeetingButton_WhenMultiRoleAnotherButtonAbility_ReturnsFirstRowButton()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		var mainRole = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white));
		var subRole = new DummyButtonRole(RoleCore.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white));
		var multiRole = new Lover();
		multiRole.CanHasAnotherRole = true;
		multiRole.SetAnotherRole(subRole);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(multiRole);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);
		var (pva, _, _) = CreateMockPlayerVoteArea(playerId: 1);

		// Act
		bool result = patchBody.TryGetMeetingButton(pva, mockContext.Object, out var buttonEnumerable);

		// Assert
		Assert.True(result);
		Assert.NotNull(buttonEnumerable);
	}

	[Fact]
	public void TryGetMeetingButton_WhenDualMeetingAbilityButton_ReturnsBothButtons()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		var subRole = new DummyButtonRole(RoleCore.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white));
		var multiRole = new DummyMultiButtonRole(RoleCore.BuildCrewmate(ExtremeRoleId.Lover, Color.white), subRole);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(multiRole);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);
		var (pva, _, _) = CreateMockPlayerVoteArea(playerId: 1);

		// Act
		bool result = patchBody.TryGetMeetingButton(pva, mockContext.Object, out var buttonEnumerable);

		// Assert
		Assert.True(result);
		Assert.NotNull(buttonEnumerable);
	}

	[Fact]
	public void TryGetMeetingButton_WhenNoButtonAbility_ReturnsTrueAndNullResult()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		var role = new DummySingleRole(RoleCore.BuildCrewmate(ExtremeRoleId.Bait, Color.white));
		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(role);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);
		var (pva, _, _) = CreateMockPlayerVoteArea(playerId: 1);

		// Act
		bool result = patchBody.TryGetMeetingButton(pva, mockContext.Object, out var buttonEnumerable);

		// Assert
		Assert.True(result);
		Assert.Null(buttonEnumerable);
	}

	[Fact]
	public void TryGetMeetingButton_WhenMultiedJudgeRole_WithUsableOverrule_CreatesJudgeButton()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		var localPlayer = MockSetupHelper.SetupPlayerControlMocks();
		var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		localPlayer.SetupGet(p => p.Data).Returns(mockPlayerInfo.Object);

		var mockRole = new Mock<JudgeRole>(IntPtr.Zero);
		mockRole.Setup(j => j.IsBlockedByTasks()).Returns(false);
		mockRole.SetupGet(j => j.HasAlreadyOverruledThisMeeting).Returns(false);
		mockRole.SetupGet(j => j.HasAnOverruleUse).Returns(true);
		mockPlayerInfo.SetupGet(d => d.Role).Returns(mockRole.Object);

		var abilityRole = new DummyButtonRole(RoleCore.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white));
		var vanillaJudgeRole = new VanillaRoleWrapper(RoleTypes.Judge);
		vanillaJudgeRole.CanHasAnotherRole = true;
		vanillaJudgeRole.SetAnotherRole(abilityRole);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(vanillaJudgeRole);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockOverruleButton = new Mock<UiElement>(IntPtr.Zero);
		var mockOverruleGameObject = new Mock<GameObject>(IntPtr.Zero);
		var mockOverruleCommsDisable = new Mock<GameObject>(IntPtr.Zero);
		var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);

		mockOverruleButton.SetupGet(o => o.gameObject).Returns(mockOverruleGameObject.Object);
		mockOverruleButton.Setup(o => o.GetComponent<SpriteRenderer>()).Returns(mockSpriteRenderer.Object);

		var (pva, _, _) = CreateMockPlayerVoteArea(playerId: 1);
		Mock.Get(pva).SetupGet(p => p.JudgeOverruleButton).Returns(mockOverruleButton.Object);
		Mock.Get(pva).SetupGet(p => p.JudgeOverruleButtonCommsDisable).Returns(mockOverruleCommsDisable.Object);

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);

		// Act
		bool result = patchBody.TryGetMeetingButton(pva, mockContext.Object, out var buttonEnumerable);

		// Assert
		Assert.True(result);
		Assert.NotNull(buttonEnumerable);
		mockOverruleGameObject.Verify(g => g.SetActive(true), Times.Once);
	}

	[Fact]
	public void TryGetMeetingButton_WhenMultiedJudgeRole_WithCommsDisabled_SetsCommsDisableActive()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		var localPlayer = MockSetupHelper.SetupPlayerControlMocks();
		var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		localPlayer.SetupGet(p => p.Data).Returns(mockPlayerInfo.Object);

		var mockRole = new Mock<JudgeRole>(IntPtr.Zero);
		mockRole.Setup(j => j.IsBlockedByTasks()).Returns(false);
		mockRole.SetupGet(j => j.HasAlreadyOverruledThisMeeting).Returns(false);
		mockRole.SetupGet(j => j.HasAnOverruleUse).Returns(true);
		mockRole.SetupGet(r => r.IsAffectedByComms).Returns(true);
		mockPlayerInfo.SetupGet(d => d.Role).Returns(mockRole.Object);

		var abilityRole = new DummyButtonRole(RoleCore.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white));
		var vanillaJudgeRole = new VanillaRoleWrapper(RoleTypes.Judge);
		vanillaJudgeRole.CanHasAnotherRole = true;
		vanillaJudgeRole.SetAnotherRole(abilityRole);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(vanillaJudgeRole);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockOverruleButton = new Mock<UiElement>(IntPtr.Zero);
		var mockOverruleGameObject = new Mock<GameObject>(IntPtr.Zero);
		var mockOverruleCommsDisable = new Mock<GameObject>(IntPtr.Zero);
		var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);

		mockOverruleButton.SetupGet(o => o.gameObject).Returns(mockOverruleGameObject.Object);
		mockOverruleButton.Setup(o => o.GetComponent<SpriteRenderer>()).Returns(mockSpriteRenderer.Object);

		var (pva, _, _) = CreateMockPlayerVoteArea(playerId: 1);
		Mock.Get(pva).SetupGet(p => p.JudgeOverruleButton).Returns(mockOverruleButton.Object);
		Mock.Get(pva).SetupGet(p => p.JudgeOverruleButtonCommsDisable).Returns(mockOverruleCommsDisable.Object);

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);

		// Act
		bool result = patchBody.TryGetMeetingButton(pva, mockContext.Object, out var buttonEnumerable);

		// Assert
		Assert.True(result);
		Assert.NotNull(buttonEnumerable);
		mockOverruleCommsDisable.Verify(g => g.SetActive(true), Times.Once);
	}

	[Fact]
	public void TryGetMeetingButton_WhenMultiedJudgeRole_UnusableOverrule_HidesOverruleButton()
	{
		// Arrange
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		var mockObjectProvider = new Mock<IIl2CppObjectProvider>();
		var mockLogger = new Mock<IModLogger>();

		var localPlayer = MockSetupHelper.SetupPlayerControlMocks();
		var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		localPlayer.SetupGet(p => p.Data).Returns(mockPlayerInfo.Object);

		var mockRole = new Mock<JudgeRole>(IntPtr.Zero);
		mockRole.Setup(j => j.IsBlockedByTasks()).Returns(true); // Overrule blocked by tasks
		mockPlayerInfo.SetupGet(d => d.Role).Returns(mockRole.Object);

		var abilityRole = new DummyButtonRole(RoleCore.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white));
		var vanillaJudgeRole = new VanillaRoleWrapper(RoleTypes.Judge);
		vanillaJudgeRole.CanHasAnotherRole = true;
		vanillaJudgeRole.SetAnotherRole(abilityRole);

		var mockRoleContainer = new Mock<INomalGameRoleContainer>();
		mockRoleContainer.Setup(r => r.GetLocalPlayerRole()).Returns(vanillaJudgeRole);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoleContainer.Object);

		var mockOverruleButton = new Mock<UiElement>(IntPtr.Zero);
		var mockOverruleGameObject = new Mock<GameObject>(IntPtr.Zero);
		var mockOverruleCommsDisable = new Mock<GameObject>(IntPtr.Zero);

		mockOverruleButton.SetupGet(o => o.gameObject).Returns(mockOverruleGameObject.Object);

		var (pva, _, _) = CreateMockPlayerVoteArea(playerId: 1);
		Mock.Get(pva).SetupGet(p => p.JudgeOverruleButton).Returns(mockOverruleButton.Object);
		Mock.Get(pva).SetupGet(p => p.JudgeOverruleButtonCommsDisable).Returns(mockOverruleCommsDisable.Object);

		var patchBody = new PlayerVoteAreaSelectPatchBody(mockProgress.Object, mockRuntime.Object, mockObjectProvider.Object, mockLogger.Object);

		// Act
		bool result = patchBody.TryGetMeetingButton(pva, mockContext.Object, out var buttonEnumerable);

		// Assert
		Assert.True(result);
		Assert.NotNull(buttonEnumerable);
		mockOverruleGameObject.Verify(g => g.SetActive(false), Times.AtLeastOnce);
	}
}
