using System;
using System.Reflection;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using UnityEngine;
using Xunit;

using ExtremeRoles.Module;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ScreamerTests
{
	private readonly Mock<GameData> mockGameData;

	public ScreamerTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupPaletteHelpers();
		MockSetupHelper.SetupObjectImplicitHelpers();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupLobbyMock();
		MockSetupHelper.SetupOptionManager();

		this.mockGameData = MockSetupHelper.SetupGameDataMock();

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);

		ExtremeRoleManager.GameRole.Clear();
	}

	[Fact]
	public void ScreamerAbilityHandler_GetOverrideInfo_SetsExiledPlayerAndText()
	{
		// Arrange
		var handler = new ScreamerAbilityHandler();
		var mockPlayer = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);

		// Act
		var info = handler.GetOverrideInfo(mockPlayer.Object);

		// Assert
		Assert.NotNull(info);
		Assert.Same(mockPlayer.Object, info.ExiledPlayer);
		Assert.False(string.IsNullOrEmpty(info.AnimationText));
	}

	[Fact]
	public void GetRandomScreamIndex_ReturnsValidRange()
	{
		// Act & Assert
		for (int i = 0; i < 100; i++)
		{
			int index = ScreamerAbilityHandler.GetRandomScreamIndex();
			Assert.True(index >= 0 && index <= 4);
		}
	}

	[Fact]
	public void RoleSpecificInit_LoadsCustomOptions()
	{
		// Arrange
		var screamer = new Screamer();
		screamer.CreateRoleAllOption();

		// Act
		screamer.Initialize();

		// Assert
		var isScreamField = typeof(Screamer).GetField("isScreamWhenKilled", BindingFlags.NonPublic | BindingFlags.Instance);
		var sizeField = typeof(Screamer).GetField("screamImageSize", BindingFlags.NonPublic | BindingFlags.Instance);

		bool isScream = (bool)isScreamField?.GetValue(screamer)!;
		float size = (float)sizeField?.GetValue(screamer)!;

		Assert.True(isScream);
		Assert.Equal(1.0f, size);
	}

	[Fact]
	public void RolePlayerKilledAction_WhenIsScreamWhenKilledIsFalse_ReturnsEarlyWithoutQueryingDeadBody()
	{
		// Arrange
		var screamer = new Screamer();
		screamer.CreateRoleAllOption();
		screamer.Initialize();

		var isScreamField = typeof(Screamer).GetField("isScreamWhenKilled", BindingFlags.NonPublic | BindingFlags.Instance);
		isScreamField?.SetValue(screamer, false);

		bool findObjectsCalled = false;
		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<DeadBody>()).Callback(() => findObjectsCalled = true)
			.Returns(new Il2CppReferenceArray<DeadBody>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var mockRolePlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockRolePlayer.SetupGet(p => p.PlayerId).Returns((byte)5);
		var mockKillerPlayer = new Mock<PlayerControl>(IntPtr.Zero);

		// Act
		screamer.RolePlayerKilledAction(mockRolePlayer.Object, mockKillerPlayer.Object);

		// Assert
		Assert.False(findObjectsCalled);
	}

	[Fact]
	public void RolePlayerKilledAction_WhenDeadBodyIsNull_ReturnsEarlyWithoutException()
	{
		// Arrange
		var screamer = new Screamer();
		screamer.CreateRoleAllOption();
		screamer.Initialize();

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<DeadBody>())
			.Returns(new Il2CppReferenceArray<DeadBody>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var mockRolePlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockRolePlayer.SetupGet(p => p.PlayerId).Returns((byte)5);
		var mockKillerPlayer = new Mock<PlayerControl>(IntPtr.Zero);

		// Act & Assert
		screamer.RolePlayerKilledAction(mockRolePlayer.Object, mockKillerPlayer.Object);
	}
}
