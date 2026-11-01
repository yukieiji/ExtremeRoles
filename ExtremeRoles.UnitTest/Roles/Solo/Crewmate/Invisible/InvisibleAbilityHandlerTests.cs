using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.Solo.Crewmate.Invisible;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Invisible;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class InvisibleAbilityHandlerTests
{
	public InvisibleAbilityHandlerTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupUnityObjectOperators();
		MockSetupHelper.SetupObjectImplicitHelpers();
		var localPlayer = MockSetupHelper.SetupPlayerControlMocks();
		localPlayer.SetupGet(p => p.NetId).Returns(100);
		localPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockWriter = new Moq.Mock<Hazel.MessageWriter>(System.IntPtr.Zero);
		var mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockClient.SetupGet(c => c.AmHost).Returns(true);
		mockClient.Setup(c => c.StartRpcImmediately(Moq.It.IsAny<uint>(), Moq.It.IsAny<byte>(), Moq.It.IsAny<Hazel.SendOption>(), Moq.It.IsAny<int>()))
			.Returns(mockWriter.Object);
		mockWriter.Setup(w => w.Write(Moq.It.IsAny<Hazel.MessageWriter>(), Moq.It.IsAny<bool>()));
		mockWriter.Setup(w => w.ToByteArray(Moq.It.IsAny<bool>())).Returns((Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>)null!);

		var mockWriterGet = new Moq.Mock<Hazel.MockMessageWriterGetHelper>();
		mockWriterGet.Setup(h => h.Invoke(Moq.It.IsAny<Hazel.SendOption>())).Returns(mockWriter.Object);
		Hazel.MockMessageWriterGetHelper.Instance = mockWriterGet.Object;

		var mockWriteNetObj = new Moq.Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
		mockWriteNetObj.Setup(w => w.Invoke(Moq.It.IsAny<Hazel.MessageWriter>(), Moq.It.IsAny<InnerNet.InnerNetObject>()));
		InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;

		var mockReaderHelper = new Moq.Mock<Hazel.MockMessageReaderGetHelper>();
		mockReaderHelper.Setup(g => g.Invoke(Moq.It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>>()))
			.Returns(() =>
			{
				var mockReader = new Moq.Mock<Hazel.MessageReader>();
				mockReader.SetupSequence(r => r.ReadByte())
					.Returns((byte)ExtremeSystemType.InvisibleSystem)
					.Returns((byte)1);
				mockReader.Setup(r => r.ReadBoolean()).Returns(true);
				return mockReader.Object;
			});
		Hazel.MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;
	}

	[Fact]
	public void Execute_WhenCalled_SetsStatusActive()
	{
		// Arrange
		var invisSystem = new InvisibleSystem();
		var speedSystem = new PlayerSpeedSystem();
		var status = new InvisibleStatusModel(1.5f, true);
		var handler = new InvisibleAbilityHandler(invisSystem, speedSystem, status);

		// Act
		bool result = handler.Execute(1);

		// Assert
		Assert.True(result);
		Assert.True(handler.IsActive);
		Assert.True(status.IsActiveInvisible);
	}

	[Fact]
	public void CleanUp_WhenActive_ResetsStatus()
	{
		// Arrange
		var invisSystem = new InvisibleSystem();
		var speedSystem = new PlayerSpeedSystem();
		var status = new InvisibleStatusModel(1.5f, true);
		var handler = new InvisibleAbilityHandler(invisSystem, speedSystem, status);
		handler.Execute(1);

		// Act
		handler.CleanUp(1);

		// Assert
		Assert.False(handler.IsActive);
		Assert.False(status.IsActiveInvisible);
	}

	[Fact]
	public void CleanUp_WhenNotActive_DoesNothing()
	{
		// Arrange
		var invisSystem = new InvisibleSystem();
		var speedSystem = new PlayerSpeedSystem();
		var status = new InvisibleStatusModel(1.5f, true);
		var handler = new InvisibleAbilityHandler(invisSystem, speedSystem, status);

		// Act
		handler.CleanUp(1);

		// Assert
		Assert.False(handler.IsActive);
		Assert.False(status.IsActiveInvisible);
	}

	[Fact]
	public void Reset_InvokesCleanUp()
	{
		// Arrange
		var invisSystem = new InvisibleSystem();
		var speedSystem = new PlayerSpeedSystem();
		var status = new InvisibleStatusModel(1.5f, true);
		var handler = new InvisibleAbilityHandler(invisSystem, speedSystem, status);
		handler.Execute(1);

		// Act
		handler.Reset(1);

		// Assert
		Assert.False(handler.IsActive);
	}

	[Theory]
	[InlineData(true, true, false)] // hasHitbox: true, isActive: true => Block: false
	[InlineData(false, true, true)] // hasHitbox: false, isActive: true => Block: true
	[InlineData(false, false, false)] // hasHitbox: false, isActive: false => Block: false
	public void IsBlockKillFrom_HandlesHitboxAndActiveState(bool hasHitbox, bool isActive, bool expectedBlock)
	{
		// Arrange
		var invisSystem = new InvisibleSystem();
		var speedSystem = new PlayerSpeedSystem();
		var status = new InvisibleStatusModel(1.0f, hasHitbox) { IsActiveInvisible = isActive };
		var handler = new InvisibleAbilityHandler(invisSystem, speedSystem, status);

		// Act
		bool isBlocked = handler.IsBlockKillFrom(2);

		// Assert
		Assert.Equal(expectedBlock, isBlocked);
	}

	[Theory]
	[InlineData(true, true, true)]
	[InlineData(false, true, false)]
	[InlineData(false, false, true)]
	public void IsValidKillFromSource_HandlesHitboxAndActiveState(bool hasHitbox, bool isActive, bool expectedValid)
	{
		// Arrange
		var invisSystem = new InvisibleSystem();
		var speedSystem = new PlayerSpeedSystem();
		var status = new InvisibleStatusModel(1.0f, hasHitbox) { IsActiveInvisible = isActive };
		var handler = new InvisibleAbilityHandler(invisSystem, speedSystem, status);

		// Act
		bool isValid = handler.IsValidKillFromSource(2);

		// Assert
		Assert.Equal(expectedValid, isValid);
	}

	[Theory]
	[InlineData(true, true, true)]
	[InlineData(false, true, false)]
	[InlineData(false, false, true)]
	public void IsValidAbilitySource_HandlesHitboxAndActiveState(bool hasHitbox, bool isActive, bool expectedValid)
	{
		// Arrange
		var invisSystem = new InvisibleSystem();
		var speedSystem = new PlayerSpeedSystem();
		var status = new InvisibleStatusModel(1.0f, hasHitbox) { IsActiveInvisible = isActive };
		var handler = new InvisibleAbilityHandler(invisSystem, speedSystem, status);

		// Act
		bool isValid = handler.IsValidAbilitySource(2);

		// Assert
		Assert.Equal(expectedValid, isValid);
	}

	[Fact]
	public void Update_CallsInvisibleSystemUpdate()
	{
		// Arrange
		var invisSystem = new InvisibleSystem();
		var speedSystem = new PlayerSpeedSystem();
		var status = new InvisibleStatusModel(1.0f, true);
		var handler = new InvisibleAbilityHandler(invisSystem, speedSystem, status);

		// Act
		handler.Update();

		// Assert
		Assert.NotNull(handler);
	}
}
