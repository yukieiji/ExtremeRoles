using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate.Delusioner;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Delusioner;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class DelusionerRoleTests
{
	private readonly Mock<AmongUsClient> mockClient;

	public DelusionerRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupGameDataMock();

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		var mockLocalTransform = new Mock<Transform>(IntPtr.Zero);
		localPlayerMock.SetupGet(p => p.transform).Returns(mockLocalTransform.Object);
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		var mockRole = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRole.SetupGet(r => r.Blurb).Returns("Crewmate Blurb");
		mockData.SetupGet(d => d.Role).Returns(mockRole.Object);
		localPlayerMock.SetupGet(p => p.Data).Returns(mockData.Object);

		mockClient = MockSetupHelper.SetupAmongUsClientMock();

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
			.Returns((string id, string defaultStr, Il2CppReferenceArray<Il2CppSystem.Object> parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);

		MockSetupHelper.SetupOptionManager();

		var mockTargetPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockTargetPlayer.SetupGet(p => p.PlayerId).Returns((byte)2);

		var mockAllList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerControl>>(IntPtr.Zero);
		mockAllList.SetupGet(l => l.Count).Returns(1);
		mockAllList.Setup(l => l[0]).Returns(mockTargetPlayer.Object);

		var mockAllHelper = new Mock<MockPlayerControlget_AllPlayerControlsHelper>();
		mockAllHelper.Setup(h => h.Invoke()).Returns(mockAllList.Object);
		MockPlayerControlget_AllPlayerControlsHelper.Instance = mockAllHelper.Object;
	}

	[Fact]
	public void Constructor_InitializesCorrectly()
	{
		var role = new DelusionerRole();
		Assert.NotNull(role);
		Assert.Equal(ExtremeRoleId.Delusioner, role.Core.Id);
		Assert.Equal((int)ExtremeRoles.Roles.API.Interface.IRoleVoteModifier.ModOrder.DelusionerCheckVote, role.Order);
		Assert.Equal(RoleTypes.Crewmate, role.NoneAwakeRole);
		Assert.Equal("", role.GetFakeOptionString());
	}

	[Fact]
	public void IsAwake_WhenInLobby_ReturnsTrue()
	{
		mockClient.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.NotJoined);
		var role = new DelusionerRole();
		Assert.True(role.IsAwake);
	}

	[Fact]
	public void IsAwake_WhenInGameAndNotAwake_ReturnsFalse()
	{
		mockClient.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
		var role = new DelusionerRole();
		Assert.False(role.IsAwake);
	}

	[Fact]
	public void DescriptionsAndNames_WhenAwakeOrTruthColor()
	{
		mockClient.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.NotJoined); // Makes IsAwake = true
		var role = new DelusionerRole();

		string roleNameStr = ExtremeRoles.Extension.Controller.TranslationControllerExtension.GetString(role.Core.Id.ToString());
		string expectedColoredName = Design.ColoredString(role.Core.Color, roleNameStr);
		Assert.Equal(expectedColoredName, role.GetColoredRoleName(true));
		Assert.Equal(expectedColoredName, role.GetColoredRoleName(false));

		string expectedFullDesc = ExtremeRoles.Extension.Controller.TranslationControllerExtension.GetString($"{role.Core.Id}FullDescription");
		Assert.Equal(expectedFullDesc, role.GetFullDescription());

		string shortDescStr = ExtremeRoles.Extension.Controller.TranslationControllerExtension.GetString($"{role.Core.Id}ShortDescription");
		string expectedImportant = Design.ColoredString(
			role.Core.Color,
			string.Format("{0}: {1}",
				Design.ColoredString(role.Core.Color, roleNameStr),
				shortDescStr));
		Assert.Equal(expectedImportant, role.GetImportantText(true));
		Assert.Equal(expectedImportant, role.GetImportantText(false));

		string expectedIntro = ExtremeRoles.Extension.Controller.TranslationControllerExtension.GetString($"{role.Core.Id}IntroDescription");
		Assert.Equal(expectedIntro, role.GetIntroDescription());

		Assert.Equal(role.Core.Color, role.GetNameColor(true));
		Assert.Equal(role.Core.Color, role.GetNameColor(false));
	}

	[Fact]
	public void DescriptionsAndNames_WhenNotAwakeAndNotTruthColor()
	{
		mockClient.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started); // IsAwake = false
		var role = new DelusionerRole();

		string expectedName = Design.ColoredString(Palette.White, ExtremeRoles.Extension.Controller.TranslationControllerExtension.GetString(RoleTypes.Crewmate.ToString()));
		Assert.Equal(expectedName, role.GetColoredRoleName(false));

		string expectedFullDesc = ExtremeRoles.Extension.Controller.TranslationControllerExtension.GetString($"{RoleTypes.Crewmate}FullDescription");
		Assert.Equal(expectedFullDesc, role.GetFullDescription());

		string expectedImportant = Design.ColoredString(Palette.White, $"{expectedName}: {ExtremeRoles.Extension.Controller.TranslationControllerExtension.GetString("crewImportantText")}");
		Assert.Equal(expectedImportant, role.GetImportantText(true));

		string expectedIntro = Design.ColoredString(Palette.CrewmateBlue, "Crewmate Blurb");
		Assert.Equal(expectedIntro, role.GetIntroDescription());

		Assert.Equal(Palette.White, role.GetNameColor(false));
	}

	[Fact]
	public void ButtonProperty_GetterAndSetter()
	{
		var role = new DelusionerRole();
		Assert.Null(role.Button);

		// DelusionerRole.Button setter does nothing when ability is null
		role.Button = null;
		Assert.Null(role.Button);
	}

	[Fact]
	public void IsAbilityUse_WhenNotAwake_ReturnsFalse()
	{
		mockClient.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
		var role = new DelusionerRole();
		Assert.False(role.IsAbilityUse());
	}


	[Fact]
	public void HookVoteEnd_AwakeConditionAndCoolTimeReduce()
	{
		mockClient.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
		var role = new DelusionerRole();

		var mockHud = new Mock<MeetingHud>(IntPtr.Zero);
		var mockNetInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockNetInfo.SetupGet(p => p.PlayerId).Returns((byte)1);

		var voteDict = new Dictionary<byte, int> { { (byte)1, 3 } };

		// Default awakeVoteCount = 3, so 3 votes will awaken
		role.HookVoteEnd(mockHud.Object, mockNetInfo.Object, voteDict);

		Assert.True(role.IsAwake);
	}

	[Fact]
	public void HookVoteEnd_WhenPlayerNotInVoteDict_DoesNotAwake()
	{
		mockClient.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
		var role = new DelusionerRole();

		var mockHud = new Mock<MeetingHud>(IntPtr.Zero);
		var mockNetInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockNetInfo.SetupGet(p => p.PlayerId).Returns((byte)1);

		var voteDict = new Dictionary<byte, int> { { (byte)2, 5 } };

		role.HookVoteEnd(mockHud.Object, mockNetInfo.Object, voteDict);

		Assert.False(role.IsAwake);
	}

	[Fact]
	public void UseAbility_WhenAbilityIsNull_ReturnsFalse()
	{
		var role = new DelusionerRole();
		Assert.False(role.UseAbility());
	}

	[Fact]
	public void ResetOnMeetingStart_ResetsCoolTimeOnStatus()
	{
		var role = new DelusionerRole();
		var status = new DelusionerStatusModel(2.5f, false, 0.9f);
		status.CurCoolTime = 99.0f;

		typeof(DelusionerRole).GetField("status", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(role, status);
		typeof(DelusionerRole).GetField("defaultCoolTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(role, 15.0f);

		role.ResetOnMeetingStart();

		Assert.Equal(15.0f, status.CurCoolTime);
	}

	[Fact]
	public void UseAbility_WhenAbilityIsNotNull_ReturnsTrue()
	{
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		mockWriter.Setup(w => w.Write(It.IsAny<MessageWriter>(), It.IsAny<bool>()));
		mockWriter.Setup(w => w.ToByteArray(It.IsAny<bool>())).Returns((Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>)null!);
		mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var role = new DelusionerRole();
		var status = new DelusionerStatusModel(2.5f, false, 1.0f);
		var ability = new DelusionerAbilityHandler(null, status, role);

		typeof(DelusionerRole).GetField("ability", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(role, ability);
		typeof(DelusionerRole).GetField("includeLocalPlayer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(role, true);
		typeof(DelusionerRole).GetField("targetPlayerId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(role, (byte)2);

		bool result = role.UseAbility();

		Assert.True(result);
	}
}
