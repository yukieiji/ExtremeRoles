using System;
using System.Reflection;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using Moq;
using UnityEngine;
using Xunit;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ChargerTests
{
	private readonly Mock<AmongUsClient> clientMock;

	public ChargerTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupPaletteHelpers();
		MockSetupHelper.SetupObjectImplicitHelpers();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.PlayerId).Returns((byte)1);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockLocalPlayer.Object);
		mockLocalPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		MockSetupHelper.SetupGameDataMock();
		MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		MockSetupHelper.SetupOptionManager();
		SetupGameOptionsManagerMock();

		var shipStateField = typeof(ExtremeRolesPlugin).GetField("<ShipState>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
		shipStateField?.SetValue(null, new ExtremeShipStatus());

		ExtremeRoleManager.GameRole.Clear();

		this.clientMock = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		this.clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);

		SetLobbyMode(false);
	}

	private void SetLobbyMode(bool isLobby)
	{
		if (isLobby)
		{
			this.clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.NotJoined);
			var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
			var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
			mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
			MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
		}
		else
		{
			this.clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
			var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
			mockLobbyHelper.Setup(x => x.Invoke()).Returns((LobbyBehaviour)null!);
			MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
		}
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
	public void RoleSpecificInit_LoadsOptionValuesCorrectly()
	{
		// Arrange
		SetLobbyMode(false);
		var charger = new Charger();
		charger.CreateRoleAllOption();

		// Act
		charger.Initialize();

		// Assert
		var awakeTaskGageField = typeof(Charger).GetField("awakeTaskGage", BindingFlags.NonPublic | BindingFlags.Instance);
		var chargeTimeField = typeof(Charger).GetField("chargeTime", BindingFlags.NonPublic | BindingFlags.Instance);
		var chargeRangeField = typeof(Charger).GetField("chargeRange", BindingFlags.NonPublic | BindingFlags.Instance);
		var addAbilityCountField = typeof(Charger).GetField("addAbilityCount", BindingFlags.NonPublic | BindingFlags.Instance);
		var restoreKillCooldownField = typeof(Charger).GetField("restoreKillCooldown", BindingFlags.NonPublic | BindingFlags.Instance);

		float awakeTaskGage = (float)awakeTaskGageField?.GetValue(charger)!;
		float chargeTime = (float)chargeTimeField?.GetValue(charger)!;
		float chargeRange = (float)chargeRangeField?.GetValue(charger)!;
		int addAbilityCount = (int)addAbilityCountField?.GetValue(charger)!;
		bool restoreKillCooldown = (bool)restoreKillCooldownField?.GetValue(charger)!;

		Assert.Equal(0.5f, awakeTaskGage);
		Assert.Equal(3.0f, chargeTime);
		Assert.Equal(2.5f, chargeRange);
		Assert.Equal(1, addAbilityCount);
		Assert.False(restoreKillCooldown);
	}

	[Fact]
	public void Awake_Property_ReflectsAwakeStatus()
	{
		// Arrange
		SetLobbyMode(false);
		var charger = new Charger();
		charger.CreateRoleAllOption();
		charger.Initialize();

		// Assert
		Assert.False(charger.IsAwake);

		var awakeRoleField = typeof(Charger).GetField("awakeRole", BindingFlags.NonPublic | BindingFlags.Instance);
		awakeRoleField?.SetValue(charger, true);

		Assert.True(charger.IsAwake);
	}

	[Fact]
	public void RoleName_And_Color_MatchExpectedValues()
	{
		// Arrange
		var charger = new Charger();

		// Assert
		Assert.Equal(ExtremeRoleId.Charger, charger.Core.Id);
		Assert.Equal(ColorPalette.ChargerElectricYellow, charger.Core.Color);
	}

	[Fact]
	public void GetFakeOptionString_ReturnsEmptyString()
	{
		// Arrange
		var charger = new Charger();

		// Act
		string result = charger.GetFakeOptionString();

		// Assert
		Assert.Equal("", result);
	}
}
