using System;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.Solo.Impostor.RemoteKiller;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class RemoteKillerHandlerTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;

	public RemoteKillerHandlerTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		mockClient = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		SetupTranslationControllerMock();
		SetupSpriteCacheMock();

		mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		MockSetupHelper.SetupGameDataMock();
	}

	private static void SetupTranslationControllerMock()
	{
		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");
	}

	private static void SetupSpriteCacheMock()
	{
		var mockSprite = new Mock<Sprite>(IntPtr.Zero);

		string suicideKey = $"{ObjectPath.SucideSprite}115";
		if (!LruCache<string, Sprite>.TryGetValue(suicideKey, out _))
		{
			LruCache<string, Sprite>.Add(suicideKey, mockSprite.Object);
		}

		string umbrerKey = $"{ObjectPath.UmbrerFeatVirus}115";
		if (!LruCache<string, Sprite>.TryGetValue(umbrerKey, out _))
		{
			LruCache<string, Sprite>.Add(umbrerKey, mockSprite.Object);
		}
	}

	[Fact]
	public void RobHandler_CreateBehavior_ConfiguresActiveTimeCorrectly()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 2.5f, 1, 5.0f);
		var role = new RemoteKillerRole();
		var handler = new RemoteKillerRobHandler(status, role);

		// Act
		var behavior = handler.CreateBehavior(30.0f);

		// Assert
		Assert.NotNull(behavior);
		Assert.Equal(2.5f, behavior.ActiveTime);
	}

	[Fact]
	public void RobHandler_RobCleanUp_AddsTargetToExecutionTargets()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 2.5f, 1, 5.0f);
		var role = new RemoteKillerRole();
		var handler = new RemoteKillerRobHandler(status, role);

		var mockTarget = new Mock<PlayerControl>(IntPtr.Zero);
		mockTarget.SetupGet(p => p.PlayerId).Returns((byte)2);

		var field = typeof(RemoteKillerRobHandler).GetField("currentRobTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field?.SetValue(handler, mockTarget.Object);

		// Act
		handler.RobCleanUp();

		// Assert
		Assert.True(status.HasExecutionTarget(2));
	}

	[Fact]
	public void PurgeHandler_CreateBehavior_ConfiguresActiveTimeAndCountCorrectly()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 2.5f, 1, 4.5f);
		var role = new RemoteKillerRole();
		var handler = new RemoteKillerPurgeHandler(status, role);

		// Act
		var behavior = handler.CreateBehavior(30.0f, 3);

		// Assert
		Assert.NotNull(behavior);
		Assert.Equal(4.5f, behavior.ActiveTime);
		Assert.Equal(3, behavior.AbilityCount);
	}

	[Fact]
	public void PurgeHandler_PurgeStartAbility_WhenTargetSelected_TriggersRpcAndReturnsTrue()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 2.5f, 1, 4.5f);
		var role = new RemoteKillerRole();
		var handler = new RemoteKillerPurgeHandler(status, role);

		var mockTarget = new Mock<PlayerControl>(IntPtr.Zero);
		mockTarget.SetupGet(p => p.PlayerId).Returns((byte)2);

		var field = typeof(RemoteKillerPurgeHandler).GetField("selectedPurgeTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field?.SetValue(handler, mockTarget.Object);

		mockClient.Invocations.Clear();

		// Act
		bool result = handler.PurgeStartAbility(0f);

		// Assert
		Assert.True(result);
		mockClient.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()), Times.Once);
	}

	[Fact]
	public void PurgeHandler_PurgeCleanUp_RemovesExecutionTargetFromStatus()
	{
		// Arrange
		var status = new RemoteKillerStatusModel(1.5f, 2.5f, 1, 4.5f);
		status.AddExecutionTarget(2);

		var role = new RemoteKillerRole();
		var handler = new RemoteKillerPurgeHandler(status, role);

		var mockTarget = new Mock<PlayerControl>(IntPtr.Zero);
		mockTarget.SetupGet(p => p.PlayerId).Returns((byte)2);

		var field = typeof(RemoteKillerPurgeHandler).GetField("selectedPurgeTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field?.SetValue(handler, mockTarget.Object);

		mockClient.Invocations.Clear();

		// Act
		handler.PurgeCleanUp();

		// Assert
		Assert.False(status.HasExecutionTarget(2));
		mockClient.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()), Times.AtLeastOnce);
	}
}
