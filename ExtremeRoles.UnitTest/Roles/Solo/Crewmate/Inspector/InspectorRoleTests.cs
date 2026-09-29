using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
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

		if (Hazel.MockMessageReaderGetHelper.Instance == null)
		{
			var mockReaderHelper = new Mock<Hazel.MockMessageReaderGetHelper>();
			var mockReader = new Mock<MessageReader>();
			mockReaderHelper.Setup(g => g.Invoke(It.IsAny<Il2CppStructArray<byte>>()))
				.Returns(mockReader.Object);
			Hazel.MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;
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
		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");
	}

	private static void SetupHudManagerMock()
	{
		if (MockVector3get_oneHelper.Instance == null)
		{
			var mockOne = new Mock<MockVector3get_oneHelper>();
			mockOne.Setup(x => x.Invoke()).Returns(new Vector3(1f, 1f, 1f));
			MockVector3get_oneHelper.Instance = mockOne.Object;
		}

		var mockObjectImplicitInt = new Mock<Il2CppSystem.MockObjectop_ImplicitHelper6>();
		mockObjectImplicitInt.Setup(x => x.Invoke(It.IsAny<int>())).Returns(new Mock<Il2CppSystem.Object>(IntPtr.Zero).Object);
		Il2CppSystem.MockObjectop_ImplicitHelper6.Instance = mockObjectImplicitInt.Object;

		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockGameObject.Setup(g => g.SetActive(It.IsAny<bool>()));

		var mockGridArrange = new Mock<GridArrange>(IntPtr.Zero);
		mockGridArrange.Setup(g => g.ArrangeChilds());

		var mockParentGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockParentGameObject.Setup(g => g.GetComponent<GridArrange>()).Returns(mockGridArrange.Object);

		var mockParentTransform = new Mock<Transform>(IntPtr.Zero);
		mockParentTransform.SetupGet(t => t.gameObject).Returns(mockParentGameObject.Object);

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		mockTransform.SetupGet(t => t.parent).Returns(mockParentTransform.Object);
		mockTransform.Setup(t => t.FindChild(It.IsAny<string>())).Returns((Transform)null!);

		var mockMaterial = new Mock<Material>(IntPtr.Zero);
		mockMaterial.Setup(m => m.SetFloat(It.IsAny<string>(), It.IsAny<float>()));

		var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockSpriteRenderer.SetupProperty(s => s.sprite);
		mockSpriteRenderer.SetupProperty(s => s.color);
		mockSpriteRenderer.SetupProperty(s => s.enabled);
		mockSpriteRenderer.SetupGet(s => s.material).Returns(mockMaterial.Object);

		var mockLabelText = new Mock<TextMeshPro>(IntPtr.Zero);
		mockLabelText.SetupProperty(t => t.color);
		mockLabelText.SetupProperty(t => t.fontMaterial);
		mockLabelText.SetupProperty(t => t.text);
		mockLabelText.SetupGet(t => t.transform).Returns(mockTransform.Object);

		var mockCoolText = new Mock<TextMeshPro>(IntPtr.Zero);
		mockCoolText.SetupProperty(t => t.color);
		mockCoolText.SetupProperty(t => t.enableWordWrapping);
		mockCoolText.SetupProperty(t => t.text);
		mockCoolText.SetupGet(t => t.gameObject).Returns(mockGameObject.Object);
		mockCoolText.SetupGet(t => t.transform).Returns(mockTransform.Object);

		var mockPersistentCallGroup = new Mock<PersistentCallGroup>(IntPtr.Zero);
		mockPersistentCallGroup.Setup(p => p.Clear());

		var mockOnClick = new Mock<UnityEngine.UI.Button.ButtonClickedEvent>(IntPtr.Zero);
		mockOnClick.Setup(e => e.RemoveAllListeners());
		mockOnClick.Setup(e => e.AddListener(It.IsAny<UnityAction>()));
		mockOnClick.SetupGet(e => e.m_PersistentCalls).Returns(mockPersistentCallGroup.Object);

		var mockPassiveButton = new Mock<PassiveButton>(IntPtr.Zero);
		mockPassiveButton.SetupGet(p => p.OnClick).Returns(mockOnClick.Object);

		var mockKillButton = new Mock<KillButton>(IntPtr.Zero);
		mockKillButton.SetupGet(b => b.transform).Returns(mockTransform.Object);
		mockKillButton.SetupGet(b => b.gameObject).Returns(mockGameObject.Object);
		mockKillButton.SetupGet(b => b.graphic).Returns(mockSpriteRenderer.Object);
		mockKillButton.SetupGet(b => b.buttonLabelText).Returns(mockLabelText.Object);
		mockKillButton.SetupGet(b => b.cooldownTimerText).Returns(mockCoolText.Object);
		mockKillButton.Setup(b => b.GetComponent<PassiveButton>()).Returns(mockPassiveButton.Object);
		mockKillButton.SetupGet(b => b.isActiveAndEnabled).Returns(true);
		mockKillButton.Setup(b => b.OverrideText(It.IsAny<string>()));
		mockKillButton.Setup(b => b.SetCoolDown(It.IsAny<float>(), It.IsAny<float>()));
		mockKillButton.Setup(b => b.SetCooldownFill(It.IsAny<float>()));

		var mockUseButton = new Mock<UseButton>(IntPtr.Zero);
		mockUseButton.SetupGet(b => b.buttonLabelText).Returns(mockLabelText.Object);
		mockUseButton.SetupGet(b => b.transform).Returns(mockTransform.Object);

		var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
		mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

		var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
		mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

		var mockUnityActionImplicit = new Mock<MockUnityActionop_ImplicitHelper>();
		mockUnityActionImplicit.Setup(x => x.Invoke(It.IsAny<Action>()))
			.Returns((Action action) => action != null ? new UnityAction(IntPtr.Zero) : null!);
		MockUnityActionop_ImplicitHelper.Instance = mockUnityActionImplicit.Object;

		var mockHud = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		mockHud.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);
		mockHud.SetupGet(h => h.UseButton).Returns(mockUseButton.Object);
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
	public void ButtonProperty_GetterAndSetterWork()
	{
		SetupHudManagerMock();

		var role = new Inspector();
		var behavior = new NullBehaviour();
		var mockActivator = new Mock<IButtonAutoActivator>();
		var button = new ExtremeAbilityButton(behavior, mockActivator.Object, KeyCode.F);

		role.Button = button;

		Assert.Same(button, role.Button);
	}

	[Fact]
	public void UseAbility_ReturnsTrue()
	{
		var role = new Inspector();

		bool result = role.UseAbility();

		Assert.True(result);
	}

	[Fact]
	public void IsAbilityUse_ReturnsCommonUseValue()
	{
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
	public void CleanUp_And_ResetOnMeetingStart_And_ResetOnMeetingEnd_ExecuteWithoutThrowing()
	{
		var role = new Inspector();
		role.CreateRoleAllOption();
		role.Initialize();

		role.CleanUp();
		role.ResetOnMeetingStart();
		role.ResetOnMeetingEnd(null);
	}
}
