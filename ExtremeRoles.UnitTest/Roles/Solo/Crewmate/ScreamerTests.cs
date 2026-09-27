using System;
using System.Reflection;
using AmongUs.GameOptions;
using Hazel;
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
		Assert.NotNull(screamer.AbilityClass);
		Assert.IsType<ScreamerAbilityHandler>(screamer.AbilityClass);
	}

	[Fact]
	public void RoleSpecificInit_LoadsOptionValues()
	{
		// Arrange
		var screamer = new Screamer();
		screamer.CreateRoleAllOption();

		// Act
		screamer.Initialize();

		// Assert
		Assert.True(screamer.IsScreamOnKill);
		Assert.Equal(100.0f, screamer.ScreamImageScale);
	}

	[Fact]
	public void ScreamerAbilityHandler_GetOverrideInfo_ReturnsExileInfoWithPlayer()
	{
		// Arrange
		var mockFactory = new Mock<IUnityObjectFactory>();
		var mockResources = new Mock<IResourcesProvider>();
		var handler = new ScreamerAbilityHandler(mockFactory.Object, mockResources.Object);

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
	public void RolePlayerKilledAction_WhenIsScreamOnKillDisabled_DoesNotSpawn()
	{
		// Arrange
		var mockFactory = new Mock<IUnityObjectFactory>();
		var mockResources = new Mock<IResourcesProvider>();
		var screamer = new Screamer(mockFactory.Object, mockResources.Object);
		screamer.CreateRoleAllOption();
		screamer.Initialize();

		var isScreamProperty = typeof(Screamer).GetProperty("IsScreamOnKill", BindingFlags.Public | BindingFlags.Instance);
		isScreamProperty?.SetValue(screamer, false);

		var mockVictim = new Mock<PlayerControl>(IntPtr.Zero);
		var mockKiller = new Mock<PlayerControl>(IntPtr.Zero);

		// Act
		screamer.RolePlayerKilledAction(mockVictim.Object, mockKiller.Object);

		// Assert
		mockFactory.Verify(f => f.CreateGameObject(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void RolePlayerKilledAction_WhenIsScreamOnKillEnabled_SpawnsScreamImageUsingFactory()
	{
		// Arrange
		var mockRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		var mockTransform = new Mock<Transform>(IntPtr.Zero);

		mockGameObject.SetupGet(g => g.transform).Returns(mockTransform.Object);
		mockGameObject.Setup(g => g.AddComponent<SpriteRenderer>()).Returns(mockRenderer.Object);

		var mockFactory = new Mock<IUnityObjectFactory>();
		mockFactory.Setup(f => f.CreateGameObject(It.IsAny<string>())).Returns(mockGameObject.Object);

		var mockResources = new Mock<IResourcesProvider>();

		var screamer = new Screamer(mockFactory.Object, mockResources.Object);
		screamer.CreateRoleAllOption();
		screamer.Initialize();

		var mockVictimTransform = new Mock<Transform>(IntPtr.Zero);
		mockVictimTransform.SetupGet(t => t.position).Returns(Vector3.zero);

		var mockVictim = new Mock<PlayerControl>(IntPtr.Zero);
		mockVictim.SetupGet(p => p.PlayerId).Returns((byte)1);
		mockVictim.SetupGet(p => p.transform).Returns(mockVictimTransform.Object);

		var mockKiller = new Mock<PlayerControl>(IntPtr.Zero);

		// Act
		screamer.RolePlayerKilledAction(mockVictim.Object, mockKiller.Object);

		// Assert
		mockFactory.Verify(f => f.CreateGameObject("ScreamerScreamImage"), Times.Once);
		mockResources.Verify(r => r.LoadRoleSprite(ExtremeRoleId.Screamer, It.IsAny<string>()), Times.Once);
	}
}
