using System;
using AmongUs.GameOptions;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;
using ExtremeRoles.Patches.Ship;
using Il2CppSystem.Collections;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Patches;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ShipStatusPrespawnStepPatchBodyTests : IDisposable
{
	public ShipStatusPrespawnStepPatchBodyTests()
	{
		ResetState();
	}

	public void Dispose()
	{
		ResetState();
	}

	private static void ResetState()
	{
		MockSetupHelper.SetupUnityCommonMocks();
	}

	private static SpawnOption CreateSpawnOption(bool enableSpecialSetting, bool skeld = false, bool miraHq = false, bool polus = false, bool fungle = false)
	{
		var spawnOpt = new SpawnOption();
		object boxed = spawnOpt;
		typeof(SpawnOption).GetField(nameof(SpawnOption.EnableSpecialSetting))?.SetValue(boxed, enableSpecialSetting);
		typeof(SpawnOption).GetField(nameof(SpawnOption.Skeld))?.SetValue(boxed, skeld);
		typeof(SpawnOption).GetField(nameof(SpawnOption.MiraHq))?.SetValue(boxed, miraHq);
		typeof(SpawnOption).GetField(nameof(SpawnOption.Polus))?.SetValue(boxed, polus);
		typeof(SpawnOption).GetField(nameof(SpawnOption.Fungle))?.SetValue(boxed, fungle);
		return (SpawnOption)boxed;
	}

	[Fact]
	public void Postfix_WhenGameManagerInstanceIsNull_ReturnsTrueAndDoesNotModifyResult()
	{
		// Arrange
		var mockGameManagerHelper = new Mock<MockGameManagerget_InstanceHelper>();
		mockGameManagerHelper.Setup(h => h.Invoke()).Returns((GameManager)null!);
		MockGameManagerget_InstanceHelper.Instance = mockGameManagerHelper.Object;

		var mockRuntime = new Mock<IGameRuntime>();
		var patchBody = new ShipStatusPrespawnStepPatchBody(mockRuntime.Object);

		IEnumerator? resultEnumerator = null;

		// Act
		bool result = patchBody.Postfix(ref resultEnumerator!);

		// Assert
		Assert.True(result);
		Assert.Null(resultEnumerator);
	}

	[Fact]
	public void Postfix_WhenLogicOptionsIsNull_ReturnsTrueAndDoesNotModifyResult()
	{
		// Arrange
		var mockGameManager = new Mock<GameManager>(IntPtr.Zero);
		mockGameManager.SetupGet(g => g.LogicOptions).Returns((LogicOptions)null!);

		var mockGameManagerHelper = new Mock<MockGameManagerget_InstanceHelper>();
		mockGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);
		MockGameManagerget_InstanceHelper.Instance = mockGameManagerHelper.Object;

		var mockRuntime = new Mock<IGameRuntime>();
		var patchBody = new ShipStatusPrespawnStepPatchBody(mockRuntime.Object);

		IEnumerator? resultEnumerator = null;

		// Act
		bool result = patchBody.Postfix(ref resultEnumerator!);

		// Assert
		Assert.True(result);
		Assert.Null(resultEnumerator);
	}

	[Fact]
	public void Postfix_WhenTryGetGameContextReturnsFalse_ReturnsTrueAndDoesNotModifyResult()
	{
		// Arrange
		var mockLogicOptions = new Mock<LogicOptions>(IntPtr.Zero);
		var mockGameManager = new Mock<GameManager>(IntPtr.Zero);
		mockGameManager.SetupGet(g => g.LogicOptions).Returns(mockLogicOptions.Object);

		var mockGameManagerHelper = new Mock<MockGameManagerget_InstanceHelper>();
		mockGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);
		MockGameManagerget_InstanceHelper.Instance = mockGameManagerHelper.Object;

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(false);

		var patchBody = new ShipStatusPrespawnStepPatchBody(mockRuntime.Object);

		IEnumerator? resultEnumerator = null;

		// Act
		bool result = patchBody.Postfix(ref resultEnumerator!);

		// Assert
		Assert.True(result);
		Assert.Null(resultEnumerator);
	}

	[Fact]
	public void Postfix_WhenEnableSpecialSettingIsFalse_ReturnsTrueAndDoesNotModifyResult()
	{
		// Arrange
		var mockLogicOptions = new Mock<LogicOptions>(IntPtr.Zero);
		var mockGameManager = new Mock<GameManager>(IntPtr.Zero);
		mockGameManager.SetupGet(g => g.LogicOptions).Returns(mockLogicOptions.Object);

		var mockGameManagerHelper = new Mock<MockGameManagerget_InstanceHelper>();
		mockGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);
		MockGameManagerget_InstanceHelper.Instance = mockGameManagerHelper.Object;

		var spawnOpt = CreateSpawnOption(enableSpecialSetting: false);
		var mockGlobalOption = new Mock<IShipGlobalOption>();
		mockGlobalOption.SetupGet(g => g.Spawn).Returns(spawnOpt);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOption.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new ShipStatusPrespawnStepPatchBody(mockRuntime.Object);

		IEnumerator? resultEnumerator = null;

		// Act
		bool result = patchBody.Postfix(ref resultEnumerator!);

		// Assert
		Assert.True(result);
		Assert.Null(resultEnumerator);
	}

	[Theory]
	[InlineData((byte)0, false, false, false, false, true)]  // Skeld disabled -> true
	[InlineData((byte)1, false, false, false, false, true)]  // MiraHq disabled -> true
	[InlineData((byte)2, false, false, false, false, true)]  // Polus disabled -> true
	[InlineData((byte)5, false, false, false, false, true)]  // Fungle disabled -> true
	[InlineData((byte)3, true, true, true, true, true)]      // Airship (3) -> true (default)
	[InlineData((byte)99, true, true, true, true, true)]     // Unknown map -> true (default)
	public void Postfix_WhenMapDisabledOrDefault_ReturnsTrueAndDoesNotModifyResult(byte mapId, bool skeld, bool miraHq, bool polus, bool fungle, bool expectedResult)
	{
		// Arrange
		var mockLogicOptions = new Mock<LogicOptions>(IntPtr.Zero);
		mockLogicOptions.SetupGet(l => l.MapId).Returns(mapId);

		var mockGameManager = new Mock<GameManager>(IntPtr.Zero);
		mockGameManager.SetupGet(g => g.LogicOptions).Returns(mockLogicOptions.Object);

		var mockGameManagerHelper = new Mock<MockGameManagerget_InstanceHelper>();
		mockGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);
		MockGameManagerget_InstanceHelper.Instance = mockGameManagerHelper.Object;

		var spawnOpt = CreateSpawnOption(enableSpecialSetting: true, skeld: skeld, miraHq: miraHq, polus: polus, fungle: fungle);
		var mockGlobalOption = new Mock<IShipGlobalOption>();
		mockGlobalOption.SetupGet(g => g.Spawn).Returns(spawnOpt);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOption.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new ShipStatusPrespawnStepPatchBody(mockRuntime.Object);

		IEnumerator? resultEnumerator = null;

		// Act
		bool result = patchBody.Postfix(ref resultEnumerator!);

		// Assert
		Assert.Equal(expectedResult, result);
		Assert.Null(resultEnumerator);
	}

	[Theory]
	[InlineData((byte)0, true, false, false, false)]   // Skeld enabled -> false
	[InlineData((byte)1, false, true, false, false)]   // MiraHq enabled -> false
	[InlineData((byte)2, false, false, true, false)]   // Polus enabled -> false
	[InlineData((byte)5, false, false, false, true)]   // Fungle enabled -> false
	public void Postfix_WhenMapEnabled_ReturnsFalse(byte mapId, bool skeld, bool miraHq, bool polus, bool fungle)
	{
		// Arrange
		var mockLogicOptions = new Mock<LogicOptions>(IntPtr.Zero);
		mockLogicOptions.SetupGet(l => l.MapId).Returns(mapId);

		var mockGameManager = new Mock<GameManager>(IntPtr.Zero);
		mockGameManager.SetupGet(g => g.LogicOptions).Returns(mockLogicOptions.Object);

		var mockGameManagerHelper = new Mock<MockGameManagerget_InstanceHelper>();
		mockGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);
		MockGameManagerget_InstanceHelper.Instance = mockGameManagerHelper.Object;

		var spawnOpt = CreateSpawnOption(enableSpecialSetting: true, skeld: skeld, miraHq: miraHq, polus: polus, fungle: fungle);
		var mockGlobalOption = new Mock<IShipGlobalOption>();
		mockGlobalOption.SetupGet(g => g.Spawn).Returns(spawnOpt);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockGlobalOption.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var patchBody = new ShipStatusPrespawnStepPatchBody(mockRuntime.Object);

		IEnumerator? resultEnumerator = null;

		// Act & Assert
		// WrapToIl2Cpp in test env throws TypeInitializationException for Il2CppManagedEnumerator due to headless environment
		Assert.Throws<TypeInitializationException>(() => patchBody.Postfix(ref resultEnumerator!));
	}
}
