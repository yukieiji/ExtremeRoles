using System;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using Moq;
using Xunit;

using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class BlackmailerSystemTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;

	public BlackmailerSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupUnityCommonMocks();

		this.mockClient = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		this.mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		this.mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		this.mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		if (Hazel.MockMessageWriterGetHelper.Instance == null)
		{
			var mockGet = new Mock<Hazel.MockMessageWriterGetHelper>();
			mockGet.Setup(g => g.Invoke(It.IsAny<SendOption>())).Returns(mockWriter.Object);
			Hazel.MockMessageWriterGetHelper.Instance = mockGet.Object;
		}

		if (Hazel.MockMessageReaderGetHelper.Instance == null)
		{
			var mockReaderHelper = new Mock<Hazel.MockMessageReaderGetHelper>();
			mockReaderHelper.Setup(g => g.Invoke(It.IsAny<Il2CppStructArray<byte>>()))
				.Returns((Il2CppStructArray<byte> buffer) =>
				{
					var mockReader = new Mock<MessageReader>();
					if (buffer != null && buffer.Length >= 1)
					{
						var seq = mockReader.SetupSequence(r => r.ReadByte());
						for (int i = 0; i < buffer.Length; i++)
						{
							seq.Returns(buffer[i]);
						}
					}
					return mockReader.Object;
				});
			Hazel.MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;
		}

		if (InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance == null)
		{
			var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
			mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<MessageWriter>(), It.IsAny<InnerNetObject>()));
			InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;
		}
	}

	[Fact]
	public void GetOrRegister_And_TryGet_ReturnsSystemInstance()
	{
		// Act
		var system = BlackmailerSystem.GetOrRegister();

		// Assert
		Assert.NotNull(system);

		// Act
		bool success = BlackmailerSystem.TryGet(out var retrievedSystem);

		// Assert
		Assert.True(success);
		Assert.Same(system, retrievedSystem);
	}

	[Fact]
	public void AddBlackmail_And_IsBlackmailed_IsBlackmailedBy_WorksCorrectly()
	{
		// Arrange
		var system = new BlackmailerSystem();
		byte blackmailerId = 1;
		byte targetId = 2;

		var mockTargetPlayer = new Mock<PlayerControl>();
		mockTargetPlayer.SetupGet(p => p.PlayerId).Returns(targetId);

		var mockOtherPlayer = new Mock<PlayerControl>();
		mockOtherPlayer.SetupGet(p => p.PlayerId).Returns((byte)3);

		var readerMock = new Mock<MessageReader>();
		readerMock.SetupSequence(r => r.ReadByte())
			.Returns((byte)BlackmailerSystem.Ops.AddBlackmail)
			.Returns(blackmailerId)
			.Returns(targetId);

		// Act
		system.UpdateSystem(null!, readerMock.Object);

		// Assert: Byte checks
		bool isTargetBlackmailed = system.IsBlackmailed(targetId);
		bool isOtherBlackmailed = system.IsBlackmailed(3);
		bool isBlackmailedBy1 = system.IsBlackmailedBy(1, targetId);
		bool isBlackmailedByOtherBM = system.IsBlackmailedBy(9, targetId);

		Assert.True(isTargetBlackmailed);
		Assert.False(isOtherBlackmailed);
		Assert.True(isBlackmailedBy1);
		Assert.False(isBlackmailedByOtherBM);

		// Assert: PlayerControl checks
		bool isTargetPlayerBlackmailed = system.IsBlackmailed(mockTargetPlayer.Object);
		bool isOtherPlayerBlackmailed = system.IsBlackmailed(mockOtherPlayer.Object);

		Assert.True(isTargetPlayerBlackmailed);
		Assert.False(isOtherPlayerBlackmailed);
	}

	[Fact]
	public void ClearBlackmail_RemovesBlackmailForSpecificBlackmailer()
	{
		// Arrange
		var system = new BlackmailerSystem();

		var addReader1 = new Mock<MessageReader>();
		addReader1.SetupSequence(r => r.ReadByte())
			.Returns((byte)BlackmailerSystem.Ops.AddBlackmail)
			.Returns((byte)1)
			.Returns((byte)10);
		system.UpdateSystem(null!, addReader1.Object);

		var addReader2 = new Mock<MessageReader>();
		addReader2.SetupSequence(r => r.ReadByte())
			.Returns((byte)BlackmailerSystem.Ops.AddBlackmail)
			.Returns((byte)2)
			.Returns((byte)20);
		system.UpdateSystem(null!, addReader2.Object);

		var clearReader = new Mock<MessageReader>();
		clearReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)BlackmailerSystem.Ops.ClearBlackmail)
			.Returns((byte)1);

		// Act
		system.UpdateSystem(null!, clearReader.Object);

		// Assert
		bool is10Blackmailed = system.IsBlackmailed(10);
		bool is20Blackmailed = system.IsBlackmailed(20);

		Assert.False(is10Blackmailed);
		Assert.True(is20Blackmailed);
	}

	[Fact]
	public void Reset_WhenMeetingEndOrExiledEnd_ClearsMapAndSetsDirtyFlag()
	{
		// Arrange
		var system = new BlackmailerSystem();
		var addReader = new Mock<MessageReader>();
		addReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)BlackmailerSystem.Ops.AddBlackmail)
			.Returns((byte)1)
			.Returns((byte)10);
		system.UpdateSystem(null!, addReader.Object);

		// Act: MeetingStart (should not reset)
		system.Reset(ResetTiming.MeetingStart, null);

		// Assert
		bool is10BlackmailedBeforeMeetingEnd = system.IsBlackmailed(10);
		Assert.True(is10BlackmailedBeforeMeetingEnd);

		// Act: MeetingEnd
		system.Reset(ResetTiming.MeetingEnd, null);

		// Assert
		bool is10BlackmailedAfterMeetingEnd = system.IsBlackmailed(10);
		Assert.False(is10BlackmailedAfterMeetingEnd);
		Assert.True(system.IsDirty);

		// Re-add blackmail
		var addReader2 = new Mock<MessageReader>();
		addReader2.SetupSequence(r => r.ReadByte())
			.Returns((byte)BlackmailerSystem.Ops.AddBlackmail)
			.Returns((byte)1)
			.Returns((byte)10);
		system.UpdateSystem(null!, addReader2.Object);

		// Act: ExiledEnd
		system.Reset(ResetTiming.ExiledEnd, null);

		// Assert
		bool is10BlackmailedAfterExiledEnd = system.IsBlackmailed(10);
		Assert.False(is10BlackmailedAfterExiledEnd);
	}

	[Fact]
	public void RpcAddBlackmail_And_RpcClearBlackmail_ExecutesWithoutError()
	{
		// Arrange
		var system = new BlackmailerSystem();

		// Act & Assert
		system.RpcAddBlackmail(1, 2);
		system.RpcClearBlackmail(1);
	}
}
