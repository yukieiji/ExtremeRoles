using System;
using System.Collections.Generic;
using System.Reflection;

using AmongUs.GameOptions;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor.Darkpointer;

using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class DarkpointerTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;
	private readonly Mock<IUnityObjectFactory> mockFactory;

	public DarkpointerTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupUnityObjectOperators();
		MockSetupHelper.SetupVector3Helpers();
		MockSetupHelper.SetupPaletteHelpers();

		this.mockFactory = new Mock<IUnityObjectFactory>();
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		var mockSprite = new Mock<Sprite>(IntPtr.Zero);

		mockGameObject.SetupGet(g => g.transform).Returns(mockTransform.Object);
		mockGameObject.Setup(g => g.AddComponent<SpriteRenderer>()).Returns(mockSpriteRenderer.Object);
		this.mockFactory.Setup(f => f.CreateGameObject(It.IsAny<string>())).Returns(mockGameObject.Object);
		this.mockFactory.Setup(f => f.LoadRoleSprite(It.IsAny<ExtremeRoleId>())).Returns(mockSprite.Object);

		var system = new DarkpointerDarknessSystem(2.5f, this.mockFactory.Object);
		ExtremeSystemTypeManager.Instance.TryAdd(ExtremeSystemType.DarkpointerDarkness, system);

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		if (ExtremeGameModeManager.Instance == null)
		{
			ExtremeGameModeManager.Create(GameModes.Normal);
		}

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
					seqSingle.Returns(10f).Returns(20f);
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

		var cachedPtrField = typeof(UnityEngine.Object).GetField("m_CachedPtr", BindingFlags.NonPublic | BindingFlags.Instance);

		this.mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		cachedPtrField?.SetValue(this.mockLocalPlayer.Object, (IntPtr)1);
		this.mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		this.mockLocalPlayer.SetupGet(p => p.NetId).Returns((uint)1);
		this.mockLocalPlayer.SetupGet(p => p.CanMove).Returns(true);
		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(10f, 20f));

		var mockLocalData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		cachedPtrField?.SetValue(mockLocalData.Object, (IntPtr)1);
		mockLocalData.SetupGet(d => d.PlayerId).Returns((byte)1);
		mockLocalData.SetupGet(d => d.IsDead).Returns(false);
		mockLocalData.SetupGet(d => d.Disconnected).Returns(false);
		mockLocalData.SetupGet(d => d.Object).Returns(this.mockLocalPlayer.Object);
		this.mockLocalPlayer.SetupGet(p => p.Data).Returns(mockLocalData.Object);

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

		var field = typeof(UnityObjectLoader).GetField("cachedBundle", BindingFlags.NonPublic | BindingFlags.Static);
		if (field?.GetValue(null) is Dictionary<string, AssetBundle> dict)
		{
			dict[ObjectPath.GetRoleAssetPath(ExtremeRoleId.Darkpointer)] = mockAssetBundle.Object;
		}
	}

	private static void InitializeRole(SingleRoleBase role, byte playerId = 1)
	{
		role.CreateRoleAllOption();
		role.Initialize();
		ExtremeRoleManager.GameRole[playerId] = role;
	}

	[Fact]
	public void Initialize_RegistersDarkpointerRoleAndImpostorTeam()
	{
		var role = new DarkpointerRole();
		InitializeRole(role);

		Assert.Equal(ExtremeRoleId.Darkpointer, role.Core.Id);
		Assert.True(role.IsImpostor());
	}

	[Fact]
	public void DarkpointerDarknessSystem_PlaceDarknessAndIsPlayerInDarkness()
	{
		var system = ExtremeSystemTypeManager.Instance.CreateOrGet<DarkpointerDarknessSystem>(
			ExtremeSystemType.DarkpointerDarkness, () => new DarkpointerDarknessSystem(2.5f, this.mockFactory.Object));

		system.Reset(ResetTiming.MeetingStart);

		var pos = new Vector2(10f, 20f);
		system.PlaceDarkness(1, pos);

		Assert.True(system.IsPlayerInDarkness(this.mockLocalPlayer.Object));

		// Move player outside range
		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(50f, 50f));
		Assert.False(system.IsPlayerInDarkness(this.mockLocalPlayer.Object));

		// Reset position
		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(10f, 20f));
	}

	[Fact]
	public void DarkpointerDarknessSystem_RemoveDarknessRemovesCircleAndUnlocksReport()
	{
		var system = ExtremeSystemTypeManager.Instance.CreateOrGet<DarkpointerDarknessSystem>(
			ExtremeSystemType.DarkpointerDarkness, () => new DarkpointerDarknessSystem(2.5f, this.mockFactory.Object));

		system.Reset(ResetTiming.MeetingStart);

		var pos = new Vector2(10f, 20f);
		system.PlaceDarkness(1, pos);

		system.Deteriorate(1f);
		Assert.True(ButtonLockSystem.IsReportButtonLock());

		system.RemoveDarkness(1);
		system.Deteriorate(1f);
		Assert.False(system.IsPlayerInDarkness(this.mockLocalPlayer.Object));
		Assert.False(ButtonLockSystem.IsReportButtonLock());
	}

	[Fact]
	public void DarkpointerDarknessSystem_ResetOnMeetingStartClearsCirclesAndUnlocksReport()
	{
		var system = ExtremeSystemTypeManager.Instance.CreateOrGet<DarkpointerDarknessSystem>(
			ExtremeSystemType.DarkpointerDarkness, () => new DarkpointerDarknessSystem(2.5f, this.mockFactory.Object));

		system.PlaceDarkness(1, new Vector2(10f, 20f));
		system.Deteriorate(1f);
		Assert.True(ButtonLockSystem.IsReportButtonLock());

		system.Reset(ResetTiming.MeetingStart);
		Assert.False(system.IsPlayerInDarkness(this.mockLocalPlayer.Object));
		Assert.False(ButtonLockSystem.IsReportButtonLock());
	}

	[Fact]
	public void DarkpointerRole_UseAbilityAndCleanUp_PlacesAndRemovesDarkness()
	{
		var role = new DarkpointerRole();
		InitializeRole(role);

		var system = ExtremeSystemTypeManager.Instance.CreateOrGet<DarkpointerDarknessSystem>(
			ExtremeSystemType.DarkpointerDarkness, () => new DarkpointerDarknessSystem(2.5f, this.mockFactory.Object));

		system.Reset(ResetTiming.MeetingStart);

		bool used = role.UseAbility();
		Assert.True(used);
		Assert.True(system.IsPlayerInDarkness(this.mockLocalPlayer.Object));

		role.CleanUp();
		Assert.False(system.IsPlayerInDarkness(this.mockLocalPlayer.Object));
	}
}
