using System;
using System.Reflection;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using UnityEngine;
using Xunit;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
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

		var mockSprite = new Mock<Sprite>(IntPtr.Zero);
		for (int i = 0; i < 5; i++)
		{
			string path = string.Format(ObjectPath.ScreamerScreamFormat, i);
			string key = $"{path}115";
			if (!LruCache<string, Sprite>.TryGetValue(key, out _))
			{
				LruCache<string, Sprite>.Add(key, mockSprite.Object);
			}
		}
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
	public void CreateScreamEffect_WhenDeadBodyIsNull_ReturnsNullWithoutCreatingGameObject()
	{
		// Arrange
		var mockFactory = new Mock<IUnityObjectFactory>();
		var handler = new ScreamerAbilityHandler(mockFactory.Object);

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<DeadBody>())
			.Returns(new Il2CppReferenceArray<DeadBody>(IntPtr.Zero));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		// Act
		var result = handler.CreateScreamEffect(5, 1.0f);

		// Assert
		Assert.Null(result);
		mockFactory.Verify(f => f.CreateGameObject(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void CreateScreamEffect_WhenDeadBodyExists_CreatesGameObjectAndConfiguresEffect()
	{
		// Arrange
		var mockFactory = new Mock<IUnityObjectFactory>();
		var handler = new ScreamerAbilityHandler(mockFactory.Object);

		byte killedPlayerId = 5;

		this.mockGameData.Setup(g => g.GetPlayerById(It.IsAny<byte>())).Returns<byte>(id =>
		{
			var mockPlayer = new Mock<NetworkedPlayerInfo>();
			mockPlayer.SetupGet(p => p.PlayerId).Returns(id);
			return mockPlayer.Object;
		});

		var mockBodyTransform = new Mock<Transform>(IntPtr.Zero);
		var mockDeadBody = new Mock<DeadBody>(IntPtr.Zero);
		mockDeadBody.SetupGet(d => d.ParentId).Returns(killedPlayerId);
		mockDeadBody.SetupGet(d => d.transform).Returns(mockBodyTransform.Object);

		var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper3>();
		mockFindObjects.Setup(x => x.Invoke<DeadBody>())
			.Returns(new Il2CppReferenceArray<DeadBody>([mockDeadBody.Object]));
		MockObjectFindObjectsOfTypeHelper3.Instance = mockFindObjects.Object;

		var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockSpriteRenderer.SetupProperty(s => s.sprite);

		var mockScreamTransform = new Mock<Transform>(IntPtr.Zero);
		mockScreamTransform.SetupProperty(t => t.localPosition);
		mockScreamTransform.SetupProperty(t => t.localScale);

		var mockScreamGO = new Mock<GameObject>(IntPtr.Zero);
		mockScreamGO.SetupGet(g => g.transform).Returns(mockScreamTransform.Object);
		mockScreamGO.Setup(g => g.AddComponent<SpriteRenderer>()).Returns(mockSpriteRenderer.Object);

		mockFactory.Setup(f => f.CreateGameObject("ScreamerScreamEffect"))
			.Returns(mockScreamGO.Object);

		// Act
		var resultObj = handler.CreateScreamEffect(killedPlayerId, 1.5f);

		// Assert
		Assert.NotNull(resultObj);
		Assert.Same(mockScreamGO.Object, resultObj);

		mockFactory.Verify(f => f.CreateGameObject("ScreamerScreamEffect"), Times.Once);
		mockScreamGO.Verify(g => g.AddComponent<SpriteRenderer>(), Times.Once);
		mockScreamTransform.Verify(t => t.SetParent(mockBodyTransform.Object, false), Times.Once);
		Assert.Equal(new Vector3(0f, 0f, -1f), mockScreamTransform.Object.localPosition);
		Assert.Equal(new Vector3(1.5f, 1.5f, 1f), mockScreamTransform.Object.localScale);
		Assert.NotNull(mockSpriteRenderer.Object.sprite);
	}
}
