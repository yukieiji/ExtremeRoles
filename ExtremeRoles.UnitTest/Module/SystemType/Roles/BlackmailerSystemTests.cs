using System;
using System.Collections.Generic;
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
	private readonly Mock<MessageWriter> mockWriter;

	public BlackmailerSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupUnityCommonMocks();

		this.mockClient = MockSetupHelper.SetupAmongUsClientMock();

		var writtenBytes = new List<byte>();
		this.mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		this.mockWriter.Setup(w => w.Write(It.IsAny<byte>())).Callback((byte b) => writtenBytes.Add(b));
		this.mockWriter.Setup(w => w.ToByteArray(It.IsAny<bool>())).Returns(new Mock<Il2CppStructArray<byte>>(IntPtr.Zero).Object);

		this.mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(this.mockWriter.Object);

		this.mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		this.mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		if (Hazel.MockMessageWriterGetHelper.Instance == null)
		{
			var mockGet = new Mock<Hazel.MockMessageWriterGetHelper>();
			mockGet.Setup(g => g.Invoke(It.IsAny<SendOption>())).Returns(this.mockWriter.Object);
			Hazel.MockMessageWriterGetHelper.Instance = mockGet.Object;
		}

		if (Hazel.MockMessageReaderGetHelper.Instance == null)
		{
			var mockReaderHelper = new Mock<Hazel.MockMessageReaderGetHelper>();
			mockReaderHelper.Setup(g => g.Invoke(It.IsAny<Il2CppStructArray<byte>>()))
				.Returns(() =>
				{
					var mockReader = new Mock<MessageReader>();
					if (writtenBytes.Count > 0)
					{
						var bytesCopy = new List<byte>(writtenBytes);
						writtenBytes.Clear();
						var seq = mockReader.SetupSequence(r => r.ReadByte());
						foreach (var b in bytesCopy)
						{
							seq.Returns(b);
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
	public void GetOrRegister_ReturnsSystemInstance()
	{
		// Act
		var system = BlackmailerSystem.GetOrRegister();

		// Assert
		Assert.NotNull(system);
	}

	[Fact]
	public void TryGet_ReturnsTrueAndInstance()
	{
		// Arrange
		var expectedSystem = BlackmailerSystem.GetOrRegister();

		// Act
		bool success = BlackmailerSystem.TryGet(out var retrievedSystem);

		// Assert
		Assert.True(success);
		Assert.Same(expectedSystem, retrievedSystem);
	}

	[Fact]
	public void AddBlackmail_UpdatesSystemState()
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

		// Assert
		Assert.True(system.IsBlackmailed(targetId));
		Assert.False(system.IsBlackmailed(3));
		Assert.True(system.IsBlackmailedBy(1, targetId));
		Assert.False(system.IsBlackmailedBy(9, targetId));
		Assert.True(system.IsBlackmailed(mockTargetPlayer.Object));
		Assert.False(system.IsBlackmailed(mockOtherPlayer.Object));
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
		Assert.False(system.IsBlackmailed(10));
		Assert.True(system.IsBlackmailed(20));
	}

	[Fact]
	public void Reset_WhenMeetingStart_DoesNotClearMap()
	{
		// Arrange
		var system = new BlackmailerSystem();
		var addReader = new Mock<MessageReader>();
		addReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)BlackmailerSystem.Ops.AddBlackmail)
			.Returns((byte)1)
			.Returns((byte)10);
		system.UpdateSystem(null!, addReader.Object);

		// Act
		system.Reset(ResetTiming.MeetingStart, null);

		// Assert
		Assert.True(system.IsBlackmailed(10));
	}

	[Fact]
	public void Reset_WhenMeetingEnd_ClearsMapAndSetsDirtyFlag()
	{
		// Arrange
		var system = new BlackmailerSystem();
		var addReader = new Mock<MessageReader>();
		addReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)BlackmailerSystem.Ops.AddBlackmail)
			.Returns((byte)1)
			.Returns((byte)10);
		system.UpdateSystem(null!, addReader.Object);

		// Act
		system.Reset(ResetTiming.MeetingEnd, null);

		// Assert
		Assert.False(system.IsBlackmailed(10));
		Assert.True(system.IsDirty);
	}

	[Fact]
	public void Reset_WhenExiledEnd_ClearsMap()
	{
		// Arrange
		var system = new BlackmailerSystem();
		var addReader = new Mock<MessageReader>();
		addReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)BlackmailerSystem.Ops.AddBlackmail)
			.Returns((byte)1)
			.Returns((byte)10);
		system.UpdateSystem(null!, addReader.Object);

		// Act
		system.Reset(ResetTiming.ExiledEnd, null);

		// Assert
		Assert.False(system.IsBlackmailed(10));
	}

	[Fact]
	public void RpcAddBlackmail_AddsBlackmailToSystemState()
	{
		// Arrange
		var system = BlackmailerSystem.GetOrRegister();
		system.Reset(ResetTiming.MeetingEnd);
		byte blackmailerId = 1;
		byte targetId = 2;

		// Act
		system.RpcAddBlackmail(blackmailerId, targetId);

		// Assert
		Assert.True(system.IsBlackmailedBy(blackmailerId, targetId));
	}

	[Fact]
	public void RpcClearBlackmail_ClearsBlackmailFromSystemState()
	{
		// Arrange
		var system = BlackmailerSystem.GetOrRegister();
		system.Reset(ResetTiming.MeetingEnd);
		system.RpcAddBlackmail(1, 2);

		// Act
		system.RpcClearBlackmail(1);

		// Assert
		Assert.False(system.IsBlackmailedBy(1, 2));
	}
}
