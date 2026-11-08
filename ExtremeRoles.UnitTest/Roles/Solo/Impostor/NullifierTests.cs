using System;
using System.Collections.Generic;
using System.Reflection;

using AmongUs.GameOptions;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor.Nullifier;

using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class NullifierTests
{
	private sealed class MockUnityObjectFactory : IUnityObjectFactory
	{
		public readonly List<GameObject> CreatedGameObjects = [];

		public GameObject CreateGameObject(string name)
		{
			var mockGo = new Mock<GameObject>(IntPtr.Zero);
			var mockTrans = new Mock<Transform>(IntPtr.Zero);
			var mockSr = new Mock<SpriteRenderer>(IntPtr.Zero);

			mockGo.SetupGet(g => g.transform).Returns(mockTrans.Object);
			mockGo.Setup(g => g.AddComponent<SpriteRenderer>()).Returns(mockSr.Object);
			mockGo.Setup(g => g.AddComponent(It.IsAny<Il2CppSystem.Type>())).Returns(mockSr.Object);

			this.CreatedGameObjects.Add(mockGo.Object);
			return mockGo.Object;
		}

		public Sprite LoadRoleSprite(ExtremeRoleId roleId) => new Mock<Sprite>(IntPtr.Zero).Object;
		public Sprite LoadRoleSprite(ExtremeRoleId roleId, string img) => new Mock<Sprite>(IntPtr.Zero).Object;
		public Material CreateSpriteMaterial() => new Mock<Material>(IntPtr.Zero).Object;
		public Vector3 CreateMapPos(Vector2 target, float offset = 1000.0f) => new Vector3(target.x, target.y, target.y / offset);
		public Mesh CreateMesh() => new Mock<Mesh>(IntPtr.Zero).Object;
	}

	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;
	private readonly MockUnityObjectFactory mockFactory;

	public NullifierTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupUnityObjectOperators();
		MockSetupHelper.SetupVector3Helpers();
		MockSetupHelper.SetupPaletteHelpers();

		var cachedPtrField = typeof(UnityEngine.Object).GetField("m_CachedPtr", BindingFlags.NonPublic | BindingFlags.Instance);

		MockPlayerControlget_LocalPlayerHelper.Instance = null!;
		this.mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		cachedPtrField?.SetValue(this.mockLocalPlayer.Object, (IntPtr)1);
		this.mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)0);
		this.mockLocalPlayer.SetupGet(p => p.NetId).Returns((uint)1);
		this.mockLocalPlayer.SetupGet(p => p.CanMove).Returns(true);

		var mockPlayerTransform = new Mock<Transform>(IntPtr.Zero);
		this.mockLocalPlayer.SetupGet(p => p.transform).Returns(mockPlayerTransform.Object);
		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));

		var mockLocalData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		cachedPtrField?.SetValue(mockLocalData.Object, (IntPtr)1);
		mockLocalData.SetupGet(d => d.PlayerId).Returns((byte)0);
		mockLocalData.SetupGet(d => d.IsDead).Returns(false);
		mockLocalData.SetupGet(d => d.Disconnected).Returns(false);
		mockLocalData.SetupGet(d => d.Object).Returns(this.mockLocalPlayer.Object);
		this.mockLocalPlayer.SetupGet(p => p.Data).Returns(mockLocalData.Object);

		PlayerCache.RemovePlayerControl(_ => true);
		PlayerCache.AddPlayerControl(this.mockLocalPlayer.Object);

		this.mockFactory = new MockUnityObjectFactory();

		var system = new NullifierSystem(
			2.5f,
			10,
			10.0f,
			NullifierTargetTeam.AllImpostorOther,
			this.mockFactory);
		ExtremeSystemTypeManager.Instance.TryAdd(ExtremeSystemType.Nullifier, system);

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		if (ExtremeGameModeManager.Instance == null)
		{
			ExtremeGameModeManager.Create(GameModes.Normal);
		}

		MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		SetupAssetBundleMock();

		this.mockClient = MockSetupHelper.SetupAmongUsClientMock();
		this.mockClient.SetupGet(c => c.AmHost).Returns(true);

		var writtenBytes = new List<byte>();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		mockWriter.Setup(w => w.Write(It.IsAny<byte>())).Callback((byte b) => writtenBytes.Add(b));
		mockWriter.Setup(w => w.ToByteArray(It.IsAny<bool>())).Returns(new Mock<Il2CppStructArray<byte>>(IntPtr.Zero).Object);

		this.mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockGet = new Mock<Hazel.MockMessageWriterGetHelper>();
		mockGet.Setup(g => g.Invoke(It.IsAny<SendOption>())).Returns(mockWriter.Object);
		Hazel.MockMessageWriterGetHelper.Instance = mockGet.Object;

		var mockReaderHelper = new Mock<Hazel.MockMessageReaderGetHelper>();
		mockReaderHelper.Setup(g => g.Invoke(It.IsAny<Il2CppStructArray<byte>>()))
			.Returns(() =>
			{
				var mockReader = new Mock<MessageReader>();
				if (writtenBytes.Count > 0)
				{
					var bytesCopy = new List<byte>(writtenBytes);
					writtenBytes.Clear();
					var seqByte = mockReader.SetupSequence(r => r.ReadByte());
					foreach (var b in bytesCopy)
					{
						seqByte.Returns(b);
					}
					var seqSingle = mockReader.SetupSequence(r => r.ReadSingle());
					seqSingle.Returns(0f).Returns(0f);
				}
				return mockReader.Object;
			});
		Hazel.MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;

		var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
		mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<MessageWriter>(), It.IsAny<InnerNetObject>()));
		InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

		var mockExileControllerHelper = new Mock<MockExileControllerget_InstanceHelper>();
		mockExileControllerHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
		MockExileControllerget_InstanceHelper.Instance = mockExileControllerHelper.Object;

		if (ExtremeRolesPlugin.ShipState == null)
		{
			var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(
				nameof(ExtremeRolesPlugin.ShipState),
				BindingFlags.Public | BindingFlags.Static);
			shipStateProp?.SetValue(null, new ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus());
		}
	}

	private static void SetupAssetBundleMock()
	{
		var mockSprite = new Mock<Sprite>(IntPtr.Zero);

		var mockAssetBundle = new Mock<AssetBundle>(IntPtr.Zero);
		mockAssetBundle.Setup(b => b.LoadAsset(It.IsAny<string>(), It.IsAny<Il2CppSystem.Type>()))
			.Returns(mockSprite.Object);

		var field = typeof(UnityObjectLoader).GetField("cachedBundle", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
		if (field?.GetValue(null) is Dictionary<string, AssetBundle> dict)
		{
			dict[ObjectPath.GetRoleAssetPath(ExtremeRoleId.Nullifier)] = mockAssetBundle.Object;
		}
	}

	private static void InitializeRole(SingleRoleBase role, byte playerId = 0)
	{
		role.CreateRoleAllOption();
		role.Initialize();
		ExtremeRoleManager.GameRole[playerId] = role;
	}

	[Fact]
	public void Initialize_RegistersNullifierRoleAndImpostorTeam()
	{
		var role = new NullifierRole();
		InitializeRole(role);

		Assert.Equal(ExtremeRoleId.Nullifier, role.Core.Id);
		Assert.True(role.IsImpostor());
	}

	[Fact]
	public void NullifierRole_CheckAbility_ReturnsFalseIfPlayerMoves()
	{
		var role = new NullifierRole();
		InitializeRole(role);

		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));
		role.UseAbility();

		// 静止している間はtrue
		Assert.True(role.CheckAbility());

		// 移動した場合はfalse（詠唱中断）
		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(1f, 1f));
		Assert.False(role.CheckAbility());
	}

	[Fact]
	public void NullifierSystem_ReserveSeal_PlacesAfterMeetingEnd()
	{
		var system = new NullifierSystem(
			2.5f,
			10,
			10.0f,
			NullifierTargetTeam.AllImpostorOther,
			this.mockFactory);

		system.ReserveSeal(new Vector2(1f, 2f));
		Assert.Single(system.ScheduledReservations);
		Assert.Empty(system.PlacedSeals);

		system.Reset(ResetTiming.MeetingEnd);
		Assert.Empty(system.ScheduledReservations);
		Assert.Single(system.PlacedSeals);
	}

	[Fact]
	public void NullifierSystem_LocksAndUnlocksAbilityButton_ForCrewmate()
	{
		var system = new NullifierSystem(
			2.5f,
			10,
			10.0f,
			NullifierTargetTeam.CrewmateOnly,
			this.mockFactory);

		// 役職をクルーメイトとして登録
		var crewRole = new Sheriff();
		InitializeRole(crewRole, 0);

		system.AddSealForTest(0, new Vector2(0f, 0f));

		// 陣の範囲内にいるのでロックされる
		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));
		system.Deteriorate(0.1f);
		Assert.True(ButtonLockSystem.IsAbilityButtonLock());

		// 陣の範囲外へ移動するとアンロック
		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(50f, 50f));
		system.Deteriorate(0.1f);
		Assert.False(ButtonLockSystem.IsAbilityButtonLock());
	}

	[Fact]
	public void NullifierSystem_DoesNotLockImpostor()
	{
		var system = new NullifierSystem(
			2.5f,
			10,
			10.0f,
			NullifierTargetTeam.AllImpostorOther,
			this.mockFactory);

		// 役職をナリファイア（インポスター）として登録
		var role = new NullifierRole();
		InitializeRole(role, 0);

		system.AddSealForTest(0, new Vector2(0f, 0f));

		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));
		system.Deteriorate(0.1f);
		Assert.False(ButtonLockSystem.IsAbilityButtonLock());
	}

	[Fact]
	public void NullifierConsoleBehavior_ClickAndRestoreTimer()
	{
		var behavior = new NullifierSystem.NullifierConsoleBehavior(0, 10, null);

		Assert.Equal(10, behavior.RemainingClicks);

		behavior.Use();
		Assert.Equal(9, behavior.RemainingClicks);
		Assert.Equal(0.0f, behavior.InactiveTimer);

		// 5秒経過（まだリセットされない）
		behavior.UpdateTimer(5.0f, 10.0f, 10);
		Assert.Equal(9, behavior.RemainingClicks);

		// さらに5秒経過（合計10秒に達したので10回に復元）
		behavior.UpdateTimer(5.0f, 10.0f, 10);
		Assert.Equal(10, behavior.RemainingClicks);
		Assert.Equal(0.0f, behavior.InactiveTimer);
	}
}
