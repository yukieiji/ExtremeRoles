using System;
using System.Reflection;
using AmongUs.GameOptions;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.Interface;
using UnityEngine.Events;
using ExtremeRoles.Module.Event;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ItakoRoleTests
{
	private sealed class TestCountBehavior : BehaviorBase, ICountBehavior
	{
		public int AbilityCount { get; private set; } = 0;

		public TestCountBehavior() : base("Test", null!) { }

		public override void Initialize(ActionButton button) { }
		public override void ForceAbilityOff() { }
		public override void AbilityOff() { }
		public override bool IsUse() => true;
		public override AbilityState Update(AbilityState curState) => curState;
		public override bool TryUseAbility(float timer, AbilityState curState, out AbilityState newState)
		{
			newState = AbilityState.CoolDown;
			return true;
		}

		public void SetAbilityCount(int count)
		{
			AbilityCount = count;
		}

		public void SetButtonTextFormat(string newTextFormat) { }
	}

	public ItakoRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupPaletteHelpers();
		MockSetupHelper.SetupObjectImplicitHelpers();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupLobbyMock();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupPlayerControlMocks();
		MockSetupHelper.SetupGameDataMock();
		MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		SetupGameOptionsManagerMock();

		var shipStateField = typeof(ExtremeRolesPlugin).GetField("<ShipState>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
		shipStateField?.SetValue(null, new ExtremeShipStatus());

		var eventManagerField = typeof(ExtremeRoles.Module.Event.EventManager).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
		eventManagerField?.SetValue(null, new ExtremeRoles.Module.Event.EventManager());

		ExtremeRoleManager.GameRole.Clear();

		var clientMock = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);
	}

	private static void SetupGameOptionsManagerMock()
	{
		if (MockGameOptionsManagerget_InstanceHelper.Instance == null)
		{
			var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
			var mockGameOptionsManager = new Mock<GameOptionsManager>(IntPtr.Zero);
			mockGameOptionsManager.SetupGet(g => g.CurrentGameOptions).Returns(mockGameOptions.Object);

			var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
			mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockGameOptionsManager.Object);
			MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;
		}
	}

	[Fact]
	public void Constructor_InitializesItakoRoleCorrectly()
	{
		// Arrange & Act
		var itako = new ItakoRole();

		// Assert
		Assert.Equal(ExtremeRoleId.Itako, itako.Core.Id);
		Assert.Equal(ExtremeRoleType.Crewmate, itako.Core.Team);
		Assert.Equal(ColorPalette.ItakoSkyBlue, itako.Core.Color);
		Assert.True(itako.CanHasAnotherRole);
	}

	[Fact]
	public void RoleSpecificInit_LoadsOptionValues()
	{
		// Arrange
		var role = new ItakoRole();
		role.CreateRoleAllOption();

		// Act
		role.Initialize();

		// Assert
		var rangeField = typeof(ItakoRole).GetField("range", BindingFlags.NonPublic | BindingFlags.Instance);
		var requiredTaskRateField = typeof(ItakoRole).GetField("requiredTaskRate", BindingFlags.NonPublic | BindingFlags.Instance);

		float range = (float)rangeField?.GetValue(role)!;
		float requiredTaskRate = (float)requiredTaskRateField?.GetValue(role)!;

		Assert.Equal(1.0f, range);
		Assert.Equal(0.5f, requiredTaskRate);
	}

	[Fact]
	public void ExtractInheritedRole_SingleRole_ReturnsClonedSingleRole()
	{
		// Arrange
		var sheriff = new Sheriff();

		// Act
		var inherited = ItakoRole.ExtractInheritedRole(sheriff);

		// Assert
		Assert.NotNull(inherited);
		Assert.Equal(ExtremeRoleId.Sheriff, inherited.Core.Id);
	}

	[Fact]
	public void ExtractInheritedRole_MultiAssignRoleWithOffsetInfo_ReturnsClonedRoleWithoutAnotherRole()
	{
		// Arrange
		var buddyRole = new Buddy();
		buddyRole.OffsetInfo = new MultiAssignRoleBase.OptionOffsetInfo(CombinationRoleType.Buddy, 100);
		var bakery = new Bakary();
		buddyRole.SetAnotherRole(bakery);

		// Act
		var inherited = ItakoRole.ExtractInheritedRole(buddyRole);

		// Assert
		Assert.NotNull(inherited);
		Assert.Equal(ExtremeRoleId.Buddy, inherited.Core.Id);
		if (inherited is MultiAssignRoleBase multiAssign)
		{
			Assert.Null(multiAssign.AnotherRole);
		}
	}

	[Fact]
	public void InheritTargetRole_SetsAnotherRoleAndOverwritesPreviousRole()
	{
		// Arrange
		byte itakoId = 1;
		byte targetId = 2;

		var itako = new ItakoRole();
		itako.CreateRoleAllOption();
		itako.Initialize();

		var sheriff = new Sheriff();
		sheriff.CreateRoleAllOption();
		sheriff.Initialize();

		ExtremeRoleManager.GameRole[itakoId] = itako;
		ExtremeRoleManager.GameRole[targetId] = sheriff;

		// Act 1: Inherit Sheriff
		ItakoRole.InheritTargetRole(itakoId, targetId);

		// Assert 1
		Assert.NotNull(itako.AnotherRole);
		Assert.Equal(ExtremeRoleId.Sheriff, itako.AnotherRole.Core.Id);

		// Act 2: Overwrite with Bakary
		byte newTargetId = 3;
		var bakery = new Bakary();
		bakery.CreateRoleAllOption();
		bakery.Initialize();
		ExtremeRoleManager.GameRole[newTargetId] = bakery;

		ItakoRole.InheritTargetRole(itakoId, newTargetId);

		// Assert 2
		Assert.NotNull(itako.AnotherRole);
		Assert.Equal(ExtremeRoleId.Bakary, itako.AnotherRole.Core.Id);
	}

	[Fact]
	public void ForceCleanUp_WhenChannelingActive_RefundsAbilityCount()
	{
		// Arrange
		var itako = new ItakoRole();
		itako.CreateRoleAllOption();
		itako.Initialize();

		var mockBehavior = new TestCountBehavior();
		var mockActivator = new Mock<IButtonAutoActivator>();

		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		var mockGridArrange = new Mock<GridArrange>(IntPtr.Zero);
		var mockParentGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockParentGameObject.Setup(g => g.GetComponent<GridArrange>()).Returns(mockGridArrange.Object);

		var mockParentTransform = new Mock<Transform>(IntPtr.Zero);
		mockParentTransform.SetupGet(t => t.gameObject).Returns(mockParentGameObject.Object);

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		mockTransform.SetupGet(t => t.parent).Returns(mockParentTransform.Object);

		var mockMaterial = new Mock<Material>(IntPtr.Zero);
		var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockSpriteRenderer.SetupGet(s => s.material).Returns(mockMaterial.Object);

		var mockLabelText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		var mockCoolText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		mockCoolText.SetupGet(t => t.gameObject).Returns(mockGameObject.Object);

		var mockPersistentCallGroup = new Mock<PersistentCallGroup>(IntPtr.Zero);
		var mockOnClick = new Mock<UnityEngine.UI.Button.ButtonClickedEvent>(IntPtr.Zero);
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

		var mockUseButton = new Mock<UseButton>(IntPtr.Zero);
		mockUseButton.SetupGet(b => b.buttonLabelText).Returns(mockLabelText.Object);
		mockUseButton.SetupGet(b => b.transform).Returns(mockTransform.Object);

		var mockHud = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		mockHud.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);
		mockHud.SetupGet(h => h.UseButton).Returns(mockUseButton.Object);

		var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
		mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

		var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
		mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

		var button = new ExtremeAbilityButton(mockBehavior, mockActivator.Object, KeyCode.F);
		itako.Button = button;

		var channelingField = typeof(ItakoRole).GetField("isChannelingActive", BindingFlags.NonPublic | BindingFlags.Instance);
		channelingField?.SetValue(itako, true);

		// Act
		itako.ForceCleanUp();

		// Assert
		Assert.Equal(1, mockBehavior.AbilityCount);
		Assert.False((bool)channelingField?.GetValue(itako)!);
	}
}
