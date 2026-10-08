using System.Reflection;

using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Liberal;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Liberal;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class ScapeactorTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;
	private readonly Mock<GameData> mockGameData;

	public ScapeactorTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		SetupLobbyBehaviourMock();

		mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockClient.SetupGet(c => c.AmHost).Returns(true);
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockWriterGet = new Mock<MockMessageWriterGetHelper>();
		mockWriterGet.Setup(h => h.Invoke(It.IsAny<SendOption>())).Returns(mockWriter.Object);
		MockMessageWriterGetHelper.Instance = mockWriterGet.Object;

		var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
		mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<MessageWriter>(), It.IsAny<InnerNet.InnerNetObject>()));
		InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;

		mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		mockGameData = MockSetupHelper.SetupGameDataMock();

		var mockMeetingHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHelper.Setup(h => h.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHelper.Object;

		var mockExileHelper = new Mock<MockExileControllerget_InstanceHelper>();
		mockExileHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
		MockExileControllerget_InstanceHelper.Instance = mockExileHelper.Object;

		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
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

	private static void InitializeRole(Scapeactor role, byte playerId = 1)
	{
		role.CreateRoleAllOption();
		role.Initialize();

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[playerId] = role;
	}

	private static void SetWinMoney(Scapeactor role, float value)
	{
		var field = typeof(Scapeactor).GetField(
			"winMoney", BindingFlags.NonPublic | BindingFlags.Instance);
		field?.SetValue(role, value);
	}

	private LiberalMoneyBankSystem CreateBank(int winMoney)
	{
		var option = new Mock<ILiberalOptionLoader>();
		option.Setup(o => o.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.WinMoney)).Returns(winMoney);

		var bank = new LiberalMoneyBankSystem(option.Object);
		ExtremeSystemTypeManager.Instance.TryAdd(LiberalMoneyBankSystem.SystemType, bank);
		return bank;
	}

	private void SetupTaskPhase()
	{
		ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);
		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;
	}

	private void SetupBankRpcReader(float money, LiberalMoneyHistory.Reason reason)
	{
		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)ExtremeSystemType.LiberalMoneyBank)
			.Returns((byte)1)
			.Returns((byte)reason);
		mockReader.SetupSequence(r => r.ReadSingle()).Returns(money).Returns(0f);

		var mockReaderHelper = new Mock<MockMessageReaderGetHelper>();
		mockReaderHelper.Setup(g => g.Invoke(It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>>()))
			.Returns(mockReader.Object);
		MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;
	}

	private void SetupLocalPlayerWithTasks()
	{
		var mockInfo = new Mock<NetworkedPlayerInfo>();
		mockInfo.SetupGet(i => i.PlayerId).Returns((byte)1);
		mockInfo.SetupGet(i => i.Object).Returns(mockLocalPlayer.Object);

		var task1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		task1.SetupGet(t => t.Id).Returns(10u);
		task1.SetupGet(t => t.Complete).Returns(true);

		var task2 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		task2.SetupGet(t => t.Id).Returns(20u);
		task2.SetupGet(t => t.Complete).Returns(false);

		var mockTasks = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		mockTasks.SetupGet(t => t.Count).Returns(2);
		mockTasks.Setup(t => t[0]).Returns(task1.Object);
		mockTasks.Setup(t => t[1]).Returns(task2.Object);
		mockInfo.SetupGet(i => i.Tasks).Returns(mockTasks.Object);

		mockGameData.Setup(g => g.GetPlayerById(1)).Returns(mockInfo.Object);
	}

	[Fact]
	public void Initialize_RegistersOptionsAndProperties()
	{
		// Arrange
		var role = new Scapeactor();

		// Act
		InitializeRole(role);

		// Assert
		Assert.True(role.IsLiberal());
		Assert.True(role.HasTask);
		Assert.Equal("Sc", role.GetRoleTag());
	}

	[Fact]
	public void ExiledAction_GrantsWinMoneyToBankAndSendsRpc()
	{
		// Arrange
		var role = new Scapeactor();
		InitializeRole(role);
		var bank = CreateBank(100);
		SetupBankRpcReader(100f, LiberalMoneyHistory.Reason.AddOnExile);

		// Act
		role.ExiledAction(mockLocalPlayer.Object);

		// Assert
		Assert.Equal(100f, bank.Money);
		mockClient.Verify(
			c => c.StartRpcImmediately(
				It.IsAny<uint>(),
				It.Is<byte>(cmd => cmd == (byte)RPCOperator.Command.UpdateExtremeSystemType),
				It.IsAny<SendOption>(),
				It.IsAny<int>()),
			Times.AtLeastOnce);
	}

	[Fact]
	public void ExiledAction_WhenWinMoneyIsZero_GrantsNothing()
	{
		// Arrange
		var role = new Scapeactor();
		InitializeRole(role);
		SetWinMoney(role, 0f);
		var bank = CreateBank(100);
		SetupBankRpcReader(0f, LiberalMoneyHistory.Reason.AddOnExile);

		// Act
		role.ExiledAction(mockLocalPlayer.Object);

		// Assert
		Assert.Equal(0f, bank.Money);
	}

	[Fact]
	public void ExiledAction_ClearsTaskOfLocalPlayer()
	{
		// Arrange
		var role = new Scapeactor();
		InitializeRole(role);
		SetupBankRpcReader(100f, LiberalMoneyHistory.Reason.AddOnExile);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		// Act
		role.ExiledAction(mockPlayer.Object);

		// Assert
		mockPlayer.Verify(p => p.ClearTasks(), Times.Once);
	}

	[Fact]
	public void RolePlayerKilledAction_ClearsTaskOfLocalPlayer()
	{
		// Arrange
		var role = new Scapeactor();
		InitializeRole(role);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		// Act
		role.RolePlayerKilledAction(mockPlayer.Object, mockLocalPlayer.Object);

		// Assert
		mockPlayer.Verify(p => p.ClearTasks(), Times.Once);
	}

	[Fact]
	public void Update_TaskPhase_ForwardsTaskMoneyToBank()
	{
		// Arrange
		var role = new Scapeactor();
		InitializeRole(role);
		var bank = CreateBank(100);
		SetupTaskPhase();
		SetupLocalPlayerWithTasks();
		SetupBankRpcReader(5f, LiberalMoneyHistory.Reason.AddOnTask);

		// Act
		role.Update(mockLocalPlayer.Object); // waitTimer: 0.0 -> -0.1
		role.Update(mockLocalPlayer.Object); // スキャン → AddOnTask RPC

		// Assert
		Assert.Equal(5f, bank.Money);
	}
}
