using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.UnitTest;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using Moq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.InspectorTests;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class InspectorRoleTests
{
	private readonly Mock<AmongUsClient> clientMock;

	public InspectorRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupPlayerControlMocks();
		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		SetupLobbyBehaviourMock();
		SetupTranslationControllerMock();
		SetupShipStatusMock();

		clientMock = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		if (Hazel.MockMessageWriterGetHelper.Instance == null)
		{
			var mockGet = new Mock<Hazel.MockMessageWriterGetHelper>();
			mockGet.Setup(g => g.Invoke(It.IsAny<SendOption>())).Returns(mockWriter.Object);
			Hazel.MockMessageWriterGetHelper.Instance = mockGet.Object;
		}

		if (InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance == null)
		{
			var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
			mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<MessageWriter>(), It.IsAny<InnerNetObject>()));
			InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;
		}
	}

	private static void SetupShipStatusMock()
	{
		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(x => x.Invoke()).Returns(mockShipStatus.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;
	}

	private static void SetupLobbyBehaviourMock()
	{
		var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
		var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
		mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
		MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
	}

	private static void SetupTranslationControllerMock()
	{
		MockDestroyableSingletonget_InstanceHelper<TranslationController>.Instance = null;
		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
			.Returns((string id, string defaultStr, Il2CppReferenceArray<Il2CppSystem.Object> parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");
	}

	private static void SetupMessageReaderMock(InspectorInspectSystem.Ops ops)
	{
		var mockReaderHelper = new Mock<Hazel.MockMessageReaderGetHelper>();
		mockReaderHelper.Setup(g => g.Invoke(It.IsAny<Il2CppStructArray<byte>>()))
			.Returns(() =>
			{
				var mockReader = new Mock<MessageReader>();
				mockReader.SetupSequence(r => r.ReadByte())
					.Returns((byte)ExtremeSystemType.InspectorInspect)
					.Returns((byte)ops);
				return mockReader.Object;
			});
		Hazel.MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;
	}

	[Fact]
	public void CreateRoleAllOption_CreatesExpectedOptionsWithDefaultValues()
	{
		int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Inspector);
		var role = new Inspector();

		role.CreateRoleAllOption();

		Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
		Assert.NotNull(category);

		Assert.True(role.Loader.TryGet(Inspector.Option.InspectSabotage, out var sabotageOpt));
		Assert.NotNull(sabotageOpt);
		Assert.Equal(1, sabotageOpt.Selection);

		Assert.True(role.Loader.TryGet(Inspector.Option.InspectVent, out var ventOpt));
		Assert.NotNull(ventOpt);
		Assert.Equal(1, ventOpt.Selection);

		Assert.True(role.Loader.TryGet(Inspector.Option.InspectAbility, out var abilityOpt));
		Assert.NotNull(abilityOpt);
		Assert.Equal(0, abilityOpt.Selection);
	}

	[Fact]
	public void Initialize_RegistersInspectorInspectSystem_WithDefaultOptions()
	{
		var role = new Inspector();
		role.CreateRoleAllOption();

		role.Initialize();

		Assert.True(ExtremeSystemTypeManager.Instance.TryGet<InspectorInspectSystem>(ExtremeSystemType.InspectorInspect, out var system));
		Assert.NotNull(system);

		var modeField = typeof(InspectorInspectSystem).GetField("mode", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(modeField);

		var inspectMode = (InspectorInspectSystem.InspectMode)modeField.GetValue(system)!;
		Assert.Equal(InspectorInspectSystem.InspectMode.Sabotage | InspectorInspectSystem.InspectMode.Vent, inspectMode);
	}

	[Fact]
	public void Initialize_RegistersInspectorInspectSystem_WithCustomOptions()
	{
		var role = new Inspector();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Inspector.Option.InspectSabotage, out var sabotageOpt) && sabotageOpt != null)
		{
			sabotageOpt.Selection = 0;
		}
		if (role.Loader.TryGet(Inspector.Option.InspectVent, out var ventOpt) && ventOpt != null)
		{
			ventOpt.Selection = 0;
		}
		if (role.Loader.TryGet(Inspector.Option.InspectAbility, out var abilityOpt) && abilityOpt != null)
		{
			abilityOpt.Selection = 1;
		}

		role.Initialize();

		Assert.True(ExtremeSystemTypeManager.Instance.TryGet<InspectorInspectSystem>(ExtremeSystemType.InspectorInspect, out var system));
		Assert.NotNull(system);

		var modeField = typeof(InspectorInspectSystem).GetField("mode", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(modeField);

		var inspectMode = (InspectorInspectSystem.InspectMode)modeField.GetValue(system)!;
		Assert.Equal(InspectorInspectSystem.InspectMode.Ability, inspectMode);
	}

	[Fact]
	public void UseAbility_StartsInspectInInspectorInspectSystem_AndReturnsTrue()
	{
		SetupMessageReaderMock(InspectorInspectSystem.Ops.StartInspect);

		var role = new Inspector();
		role.CreateRoleAllOption();
		role.Initialize();

		Assert.True(ExtremeSystemTypeManager.Instance.TryGet<InspectorInspectSystem>(ExtremeSystemType.InspectorInspect, out var system));
		Assert.NotNull(system);

		byte localPlayerId = PlayerControl.LocalPlayer.PlayerId;

		bool result = role.UseAbility();

		Assert.True(result);

		var allTargetField = typeof(InspectorInspectSystem).GetField("allTarget", BindingFlags.NonPublic | BindingFlags.Instance);
		var allTarget = (IDictionary)allTargetField!.GetValue(system)!;
		Assert.True(allTarget.Contains(localPlayerId));
	}

	[Fact]
	public void IsAbilityUse_ReturnsCommonUseValue()
	{
		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.CanMove).Returns(false);

		var role = new Inspector();

		bool result = role.IsAbilityUse();

		Assert.False(result);
	}

	[Fact]
	public void RolePlayerKilledAction_EndsInspectForRolePlayerInSystem()
	{
		var role = new Inspector();
		role.CreateRoleAllOption();
		role.Initialize();

		Assert.True(ExtremeSystemTypeManager.Instance.TryGet<InspectorInspectSystem>(ExtremeSystemType.InspectorInspect, out var system));
		Assert.NotNull(system);

		byte rolePlayerId = 3;
		var rolePlayerMock = new Mock<PlayerControl>(IntPtr.Zero);
		rolePlayerMock.SetupGet(p => p.PlayerId).Returns(rolePlayerId);

		var killerPlayerMock = new Mock<PlayerControl>(IntPtr.Zero);
		killerPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)2);

		var rStart = new Mock<MessageReader>();
		rStart.SetupSequence(r => r.ReadByte())
			.Returns((byte)InspectorInspectSystem.Ops.StartInspect);
		system.UpdateSystem(rolePlayerMock.Object, rStart.Object);

		var allTargetField = typeof(InspectorInspectSystem).GetField("allTarget", BindingFlags.NonPublic | BindingFlags.Instance);
		var allTarget = (IDictionary)allTargetField!.GetValue(system)!;
		Assert.True(allTarget.Contains(rolePlayerId));

		role.RolePlayerKilledAction(rolePlayerMock.Object, killerPlayerMock.Object);

		Assert.False(allTarget.Contains(rolePlayerId));
	}

	[Fact]
	public void CleanUp_EndsInspectForLocalPlayerInSystem()
	{
		SetupMessageReaderMock(InspectorInspectSystem.Ops.StartInspect);

		var role = new Inspector();
		role.CreateRoleAllOption();
		role.Initialize();

		Assert.True(ExtremeSystemTypeManager.Instance.TryGet<InspectorInspectSystem>(ExtremeSystemType.InspectorInspect, out var system));
		Assert.NotNull(system);

		byte localPlayerId = PlayerControl.LocalPlayer.PlayerId;

		role.UseAbility();

		var allTargetField = typeof(InspectorInspectSystem).GetField("allTarget", BindingFlags.NonPublic | BindingFlags.Instance);
		var allTarget = (IDictionary)allTargetField!.GetValue(system)!;
		Assert.True(allTarget.Contains(localPlayerId));

		SetupMessageReaderMock(InspectorInspectSystem.Ops.EndInspect);

		role.CleanUp();

		Assert.False(allTarget.Contains(localPlayerId));
	}

	[Fact]
	public void ResetOnMeetingStart_CallsCleanUp_AndEndsInspectForLocalPlayer()
	{
		SetupMessageReaderMock(InspectorInspectSystem.Ops.StartInspect);

		var role = new Inspector();
		role.CreateRoleAllOption();
		role.Initialize();

		Assert.True(ExtremeSystemTypeManager.Instance.TryGet<InspectorInspectSystem>(ExtremeSystemType.InspectorInspect, out var system));
		Assert.NotNull(system);

		byte localPlayerId = PlayerControl.LocalPlayer.PlayerId;

		role.UseAbility();

		var allTargetField = typeof(InspectorInspectSystem).GetField("allTarget", BindingFlags.NonPublic | BindingFlags.Instance);
		var allTarget = (IDictionary)allTargetField!.GetValue(system)!;
		Assert.True(allTarget.Contains(localPlayerId));

		SetupMessageReaderMock(InspectorInspectSystem.Ops.EndInspect);

		role.ResetOnMeetingStart();

		Assert.False(allTarget.Contains(localPlayerId));
	}

	[Fact]
	public void ResetOnMeetingEnd_ExecutesWithoutError()
	{
		var role = new Inspector();
		role.ResetOnMeetingEnd(null);
	}
}
