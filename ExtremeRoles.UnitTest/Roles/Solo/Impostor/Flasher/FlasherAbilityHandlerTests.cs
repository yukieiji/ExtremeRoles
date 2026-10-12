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
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Impostor.Flasher;
using ExtremeRoles.Roles.Solo.Neutral;

using MonoMod.RuntimeDetour;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor.Flasher;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class FlasherAbilityHandlerTests : IDisposable
{
	private delegate float Vector2MagOrig(ref Vector2 self);
	private delegate float Vector2MagHook(Vector2MagOrig orig, ref Vector2 self);

	private delegate void Vector2CtorOrig(ref Vector2 self, float x, float y);
	private delegate void Vector2CtorHook(Vector2CtorOrig orig, ref Vector2 self, float x, float y);

	private const byte LocalPlayerId = 1;
	private const byte TargetPlayerId = 2;
	private const float ActiveTime = 1.5f;

	private readonly Hook magHook;
	private readonly Hook ctorHook;
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;
	private readonly Mock<NetworkedPlayerInfo> mockLocalPlayerInfo;
	private readonly Mock<IFlashScreenEffect> mockScreenEffect;
	private readonly List<NetworkedPlayerInfo> playerInfoList = new();
	private readonly List<byte> writtenBytes = new();
	private readonly List<float> writtenFloats = new();

	public FlasherAbilityHandlerTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();

		var magTarget = typeof(Vector2).GetProperty("magnitude")!.GetGetMethod()!;
		var magHookDelegate = new Vector2MagHook((Vector2MagOrig orig, ref Vector2 self) =>
			(float)Math.Sqrt(self.x * self.x + self.y * self.y));
		this.magHook = new Hook(magTarget, magHookDelegate);

		var ctorTarget = typeof(Vector2).GetConstructor(new[] { typeof(float), typeof(float) })!;
		var ctorHookDelegate = new Vector2CtorHook((Vector2CtorOrig orig, ref Vector2 self, float x, float y) =>
		{
			self.x = x;
			self.y = y;
		});
		this.ctorHook = new Hook(ctorTarget, ctorHookDelegate);
		MockSetupHelper.SetupMathfHelpers();
		MockSetupHelper.SetupPaletteHelpers();
		MockSetupHelper.SetupVector2Helpers();
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
		mockWriter.Setup(w => w.Write(It.IsAny<float>()))
			.Callback((float value) => this.writtenFloats.Add(value));
		mockWriter.Setup(w => w.ToByteArray(It.IsAny<bool>()))
			.Returns(new Mock<Il2CppStructArray<byte>>(IntPtr.Zero).Object);
		this.mockClient.Setup(c => c.StartRpcImmediately(
			It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		this.mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		this.mockLocalPlayer.SetupGet(p => p.PlayerId).Returns(LocalPlayerId);
		this.mockLocalPlayer.SetupGet(p => p.CanMove).Returns(true);
		this.mockLocalPlayer.Setup(p => p.GetTruePosition())
			.Returns(new Vector2(10.0f, 10.0f));

		this.mockLocalPlayerInfo = createPlayerInfoMock(LocalPlayerId, this.mockLocalPlayer);
		this.mockLocalPlayer.SetupGet(p => p.Data).Returns(this.mockLocalPlayerInfo.Object);
		this.playerInfoList.Add(this.mockLocalPlayerInfo.Object);

		var mockGameData = MockSetupHelper.SetupGameDataMock();
		var mockPlayerList =
			new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		mockPlayerList.SetupGet(l => l.Count).Returns(() => this.playerInfoList.Count);
		mockPlayerList.Setup(l => l[It.IsAny<int>()])
			.Returns((int index) => this.playerInfoList[index]);
		mockGameData.SetupGet(g => g.AllPlayers).Returns(mockPlayerList.Object);

		this.mockScreenEffect = new Mock<IFlashScreenEffect>();

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

	private static Mock<NetworkedPlayerInfo> createPlayerInfoMock(
		byte playerId, Mock<PlayerControl> mockPlayer)
	{
		var mockInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockInfo.SetupGet(d => d.PlayerId).Returns(playerId);
		mockInfo.SetupGet(d => d.IsDead).Returns(false);
		mockInfo.SetupGet(d => d.Disconnected).Returns(false);
		mockInfo.SetupGet(d => d.Object).Returns(mockPlayer.Object);

		return mockInfo;
	}

	private FlasherRole CreateRole()
	{
		var role = new FlasherRole();
		ExtremeRoleManager.GameRole[LocalPlayerId] = role;

		return role;
	}

	/// <summary>
	/// ローカルプレイヤーから見た位置と状態のターゲットをプレイヤー一覧へ追加する。
	/// </summary>
	private void CreateTarget(
		Vector2 position, bool isDead, bool inVent, bool isImpostorTeam)
	{
		var cachedPtrField = typeof(UnityEngine.Object).GetField("m_CachedPtr", BindingFlags.NonPublic | BindingFlags.Instance);

		var mockTarget = new Mock<PlayerControl>(IntPtr.Zero);
		cachedPtrField?.SetValue(mockTarget.Object, (IntPtr)TargetPlayerId);
		mockTarget.SetupGet(p => p.PlayerId).Returns(TargetPlayerId);
		mockTarget.Setup(p => p.GetTruePosition()).Returns(position);
		mockTarget.SetupGet(p => p.inVent).Returns(inVent);
		mockTarget.SetupGet(p => p.inMovingPlat).Returns(false);
		mockTarget.SetupGet(p => p.onLadder).Returns(false);

		var mockInfo = createPlayerInfoMock(TargetPlayerId, mockTarget);
		mockInfo.SetupGet(d => d.IsDead).Returns(isDead);
		mockTarget.SetupGet(p => p.Data).Returns(mockInfo.Object);

		this.playerInfoList.Add(mockInfo.Object);

		SingleRoleBase targetRole = isImpostorTeam
			? new FlasherRole()
			: new Jester();
		ExtremeRoleManager.GameRole[TargetPlayerId] = targetRole;
	}

	private FlasherAbilityHandler CreateHandler(
		FlasherRole role, FlasherColor flashColor)
	{
		var status = new FlasherStatusModel(1.5f, flashColor);

		return new FlasherAbilityHandler(
			status, role, this.mockScreenEffect.Object, ActiveTime);
	}

	private void ClearRpcRecords()
	{
		this.mockClient.Invocations.Clear();
		this.writtenBytes.Clear();
		this.writtenFloats.Clear();
	}

	private void VerifySendRpcCount(int expected)
	{
		this.mockClient.Verify(
			c => c.StartRpcImmediately(
				It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()),
			Times.Exactly(expected));
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void IsUse_WhenAbilityCannotUse_ReturnsFalse(bool isCharging)
	{
		// Arrange
		FlasherRole role = CreateRole();
		this.mockLocalPlayer.SetupGet(p => p.CanMove).Returns(false);
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);

		// Act
		bool result = handler.IsUse(isCharging, 1.0f);

		// Assert
		Assert.False(result);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void IsUse_WhenAbilityCanUse_ReturnsTrueWithoutTargetCheck(bool isCharging)
	{
		// Arrange: 対象となる他陣営プレイヤーは誰もいない
		FlasherRole role = CreateRole();
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);

		// Act
		bool result = handler.IsUse(isCharging, 0.0f);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void TryFlash_WhenNoTargetInRange_ReturnsFalseAndSendsNothing()
	{
		// Arrange
		FlasherRole role = CreateRole();
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);
		ClearRpcRecords();

		// Act
		bool result = handler.TryFlash(1.0f);

		// Assert
		Assert.False(result);
		VerifySendRpcCount(0);
	}

	[Fact]
	public void TryFlash_WhenTargetOutOfRange_ReturnsFalseAndSendsNothing()
	{
		// Arrange
		FlasherRole role = CreateRole();
		CreateTarget(new Vector2(100.0f, 100.0f), false, false, false);
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);
		ClearRpcRecords();

		// Act
		bool result = handler.TryFlash(1.0f);

		// Assert
		Assert.False(result);
		VerifySendRpcCount(0);
	}

	[Fact]
	public void TryFlash_WhenTargetInVent_ReturnsFalseAndSendsNothing()
	{
		// Arrange
		FlasherRole role = CreateRole();
		CreateTarget(new Vector2(10.0f, 10.0f), false, true, false);
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);
		ClearRpcRecords();

		// Act
		bool result = handler.TryFlash(1.0f);

		// Assert
		Assert.False(result);
		VerifySendRpcCount(0);
	}

	[Fact]
	public void TryFlash_WhenTargetIsDead_ReturnsFalseAndSendsNothing()
	{
		// Arrange
		FlasherRole role = CreateRole();
		CreateTarget(new Vector2(10.0f, 10.0f), true, false, false);
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);
		ClearRpcRecords();

		// Act
		bool result = handler.TryFlash(1.0f);

		// Assert
		Assert.False(result);
		VerifySendRpcCount(0);
	}

	[Fact]
	public void TryFlash_WhenTargetIsSameTeam_ReturnsFalseAndSendsNothing()
	{
		// Arrange
		FlasherRole role = CreateRole();
		CreateTarget(new Vector2(10.0f, 10.0f), false, false, true);
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);
		ClearRpcRecords();

		// Act
		bool result = handler.TryFlash(1.0f);

		// Assert
		Assert.False(result);
		VerifySendRpcCount(0);
	}

	[Fact]
	public void TryFlash_WhenTargetInRange_SendsFlashStartWithColorAndTargets()
	{
		// Arrange
		FlasherRole role = CreateRole();
		CreateTarget(new Vector2(11.0f, 10.0f), false, false, false);
		var handler = CreateHandler(role, FlasherColor.FlasherColorBlack);
		ClearRpcRecords();

		// Act
		bool result = handler.TryFlash(1.0f);

		// Assert
		Assert.True(result);
		VerifySendRpcCount(1);
		Assert.Equal(
			new List<byte>()
			{
				(byte)FlasherRole.FlasherRpcOps.FlashStart,
				LocalPlayerId,
				(byte)FlasherColor.FlasherColorBlack,
				1,
				TargetPlayerId,
			},
			this.writtenBytes);
	}

	[Fact]
	public void ToAfterglow_WhenSequenceActive_SendsAfterglowTimeAndEndsSequence()
	{
		// Arrange
		FlasherRole role = CreateRole();
		CreateTarget(new Vector2(11.0f, 10.0f), false, false, false);
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);
		handler.TryFlash(1.0f);
		ClearRpcRecords();

		// Act
		handler.ToAfterglow();
		// 余韻移行送信中に会議開始しても終了送信はしない
		handler.ForceEnd();

		// Assert
		VerifySendRpcCount(1);
		Assert.Equal((byte)FlasherRole.FlasherRpcOps.FlashAfterglow, this.writtenBytes[0]);
		Assert.Equal(FlasherStatusModel.AfterglowMaxTime, this.writtenFloats[0], 3);
	}

	[Fact]
	public void ToAfterglow_WhenSequenceNotActive_SendsNothing()
	{
		// Arrange
		FlasherRole role = CreateRole();
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);
		ClearRpcRecords();

		// Act
		handler.ToAfterglow();

		// Assert
		VerifySendRpcCount(0);
	}

	[Fact]
	public void ForceEnd_WhenSequenceActive_SendsEndOnlyOnce()
	{
		// Arrange
		FlasherRole role = CreateRole();
		CreateTarget(new Vector2(11.0f, 10.0f), false, false, false);
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);
		handler.TryFlash(1.0f);
		ClearRpcRecords();

		// Act
		handler.ForceEnd();
		// 会議開始と強制終了が連続で呼ばれても送信は1回だけ
		handler.ForceEnd();

		// Assert
		VerifySendRpcCount(1);
		Assert.Equal(
			new List<byte>()
			{
				(byte)FlasherRole.FlasherRpcOps.FlashEnd,
				LocalPlayerId,
			},
			this.writtenBytes);
	}

	[Fact]
	public void ForceEnd_WhenSequenceNotActive_SendsNothing()
	{
		// Arrange
		FlasherRole role = CreateRole();
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);
		ClearRpcRecords();

		// Act
		handler.ForceEnd();

		// Assert
		VerifySendRpcCount(0);
	}

	[Fact]
	public void ApplyRpcOps_WhenFlashStartTargetsLocalPlayer_StartsHold()
	{
		// Arrange
		FlasherRole role = CreateRole();
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);
		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)FlasherColor.FlasherColorBlack)
			.Returns((byte)2)
			.Returns(TargetPlayerId)
			.Returns(LocalPlayerId);

		// Act
		handler.ApplyRpcOps(FlasherRole.FlasherRpcOps.FlashStart, mockReader.Object);

		// Assert
		this.mockScreenEffect.Verify(
			s => s.StartHold(ActiveTime + FlasherStatusModel.AfterglowMaxTime, Color.black),
			Times.Once);
	}

	[Fact]
	public void ApplyRpcOps_WhenFlashStartTargetsOtherPlayer_DoesNotStartHold()
	{
		// Arrange
		FlasherRole role = CreateRole();
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);
		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)FlasherColor.FlasherColorWhite)
			.Returns((byte)1)
			.Returns(TargetPlayerId);

		// Act
		handler.ApplyRpcOps(FlasherRole.FlasherRpcOps.FlashStart, mockReader.Object);

		// Assert
		this.mockScreenEffect.Verify(
			s => s.StartHold(It.IsAny<float>(), It.IsAny<Color>()), Times.Never);
	}

	[Fact]
	public void ApplyRpcOps_WhenFlashStartHasNoTarget_DoesNotStartHold()
	{
		// Arrange
		FlasherRole role = CreateRole();
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);
		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)FlasherColor.FlasherColorWhite)
			.Returns((byte)0);

		// Act
		handler.ApplyRpcOps(FlasherRole.FlasherRpcOps.FlashStart, mockReader.Object);

		// Assert
		this.mockScreenEffect.Verify(
			s => s.StartHold(It.IsAny<float>(), It.IsAny<Color>()), Times.Never);
	}

	[Fact]
	public void ApplyRpcOps_WhenLocalPlayerIsDead_DoesNotStartHold()
	{
		// Arrange
		FlasherRole role = CreateRole();
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);
		this.mockLocalPlayerInfo.SetupGet(d => d.IsDead).Returns(true);
		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)FlasherColor.FlasherColorWhite)
			.Returns((byte)1)
			.Returns(LocalPlayerId);

		// Act
		handler.ApplyRpcOps(FlasherRole.FlasherRpcOps.FlashStart, mockReader.Object);

		// Assert
		this.mockScreenEffect.Verify(
			s => s.StartHold(It.IsAny<float>(), It.IsAny<Color>()), Times.Never);
	}

	[Fact]
	public void ApplyRpcOps_WhenFlashAfterglow_StartsAfterglowWithSentTime()
	{
		// Arrange
		FlasherRole role = CreateRole();
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);
		var mockReader = new Mock<MessageReader>();
		mockReader.Setup(r => r.ReadSingle()).Returns(0.75f);

		// Act
		handler.ApplyRpcOps(FlasherRole.FlasherRpcOps.FlashAfterglow, mockReader.Object);

		// Assert
		this.mockScreenEffect.Verify(s => s.StartAfterglow(0.75f), Times.Once);
	}

	[Fact]
	public void ApplyRpcOps_WhenFlashEnd_StopsEffect()
	{
		// Arrange
		FlasherRole role = CreateRole();
		var handler = CreateHandler(role, FlasherColor.FlasherColorWhite);
		var mockReader = new Mock<MessageReader>();

		// Act
		handler.ApplyRpcOps(FlasherRole.FlasherRpcOps.FlashEnd, mockReader.Object);

		// Assert
		this.mockScreenEffect.Verify(s => s.Stop(), Times.Once);
	}

	public void Dispose()
	{
		this.magHook.Dispose();
		this.ctorHook.Dispose();
	}
}
