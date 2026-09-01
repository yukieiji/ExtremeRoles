using System;
using AmongUs.GameOptions;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.SecurityDummySystem;
using ExtremeRoles.Performance;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.SecurityDummySystem;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class SecurityDummySystemManagerTests : IDisposable
{
	public SecurityDummySystemManagerTests()
	{
		ResetState();
	}

	public void Dispose()
	{
		ResetState();
	}

	private static void ResetState()
	{
		PlayerCache.RemovePlayerControl(_ => true);
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupLogger();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		var mockGameOptionsManager = new Mock<GameOptionsManager>(IntPtr.Zero);
		var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
		mockGameOptionsManager.SetupGet(g => g.CurrentGameOptions).Returns(mockGameOptions.Object);

		var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
		mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockGameOptionsManager.Object);
		MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;
	}

	[Fact]
	public void Get_ReturnsInstanceFromExtremeSystemTypeManager()
	{
		// Act
		var system = SecurityDummySystemManager.Get();

		// Assert
		Assert.NotNull(system);
		Assert.Same(system, SecurityDummySystemManager.Get());
	}

	[Fact]
	public void TryGet_WhenCreated_ReturnsTrueAndInstance()
	{
		// Arrange
		var created = SecurityDummySystemManager.Get();

		// Act
		bool result = SecurityDummySystemManager.TryGet(out var system);

		// Assert
		Assert.True(result);
		Assert.Same(created, system);
	}

	[Fact]
	public void Properties_DefaultValuesAndCanBeSet()
	{
		// Arrange
		var system = SecurityDummySystemManager.Get();

		// Assert defaults
		Assert.False(system.IsActive);
		Assert.Equal(SecurityDummySystemManager.DummyMode.Normal, system.Mode);

		// Act
		system.IsActive = true;
		system.Mode = SecurityDummySystemManager.DummyMode.No;

		// Assert modified values
		Assert.True(system.IsActive);
		Assert.Equal(SecurityDummySystemManager.DummyMode.No, system.Mode);
	}

	[Fact]
	public void PostfixBegin_PostfixClose_Add_Remove_DoNotThrow()
	{
		// Arrange
		var system = SecurityDummySystemManager.Get();

		// Act & Assert
		system.Add(1, 2);
		system.Remove(1);
		system.PostfixBegin();
		system.PostfixClose();
	}

	[Fact]
	public void Reset_OnPlayer_CallsClearWithoutThrowing()
	{
		// Arrange
		var system = SecurityDummySystemManager.Get();

		// Act & Assert
		system.Reset(ResetTiming.OnPlayer);
		system.Reset(ResetTiming.MeetingStart);
	}

	[Fact]
	public void UpdateSystem_OptionAdd_CallsAdd()
	{
		// Arrange
		var system = SecurityDummySystemManager.Get();
		var mockPlayer = new Mock<PlayerControl>();
		var mockReader = new Mock<MessageReader>(IntPtr.Zero);

		// ReadByte called twice: option then playerId
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)SecurityDummySystemManager.Option.Add)
			.Returns((byte)5);

		// Act
		system.UpdateSystem(mockPlayer.Object, mockReader.Object);

		// Assert
		mockReader.Verify(r => r.ReadByte(), Times.Exactly(2));
	}

	[Fact]
	public void UpdateSystem_OptionRemove_CallsRemove()
	{
		// Arrange
		var system = SecurityDummySystemManager.Get();
		var mockPlayer = new Mock<PlayerControl>();
		var mockReader = new Mock<MessageReader>(IntPtr.Zero);

		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)SecurityDummySystemManager.Option.Remove)
			.Returns((byte)5);

		// Act
		system.UpdateSystem(mockPlayer.Object, mockReader.Object);

		// Assert
		mockReader.Verify(r => r.ReadByte(), Times.Exactly(2));
	}

	[Fact]
	public void UpdateSystem_InvalidOption_HandlesGracefully()
	{
		// Arrange
		var system = SecurityDummySystemManager.Get();
		var mockPlayer = new Mock<PlayerControl>();
		var mockReader = new Mock<MessageReader>(IntPtr.Zero);

		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)99) // Invalid option
			.Returns((byte)5);

		// Act
		system.UpdateSystem(mockPlayer.Object, mockReader.Object);

		// Assert
		mockReader.Verify(r => r.ReadByte(), Times.Exactly(2));
	}
}
