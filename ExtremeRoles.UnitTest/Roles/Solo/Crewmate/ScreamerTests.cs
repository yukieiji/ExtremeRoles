using System;
using System.Reflection;
using AmongUs.GameOptions;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using UnityEngine;
using Xunit;

using ExtremeRoles.Module;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ScreamerTests
{
	public ScreamerTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupPaletteHelpers();
		MockSetupHelper.SetupObjectImplicitHelpers();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupLobbyMock();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupPlayerControlMocks();
		MockSetupHelper.SetupGameDataMock();
		MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		MockSetupHelper.SetupOptionManager();
		SetupGameOptionsManagerMock();

		var shipStateField = typeof(ExtremeRolesPlugin).GetField("<ShipState>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
		shipStateField?.SetValue(null, new ExtremeShipStatus());

		ExtremeRoleManager.GameRole.Clear();

		var clientMock = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);

		var mockFindObjects3 = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects3.Setup(m => m.Invoke<DeadBody>())
			.Returns(new Il2CppReferenceArray<DeadBody>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects3.Object;
	}

	private static void SetupGameOptionsManagerMock()
	{
		if (MockGameOptionsManagerget_InstanceHelper.Instance == null)
		{
			var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
			var mockGameOptionsManager = new Mock<GameOptionsManager>(IntPtr.Zero);
			mockGameOptionsManager.SetupGet(g => g.CurrentGameOptions).Returns(mockGameOptions.Object);

			var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
			mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockGameOptionsManager.Object);
			MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;
		}
	}

	[Fact]
	public void Constructor_InitializesScreamerRoleCorrectly()
	{
		// Arrange & Act
		var screamer = new Screamer();

		// Assert
		Assert.Equal(ExtremeRoleId.Screamer, screamer.Core.Id);
		Assert.Equal(ExtremeRoleType.Crewmate, screamer.Core.Team);
		Assert.Equal(ColorPalette.ScreamerColor, screamer.Core.Color);
	}

	[Fact]
	public void RoleSpecificInit_LoadsOptionValuesAndCreatesAbilityHandler()
	{
		// Arrange
		var screamer = new Screamer();
		screamer.CreateRoleAllOption();

		// Act
		screamer.Initialize();

		// Assert
		Assert.NotNull(screamer.AbilityClass);
		Assert.IsType<ScreamerAbilityHandler>(screamer.AbilityClass);
	}

	[Fact]
	public void ScreamerAbilityHandler_MaxExileIndex_IsFour()
	{
		// Assert
		Assert.Equal(4, ScreamerAbilityHandler.MaxExileIndex);
	}

	[Fact]
	public void ScreamerAbilityHandler_GetOverrideInfo_ReturnsExileInfoWithPlayer()
	{
		// Arrange
		var mockFactory = new Mock<IUnityObjectFactory>();
		var handler = new ScreamerAbilityHandler(true, 1.0f, mockFactory.Object);

		var mockExiledPlayer = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);

		// Act
		var overrideInfo = handler.GetOverrideInfo(mockExiledPlayer.Object);

		// Assert
		Assert.NotNull(overrideInfo);
		Assert.Equal(mockExiledPlayer.Object, overrideInfo!.ExiledPlayer);
		Assert.NotNull(overrideInfo.AnimationText);
		Assert.NotEmpty(overrideInfo.AnimationText);
	}

	[Fact]
	public void ScreamerAbilityHandler_GetOverrideInfo_ReturnsValidIndicesWithinRange()
	{
		// Arrange
		var mockFactory = new Mock<IUnityObjectFactory>();
		var handler = new ScreamerAbilityHandler(true, 1.0f, mockFactory.Object);
		var mockExiledPlayer = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);

		// Act & Assert - test 100 random calls to verify returned index range 0..MaxExileIndex
		for (int i = 0; i < 100; i++)
		{
			var overrideInfo = handler.GetOverrideInfo(mockExiledPlayer.Object);
			Assert.NotNull(overrideInfo?.AnimationText);
			Assert.StartsWith("ScreamerExile", overrideInfo!.AnimationText);

			string indexStr = overrideInfo.AnimationText.Replace("ScreamerExile", "");
			Assert.True(int.TryParse(indexStr, out int index));
			Assert.InRange(index, 0, ScreamerAbilityHandler.MaxExileIndex);
		}
	}

	[Fact]
	public void RolePlayerKilledAction_WhenIsScreamOnKillDisabled_DoesNotSpawn()
	{
		// Arrange
		var mockFactory = new Mock<IUnityObjectFactory>();
		var handler = new ScreamerAbilityHandler(false, 1.0f, mockFactory.Object);

		var screamer = new Screamer();
		screamer.CreateRoleAllOption();

		var abilityClassProperty = typeof(SingleRoleBase).GetProperty("AbilityClass", BindingFlags.Public | BindingFlags.Instance);
		abilityClassProperty?.SetValue(screamer, handler);

		var mockVictim = new Mock<PlayerControl>(IntPtr.Zero);
		var mockKiller = new Mock<PlayerControl>(IntPtr.Zero);

		// Act
		screamer.RolePlayerKilledAction(mockVictim.Object, mockKiller.Object);

		// Assert
		mockFactory.Verify(f => f.CreateGameObject(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void RolePlayerKilledAction_WhenTargetBodyIsNull_DoesNotSpawnImage()
	{
		// Arrange
		var mockFactory = new Mock<IUnityObjectFactory>();
		var handler = new ScreamerAbilityHandler(true, 1.0f, mockFactory.Object);

		var screamer = new Screamer();
		screamer.CreateRoleAllOption();

		var abilityClassProperty = typeof(SingleRoleBase).GetProperty("AbilityClass", BindingFlags.Public | BindingFlags.Instance);
		abilityClassProperty?.SetValue(screamer, handler);

		var mockVictim = new Mock<PlayerControl>(IntPtr.Zero);
		mockVictim.SetupGet(p => p.PlayerId).Returns((byte)99);

		var mockKiller = new Mock<PlayerControl>(IntPtr.Zero);

		// Act
		screamer.RolePlayerKilledAction(mockVictim.Object, mockKiller.Object);

		// Assert
		mockFactory.Verify(f => f.CreateGameObject(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void RolePlayerKilledAction_WhenTargetBodyExists_SpawnsScreamText()
	{
		// Arrange
		var mockDeadBody = new Mock<DeadBody>(IntPtr.Zero);
		mockDeadBody.SetupGet(b => b.ParentId).Returns((byte)1);

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		mockDeadBody.SetupGet(b => b.transform).Returns(mockTransform.Object);

		var mockGameData = MockSetupHelper.SetupGameDataMock();
		var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockPlayerInfo.SetupGet(p => p.PlayerId).Returns((byte)1);
		mockGameData.Setup(g => g.GetPlayerById((byte)1)).Returns(mockPlayerInfo.Object);

		var mockFindObjects3 = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects3.Setup(m => m.Invoke<DeadBody>())
			.Returns(new Il2CppReferenceArray<DeadBody>(new DeadBody[] { mockDeadBody.Object }));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects3.Object;

		var mockTextMesh = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockGameObject.SetupGet(g => g.transform).Returns(mockTransform.Object);
		mockGameObject.Setup(g => g.AddComponent<TMPro.TextMeshPro>()).Returns(mockTextMesh.Object);

		var mockFactory = new Mock<IUnityObjectFactory>();
		mockFactory.Setup(f => f.CreateGameObject(It.IsAny<string>())).Returns(mockGameObject.Object);

		var handler = new ScreamerAbilityHandler(true, 1.0f, mockFactory.Object);

		var screamer = new Screamer();
		screamer.CreateRoleAllOption();

		var abilityClassProperty = typeof(SingleRoleBase).GetProperty("AbilityClass", BindingFlags.Public | BindingFlags.Instance);
		abilityClassProperty?.SetValue(screamer, handler);

		var mockVictim = new Mock<PlayerControl>(IntPtr.Zero);
		mockVictim.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockKiller = new Mock<PlayerControl>(IntPtr.Zero);

		// Act
		screamer.RolePlayerKilledAction(mockVictim.Object, mockKiller.Object);

		// Assert
		mockFactory.Verify(f => f.CreateGameObject("ScreamerScreamText"), Times.Once);
		mockTextMesh.VerifySet(t => t.text = It.IsAny<string>(), Times.Once);
	}
}
