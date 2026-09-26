using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
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
		var v = new Vector2();
		v.x = x;
		v.y = y;
		return v;
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
	public void IsPointInPolygon_InsidePolygon_ReturnsTrue()
	{
		// Arrange
		var polygon = new List<Vector2>
		{
			CreateVec2(0, 0),
			CreateVec2(10, 0),
			CreateVec2(10, 10),
			CreateVec2(0, 10)
		};
		var testPoint = CreateVec2(5, 5);

		// Act
		bool isInside = Encloser.IsPointInPolygon(testPoint, polygon);

		// Assert
		Assert.True(isInside);
	}

	[Fact]
	public void IsPointInPolygon_OutsidePolygon_ReturnsFalse()
	{
		// Arrange
		var polygon = new List<Vector2>
		{
			CreateVec2(0, 0),
			CreateVec2(10, 0),
			CreateVec2(10, 10),
			CreateVec2(0, 10)
		};
		var testPoint = CreateVec2(15, 5);

		// Act
		bool isInside = Encloser.IsPointInPolygon(testPoint, polygon);

		// Assert
		Assert.False(isInside);
	}

	[Fact]
	public void IsPointInPolygon_LessThan3Vertices_ReturnsFalse()
	{
		// Arrange
		var polygon = new List<Vector2>
		{
			CreateVec2(0, 0),
			CreateVec2(10, 0)
		};
		var testPoint = CreateVec2(5, 0);

		// Act
		bool isInside = Encloser.IsPointInPolygon(testPoint, polygon);

		// Assert
		Assert.False(isInside);
	}
}
