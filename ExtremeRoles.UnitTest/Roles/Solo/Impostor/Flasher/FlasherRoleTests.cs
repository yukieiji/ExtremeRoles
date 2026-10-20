using System;
using System.Collections.Generic;
using System.Reflection;

using AmongUs.GameOptions;

using Hazel;

using Il2CppInterop.Runtime.InteropTypes.Arrays;

using Moq;

using UnityEngine;

using Xunit;

using ExtremeRoles.GameMode;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Impostor.Flasher;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor.Flasher;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class FlasherRoleTests
{
	private const byte LocalPlayerId = 1;
	private const byte NotRegisteredPlayerId = 99;

	private readonly Mock<AmongUsClient> mockClient;
	private readonly List<byte> writtenBytes = new();

	public FlasherRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupMathfHelpers();
		MockSetupHelper.SetupPaletteHelpers();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		if (ExtremeGameModeManager.Instance == null)
		{
			ExtremeGameModeManager.Create(GameModes.Normal);
		}

		SetupTranslationControllerMock();

		this.mockClient = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		mockWriter.Setup(w => w.Write(It.IsAny<byte>()))
			.Callback((byte value) => this.writtenBytes.Add(value));
		this.mockClient.Setup(c => c.StartRpcImmediately(
			It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns(LocalPlayerId);

		MockSetupHelper.SetupGameDataMock();
		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();
	}

	private static void SetupTranslationControllerMock()
	{
		var mockTranslation =
			MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(
			It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
			.Returns("TestString");
		mockTranslation.Setup(t => t.GetString(
			It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns("TestString");
	}

	private static FlasherStatusModel? GetStatus(FlasherRole role)
		=> typeof(FlasherRole)
			.GetField("status", BindingFlags.NonPublic | BindingFlags.Instance)?
			.GetValue(role) as FlasherStatusModel;

	/// <summary>
	/// 演出の差し替え。RPCの反映先をモックに置き換える。
	/// </summary>
	private static void ReplaceScreenEffect(FlasherRole role, IFlashScreenEffect screenEffect)
	{
		var handler = typeof(FlasherRole)
			.GetField("handler", BindingFlags.NonPublic | BindingFlags.Instance)!
			.GetValue(role)!;

		typeof(FlasherAbilityHandler)
			.GetField("screenEffect", BindingFlags.NonPublic | BindingFlags.Instance)!
			.SetValue(handler, screenEffect);
	}

	private static void InitializeRole(FlasherRole role, byte playerId)
	{
		role.CreateRoleAllOption();
		role.Initialize();

		ExtremeRoleManager.GameRole[playerId] = role;
	}

	/// <summary>
	/// 役職の初期化で送信されたRPCを計数対象から除外する。
	/// </summary>
	private void ClearRpcRecords()
	{
		this.mockClient.Invocations.Clear();
		this.writtenBytes.Clear();
	}

	private void VerifySendRpcCount(int expected)
	{
		this.mockClient.Verify(
			c => c.StartRpcImmediately(
				It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()),
			Times.Exactly(expected));
	}

	[Fact]
	public void CreateRoleAllOption_CreatesRangeAndColorOptions()
	{
		// Arrange
		var role = new FlasherRole();

		// Act
		role.CreateRoleAllOption();

		// Assert
		Assert.True(role.Loader.TryGetValue<FlasherRole.Option, float>(
			FlasherRole.Option.Range, out float range));
		Assert.True(role.Loader.TryGetValue<FlasherRole.Option, int>(
			FlasherRole.Option.FlashColor, out int flashColor));
	}


	[Fact]
	public void ResetOnMeetingStart_WhenSequenceActive_SendsFlashEnd()
	{
		// Arrange
		var role = new FlasherRole();
		InitializeRole(role, LocalPlayerId);
		FlasherStatusModel? status = GetStatus(role);
		status!.StartSequence(1.0f);
		ClearRpcRecords();

		// Act
		role.ResetOnMeetingStart();

		// Assert
		VerifySendRpcCount(1);
		Assert.Equal(
			new List<byte>()
			{
				(byte)FlasherRole.FlasherRpcOps.FlashEnd,
				LocalPlayerId,
			},
			this.writtenBytes);
		Assert.False(status.IsSequenceActive);
	}

	[Fact]
	public void ResetOnMeetingStart_WhenSequenceNotActive_SendsNothing()
	{
		// Arrange
		var role = new FlasherRole();
		InitializeRole(role, LocalPlayerId);
		ClearRpcRecords();

		// Act
		role.ResetOnMeetingStart();

		// Assert
		VerifySendRpcCount(0);
	}

	[Fact]
	public void RpcOps_WhenRolePlayerExists_AppliesOpsToItsEffect()
	{
		// Arrange
		var role = new FlasherRole();
		InitializeRole(role, LocalPlayerId);
		var mockScreenEffect = new Mock<IFlashScreenEffect>();
		ReplaceScreenEffect(role, mockScreenEffect.Object);

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)FlasherRole.FlasherRpcOps.FlashEnd)
			.Returns(LocalPlayerId);

		// Act
		FlasherRole.RpcOps(mockReader.Object);

		// Assert
		mockScreenEffect.Verify(s => s.Stop(), Times.Once);
	}

	[Fact]
	public void RpcOps_WhenRolePlayerNotFound_DoesNotApplyOps()
	{
		// Arrange
		var otherRole = new FlasherRole();
		InitializeRole(otherRole, LocalPlayerId);
		var mockScreenEffect = new Mock<IFlashScreenEffect>();
		ReplaceScreenEffect(otherRole, mockScreenEffect.Object);

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)FlasherRole.FlasherRpcOps.FlashEnd)
			.Returns(NotRegisteredPlayerId);

		// Act
		FlasherRole.RpcOps(mockReader.Object);

		// Assert
		mockScreenEffect.Verify(s => s.Stop(), Times.Never);
	}
}
