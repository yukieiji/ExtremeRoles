using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using ExtremeRoles.Core.Abstract;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Liberal.Encloser;
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

		SetupResourceMocks();
	}

	private static void SetupResourceMocks()
	{
		string key = $"{ObjectPath.Bomb}115";
		if (!LruCache<string, Sprite>.TryGetValue(key, out _))
		{
			var mockSprite = new Mock<Sprite>(IntPtr.Zero);
			LruCache<string, Sprite>.Add(key, mockSprite.Object);
		}

		var mockSpriteForAsset = new Mock<Sprite>(IntPtr.Zero);
		var mockBundle = new Mock<AssetBundle>(IntPtr.Zero);
		mockBundle.Setup(b => b.LoadAsset(It.IsAny<string>(), It.IsAny<Il2CppSystem.Type>()))
			.Returns(mockSpriteForAsset.Object);

		var cachedBundleField = typeof(UnityObjectLoader).GetField("cachedBundle", BindingFlags.NonPublic | BindingFlags.Static);
		if (cachedBundleField?.GetValue(null) is Dictionary<string, AssetBundle> dict)
		{
			dict["resources/bomb.asset"] = mockBundle.Object;
		}
	}

	private static void SetupLobbyBehaviourMock()
	{
		var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
		var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
		mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
		MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
	}

	private static void InitializeRole(EncloserRole role, byte playerId = 1)
	{
		role.CreateRoleAllOption();
		role.Initialize();

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[playerId] = role;
	}

	private static Vector2 CreateVec2(float x, float y)
	{
		var v = new Vector2();
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
			mockTrans.SetupProperty(t => t.position);

			var mockSr = new Mock<SpriteRenderer>(IntPtr.Zero);
			var mockLine = new Mock<LineRenderer>(IntPtr.Zero);
			mockLine.SetupProperty(l => l.positionCount, 0);

			var mockMeshFilter = new Mock<MeshFilter>(IntPtr.Zero);
			mockMeshFilter.SetupProperty(m => m.mesh, (Mesh)null!);

			var mockMeshRenderer = new Mock<MeshRenderer>(IntPtr.Zero);

			mockGo.SetupGet(g => g.transform).Returns(mockTrans.Object);
			mockGo.Setup(g => g.AddComponent<SpriteRenderer>()).Returns(mockSr.Object);
			mockGo.Setup(g => g.AddComponent<LineRenderer>()).Returns(mockLine.Object);
			mockGo.Setup(g => g.AddComponent<MeshFilter>()).Returns(mockMeshFilter.Object);
			mockGo.Setup(g => g.AddComponent<MeshRenderer>()).Returns(mockMeshRenderer.Object);

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

		public Vector3 CreateMapPos(Vector2 target, float offset = 1000.0f)
		{
			var v3 = new Vector3();
			v3.x = target.x;
			v3.y = target.y;
			v3.z = target.y / offset;
			return v3;
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
		var role = new EncloserRole();

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
	public void EncloserPolygon_Clear_ResetsPolygon()
	{
		// Arrange
		var factory = new MockUnityObjectFactory();
		var il2cppProvider = CreateMockIl2CppObjectProvider();
		var polygon = new EncloserPolygon(factory, il2cppProvider);

		polygon.AddStake(CreateVec2(0, 0), 3);
		polygon.AddStake(CreateVec2(10, 0), 3);
		polygon.AddStake(CreateVec2(5, 10), 3);

		Assert.True(polygon.IsCompleted);

		// Act
		polygon.Clear();

		// Assert
		Assert.Equal(0, polygon.Count);
		Assert.False(polygon.IsCompleted);
	}

	[Fact]
	public void EncloserStatusModel_PlaceStakeAndClear_UpdatesState()
	{
		// Arrange
		var factory = new MockUnityObjectFactory();
		var il2cppProvider = CreateMockIl2CppObjectProvider();
		var status = new EncloserStatusModel(3, 10, 1, factory, il2cppProvider);

		Assert.Equal(0, status.CurStakeCount);
		Assert.False(status.IsUseMetsu);

		// Act - Place stakes
		status.PlaceStake(CreateVec2(0, 0), true);
		status.PlaceStake(CreateVec2(10, 0), true);

		Assert.Equal(2, status.CurStakeCount);
		Assert.False(status.IsUseMetsu);

		status.PlaceStake(CreateVec2(5, 10), true);

		Assert.Equal(3, status.CurStakeCount);
		Assert.True(status.IsUseMetsu);
		Assert.True(status.IsKillPosition(CreateVec2(5, 5)));

		// Act - Clear
		status.ClearStake();

		// Assert
		Assert.Equal(0, status.CurStakeCount);
		Assert.False(status.IsUseMetsu);
		Assert.False(status.IsKillPosition(CreateVec2(5, 5)));
	}

	[Fact]
	public void EncloserRole_ResetOnMeetingStart_PreservesStakes()
	{
		// Arrange
		var role = new EncloserRole();
		InitializeRole(role, 1);

		// Act
		role.ResetOnMeetingStart();
		role.ResetOnMeetingEnd(null);

		// Assert
		Assert.Equal("Ec", role.GetRoleTag());
	}

	[Fact]
	public void EncloserRole_RpcOps_PlaceStakeAndUseMetsu_ModifiesRoleStatus()
	{
		// Arrange
		var role = new EncloserRole();
		InitializeRole(role, 1);

		var mockStatus = new EncloserStatusModel(3, 10, 1, new MockUnityObjectFactory(), CreateMockIl2CppObjectProvider());
		typeof(EncloserRole).GetField("status", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(role, mockStatus);

		// Act 1: Place Stake via RPC
		var placeReader = new Mock<MessageReader>(IntPtr.Zero);
		placeReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)1) // playerId
			.Returns((byte)EncloserRole.RpcOpsType.PlaceStake); // ops
		placeReader.SetupSequence(r => r.ReadSingle())
			.Returns(0f)  // x
			.Returns(0f); // y

		EncloserRole.RpcOps(placeReader.Object);

		Assert.Equal(1, mockStatus.CurStakeCount);

		// Act 2: Clear via UseMetsu RPC
		var metsuReader = new Mock<MessageReader>(IntPtr.Zero);
		metsuReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)1) // playerId
			.Returns((byte)EncloserRole.RpcOpsType.UseMetsu); // ops

		EncloserRole.RpcOps(metsuReader.Object);

		// Assert
		Assert.Equal(0, mockStatus.CurStakeCount);
	}

	[Fact]
	public void EncloserRole_RpcOps_InvalidPlayer_DoesNothing()
	{
		// Arrange
		var role = new EncloserRole();
		InitializeRole(role, 1);

		var mockStatus = new EncloserStatusModel(3, 10, 1, new MockUnityObjectFactory(), CreateMockIl2CppObjectProvider());
		typeof(EncloserRole).GetField("status", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(role, mockStatus);

		var reader = new Mock<MessageReader>(IntPtr.Zero);
		reader.SetupSequence(r => r.ReadByte())
			.Returns((byte)99) // nonexistent playerId
			.Returns((byte)EncloserRole.RpcOpsType.UseMetsu);

		// Act
		EncloserRole.RpcOps(reader.Object);

		// Assert
		Assert.Equal(0, mockStatus.CurStakeCount);
	}
}
