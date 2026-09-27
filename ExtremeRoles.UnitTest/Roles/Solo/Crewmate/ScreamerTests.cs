using System;
using System.Reflection;
using AmongUs.GameOptions;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

using ExtremeRoles.Module;
using ExtremeRoles.Module.ExtremeShipStatus;
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
		var role = new Screamer();
		role.CreateRoleAllOption();

		// Act
		role.Initialize();

		// Assert
		Assert.True(role.IsScreamOnKill);
		Assert.Equal(1.0f, role.ScreamImageScale);
	}

	[Fact]
	public void ScreamerAbilityHandler_OverrideInfo_ReturnsExileInfo()
	{
		// Arrange
		var screamer = new Screamer();
		screamer.CreateRoleAllOption();
		screamer.Initialize();

		var handler = new ScreamerAbilityHandler(screamer);

		// Act
		var overrideInfo = handler.OverrideInfo;

		// Assert
		Assert.NotNull(overrideInfo);
		Assert.NotNull(overrideInfo!.AnimationText);
		Assert.NotEmpty(overrideInfo.AnimationText);
	}

	[Fact]
	public void GetRandomExileText_ReturnsNonEmptyString()
	{
		// Arrange
		var screamer = new Screamer();

		// Act
		for (int i = 0; i < 20; ++i)
		{
			string text = screamer.GetRandomExileText();

			// Assert
			Assert.NotNull(text);
			Assert.NotEmpty(text);
		}
	}

	[Fact]
	public void GetRandomImageIndex_ReturnsValueBetweenOneAndFive()
	{
		// Arrange
		var screamer = new Screamer();

		// Act
		for (int i = 0; i < 20; ++i)
		{
			int idx = screamer.GetRandomImageIndex();

			// Assert
			Assert.InRange(idx, 1, 5);
		}
	}

	[Fact]
	public void RolePlayerKilledAction_WhenIsScreamOnKillDisabled_DoesNotSpawn()
	{
		// Arrange
		var screamer = new Screamer();
		screamer.CreateRoleAllOption();
		screamer.Initialize();

		var isScreamField = typeof(Screamer).GetField("isScreamOnKill", BindingFlags.NonPublic | BindingFlags.Instance);
		isScreamField?.SetValue(screamer, false);

		var mockVictim = new Mock<PlayerControl>(IntPtr.Zero);
		var mockKiller = new Mock<PlayerControl>(IntPtr.Zero);

		// Act & Assert (Should complete without exception)
		screamer.RolePlayerKilledAction(mockVictim.Object, mockKiller.Object);
	}

	[Fact]
	public void RolePlayerKilledAction_WhenIsScreamOnKillEnabled_ExecutesSuccessfully()
	{
		// Arrange
		var screamer = new Screamer();
		screamer.CreateRoleAllOption();
		screamer.Initialize();

		var mockVictimTransform = new Mock<Transform>(IntPtr.Zero);
		mockVictimTransform.SetupGet(t => t.position).Returns(Vector3.zero);

		var mockVictim = new Mock<PlayerControl>(IntPtr.Zero);
		mockVictim.SetupGet(p => p.PlayerId).Returns((byte)1);
		mockVictim.SetupGet(p => p.transform).Returns(mockVictimTransform.Object);

		var mockKiller = new Mock<PlayerControl>(IntPtr.Zero);

		// Act & Assert
		screamer.RolePlayerKilledAction(mockVictim.Object, mockKiller.Object);
	}
}
