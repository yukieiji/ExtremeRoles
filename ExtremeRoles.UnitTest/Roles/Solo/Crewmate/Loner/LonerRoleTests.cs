using System;
using System.Collections.Generic;
using System.Reflection;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate.Loner;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MonoMod.RuntimeDetour;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Loner;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class LonerRoleTests : IDisposable
{
	private delegate float Vector2MagOrig(ref Vector2 self);
	private delegate float Vector2MagHook(Vector2MagOrig orig, ref Vector2 self);

	private delegate void Vector2CtorOrig(ref Vector2 self, float x, float y);
	private delegate void Vector2CtorHook(Vector2CtorOrig orig, ref Vector2 self, float x, float y);

	private delegate void ArrowCtorOrig(Arrow self, Color color);
	private delegate void ArrowCtorHook(ArrowCtorOrig orig, Arrow self, Color color);

	private delegate void ArrowSetActiveOrig(Arrow self, bool active);
	private delegate void ArrowSetActiveHook(ArrowSetActiveOrig orig, Arrow self, bool active);

	private delegate void ArrowUpdateTargetOrig(Arrow self, Vector3? target);
	private delegate void ArrowUpdateTargetHook(ArrowUpdateTargetOrig orig, Arrow self, Vector3? target);

	private readonly Hook magHook;
	private readonly Hook ctorHook;
	private readonly Hook arrowCtorHook;
	private readonly Hook arrowSetActiveHook;
	private readonly Hook arrowUpdateTargetHook;

	public LonerRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		SetupTranslationControllerMock();

		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

		var mockExileControllerHelper = new Mock<MockExileControllerget_InstanceHelper>();
		mockExileControllerHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
		MockExileControllerget_InstanceHelper.Instance = mockExileControllerHelper.Object;

		ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);

		var mockMinigameHelper = new Mock<MockMinigameget_InstanceHelper>();
		mockMinigameHelper.Setup(h => h.Invoke()).Returns((Minigame)null!);
		MockMinigameget_InstanceHelper.Instance = mockMinigameHelper.Object;

		var mockSub = new Mock<MockVector2op_SubtractionHelper>();
		mockSub.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>()))
			.Returns((Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y));
		MockVector2op_SubtractionHelper.Instance = mockSub.Object;

		var mockTimeHelper = new Mock<MockTimeget_deltaTimeHelper>();
		mockTimeHelper.Setup(h => h.Invoke()).Returns(1.0f);
		MockTimeget_deltaTimeHelper.Instance = mockTimeHelper.Object;

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockGameData = MockSetupHelper.SetupGameDataMock();
		var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		mockList.SetupGet(l => l.Count).Returns(0);
		mockGameData.SetupGet(g => g.AllPlayers).Returns(mockList.Object);

		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		var magTarget = typeof(Vector2).GetProperty("magnitude")!.GetGetMethod()!;
		var magHookDelegate = new Vector2MagHook(getMagnitudeHook);
		this.magHook = new Hook(magTarget, magHookDelegate);

		var ctorTarget = typeof(Vector2).GetConstructor(new[] { typeof(float), typeof(float) })!;
		var ctorHookDelegate = new Vector2CtorHook(getCtorHook);
		this.ctorHook = new Hook(ctorTarget, ctorHookDelegate);

		var arrowCtorTarget = typeof(Arrow).GetConstructor(new[] { typeof(Color) })!;
		var arrowCtorDelegate = new ArrowCtorHook(getArrowCtorHook);
		this.arrowCtorHook = new Hook(arrowCtorTarget, arrowCtorDelegate);

		var arrowSetActiveTarget = typeof(Arrow).GetMethod(nameof(Arrow.SetActive), BindingFlags.Public | BindingFlags.Instance)!;
		var arrowSetActiveDelegate = new ArrowSetActiveHook(getArrowSetActiveHook);
		this.arrowSetActiveHook = new Hook(arrowSetActiveTarget, arrowSetActiveDelegate);

		var arrowUpdateTargetTarget = typeof(Arrow).GetMethod(nameof(Arrow.UpdateTarget), BindingFlags.Public | BindingFlags.Instance)!;
		var arrowUpdateTargetDelegate = new ArrowUpdateTargetHook(getArrowUpdateTargetHook);
		this.arrowUpdateTargetHook = new Hook(arrowUpdateTargetTarget, arrowUpdateTargetDelegate);
	}

	public void Dispose()
	{
		this.magHook.Dispose();
		this.ctorHook.Dispose();
		this.arrowCtorHook.Dispose();
		this.arrowSetActiveHook.Dispose();
		this.arrowUpdateTargetHook.Dispose();
	}

	private static float getMagnitudeHook(Vector2MagOrig orig, ref Vector2 self)
	{
		return (float)Math.Sqrt(self.x * self.x + self.y * self.y);
	}

	private static void getCtorHook(Vector2CtorOrig orig, ref Vector2 self, float x, float y)
	{
		self.x = x;
		self.y = y;
	}

	private static void getArrowCtorHook(ArrowCtorOrig orig, Arrow self, Color color)
	{
	}

	private static void getArrowSetActiveHook(ArrowSetActiveOrig orig, Arrow self, bool active)
	{
	}

	private static void getArrowUpdateTargetHook(ArrowUpdateTargetOrig orig, Arrow self, Vector3? target)
	{
	}

	private static void SetupTranslationControllerMock()
	{
		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>())).Returns("TestDescription {0} {1}");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestDescription {0} {1}");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>())).Returns("TestDescription {0} {1}");
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestDescription {0} {1}");
	}

	private static Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>> CreateMockPlayerList(List<NetworkedPlayerInfo?> players)
	{
		var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		mockList.SetupGet(l => l.Count).Returns(players.Count);
		mockList.Setup(l => l[It.IsAny<int>()]).Returns((int i) => players[i]!);
		return mockList;
	}

	[Fact]
	public void Constructor_SetsRoleProperties()
	{
		// Act
		var loner = new LonerRole();

		// Assert
		Assert.Equal(ExtremeRoleId.Loner, loner.Core.Id);
		Assert.Equal(ColorPalette.LonerMidnightblue, loner.Core.Color);
	}

	[Fact]
	public void CreateRoleAllOption_CreatesOptions()
	{
		// Arrange
		var loner = new LonerRole();

		// Act
		loner.CreateRoleAllOption();

		// Assert
		int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Loner);
		Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
		Assert.NotNull(category);
	}

	[Fact]
	public void Initialize_ConfiguresStatusAndAbilityClass()
	{
		// Arrange
		var loner = new LonerRole();
		loner.CreateRoleAllOption();

		// Act
		loner.Initialize();

		// Assert
		Assert.NotNull(loner.Status);
		Assert.IsType<LonerStatusModel>(loner.Status);
		Assert.NotNull(loner.AbilityClass);
		Assert.IsType<LonerAbilityHandler>(loner.AbilityClass);
	}

	[Fact]
	public void GetRolePlayerNameTag_WhenTargetIsLocalPlayer_ReturnsInitialStressInfo()
	{
		// Arrange
		var loner = new LonerRole();
		loner.CreateRoleAllOption();
		loner.Initialize();

		// Act
		string nameTag = loner.GetRolePlayerNameTag(loner, 1);

		// Assert
		Assert.Equal("(0/10)", nameTag);
	}

	[Fact]
	public void GetRolePlayerNameTag_WhenStressAccumulated_ReturnsUpdatedStressInfo()
	{
		// Arrange
		var loner = new LonerRole();
		loner.CreateRoleAllOption();
		loner.Initialize();

		var cachedPtrField = typeof(UnityEngine.Object).GetField("m_CachedPtr", BindingFlags.NonPublic | BindingFlags.Instance);

		var mockRolePlayer = MockSetupHelper.SetupPlayerControlMocks();
		cachedPtrField?.SetValue(mockRolePlayer.Object, (IntPtr)1);
		mockRolePlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		mockRolePlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockRolePlayer.Object);
		mockData.SetupGet(d => d.PlayerId).Returns((byte)1);
		mockRolePlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		var mockOtherPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		cachedPtrField?.SetValue(mockOtherPlayer.Object, (IntPtr)1);
		mockOtherPlayer.SetupGet(p => p.PlayerId).Returns((byte)2);
		mockOtherPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(1f, 0f));

		var mockOtherInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockOtherInfo.SetupGet(i => i.PlayerId).Returns((byte)2);
		mockOtherInfo.SetupGet(i => i.IsDead).Returns(false);
		mockOtherInfo.SetupGet(i => i.Disconnected).Returns(false);
		mockOtherInfo.SetupGet(i => i.Object).Returns(mockOtherPlayer.Object);
		mockOtherPlayer.SetupGet(p => p.Data).Returns(mockOtherInfo.Object);

		var playersList = new List<NetworkedPlayerInfo?> { mockData.Object, mockOtherInfo.Object };
		var mockGameData = MockSetupHelper.SetupGameDataMock();
		mockGameData.SetupGet(g => g.AllPlayers).Returns(CreateMockPlayerList(playersList).Object);

		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;

		// Act: Update stress past 10-second ignore time (12 updates)
		for (int i = 0; i < 12; i++)
		{
			loner.Update(mockRolePlayer.Object);
		}

		string nameTag = loner.GetRolePlayerNameTag(loner, 1);

		// Assert
		Assert.Equal("(1/10)", nameTag);
	}

	[Fact]
	public void GetRolePlayerNameTag_WhenTargetIsNotLocalPlayer_ReturnsBaseTag()
	{
		// Arrange
		var loner = new LonerRole();
		loner.CreateRoleAllOption();
		loner.Initialize();

		// Act
		string nameTag = loner.GetRolePlayerNameTag(loner, 2);

		// Assert
		Assert.Equal("", nameTag);
	}

	[Fact]
	public void GetFullDescription_ContainsCurAndMaxStressValues()
	{
		// Arrange
		var loner = new LonerRole();
		loner.CreateRoleAllOption();
		loner.Initialize();

		// Act
		string description = loner.GetFullDescription();

		// Assert
		Assert.Contains("0", description);
		Assert.Contains("10", description);
	}

	[Fact]
	public void Update_WhenInTaskPhaseAndPlayerNearby_UpdatesStatusStressGage()
	{
		// Arrange
		var loner = new LonerRole();
		loner.CreateRoleAllOption();
		loner.Initialize();

		var cachedPtrField = typeof(UnityEngine.Object).GetField("m_CachedPtr", BindingFlags.NonPublic | BindingFlags.Instance);

		var mockRolePlayer = MockSetupHelper.SetupPlayerControlMocks();
		cachedPtrField?.SetValue(mockRolePlayer.Object, (IntPtr)1);
		mockRolePlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		mockRolePlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockRolePlayer.Object);
		mockData.SetupGet(d => d.PlayerId).Returns((byte)1);
		mockRolePlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		var mockOtherPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		cachedPtrField?.SetValue(mockOtherPlayer.Object, (IntPtr)1);
		mockOtherPlayer.SetupGet(p => p.PlayerId).Returns((byte)2);
		mockOtherPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(1f, 0f));

		var mockOtherInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockOtherInfo.SetupGet(i => i.PlayerId).Returns((byte)2);
		mockOtherInfo.SetupGet(i => i.IsDead).Returns(false);
		mockOtherInfo.SetupGet(i => i.Disconnected).Returns(false);
		mockOtherInfo.SetupGet(i => i.Object).Returns(mockOtherPlayer.Object);
		mockOtherPlayer.SetupGet(p => p.Data).Returns(mockOtherInfo.Object);

		var playersList = new List<NetworkedPlayerInfo?> { mockData.Object, mockOtherInfo.Object };
		var mockGameData = MockSetupHelper.SetupGameDataMock();
		mockGameData.SetupGet(g => g.AllPlayers).Returns(CreateMockPlayerList(playersList).Object);

		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;

		// Act: Update stress past 10-second ignore time (12 updates)
		for (int i = 0; i < 12; i++)
		{
			loner.Update(mockRolePlayer.Object);
		}

		// Assert
		Assert.NotNull(loner.Status);
		Assert.True(((LonerStatusModel)loner.Status).StressGage > 0f);
	}
}
