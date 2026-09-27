using System;
using AmongUs.GameOptions;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.Roles.Solo.Neutral;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Neutral;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class ImitaterTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;

	public ImitaterTests()
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

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);

		MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

		SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		ExtremeRoleManager.CreateNormalRoleOptions();

		if (ExtremeRoles.GameMode.ExtremeGameModeManager.Instance == null)
		{
			ExtremeRoles.GameMode.ExtremeGameModeManager.Create(GameModes.Normal);
		}

		if (ExtremeRolesPlugin.ShipState == null)
		{
			var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(nameof(ExtremeRolesPlugin.ShipState), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
			shipStateProp?.SetValue(null, new ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus());
		}

		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		var emptyTasks = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<NormalPlayerTask>(0);

		mockShipStatus.SetupGet(s => s.CommonTasks).Returns(emptyTasks);
		mockShipStatus.SetupGet(s => s.LongTasks).Returns(emptyTasks);
		mockShipStatus.SetupGet(s => s.ShortTasks).Returns(emptyTasks);

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShipStatus.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;
	}

	private static void SetupLobbyBehaviourMock()
	{
		var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
		var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
		mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
		MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
	}

	private static void SetupGameOptionsManagerMock()
	{
		var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
		mockGameOptions.Setup(g => g.GetFloat(It.IsAny<FloatOptionNames>())).Returns(1.0f);
		mockGameOptions.Setup(g => g.GetInt(It.IsAny<Int32OptionNames>())).Returns(1);
		mockGameOptions.Setup(g => g.GetBool(It.IsAny<BoolOptionNames>())).Returns(false);

		var mockGameOptionsManager = new Mock<GameOptionsManager>(IntPtr.Zero);
		mockGameOptionsManager.SetupGet(g => g.CurrentGameOptions).Returns(mockGameOptions.Object);

		var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
		mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockGameOptionsManager.Object);
		MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;
	}

	private static void InitializeRole(SingleRoleBase role, byte playerId = 1)
	{
		role.CreateRoleAllOption();
		role.Initialize();

		ExtremeRoleManager.GameRole[playerId] = role;
	}

	[Fact]
	public void Initialize_RegistersOptionsAndProperties()
	{
		// Arrange
		var role = new Imitater();

		// Act
		InitializeRole(role);

		// Assert
		Assert.Equal(ExtremeRoleId.Imitater, role.Core.Id);
		Assert.True(role.IsNeutral());
		Assert.NotNull(role.AbilityClass);
	}

	[Fact]
	public void TryKilledFrom_WhenAttackerValid_BlocksKillAndReturnsFalse()
	{
		// Arrange
		var role = new Imitater();
		InitializeRole(role, 1);

		var attackerRole = new SpecialImpostor();
		InitializeRole(attackerRole, 2);

		var mockRolePlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockRolePlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockAttackerPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockAttackerPlayer.SetupGet(p => p.PlayerId).Returns((byte)2);

		var handler = (ImitaterAbilityHandler)role.AbilityClass!;

		// Act
		bool result = handler.TryKilledFrom(mockRolePlayer.Object, mockAttackerPlayer.Object);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void TryKilledFrom_WhenAttackerNull_ReturnsTrue()
	{
		// Arrange
		var role = new Imitater();
		InitializeRole(role, 1);

		var mockRolePlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockRolePlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var handler = (ImitaterAbilityHandler)role.AbilityClass!;

		// Act
		bool result = handler.TryKilledFrom(mockRolePlayer.Object, null!);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void TryGetExtractInheritedRole_ValidRoles_ReturnsTrue()
	{
		// Arrange & Act & Assert
		Assert.True(Imitater.TryGetExtractInheritedRole(new Sheriff(), out var extractedSheriff));
		Assert.NotNull(extractedSheriff);

		Assert.True(Imitater.TryGetExtractInheritedRole(new VanillaRoleWrapper(RoleTypes.Crewmate), out var extractedVanilla));
		Assert.NotNull(extractedVanilla);
	}

	[Fact]
	public void CheckAbility_WhenNoTargetSet_ReturnsFalse()
	{
		// Arrange
		var role = new Imitater();
		InitializeRole(role, 1);

		// Act
		bool canUse = role.CheckAbility();

		// Assert
		Assert.False(canUse);
	}
}
