using System.Collections.Generic;
using System.Reflection;

using ExtremeRoles.Module.PRNG;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Liberal;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Liberal;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class MartyrTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;

	public MartyrTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		SetupLobbyBehaviourMock();

		mockClient = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

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

	private static void InitializeRole(Martyr role, byte playerId = 1)
	{
		role.CreateRoleAllOption();
		role.Initialize();

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[playerId] = role;
	}

	private static void SetPrivateField(object target, string name, object value)
	{
		var field = target.GetType().GetField(
			name, BindingFlags.NonPublic | BindingFlags.Instance);
		field?.SetValue(target, value);
	}

	private static List<PlayerControl> CreateTargets(params byte[] ids)
	{
		var targets = new List<PlayerControl>();

		foreach (byte id in ids)
		{
			var mock = new Mock<PlayerControl>(IntPtr.Zero);
			mock.SetupGet(p => p.PlayerId).Returns(id);
			targets.Add(mock.Object);
		}

		return targets;
	}

	private static IRng CreateRng(params int[] rolls)
	{
		return new FakeRng(rolls);
	}

	private sealed class FakeRng : IRng
	{
		private readonly Queue<int> rolls;

		public FakeRng(params int[] rolls)
		{
			this.rolls = new Queue<int>(rolls);
		}

		public string InitState => string.Empty;

		public int Next()
		{
			return this.rolls.Dequeue();
		}

		public int Next(int maxExclusive)
		{
			return this.rolls.Dequeue();
		}

		public int Next(int minInclusive, int maxExclusive)
		{
			return this.rolls.Dequeue();
		}
	}

	[Fact]
	public void Initialize_RegistersOptionsAndProperties()
	{
		// Arrange
		var role = new Martyr();

		// Act
		InitializeRole(role);

		// Assert
		Assert.True(role.CanKill);
		Assert.False(role.HasTask);
		Assert.Equal("Ma", role.GetRoleTag());
	}

	[Fact]
	public void ComputeExplosion_ZeroProbability_KillsNobody()
	{
		// Arrange
		var role = new Martyr();
		SetPrivateField(role, "killProbability", 0);
		SetPrivateField(role, "killMoney", 10);
		var rng = CreateRng(0, 0, 0);

		// Act
		var outcome = role.ComputeExplosion(CreateTargets(1, 2, 3), rng);

		// Assert
		Assert.Empty(outcome.KilledTargets);
		Assert.Equal(0f, outcome.EarnedMoney);
	}

	[Fact]
	public void ComputeExplosion_FullProbability_KillsAllTargets()
	{
		// Arrange
		var role = new Martyr();
		SetPrivateField(role, "killProbability", 100);
		SetPrivateField(role, "killMoney", 10);
		var rng = CreateRng(99, 99, 99);

		// Act
		var outcome = role.ComputeExplosion(CreateTargets(1, 2, 3), rng);

		// Assert
		Assert.Equal(new byte[] { 1, 2, 3 }, outcome.KilledTargets);
		Assert.Equal(30f, outcome.EarnedMoney);
	}

	[Fact]
	public void ComputeExplosion_PartialProbability_KillsByRoll()
	{
		// Arrange
		var role = new Martyr();
		SetPrivateField(role, "killProbability", 50);
		SetPrivateField(role, "killMoney", 10);
		var rng = CreateRng(10, 60, 40);

		// Act
		var outcome = role.ComputeExplosion(CreateTargets(1, 2, 3), rng);

		// Assert
		Assert.Equal(new byte[] { 1, 3 }, outcome.KilledTargets);
		Assert.Equal(20f, outcome.EarnedMoney);
	}

	[Fact]
	public void ComputeExplosion_EmptyTargets_NoKillNoMoney()
	{
		// Arrange
		var role = new Martyr();
		SetPrivateField(role, "killProbability", 50);
		SetPrivateField(role, "killMoney", 10);
		var rng = CreateRng();

		// Act
		var outcome = role.ComputeExplosion(CreateTargets(), rng);

		// Assert
		Assert.Empty(outcome.KilledTargets);
		Assert.Equal(0f, outcome.EarnedMoney);
	}
}
