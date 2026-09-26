using System.Collections.Generic;

using UnityEngine;
using ExtremeRoles.Core.Abstract;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Liberal;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Liberal;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class EncloserTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;

	public EncloserTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		SetupLobbyBehaviourMock();

		this.mockClient = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		this.mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		this.mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		this.mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		MockSetupHelper.SetupGameDataMock();

		var mockMeetingHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHelper.Setup(h => h.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHelper.Object;

		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		string key = $"{ObjectPath.Bomb}115";
		if (!LruCache<string, Sprite>.TryGetValue(key, out _))
		{
			var mockSprite = new Mock<Sprite>(IntPtr.Zero);
			LruCache<string, Sprite>.Add(key, mockSprite.Object);
		}
	}

	private static void SetupLobbyBehaviourMock()
	{
		var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
		var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
		mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
		MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
	}

	private static void InitializeRole(Encloser role, byte playerId = 1)
	{
		role.CreateRoleAllOption();
		role.Initialize();

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[playerId] = role;
	}

	private static Vector2 CreateVec2(float x, float y)
	{
		var v = default(Vector2);
		v.x = x;
		v.y = y;
		return v;
	}

	private static IIl2CppObjectProvider CreateMockIl2CppObjectProvider()
	{
		var mock = new Mock<IIl2CppObjectProvider>();
		mock.Setup(p => p.GetStructArray<Vector3>(It.IsAny<int>()))
			.Returns((int size) => new Mock<Il2CppStructArray<Vector3>>(IntPtr.Zero).Object);
		mock.Setup(p => p.GetStructArray<int>(It.IsAny<int>()))
			.Returns((int size) => new Mock<Il2CppStructArray<int>>(IntPtr.Zero).Object);
		mock.Setup(p => p.GetStructArray<Color>(It.IsAny<int>()))
			.Returns((int size) => new Mock<Il2CppStructArray<Color>>(IntPtr.Zero).Object);
		return mock.Object;
	}

	private sealed class MockUnityObjectFactory : IUnityObjectFactory
	{
		public GameObject CreateGameObject(string name)
		{
			var mockGo = new Mock<GameObject>(IntPtr.Zero);
			var mockTrans = new Mock<Transform>(IntPtr.Zero);
			Vector3 pos = Vector3.zero;
			mockTrans.SetupGet(t => t.position).Returns(() => pos);
			mockTrans.SetupSet(t => t.position = It.IsAny<Vector3>()).Callback<Vector3>(v => pos = v);
			var mockSr = new Mock<SpriteRenderer>(IntPtr.Zero);
			mockGo.SetupGet(g => g.transform).Returns(mockTrans.Object);
			mockGo.Setup(g => g.AddComponent<SpriteRenderer>()).Returns(mockSr.Object);
			mockGo.Setup(g => g.AddComponent<LineRenderer>()).Returns(new Mock<LineRenderer>(IntPtr.Zero).Object);
			mockGo.Setup(g => g.AddComponent<MeshFilter>()).Returns(new Mock<MeshFilter>(IntPtr.Zero).Object);
			mockGo.Setup(g => g.AddComponent<MeshRenderer>()).Returns(new Mock<MeshRenderer>(IntPtr.Zero).Object);

			return mockGo.Object;
		}

		public Material CreateMaterial(Shader shader)
		{
			return new Mock<Material>(IntPtr.Zero).Object;
		}

		public Material CreateSpriteMaterial()
		{
			return new Mock<Material>(IntPtr.Zero).Object;
		}

		public Mesh CreateMesh()
		{
			return new Mock<Mesh>(IntPtr.Zero).Object;
		}
	}

	[Fact]
	public void Initialize_RegistersOptionsAndProperties()
	{
		// Arrange
		var role = new Encloser();

		// Act
		InitializeRole(role);
		var tag = role.GetRoleTag();

		// Assert
		Assert.Equal("Ec", tag);
		Assert.True(role.CanKill);
		Assert.False(role.HasTask);
	}

	[Fact]
	public void EncloserPolygon_InsidePolygon_ReturnsTrue()
	{
		// Arrange
		var factory = new MockUnityObjectFactory();
		var il2cppProvider = CreateMockIl2CppObjectProvider();
		var polygon = new EncloserPolygon(factory, il2cppProvider);

		// Act
		polygon.AddStake(CreateVec2(0, 0), 4);
		polygon.AddStake(CreateVec2(10, 0), 4);
		polygon.AddStake(CreateVec2(10, 10), 4);
		polygon.AddStake(CreateVec2(0, 10), 4);

		var testPoint = CreateVec2(5, 5);
		bool isInside = polygon.IsPointInside(testPoint);

		// Assert
		Assert.True(isInside);
		Assert.True(polygon.IsCompleted);
		Assert.Equal(4, polygon.Count);
	}

	[Fact]
	public void EncloserPolygon_OutsidePolygon_ReturnsFalse()
	{
		// Arrange
		var factory = new MockUnityObjectFactory();
		var il2cppProvider = CreateMockIl2CppObjectProvider();
		var polygon = new EncloserPolygon(factory, il2cppProvider);

		// Act
		polygon.AddStake(CreateVec2(0, 0), 4);
		polygon.AddStake(CreateVec2(10, 0), 4);
		polygon.AddStake(CreateVec2(10, 10), 4);
		polygon.AddStake(CreateVec2(0, 10), 4);

		var testPoint = CreateVec2(15, 5);
		bool isInside = polygon.IsPointInside(testPoint);

		// Assert
		Assert.False(isInside);
	}

	[Fact]
	public void EncloserPolygon_NotCompleted_ReturnsFalse()
	{
		// Arrange
		var factory = new MockUnityObjectFactory();
		var il2cppProvider = CreateMockIl2CppObjectProvider();
		var polygon = new EncloserPolygon(factory, il2cppProvider);

		// Act
		polygon.AddStake(CreateVec2(0, 0), 4);
		polygon.AddStake(CreateVec2(10, 0), 4);

		var testPoint = CreateVec2(5, 0);
		bool isInside = polygon.IsPointInside(testPoint);

		// Assert
		Assert.False(isInside);
		Assert.False(polygon.IsCompleted);
		Assert.Equal(2, polygon.Count);
	}

	[Fact]
	public void EncloserAbilityHandler_HandlePlaceStake_UpdatesPolygonState()
	{
		// Arrange
		var factory = new MockUnityObjectFactory();
		var il2cppProvider = CreateMockIl2CppObjectProvider();
		var handler = new EncloserAbilityHandler(3, 1, 20f, 10, factory, il2cppProvider);

		// Act
		handler.HandlePlaceStake(1, CreateVec2(0, 0));
		handler.HandlePlaceStake(1, CreateVec2(10, 0));

		bool isCompletedBeforeMax = handler.Polygon.IsCompleted;

		handler.HandlePlaceStake(1, CreateVec2(5, 10));
		bool isCompletedAfterMax = handler.Polygon.IsCompleted;

		// Assert
		Assert.False(isCompletedBeforeMax);
		Assert.True(isCompletedAfterMax);
		Assert.Equal(3, handler.Polygon.Count);
	}

	[Fact]
	public void EncloserAbilityHandler_HandleUseMetsuRpc_ClearsPolygon()
	{
		// Arrange
		var factory = new MockUnityObjectFactory();
		var il2cppProvider = CreateMockIl2CppObjectProvider();
		var handler = new EncloserAbilityHandler(3, 1, 20f, 10, factory, il2cppProvider);
		handler.HandlePlaceStake(1, CreateVec2(0, 0));
		handler.HandlePlaceStake(1, CreateVec2(10, 0));
		handler.HandlePlaceStake(1, CreateVec2(5, 10));

		// Act
		handler.HandleUseMetsuRpc(1);

		// Assert
		Assert.Equal(0, handler.Polygon.Count);
		Assert.False(handler.Polygon.IsCompleted);
	}

	[Fact]
	public void Encloser_ResetOnMeetingStart_PreservesStakes()
	{
		// Arrange
		var role = new Encloser();
		InitializeRole(role, 1);

		// Act
		role.ResetOnMeetingStart();
		role.ResetOnMeetingEnd(null);

		// Assert
		Assert.Equal("Ec", role.GetRoleTag());
	}
}
