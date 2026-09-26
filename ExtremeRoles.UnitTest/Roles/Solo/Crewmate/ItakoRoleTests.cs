using System;
using System.Reflection;
using AmongUs.GameOptions;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Event;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ItakoRoleTests
{
	public ItakoRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupPaletteHelpers();
		MockSetupHelper.SetupObjectImplicitHelpers();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupLobbyMock();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupPlayerControlMocks();
		MockSetupHelper.SetupGameDataMock();
		SetupGameOptionsManagerMock();

		var shipStateField = typeof(ExtremeRolesPlugin).GetField("<ShipState>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
		shipStateField?.SetValue(null, new ExtremeShipStatus());

		var eventManagerField = typeof(ExtremeRoles.Module.Event.EventManager).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
		eventManagerField?.SetValue(null, new ExtremeRoles.Module.Event.EventManager());

		ExtremeRoleManager.GameRole.Clear();

		var clientMock = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);
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
	public void Constructor_InitializesItakoRoleCorrectly()
	{
		// Arrange & Act
		var itako = new ItakoRole();

		// Assert
		Assert.Equal(ExtremeRoleId.Itako, itako.Core.Id);
		Assert.Equal(ExtremeRoleType.Crewmate, itako.Core.Team);
		Assert.Equal(ColorPalette.ItakoSkyBlue, itako.Core.Color);
		Assert.True(itako.CanHasAnotherRole);
	}

	[Fact]
	public void RoleSpecificInit_LoadsOptionValues()
	{
		// Arrange
		var role = new ItakoRole();
		role.CreateRoleAllOption();

		// Act
		role.Initialize();

		// Assert
		var rangeField = typeof(ItakoRole).GetField("range", BindingFlags.NonPublic | BindingFlags.Instance);
		var requiredTaskRateField = typeof(ItakoRole).GetField("requiredTaskRate", BindingFlags.NonPublic | BindingFlags.Instance);

		float range = (float)rangeField?.GetValue(role)!;
		float requiredTaskRate = (float)requiredTaskRateField?.GetValue(role)!;

		Assert.Equal(1.0f, range);
		Assert.Equal(0.5f, requiredTaskRate);
	}

	[Fact]
	public void ExtractInheritedRole_SingleRole_ReturnsClonedSingleRole()
	{
		// Arrange
		var sheriff = new Sheriff();

		// Act
		var inherited = ItakoRole.ExtractInheritedRole(sheriff);

		// Assert
		Assert.NotNull(inherited);
		Assert.Equal(ExtremeRoleId.Sheriff, inherited.Core.Id);
	}

	[Fact]
	public void ExtractInheritedRole_MultiAssignRoleWithOffsetInfo_ReturnsClonedRoleWithoutAnotherRole()
	{
		// Arrange
		var buddyRole = new Buddy();
		buddyRole.OffsetInfo = new MultiAssignRoleBase.OptionOffsetInfo(CombinationRoleType.Buddy, 100);
		var bakery = new Bakary();
		buddyRole.SetAnotherRole(bakery);

		// Act
		var inherited = ItakoRole.ExtractInheritedRole(buddyRole);

		// Assert
		Assert.NotNull(inherited);
		Assert.Equal(ExtremeRoleId.Buddy, inherited.Core.Id);
		if (inherited is MultiAssignRoleBase multiAssign)
		{
			Assert.Null(multiAssign.AnotherRole);
		}
	}

	[Fact]
	public void DoInherit_SetsAnotherRoleAndOverwritesPreviousRole()
	{
		// Arrange
		byte itakoId = 1;
		byte targetId = 2;

		var itako = new ItakoRole();
		itako.CreateRoleAllOption();
		itako.Initialize();

		var sheriff = new Sheriff();
		sheriff.CreateRoleAllOption();
		sheriff.Initialize();

		ExtremeRoleManager.GameRole[itakoId] = itako;
		ExtremeRoleManager.GameRole[targetId] = sheriff;

		// Act 1: Inherit Sheriff
		ItakoRole.DoInherit(itakoId, targetId, cleanBody: false);

		// Assert 1
		Assert.NotNull(itako.AnotherRole);
		Assert.Equal(ExtremeRoleId.Sheriff, itako.AnotherRole.Core.Id);

		// Act 2: Overwrite with Bakary
		byte newTargetId = 3;
		var bakery = new Bakary();
		bakery.CreateRoleAllOption();
		bakery.Initialize();
		ExtremeRoleManager.GameRole[newTargetId] = bakery;

		ItakoRole.DoInherit(itakoId, newTargetId, cleanBody: false);

		// Assert 2
		Assert.NotNull(itako.AnotherRole);
		Assert.Equal(ExtremeRoleId.Bakary, itako.AnotherRole.Core.Id);
	}

	[Fact]
	public void RpcOps_SelfDestructAndInherit_ExecutesExpectedOperations()
	{
		// Arrange
		byte itakoId = 1;
		byte targetId = 2;

		var itako = new ItakoRole();
		itako.CreateRoleAllOption();
		itako.Initialize();

		var sheriff = new Sheriff();
		sheriff.CreateRoleAllOption();
		sheriff.Initialize();

		ExtremeRoleManager.GameRole[itakoId] = itako;
		ExtremeRoleManager.GameRole[targetId] = sheriff;

		// Act: RpcOps Inherit (ops = 1)
		var mockReader1 = new Mock<MessageReader>();
		mockReader1.SetupSequence(r => r.ReadByte())
			.Returns((byte)1) // Inherit
			.Returns(itakoId)
			.Returns(targetId);
		mockReader1.Setup(r => r.ReadBoolean()).Returns(false);

		ItakoRole.RpcOps(mockReader1.Object);

		// Assert
		Assert.NotNull(itako.AnotherRole);
		Assert.Equal(ExtremeRoleId.Sheriff, itako.AnotherRole.Core.Id);

		// Act: RpcOps SelfDestruct (ops = 0)
		var mockReader0 = new Mock<MessageReader>();
		mockReader0.SetupSequence(r => r.ReadByte())
			.Returns((byte)0) // SelfDestruct
			.Returns(itakoId)
			.Returns(targetId);

		ItakoRole.RpcOps(mockReader0.Object);
	}
}
