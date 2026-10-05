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
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ItakoRoleTests
{
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
		MockSetupHelper.SetupDestroyableSingletonMock<RoleManager>();
		MockSetupHelper.SetupOptionManager();
		SetupGameOptionsManagerMock();
		SetupConstantsMock();
		SetupSpriteCacheMock();

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

	private static void SetupConstantsMock()
	{
		if (MockConstantsget_PlayersOnlyMaskHelper.Instance == null)
		{
			var mockMask = new Mock<MockConstantsget_PlayersOnlyMaskHelper>();
			mockMask.Setup(x => x.Invoke()).Returns(1);
			MockConstantsget_PlayersOnlyMaskHelper.Instance = mockMask.Object;
		}
		if (UnityEngine.MockPhysics2DOverlapCircleAllHelper.Instance == null)
		{
			var mockOverlap = new Mock<UnityEngine.MockPhysics2DOverlapCircleAllHelper>();
			mockOverlap.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<float>(), It.IsAny<int>()))
				.Returns(Array.Empty<Collider2D>());
			UnityEngine.MockPhysics2DOverlapCircleAllHelper.Instance = mockOverlap.Object;
		}
	}

	private static void SetupSpriteCacheMock()
	{
		string spriteKey = $"{ObjectPath.TestButton}115";
		if (!LruCache<string, Sprite>.TryGetValue(spriteKey, out _))
		{
			var mockSprite = new Mock<Sprite>(IntPtr.Zero);
			LruCache<string, Sprite>.Add(spriteKey, mockSprite.Object);
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
		Assert.False(itako.CanHasAnotherRole);
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
	public void TryGetExtractInheritedRole_SingleRoleInNormalRole_ReturnsTrueAndRole()
	{
		// Arrange
		var sheriff = new Sheriff();

		// Act
		bool result = ItakoRole.TryGetExtractInheritedRole(sheriff, out var inherited);

		// Assert
		Assert.True(result);
		Assert.NotNull(inherited);
		Assert.Equal(ExtremeRoleId.Sheriff, inherited.Core.Id);
	}

	[Fact]
	public void TryGetExtractInheritedRole_NullTargetRole_ReturnsFalse()
	{
		// Act
		bool result = ItakoRole.TryGetExtractInheritedRole(null, out var inherited);

		// Assert
		Assert.False(result);
		Assert.Null(inherited);
	}

	[Fact]
	public void InheritTargetRole_SetsAnotherRoleAndOverwritesPreviousRole()
	{
		// Arrange
		SetupHudManagerMock();

		byte itakoId = 1;
		byte targetId = 2;

		var localPlayerMock = new Mock<PlayerControl>(IntPtr.Zero);
		localPlayerMock.SetupGet(p => p.PlayerId).Returns(itakoId);
		var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
		mockLocalHelper.Setup(h => h.Invoke()).Returns(localPlayerMock.Object);
		MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

		var mockBehavior = new Mock<BehaviorBase>("Test", null!);
		var mockActivator = new Mock<IButtonAutoActivator>();
		var button = new ExtremeAbilityButton(mockBehavior.Object, mockActivator.Object, KeyCode.F);

		var itako = new ItakoRole();
		itako.CreateRoleAllOption();
		itako.Initialize();
		itako.Button = button;

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
		var bakery = new ExtremeRoles.Roles.Solo.Crewmate.Bakary();
		bakery.CreateRoleAllOption();
		bakery.Initialize();
		ExtremeRoleManager.GameRole[newTargetId] = bakery;

		ItakoRole.InheritTargetRole(itakoId, newTargetId);

		// Assert 2
		Assert.NotNull(itako.AnotherRole);
		Assert.Equal(ExtremeRoleId.Bakary, itako.AnotherRole.Core.Id);
	}

	[Fact]
	public void ForceCleanUp_ResetsTargetBodyAndActiveTargetId()
	{
		// Arrange
		var itako = new ItakoRole();
		itako.CreateRoleAllOption();
		itako.Initialize();

		var targetField = typeof(ItakoRole).GetField("targetBody", BindingFlags.NonPublic | BindingFlags.Instance);
		var activeIdField = typeof(ItakoRole).GetField("activeTargetBodyId", BindingFlags.NonPublic | BindingFlags.Instance);

		activeIdField?.SetValue(itako, (byte)5);

		// Act
		itako.ForceCleanUp();

		// Assert
		Assert.Null(targetField?.GetValue(itako));
		Assert.Equal(byte.MaxValue, (byte)activeIdField?.GetValue(itako)!);
	}

	[Fact]
	public void CreateAbility_ConfiguresButton()
	{
		// Arrange
		SetupHudManagerMock();

		var mockBehavior = new Mock<BehaviorBase>("Test", null!);
		var mockActivator = new Mock<IButtonAutoActivator>();
		var button = new ExtremeAbilityButton(mockBehavior.Object, mockActivator.Object, KeyCode.F);

		var itako = new ItakoRole();
		itako.CreateRoleAllOption();
		itako.Button = button;

		// Assert
		Assert.NotNull(itako.Button);
	}

	[Fact]
	public void ResetOnMeetingStart_CallsForceCleanUp()
	{
		// Arrange
		var itako = new ItakoRole();
		itako.CreateRoleAllOption();
		itako.Initialize();

		var activeIdField = typeof(ItakoRole).GetField("activeTargetBodyId", BindingFlags.NonPublic | BindingFlags.Instance);
		activeIdField?.SetValue(itako, (byte)3);

		// Act
		itako.ResetOnMeetingStart();

		// Assert
		Assert.Equal(byte.MaxValue, (byte)activeIdField?.GetValue(itako)!);
	}

	[Fact]
	public void UseAbility_WhenTmpTargetBodyIsNull_ReturnsFalse()
	{
		// Arrange
		var itako = new ItakoRole();
		itako.CreateRoleAllOption();
		itako.Initialize();

		// Act
		bool result = itako.UseAbility();

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void CheckAbility_WhenNoDeadBodyFound_ReturnsFalse()
	{
		// Arrange
		var itako = new ItakoRole();
		itako.CreateRoleAllOption();
		itako.Initialize();

		// Act
		bool result = itako.CheckAbility();

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void CleanUp_WhenTargetBodyIsNull_CallsForceCleanUp()
	{
		// Arrange
		var itako = new ItakoRole();
		itako.CreateRoleAllOption();
		itako.Initialize();

		var activeIdField = typeof(ItakoRole).GetField("activeTargetBodyId", BindingFlags.NonPublic | BindingFlags.Instance);
		activeIdField?.SetValue(itako, (byte)5);

		// Act
		itako.CleanUp();

		// Assert
		Assert.Equal(byte.MaxValue, (byte)activeIdField?.GetValue(itako)!);
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

		var mockLabelText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		mockLabelText.SetupProperty(t => t.color);
		mockLabelText.SetupProperty(t => t.fontMaterial);
		mockLabelText.SetupProperty(t => t.text);
		mockLabelText.SetupGet(t => t.transform).Returns(mockTransform.Object);

		var mockCoolText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
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
}
