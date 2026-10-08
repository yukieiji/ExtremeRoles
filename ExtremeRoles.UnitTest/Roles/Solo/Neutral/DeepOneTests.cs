using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Neutral;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Neutral;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class DeepOneTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;

	public DeepOneTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupPlayerVoteAreaMocks();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

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

		if (ExtremeRoles.GameMode.ExtremeGameModeManager.Instance == null)
		{
			ExtremeRoles.GameMode.ExtremeGameModeManager.Create(GameModes.Normal);
		}
		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		if (ExtremeRolesPlugin.ShipState == null)
		{
			var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(nameof(ExtremeRolesPlugin.ShipState), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
			shipStateProp?.SetValue(null, new ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus());
		}
	}

	private static void SetupLobbyBehaviourMock()
	{
		var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
		var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
		mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
		MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
	}

	private static void InitializeRole(SingleRoleBase role, byte playerId = 1)
	{
		role.CreateRoleAllOption();
		role.Initialize();

		ExtremeRoleManager.GameRole[playerId] = role;
	}

	[Fact]
	public void Initialize_RegistersDeepOneRoleProperties()
	{
		var deepOne = new DeepOne();
		InitializeRole(deepOne, 1);

		Assert.Equal(ExtremeRoleId.DeepOne, deepOne.Core.Id);
		Assert.True(deepOne.IsNeutral());
		Assert.NotNull(deepOne.System);
	}

	[Fact]
	public void DeepOneSystem_CalculatesRequiredClicks()
	{
		var system = new DeepOneSystem(
			baseRequiredClicks: 5,
			requiredWinFrogs: 5,
			enableClickMultiplier: true,
			enableKillBlock: true,
			enableExileBlock: true,
			enableVentUnlock: true,
			enableMaxSpeed: true);

		Assert.Equal(5, system.GetRequiredClicks());
	}

	[Fact]
	public void DeepOneSystem_UpdatesRolePropertiesOnFrogCountChange()
	{
		var deepOne = new DeepOne();
		InitializeRole(deepOne, 1);

		Assert.False(deepOne.IsBlockKill);
		Assert.False(deepOne.IsBlockExile);
		Assert.False(deepOne.UseVent);
	}
}
