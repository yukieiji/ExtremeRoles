using System;
using System.Runtime.CompilerServices;
using AmongUs.GameOptions;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using InnerNet;
using MonoMod.RuntimeDetour;
using Moq;
using UnityEngine;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.JailerTests;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class JailerRoleTests : IDisposable
{
	private delegate float Vector2MagOrig(ref Vector2 self);
	private delegate float Vector2MagHook(Vector2MagOrig orig, ref Vector2 self);

	private delegate Vector2 Vector2NormalizedOrig(ref Vector2 self);
	private delegate Vector2 Vector2NormalizedHook(Vector2NormalizedOrig orig, ref Vector2 self);

	private delegate void Vector2CtorOrig(ref Vector2 self, float x, float y);
	private delegate void Vector2CtorHook(Vector2CtorOrig orig, ref Vector2 self, float x, float y);

	private delegate bool AnyNonTriggersBetweenOrig(Vector2 src, Vector2 dir, float dist, int layerMask);
	private delegate bool AnyNonTriggersBetweenHook(AnyNonTriggersBetweenOrig orig, Vector2 src, Vector2 dir, float dist, int layerMask);

	private delegate void SetOutlineOrig(PlayerControl target, Color color);
	private delegate void SetOutlineHook(SetOutlineOrig orig, PlayerControl target, Color color);

	private readonly Hook magHook;
	private readonly Hook normHook;
	private readonly Hook ctorHook;
	private readonly Hook physicsHook;
	private readonly Hook outlineHook;
	private readonly Mock<AmongUsClient> clientMock;

	public JailerRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();

		var mockMask = new Mock<MockConstantsget_ShipAndObjectsMaskHelper>();
		mockMask.Setup(x => x.Invoke()).Returns(1);
		MockConstantsget_ShipAndObjectsMaskHelper.Instance = mockMask.Object;

		var mockSub = new Mock<MockVector2op_SubtractionHelper>();
		mockSub.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>()))
			.Returns((Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y));
		MockVector2op_SubtractionHelper.Instance = mockSub.Object;

		var magTarget = typeof(Vector2).GetProperty("magnitude")!.GetGetMethod()!;
		var magHookDelegate = new Vector2MagHook(getMagnitudeHook);
		this.magHook = new Hook(magTarget, magHookDelegate);

		var normTarget = typeof(Vector2).GetProperty("normalized")!.GetGetMethod()!;
		var normHookDelegate = new Vector2NormalizedHook(getNormalizedHook);
		this.normHook = new Hook(normTarget, normHookDelegate);

		var ctorTarget = typeof(Vector2).GetConstructor(new[] { typeof(float), typeof(float) })!;
		var ctorHookDelegate = new Vector2CtorHook(getCtorHook);
		this.ctorHook = new Hook(ctorTarget, ctorHookDelegate);

		var physicsTarget = typeof(PhysicsHelpers).GetMethod(nameof(PhysicsHelpers.AnyNonTriggersBetween), new[] { typeof(Vector2), typeof(Vector2), typeof(float), typeof(int) })!;
		var physicsHookDelegate = new AnyNonTriggersBetweenHook(getPhysicsHook);
		this.physicsHook = new Hook(physicsTarget, physicsHookDelegate);

		var outlineTarget = typeof(PlayerOutLine).GetMethod(nameof(PlayerOutLine.SetOutline), new[] { typeof(PlayerControl), typeof(Color) })!;
		var outlineHookDelegate = new SetOutlineHook(getSetOutlineHook);
		this.outlineHook = new Hook(outlineTarget, outlineHookDelegate);

		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		var mockShipStatusHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipStatusHelper.Setup(x => x.Invoke()).Returns(mockShipStatus.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipStatusHelper.Object;

		var mockAllPlayers = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		mockAllPlayers.SetupGet(a => a.Count).Returns(0);

		var mockGameData = new Mock<GameData>(IntPtr.Zero);
		mockGameData.SetupGet(g => g.AllPlayers).Returns(mockAllPlayers.Object);

		var mockGameDataHelper = new Mock<MockGameDataget_InstanceHelper>();
		mockGameDataHelper.Setup(x => x.Invoke()).Returns(mockGameData.Object);
		MockGameDataget_InstanceHelper.Instance = mockGameDataHelper.Object;

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupPlayerControlMocks();
		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);

		clientMock = MockSetupHelper.SetupAmongUsClientMock();
		SetLobbyMode(false);
	}

	public void Dispose()
	{
		this.magHook.Dispose();
		this.normHook.Dispose();
		this.ctorHook.Dispose();
		this.physicsHook.Dispose();
		this.outlineHook.Dispose();
	}

	private static float getMagnitudeHook(Vector2MagOrig orig, ref Vector2 self)
	{
		return (float)Math.Sqrt(self.x * self.x + self.y * self.y);
	}

	private static Vector2 getNormalizedHook(Vector2NormalizedOrig orig, ref Vector2 self)
	{
		float mag = (float)Math.Sqrt(self.x * self.x + self.y * self.y);
		return mag > 0.00001f ? new Vector2(self.x / mag, self.y / mag) : new Vector2(0f, 0f);
	}

	private static void getCtorHook(Vector2CtorOrig orig, ref Vector2 self, float x, float y)
	{
		self.x = x;
		self.y = y;
	}

	private static bool getPhysicsHook(AnyNonTriggersBetweenOrig orig, Vector2 src, Vector2 dir, float dist, int layerMask)
	{
		return false;
	}

	private static void getSetOutlineHook(SetOutlineOrig orig, PlayerControl target, Color color)
	{
	}

	private void SetLobbyMode(bool isLobby)
	{
		if (isLobby)
		{
			clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.NotJoined);
			var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
			var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
			mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
			MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
		}
		else
		{
			clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
			var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
			mockLobbyHelper.Setup(x => x.Invoke()).Returns((LobbyBehaviour)null!);
			MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
		}
	}

	[Fact]
	public void CreateRoleAllOption_CreatesExpectedOptions()
	{
		int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Jailer);
		var jailer = new Jailer();

		jailer.CreateRoleAllOption();

		Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
		Assert.NotNull(category);

		Assert.True(jailer.Loader.TryGet(Jailer.Option.AwakeTaskGage, out var awakeTaskGageOpt));
		Assert.NotNull(awakeTaskGageOpt);

		Assert.True(jailer.Loader.TryGet(Jailer.Option.TargetMode, out var targetModeOpt));
		Assert.NotNull(targetModeOpt);
	}

	[Theory]
	[InlineData(true, false, true)]
	[InlineData(false, true, true)]
	[InlineData(false, false, false)]
	public void IsAwake_ReturnsExpectedValue(bool isLobby, bool awakeRoleState, bool expectedIsAwake)
	{
		SetLobbyMode(isLobby);
		var jailer = new Jailer();
		jailer.CreateRoleAllOption();

		if (jailer.Loader.TryGet(Jailer.Option.AwakeTaskGage, out var gageOpt) && gageOpt != null)
		{
			gageOpt.Selection = awakeRoleState ? 0 : 7;
		}
		if (jailer.Loader.TryGet(Jailer.Option.AwakeDeadPlayerNum, out var deadOpt) && deadOpt != null)
		{
			deadOpt.Selection = awakeRoleState ? 0 : 5;
		}

		jailer.Initialize();

		bool isAwake = jailer.IsAwake;

		Assert.Equal(expectedIsAwake, isAwake);
	}

	[Fact]
	public void IsAwake_WhenAwakeRoleIsTrue_ReturnsTrue()
	{
		SetLobbyMode(false);
		var jailer = new Jailer();
		jailer.CreateRoleAllOption();

		if (jailer.Loader.TryGet(Jailer.Option.AwakeTaskGage, out var gageOpt) && gageOpt != null)
		{
			gageOpt.Selection = 0;
		}
		if (jailer.Loader.TryGet(Jailer.Option.AwakeDeadPlayerNum, out var deadOpt) && deadOpt != null)
		{
			deadOpt.Selection = 0;
		}

		jailer.Initialize();

		bool isAwake = jailer.IsAwake;

		Assert.True(isAwake);
	}

	[Fact]
	public void IsAbilityUse_WhenNoPlayerInRange_ReturnsFalse()
	{
		var jailer = new Jailer();

		bool result = jailer.IsAbilityUse();

		Assert.False(result);
	}

	[Fact]
	public void IsAbilityUse_WhenValidTargetInRange_ReturnsTrue()
	{
		byte localId = 1;
		byte targetId = 2;

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns(localId);
		localPlayerMock.SetupGet(p => p.CanMove).Returns(true);

		var mockTargetControl = new Mock<PlayerControl>(IntPtr.Zero);
		mockTargetControl.SetupGet(p => p.PlayerId).Returns(targetId);
		mockTargetControl.SetupGet(p => p.inVent).Returns(false);
		mockTargetControl.SetupGet(p => p.inMovingPlat).Returns(false);
		mockTargetControl.SetupGet(p => p.onLadder).Returns(false);

		PlayerCache.AddPlayerControl(localPlayerMock.Object);
		PlayerCache.AddPlayerControl(mockTargetControl.Object);

		var mockTargetInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTargetInfo.SetupGet(t => t.PlayerId).Returns(targetId);
		mockTargetInfo.SetupGet(t => t.IsDead).Returns(false);
		mockTargetInfo.SetupGet(t => t.Disconnected).Returns(false);
		mockTargetInfo.SetupGet(t => t.Object).Returns(mockTargetControl.Object);

		var mockSourceInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockSourceInfo.SetupGet(t => t.PlayerId).Returns(localId);
		mockSourceInfo.SetupGet(t => t.IsDead).Returns(false);
		mockSourceInfo.SetupGet(t => t.Disconnected).Returns(false);
		mockSourceInfo.SetupGet(t => t.Object).Returns(localPlayerMock.Object);
		localPlayerMock.SetupGet(p => p.Data).Returns(mockSourceInfo.Object);

		var allPlayersList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		allPlayersList.SetupGet(a => a.Count).Returns(2);
		allPlayersList.Setup(a => a[0]).Returns(mockSourceInfo.Object);
		allPlayersList.Setup(a => a[1]).Returns(mockTargetInfo.Object);

		var mockGameData = new Mock<GameData>(IntPtr.Zero);
		mockGameData.SetupGet(g => g.AllPlayers).Returns(allPlayersList.Object);

		var mockGameDataHelper = new Mock<MockGameDataget_InstanceHelper>();
		mockGameDataHelper.Setup(x => x.Invoke()).Returns(mockGameData.Object);
		MockGameDataget_InstanceHelper.Instance = mockGameDataHelper.Object;

		var jailer = new Jailer();
		var targetRole = new ExtremeRoles.Roles.Solo.Neutral.TaskMaster();

		ExtremeRoleManager.GameRole[localId] = jailer;
		ExtremeRoleManager.GameRole[targetId] = targetRole;

		bool result = jailer.IsAbilityUse();

		Assert.True(result);
	}

	[Fact]
	public void UseAbility_WhenButtonOrRoleNull_ReturnsFalse()
	{
		var jailer = new Jailer();

		bool result = jailer.UseAbility();

		Assert.False(result);
	}

	[Fact]
	public void NotCrewmateToYardbird_WhenTargetPlayerNotFound_DoesNotThrow()
	{
		Jailer.NotCrewmateToYardbird(1, 2);
	}

	[Fact]
	public void ToLawbreaker_WhenJailerRoleNotFound_DoesNotThrow()
	{
		Jailer.ToLawbreaker(1);
	}

	[Fact]
	public void Update_WhenNotInTaskPhase_DoesNothing()
	{
		SetLobbyMode(false);
		var jailer = new Jailer();
		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();

		jailer.Update(mockPlayer.Object);

		Assert.False(jailer.IsAwake);
	}

	[Theory]
	[InlineData(true, true)]
	[InlineData(true, false)]
	[InlineData(false, true)]
	public void GetColoredRoleName_WhenAwakeOrTruthColor_ReturnsColoredRoleName(bool isTruthColor, bool isAwake)
	{
		SetLobbyMode(isAwake);
		var jailer = new Jailer();

		string roleName = jailer.GetColoredRoleName(isTruthColor);

		Assert.NotNull(roleName);
		Assert.Contains("Jailer", roleName);
	}

	[Fact]
	public void GetColoredRoleName_WhenNotAwakeAndNotTruthColor_ReturnsWhiteCrewmateName()
	{
		SetLobbyMode(false);
		var jailer = new Jailer();
		jailer.CreateRoleAllOption();

		if (jailer.Loader.TryGet(Jailer.Option.AwakeTaskGage, out var gageOpt) && gageOpt != null)
		{
			gageOpt.Selection = 7;
		}
		if (jailer.Loader.TryGet(Jailer.Option.AwakeDeadPlayerNum, out var deadOpt) && deadOpt != null)
		{
			deadOpt.Selection = 5;
		}
		jailer.Initialize();

		string roleName = jailer.GetColoredRoleName(false);

		Assert.NotNull(roleName);
		Assert.Contains("Crewmate", roleName);
	}

	[Fact]
	public void GetFullDescription_WhenAwake_ReturnsJailerFullDescription()
	{
		SetLobbyMode(true);
		var jailer = new Jailer();

		string description = jailer.GetFullDescription();

		Assert.NotNull(description);
		Assert.Equal("JailerFullDescription", description);
	}

	[Fact]
	public void GetFullDescription_WhenNotAwake_ReturnsCrewmateFullDescription()
	{
		SetLobbyMode(false);
		var jailer = new Jailer();
		jailer.CreateRoleAllOption();

		if (jailer.Loader.TryGet(Jailer.Option.AwakeTaskGage, out var gageOpt) && gageOpt != null)
		{
			gageOpt.Selection = 7;
		}
		if (jailer.Loader.TryGet(Jailer.Option.AwakeDeadPlayerNum, out var deadOpt) && deadOpt != null)
		{
			deadOpt.Selection = 5;
		}
		jailer.Initialize();

		string description = jailer.GetFullDescription();

		Assert.NotNull(description);
		Assert.Equal("CrewmateFullDescription", description);
	}

	[Fact]
	public void GetImportantText_WhenAwake_ReturnsJailerImportantText()
	{
		SetLobbyMode(true);
		var jailer = new Jailer();

		string text = jailer.GetImportantText(true);

		Assert.NotNull(text);
		Assert.Contains("Jailer", text);
	}

	[Fact]
	public void GetImportantText_WhenNotAwake_ReturnsCrewmateImportantText()
	{
		SetLobbyMode(false);
		var jailer = new Jailer();
		jailer.CreateRoleAllOption();

		if (jailer.Loader.TryGet(Jailer.Option.AwakeTaskGage, out var gageOpt) && gageOpt != null)
		{
			gageOpt.Selection = 7;
		}
		if (jailer.Loader.TryGet(Jailer.Option.AwakeDeadPlayerNum, out var deadOpt) && deadOpt != null)
		{
			deadOpt.Selection = 5;
		}
		jailer.Initialize();

		string text = jailer.GetImportantText(true);

		Assert.NotNull(text);
		Assert.Contains("crewImportantText", text);
	}

	[Fact]
	public void GetIntroDescription_WhenAwake_ReturnsBaseIntroDescription()
	{
		SetLobbyMode(true);
		var jailer = new Jailer();

		string desc = jailer.GetIntroDescription();

		Assert.NotNull(desc);
		Assert.Contains("JailerIntroDescription", desc);
	}

	[Fact]
	public void GetIntroDescription_WhenNotAwake_ReturnsCrewmateIntroDescription()
	{
		SetLobbyMode(false);
		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();

		var mockRole = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRole.SetupGet(r => r.Blurb).Returns("Crewmate Blurb");

		var mockInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockInfo.SetupGet(i => i.Role).Returns(mockRole.Object);
		localPlayerMock.SetupGet(p => p.Data).Returns(mockInfo.Object);

		var jailer = new Jailer();
		jailer.CreateRoleAllOption();

		if (jailer.Loader.TryGet(Jailer.Option.AwakeTaskGage, out var gageOpt) && gageOpt != null)
		{
			gageOpt.Selection = 7;
		}
		if (jailer.Loader.TryGet(Jailer.Option.AwakeDeadPlayerNum, out var deadOpt) && deadOpt != null)
		{
			deadOpt.Selection = 5;
		}
		jailer.Initialize();

		string desc = jailer.GetIntroDescription();

		Assert.NotNull(desc);
		Assert.Contains("Crewmate Blurb", desc);
	}

	[Theory]
	[InlineData(true, true)]
	[InlineData(true, false)]
	[InlineData(false, true)]
	public void GetNameColor_WhenAwakeOrTruthColor_ReturnsRoleColor(bool isTruthColor, bool isAwake)
	{
		SetLobbyMode(isAwake);
		var jailer = new Jailer();

		Color color = jailer.GetNameColor(isTruthColor);

		Assert.Equal(ColorPalette.JailerSapin, color);
	}

	[Fact]
	public void GetNameColor_WhenNotAwakeAndNotTruthColor_ReturnsWhite()
	{
		SetLobbyMode(false);
		var jailer = new Jailer();
		jailer.CreateRoleAllOption();

		if (jailer.Loader.TryGet(Jailer.Option.AwakeTaskGage, out var gageOpt) && gageOpt != null)
		{
			gageOpt.Selection = 7;
		}
		if (jailer.Loader.TryGet(Jailer.Option.AwakeDeadPlayerNum, out var deadOpt) && deadOpt != null)
		{
			deadOpt.Selection = 5;
		}
		jailer.Initialize();

		Color color = jailer.GetNameColor(false);

		Assert.Equal(Palette.White, color);
	}
}
