using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Crewmate;
using Moq;
using UnityEngine;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.JailerTests;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class YardbirdRoleTests
{
	private readonly Mock<AmongUsClient> clientMock;
	private readonly Mock<IOptionLoader> optionLoaderMock;

	public YardbirdRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupPlayerControlMocks();
		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);

		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		var emptyCommonTasks = new Il2CppReferenceArray<NormalPlayerTask>(0);
		mockShipStatus.SetupGet(s => s.CommonTasks).Returns(emptyCommonTasks);

		var emptyLongTasks = new Il2CppReferenceArray<NormalPlayerTask>(0);
		mockShipStatus.SetupGet(s => s.LongTasks).Returns(emptyLongTasks);

		var emptyShortTasks = new Il2CppReferenceArray<NormalPlayerTask>(0);
		mockShipStatus.SetupGet(s => s.ShortTasks).Returns(emptyShortTasks);

		var mockShipStatusHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipStatusHelper.Setup(x => x.Invoke()).Returns(mockShipStatus.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipStatusHelper.Object;

		clientMock = MockSetupHelper.SetupAmongUsClientMock();
		clientMock.SetupGet(c => c.GameState).Returns(InnerNet.InnerNetClient.GameStates.Started);

		optionLoaderMock = new Mock<IOptionLoader>();
	}

	[Fact]
	public void Constructor_WhenLocalPlayerIsTarget_InitializesTasksAndMoveSpeed()
	{
		// Arrange
		byte localPlayerId = 1;
		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns(localPlayerId);

		var option = new Yardbird.Option(
			AddCommonTask: 0,
			AddNormalTask: 0,
			AddLongTask: 0,
			SpeedMod: 1.2f,
			Admin: true,
			Security: true,
			Vital: true,
			Vent: true,
			Sab: true
		);

		// Act
		var yardbird = new Yardbird(optionLoaderMock.Object, localPlayerId, option);

		// Assert
		Assert.NotNull(yardbird);
		Assert.Equal(ExtremeRoleId.Yardbird, yardbird.Core.Id);
		Assert.Equal(1.2f, yardbird.MoveSpeed);
		Assert.True(yardbird.CanUseAdmin);
		Assert.True(yardbird.CanUseSecurity);
		Assert.True(yardbird.CanUseVital);
		Assert.True(yardbird.UseVent);
		Assert.True(yardbird.UseSabotage);
	}

	[Fact]
	public void Constructor_WhenOtherPlayerIsTarget_DoesNotInitializeTasksForLocal()
	{
		// Arrange
		byte localPlayerId = 1;
		byte targetPlayerId = 2;
		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns(localPlayerId);

		var option = new Yardbird.Option(
			AddCommonTask: 1,
			AddNormalTask: 1,
			AddLongTask: 1,
			SpeedMod: 0.8f,
			Admin: false,
			Security: false,
			Vital: false,
			Vent: false,
			Sab: false
		);

		// Act
		var yardbird = new Yardbird(optionLoaderMock.Object, targetPlayerId, option);

		// Assert
		Assert.NotNull(yardbird);
		Assert.Equal(0.8f, yardbird.MoveSpeed);
		Assert.False(yardbird.CanUseAdmin);
		Assert.False(yardbird.CanUseSecurity);
		Assert.False(yardbird.CanUseVital);
		Assert.False(yardbird.UseVent);
		Assert.False(yardbird.UseSabotage);
	}

	[Fact]
	public void Update_WhenOtherPlayerTarget_HasEmptyTasks_DoesNothing()
	{
		// Arrange
		byte localPlayerId = 1;
		byte otherPlayerId = 2;
		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns(localPlayerId);

		var option = new Yardbird.Option(1, 1, 1, 1.0f, false, false, false, false, false);
		var yardbird = new Yardbird(optionLoaderMock.Object, otherPlayerId, option);

		var mockOtherPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockOtherPlayer.SetupGet(p => p.PlayerId).Returns(otherPlayerId);

		// Act & Assert (Tasks is empty, returns early)
		yardbird.Update(mockOtherPlayer.Object);
	}
}
