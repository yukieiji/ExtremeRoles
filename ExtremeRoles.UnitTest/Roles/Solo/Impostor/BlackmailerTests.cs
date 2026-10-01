using System;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using UnityEngine;
using Xunit;

using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Impostor;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class BlackmailerTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;

	public BlackmailerTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupUnityCommonMocks();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		SetupLobbyBehaviourMock();
		SetupTranslationControllerMock();

		this.mockClient = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		this.mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		this.mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		this.mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(10f, 20f));

		MockSetupHelper.SetupGameDataMock();
		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		if (ExtremeRolesPlugin.ShipState == null)
		{
			var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(
				nameof(ExtremeRolesPlugin.ShipState),
				System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
			shipStateProp?.SetValue(null, new ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus());
		}
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
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
			.Returns((string id, string defaultStr, Il2CppReferenceArray<Il2CppSystem.Object> parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
	}

	private static void InitializeRole(Blackmailer role, byte playerId = 1)
	{
		role.CreateRoleAllOption();
		role.Initialize();

		ExtremeRoleManager.GameRole[playerId] = role;
	}

	[Fact]
	public void Initialize_RegistersBlackmailerRoleAndImpostorTeam()
	{
		var role = new Blackmailer();
		InitializeRole(role);

		Assert.Equal(ExtremeRoleId.Blackmailer, role.Core.Id);
		Assert.True(role.IsImpostor());
	}

	[Fact]
	public void System_AddAndClearBlackmail_WorksCorrectly()
	{
		var system = BlackmailerSystem.GetOrRegister();

		system.Reset(ResetTiming.MeetingEnd);
		Assert.False(system.IsBlackmailed(2));
		Assert.False(system.IsBlackmailedBy(1, 2));

		var rAdd = new Mock<MessageReader>();
		rAdd.SetupSequence(r => r.ReadByte())
			.Returns((byte)BlackmailerSystem.Ops.AddBlackmail)
			.Returns((byte)1) // blackmailerId
			.Returns((byte)2); // targetId

		system.UpdateSystem(null!, rAdd.Object);
		Assert.True(system.IsBlackmailed(2));
		Assert.True(system.IsBlackmailedBy(1, 2));
		Assert.False(system.IsBlackmailedBy(3, 2));

		var rClear = new Mock<MessageReader>();
		rClear.SetupSequence(r => r.ReadByte())
			.Returns((byte)BlackmailerSystem.Ops.ClearBlackmail)
			.Returns((byte)1);

		system.UpdateSystem(null!, rClear.Object);
		Assert.False(system.IsBlackmailed(2));
		Assert.False(system.IsBlackmailedBy(1, 2));
	}

	[Fact]
	public void GetRolePlayerNameTag_WhenTargetIsBlackmailedBySelf_ReturnsTagWithBlackmailMark()
	{
		var role = new Blackmailer();
		InitializeRole(role, playerId: 1);

		var system = BlackmailerSystem.GetOrRegister();
		var rAdd = new Mock<MessageReader>();
		rAdd.SetupSequence(r => r.ReadByte())
			.Returns((byte)BlackmailerSystem.Ops.AddBlackmail)
			.Returns((byte)1) // blackmailerId = 1 (local player)
			.Returns((byte)2); // targetId

		system.UpdateSystem(null!, rAdd.Object);

		string tag = role.GetRolePlayerNameTag(role, 2);
		Assert.Contains("◆", tag);

		system.Reset(ResetTiming.MeetingEnd);
		string normalTag = role.GetRolePlayerNameTag(role, 2);
		Assert.DoesNotContain("◆", normalTag);
	}
}
