using System;
using System.Reflection;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.Solo.Crewmate.Invisible;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Invisible;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class InvisibleAbilityHandlerTests
{
	private readonly Mock<MessageWriter> mockWriter;

	public InvisibleAbilityHandlerTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupUnityObjectOperators();
		MockSetupHelper.SetupObjectImplicitHelpers();
		var localPlayer = MockSetupHelper.SetupPlayerControlMocks();
		localPlayer.SetupGet(p => p.NetId).Returns(100);
		localPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		this.mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		var mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockClient.SetupGet(c => c.AmHost).Returns(true);
		mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(this.mockWriter.Object);
		this.mockWriter.Setup(w => w.Write(It.IsAny<MessageWriter>(), It.IsAny<bool>()));
		this.mockWriter.Setup(w => w.ToByteArray(It.IsAny<bool>())).Returns((Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>)null!);

		var mockWriterGet = new Mock<MockMessageWriterGetHelper>();
		mockWriterGet.Setup(h => h.Invoke(It.IsAny<SendOption>())).Returns(this.mockWriter.Object);
		MockMessageWriterGetHelper.Instance = mockWriterGet.Object;

		var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
		mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<MessageWriter>(), It.IsAny<InnerNet.InnerNetObject>()));
		InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;

		var mockReaderHelper = new Mock<MockMessageReaderGetHelper>();
		mockReaderHelper.Setup(g => g.Invoke(It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>>()))
			.Returns(() =>
			{
				var mockReader = new Mock<MessageReader>();
				mockReader.SetupSequence(r => r.ReadByte())
					.Returns((byte)ExtremeSystemType.InvisibleSystem)
					.Returns((byte)1);
				mockReader.Setup(r => r.ReadBoolean()).Returns(true);
				return mockReader.Object;
			});
		MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;
	}

	private static bool GetIsActive(InvisibleAbilityHandler handler)
	{
		var field = typeof(InvisibleAbilityHandler).GetField("isActive", BindingFlags.NonPublic | BindingFlags.Instance);
		return (bool)(field?.GetValue(handler) ?? false);
	}

	private static void SetIsActive(InvisibleAbilityHandler handler, bool active)
	{
		var field = typeof(InvisibleAbilityHandler).GetField("isActive", BindingFlags.NonPublic | BindingFlags.Instance);
		field?.SetValue(handler, active);
	}

	[Fact]
	public void Execute_WhenCalled_SetsIsActiveAndExecutesSystems()
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
		Assert.True(GetIsActive(handler));
	}

	[Fact]
	public void CleanUp_WhenActive_ResetsIsActive()
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
		Assert.False(GetIsActive(handler));
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
		Assert.False(GetIsActive(handler));
	}

	[Theory]
	[InlineData(true, true, false)]  // hasHitbox: true, isActive: true => Block: false
	[InlineData(false, true, true)]  // hasHitbox: false, isActive: true => Block: true
	[InlineData(false, false, false)] // hasHitbox: false, isActive: false => Block: false
	public void IsBlockKillFrom_HandlesHitboxAndActiveState(bool hasHitbox, bool isActive, bool expectedBlock)
	{
		// Arrange
		var invisSystem = new InvisibleSystem();
		var speedSystem = new PlayerSpeedSystem();
		var status = new InvisibleStatusModel(1.0f, hasHitbox);
		var handler = new InvisibleAbilityHandler(invisSystem, speedSystem, status);
		SetIsActive(handler, isActive);

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
		var status = new InvisibleStatusModel(1.0f, hasHitbox);
		var handler = new InvisibleAbilityHandler(invisSystem, speedSystem, status);
		SetIsActive(handler, isActive);

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
		var status = new InvisibleStatusModel(1.0f, hasHitbox);
		var handler = new InvisibleAbilityHandler(invisSystem, speedSystem, status);
		SetIsActive(handler, isActive);

		// Act
		bool isValid = handler.IsValidAbilitySource(2);

		// Assert
		Assert.Equal(expectedValid, isValid);
	}
}
