using System;
using System.Collections;
using System.Reflection;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class InvisibleSystemTests
{
	public InvisibleSystemTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	private static int GetSavedAlphasCount(InvisibleSystem system)
	{
		var field = typeof(InvisibleSystem).GetField("savedAlphas", BindingFlags.NonPublic | BindingFlags.Instance);
		if (field?.GetValue(system) is IDictionary dict)
		{
			return dict.Count;
		}
		return 0;
	}

	[Fact]
	public void Deteriorate_MarkClean_Serialize_Deserialize_DoNotThrow()
	{
		// Arrange
		var system = new InvisibleSystem();

		// Act & Assert
		Assert.False(system.IsDirty);
		system.MarkClean();
		system.Deteriorate(1.0f);
		system.Serialize(null!, false);
		system.Deserialize(null!, false);
	}

	[Fact]
	public void UpdateSystem_WhenPlayerNotFound_DoesNotAddSavedAlphas()
	{
		// Arrange
		var system = new InvisibleSystem();
		var p1 = new Mock<PlayerControl>(IntPtr.Zero);
		p1.SetupGet(p => p.PlayerId).Returns((byte)1);

		var reader = new Mock<MessageReader>();
		reader.SetupSequence(r => r.ReadByte()).Returns((byte)99);
		reader.Setup(r => r.ReadBoolean()).Returns(true);

		// Act
		system.UpdateSystem(p1.Object, reader.Object);

		// Assert
		Assert.Equal(0, GetSavedAlphasCount(system));
	}

	[Fact]
	public void UpdateNameText_WhenSavedAlphasEmpty_ReturnsOriginalColor()
	{
		// Arrange
		var system = new InvisibleSystem();
		var targetMock = new Mock<PlayerControl>(IntPtr.Zero);
		targetMock.SetupGet(p => p.PlayerId).Returns((byte)2);

		var originalColor = Color.white;

		// Act
		var result = system.UpdateNameText(targetMock.Object, originalColor);

		// Assert
		Assert.Equal(originalColor, result);
	}

	[Fact]
	public void Reset_WhenMeetingStartAndEmpty_DoesNotThrow()
	{
		// Arrange
		var system = new InvisibleSystem();

		// Act
		system.Reset(ResetTiming.MeetingStart, null);

		// Assert
		Assert.Equal(0, GetSavedAlphasCount(system));
	}

	[Fact]
	public void SetInvisible_TriggersRpcUpdateSystem()
	{
		// Arrange
		var system = new InvisibleSystem();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		var mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockClient.Invocations.Clear();
		mockClient.SetupGet(c => c.AmHost).Returns(true);
		mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockWriterGet = new Mock<MockMessageWriterGetHelper>();
		mockWriterGet.Setup(h => h.Invoke(It.IsAny<SendOption>())).Returns(mockWriter.Object);
		MockMessageWriterGetHelper.Instance = mockWriterGet.Object;

		var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
		mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<MessageWriter>(), It.IsAny<InnerNet.InnerNetObject>()));
		InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;

		// Act
		system.SetInvisible(1, true);

		// Assert
		mockClient.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()), Times.Once);
	}
}
