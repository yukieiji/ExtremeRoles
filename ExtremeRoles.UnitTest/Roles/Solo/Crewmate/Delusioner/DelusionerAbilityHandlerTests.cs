using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.Solo.Crewmate.Delusioner;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Delusioner;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class DelusionerAbilityHandlerTests
{
	public DelusionerAbilityHandlerTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupGameDataMock();
		var localPlayer = MockSetupHelper.SetupPlayerControlMocks();
		localPlayer.SetupGet(p => p.NetId).Returns(100);
		localPlayer.SetupGet(p => p.PlayerId).Returns((byte)0);

		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		var mockClient = MockSetupHelper.SetupAmongUsClientMock();
		mockClient.SetupGet(c => c.AmHost).Returns(true);
		mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);
		mockWriter.Setup(w => w.Write(It.IsAny<MessageWriter>(), It.IsAny<bool>()));
		mockWriter.Setup(w => w.ToByteArray(It.IsAny<bool>())).Returns((Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>)null!);
		var mockWriterGet = new Mock<MockMessageWriterGetHelper>();
		mockWriterGet.Setup(h => h.Invoke(It.IsAny<SendOption>())).Returns(mockWriter.Object);
		MockMessageWriterGetHelper.Instance = mockWriterGet.Object;

		var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
		mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<MessageWriter>(), It.IsAny<InnerNet.InnerNetObject>()));
		InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;

		var mockReaderHelper = new Mock<Hazel.MockMessageReaderGetHelper>();
		mockReaderHelper.Setup(g => g.Invoke(It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>>()))
			.Returns(() =>
			{
				var mockReader = new Mock<MessageReader>();
				mockReader.SetupSequence(r => r.ReadByte())
					.Returns((byte)ExtremeSystemType.DelusionerCounter)
					.Returns((byte)DelusionerCounterSystem.Ops.Ready);
				mockReader.Setup(r => r.ReadPackedInt32()).Returns(3);
				return mockReader.Object;
			});
		Hazel.MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;

		var mockAllList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerControl>>(IntPtr.Zero);
		mockAllList.SetupGet(l => l.Count).Returns(0);
		var mockAllHelper = new Mock<MockPlayerControlget_AllPlayerControlsHelper>();
		mockAllHelper.Setup(h => h.Invoke()).Returns(mockAllList.Object);
		MockPlayerControlget_AllPlayerControlsHelper.Instance = mockAllHelper.Object;
	}

	private static ExtremeAbilityButton CreateButton(BehaviorBase behavior, AbilityState state)
	{
		var button = (ExtremeAbilityButton)RuntimeHelpers.GetUninitializedObject(typeof(ExtremeAbilityButton));
		var behaviorProp = typeof(ExtremeAbilityButton).GetProperty(nameof(ExtremeAbilityButton.Behavior), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		behaviorProp?.SetValue(button, behavior);

		var stateProp = typeof(ExtremeAbilityButton).GetProperty(nameof(ExtremeAbilityButton.State), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		stateProp?.SetValue(button, state);

		return button;
	}

	[Fact]
	public void ReduceCounterNum_WhenStateIsCoolDownAndButtonIsReadyAndCountGreaterThanZero_CallsReadyCounter()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupPlayerControlMocks();

		var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
		mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<MessageWriter>(), It.IsAny<InnerNet.InnerNetObject>()));
		InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;

		var mockReaderHelper = new Mock<Hazel.MockMessageReaderGetHelper>();
		mockReaderHelper.Setup(g => g.Invoke(It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>>()))
			.Returns(() =>
			{
				var mockReader = new Mock<MessageReader>();
				mockReader.SetupSequence(r => r.ReadByte())
					.Returns((byte)ExtremeSystemType.DelusionerCounter)
					.Returns((byte)DelusionerCounterSystem.Ops.Ready);
				mockReader.Setup(r => r.ReadPackedInt32()).Returns(3);
				return mockReader.Object;
			});
		Hazel.MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;

		var system = ExtremeSystemTypeManager.Instance.CreateOrGet<DelusionerCounterSystem>(DelusionerCounterSystem.Type);
		system.Reset(ResetTiming.MeetingStart, null);
		var status = new DelusionerStatusModel(2.5f, false, 0.9f);
		var role = new DelusionerRole();
		var handler = new DelusionerAbilityHandler(system, status, role);

		var behavior = new CountBehavior("Test", null!, () => true, () => true);
		behavior.SetAbilityCount(3);

		var button = CreateButton(behavior, AbilityState.Ready);
		handler.Button = button;

		// Initial prevState is CoolDown
		handler.ReduceCounterNum();

		Assert.True(system.TryGetCounter(PlayerControl.LocalPlayer.PlayerId, out _));
	}

	[Fact]
	public void ReduceCounterNum_WhenConditionsNotMet_DoesNotCallReadyCounter()
	{
		var system = new DelusionerCounterSystem();
		var status = new DelusionerStatusModel(2.5f, false, 0.9f);
		var role = new DelusionerRole();
		var handler = new DelusionerAbilityHandler(system, status, role);

		// Button is null
		handler.ReduceCounterNum();

		var behavior = new CountBehavior("Test", null!, () => true, () => true);
		behavior.SetAbilityCount(0); // AbilityCount <= 0

		var button = CreateButton(behavior, AbilityState.Ready);
		handler.Button = button;

		handler.ReduceCounterNum();

		Assert.False(system.TryGetCounter(0, out _));
	}

	[Fact]
	public void UpdateButtonStatus_UpdatesPrevState()
	{
		var status = new DelusionerStatusModel(2.5f, false, 0.9f);
		var role = new DelusionerRole();
		var handler = new DelusionerAbilityHandler(null, status, role);

		var behavior = new CountBehavior("Test", null!, () => true, () => true);
		var button = CreateButton(behavior, AbilityState.Charging);

		handler.Button = button;
		handler.UpdateButtonStatus();

		// If prevState updated to Charging, ReduceCounterNum shouldn't trigger ReadyCounter even if State is Ready
		var button2 = CreateButton(behavior, AbilityState.Ready);
		var system = new DelusionerCounterSystem();
		var handler2 = new DelusionerAbilityHandler(system, status, role);
		handler2.Button = button;
		handler2.UpdateButtonStatus(); // prevState becomes Charging

		handler2.Button = button2; // Current state Ready, but prevState was Charging
		handler2.ReduceCounterNum(); // prevState is Charging, not CoolDown -> should not call ReadyCounter

		Assert.False(system.TryGetCounter(0, out _));
	}

	[Fact]
	public void TryKilledFrom_WhenSystemIsNull_ReturnsTrue()
	{
		var status = new DelusionerStatusModel(2.5f, false, 0.9f);
		var role = new DelusionerRole();
		var handler = new DelusionerAbilityHandler(null, status, role);

		var mockRolePlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var mockFromPlayer = new Mock<PlayerControl>(IntPtr.Zero);

		bool result = handler.TryKilledFrom(mockRolePlayer.Object, mockFromPlayer.Object);
		Assert.True(result);
	}

	[Fact]
	public void TryKilledFrom_WhenCounterNotFound_ReturnsTrue()
	{
		var system = new DelusionerCounterSystem();
		var status = new DelusionerStatusModel(2.5f, false, 0.9f);
		var role = new DelusionerRole();
		var handler = new DelusionerAbilityHandler(system, status, role);

		var mockRolePlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockRolePlayer.SetupGet(p => p.PlayerId).Returns(1);
		var mockFromPlayer = new Mock<PlayerControl>(IntPtr.Zero);

		bool result = handler.TryKilledFrom(mockRolePlayer.Object, mockFromPlayer.Object);
		Assert.True(result);
	}

	[Fact]
	public void UseAbilityTo_WhenRandomPosEmpty_ReturnsFalse()
	{
		var status = new DelusionerStatusModel(2.5f, false, 1.0f);
		var role = new DelusionerRole();
		var handler = new DelusionerAbilityHandler(null, status, role);

		var mockRolePlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockRolePlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		// No other players in GameData, includeRolePlayer = false, IncludeSpawnPoint = false
		bool result = handler.UseAbilityTo(mockRolePlayer.Object, (byte)2, false, new System.Collections.Generic.HashSet<byte>());
		Assert.False(result);
	}
}
