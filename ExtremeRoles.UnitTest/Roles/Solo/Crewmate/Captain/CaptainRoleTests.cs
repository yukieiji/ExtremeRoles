using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using AmongUs.GameOptions;
using InnerNet;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.Solo.Crewmate;
using Hazel;
using MonoMod.RuntimeDetour;
using Moq;
using UnityEngine;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.CaptainTests;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class CaptainRoleTests : IDisposable
{
	private delegate RPCOperator.RpcCaller CreateCallerOrig(uint netId, RPCOperator.Command ops);
	private delegate RPCOperator.RpcCaller CreateCallerHook(CreateCallerOrig orig, uint netId, RPCOperator.Command ops);

	private delegate void RpcWriteByteOrig(RPCOperator.RpcCaller self, byte value);
	private delegate void RpcWriteByteHook(RpcWriteByteOrig orig, RPCOperator.RpcCaller self, byte value);

	private delegate void RpcDisposeOrig(RPCOperator.RpcCaller self);
	private delegate void RpcDisposeHook(RpcDisposeOrig orig, RPCOperator.RpcCaller self);

	private readonly Hook createCallerHook;
	private readonly Hook rpcWriteByteHook;
	private readonly Hook rpcDisposeHook;

	private readonly Mock<AmongUsClient> clientMock;

	public CaptainRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupPlayerVoteAreaMocks();

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupPlayerControlMocks();
		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		clientMock = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

		var mockDestroyableMeetingHelper = new Mock<MockDestroyableSingletonget_InstanceHelper<MeetingHud>>();
		mockDestroyableMeetingHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockDestroyableSingletonget_InstanceHelper<MeetingHud>.Instance = mockDestroyableMeetingHelper.Object;

		var createCallerTarget = typeof(RPCOperator).GetMethod(nameof(RPCOperator.CreateCaller), new[] { typeof(uint), typeof(RPCOperator.Command) })!;
		var createCallerHookDelegate = new CreateCallerHook(getCreateCallerHook);
		this.createCallerHook = new Hook(createCallerTarget, createCallerHookDelegate);

		var rpcWriteByteTarget = typeof(RPCOperator.RpcCaller).GetMethod(nameof(RPCOperator.RpcCaller.WriteByte))!;
		var rpcWriteByteHookDelegate = new RpcWriteByteHook(getRpcWriteByteHook);
		this.rpcWriteByteHook = new Hook(rpcWriteByteTarget, rpcWriteByteHookDelegate);

		var rpcDisposeTarget = typeof(RPCOperator.RpcCaller).GetMethod(nameof(RPCOperator.RpcCaller.Dispose))!;
		var rpcDisposeHookDelegate = new RpcDisposeHook(getRpcDisposeHook);
		this.rpcDisposeHook = new Hook(rpcDisposeTarget, rpcDisposeHookDelegate);

		SetLobbyMode(false);
	}

	public void Dispose()
	{
		this.createCallerHook.Dispose();
		this.rpcWriteByteHook.Dispose();
		this.rpcDisposeHook.Dispose();
	}

	private static RPCOperator.RpcCaller getCreateCallerHook(CreateCallerOrig orig, uint netId, RPCOperator.Command ops)
	{
		return (RPCOperator.RpcCaller)RuntimeHelpers.GetUninitializedObject(typeof(RPCOperator.RpcCaller));
	}

	private static void getRpcWriteByteHook(RpcWriteByteOrig orig, RPCOperator.RpcCaller self, byte value)
	{
	}

	private static void getRpcDisposeHook(RpcDisposeOrig orig, RPCOperator.RpcCaller self)
	{
	}

	private void SetLobbyMode(bool isLobby)
	{
		if (isLobby)
		{
			clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.NotJoined);
			var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
			var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
			mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
			MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
		}
		else
		{
			clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
			var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
			mockLobbyHelper.Setup(x => x.Invoke()).Returns((LobbyBehaviour)null!);
			MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
		}
	}

	private static void InitializeCaptainRole(Captain role, byte playerId = 1)
	{
		int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Captain);
		if (!OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out _))
		{
			role.CreateRoleAllOption();
		}
		role.Initialize();

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[playerId] = role;
	}

	[Fact]
	public void Constructor_InitializesCorrectly()
	{
		// Act
		var role = new Captain();

		// Assert
		Assert.NotNull(role);
		Assert.Equal(ExtremeRoleId.Captain, role.Core.Id);
		Assert.Equal(RoleTypes.Crewmate, role.NoneAwakeRole);
		Assert.Equal((int)IRoleVoteModifier.ModOrder.CaptainSpecialVote, role.Order);
	}

	[Theory]
	[InlineData(true, false, true)]
	[InlineData(false, true, true)]
	[InlineData(false, false, false)]
	public void IsAwake_ReturnsExpectedValue(bool isLobby, bool awakeRoleState, bool expectedIsAwake)
	{
		// Arrange
		SetLobbyMode(isLobby);
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = awakeRoleState ? 0 : 7; // 0% -> awakeRole = true, 70% -> awakeRole = false
		}

		role.Initialize();

		// Act
		bool isAwake = role.IsAwake;

		// Assert
		Assert.Equal(expectedIsAwake, isAwake);
	}

	[Fact]
	public void RoleSpecificInit_WhenAwakeTaskGageIsZero_SetsAwakeRoleTrue()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 0; // 0%
		}

		// Act
		role.Initialize();

		// Assert
		Assert.True(role.IsAwake);
	}

	[Fact]
	public void RoleSpecificInit_WhenAwakeTaskGageIsGreaterThanZero_SetsAwakeRoleFalse()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		// Act
		role.Initialize();

		// Assert
		Assert.False(role.IsAwake);
	}

	[Fact]
	public void ChargeVote_IncreasesChargedVoteCount()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.ChargeVoteWhenSkip, out var chargeOpt) && chargeOpt != null)
		{
			chargeOpt.Selection = 9; // 1.0 vote
		}
		if (role.Loader.TryGet(Captain.CaptainOption.AwakedDefaultVoteNum, out var defaultOpt) && defaultOpt != null)
		{
			defaultOpt.Selection = 0; // 0.0 default
		}
		role.Initialize();

		// Act
		role.ChargeVote();

		// Assert
		var infoCollector = new VoteInfoCollector();
		var networkedPlayerMock = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		networkedPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)1);

		role.SetTargetVote(2);
		var moddedVotes = new List<VoteInfo>(role.GetModdedVoteInfo(infoCollector, networkedPlayerMock.Object));

		Assert.Single(moddedVotes);
		Assert.Equal((byte)2, moddedVotes[0].TargetId);
		Assert.Equal(1, moddedVotes[0].Count);
	}

	[Fact]
	public void SetTargetVote_UpdatesVoteTarget()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.AwakedDefaultVoteNum, out var defaultOpt) && defaultOpt != null)
		{
			defaultOpt.Selection = 10; // 1.0 default vote
		}
		role.Initialize();

		// Act
		role.SetTargetVote(3);

		// Assert
		var infoCollector = new VoteInfoCollector();
		var networkedPlayerMock = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		networkedPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)1);

		var moddedVotes = new List<VoteInfo>(role.GetModdedVoteInfo(infoCollector, networkedPlayerMock.Object));

		Assert.Single(moddedVotes);
		Assert.Equal((byte)3, moddedVotes[0].TargetId);
		Assert.Equal(1, moddedVotes[0].Count);
	}

	[Fact]
	public void ResetModifier_ResetsVoteTargetAndChargedVoteToDefault()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.ChargeVoteWhenSkip, out var chargeOpt) && chargeOpt != null)
		{
			chargeOpt.Selection = 9; // 1.0
		}
		if (role.Loader.TryGet(Captain.CaptainOption.AwakedDefaultVoteNum, out var defaultOpt) && defaultOpt != null)
		{
			defaultOpt.Selection = 0; // 0.0 default
		}
		role.Initialize();

		role.SetTargetVote(3);
		role.ChargeVote(); // curChargedVote = 1.0

		// Act
		role.ResetModifier();

		// Assert
		var infoCollector = new VoteInfoCollector();
		var networkedPlayerMock = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		networkedPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)1);

		var moddedVotes = new List<VoteInfo>(role.GetModdedVoteInfo(infoCollector, networkedPlayerMock.Object));
		Assert.Empty(moddedVotes);
	}

	[Fact]
	public void ModifiedVote_WhenVoteTargetNotSetAndSkipped_ChargesVote()
	{
		// Arrange
		byte playerId = 1;
		SetLobbyMode(false);
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.ChargeVoteWhenSkip, out var chargeOpt) && chargeOpt != null)
		{
			chargeOpt.Selection = 9; // 1.0 charge
		}
		InitializeCaptainRole(role, playerId);

		var voteTargets = new Dictionary<byte, byte>
		{
			{ playerId, PlayerVoteArea.SkippedVote }
		};
		var voteResults = new Dictionary<byte, int>();

		// Act
		role.ModifiedVote(playerId, ref voteTargets, ref voteResults);

		// Assert
		role.SetTargetVote(2);
		var infoCollector = new VoteInfoCollector();
		var networkedPlayerMock = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		networkedPlayerMock.SetupGet(p => p.PlayerId).Returns(playerId);

		var moddedVotes = new List<VoteInfo>(role.GetModdedVoteInfo(infoCollector, networkedPlayerMock.Object));

		Assert.Single(moddedVotes);
		Assert.Equal(1, moddedVotes[0].Count);
	}

	[Fact]
	public void ModifiedVote_WhenVoteTargetNotSetAndDeadVote_DoesNotChargeVote()
	{
		// Arrange
		byte playerId = 1;
		SetLobbyMode(false);
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.ChargeVoteWhenSkip, out var chargeOpt) && chargeOpt != null)
		{
			chargeOpt.Selection = 9; // 1.0 charge
		}
		if (role.Loader.TryGet(Captain.CaptainOption.AwakedDefaultVoteNum, out var defaultOpt) && defaultOpt != null)
		{
			defaultOpt.Selection = 0; // 0.0 default
		}
		InitializeCaptainRole(role, playerId);

		var voteTargets = new Dictionary<byte, byte>
		{
			{ playerId, PlayerVoteArea.DeadVote }
		};
		var voteResults = new Dictionary<byte, int>();

		// Act
		role.ModifiedVote(playerId, ref voteTargets, ref voteResults);

		// Assert
		role.SetTargetVote(2);
		var infoCollector = new VoteInfoCollector();
		var networkedPlayerMock = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		networkedPlayerMock.SetupGet(p => p.PlayerId).Returns(playerId);

		var moddedVotes = new List<VoteInfo>(role.GetModdedVoteInfo(infoCollector, networkedPlayerMock.Object));
		Assert.Empty(moddedVotes);
	}

	[Fact]
	public void ModifiedVote_WhenVoteTargetSet_AppliesVoteNumToResult()
	{
		// Arrange
		byte playerId = 1;
		byte targetId = 2;
		SetLobbyMode(false);
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.AwakedDefaultVoteNum, out var defaultOpt) && defaultOpt != null)
		{
			defaultOpt.Selection = 20; // 2.0 default votes
		}
		InitializeCaptainRole(role, playerId);

		role.SetTargetVote(targetId);

		var voteTargets = new Dictionary<byte, byte>
		{
			{ playerId, targetId }
		};
		var voteResults = new Dictionary<byte, int>
		{
			{ targetId, 1 }
		};

		// Act
		role.ModifiedVote(playerId, ref voteTargets, ref voteResults);

		// Assert
		Assert.Equal(3, voteResults[targetId]);
	}

	[Fact]
	public void ModifiedVote_WhenVoteTargetSetAndResultKeyDoesNotExist_AddsTargetToVoteResults()
	{
		// Arrange
		byte playerId = 1;
		byte targetId = 2;
		SetLobbyMode(false);
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.AwakedDefaultVoteNum, out var defaultOpt) && defaultOpt != null)
		{
			defaultOpt.Selection = 20; // 2.0 default votes
		}
		InitializeCaptainRole(role, playerId);

		role.SetTargetVote(targetId);

		var voteTargets = new Dictionary<byte, byte>
		{
			{ playerId, targetId }
		};
		var voteResults = new Dictionary<byte, int>();

		// Act
		role.ModifiedVote(playerId, ref voteTargets, ref voteResults);

		// Assert
		Assert.True(voteResults.ContainsKey(targetId));
		Assert.Equal(2, voteResults[targetId]);
	}

	[Fact]
	public void GetModdedVoteInfo_WhenVoteTargetNotSet_ReturnsEmpty()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Captain();
		InitializeCaptainRole(role, 1);

		var infoCollector = new VoteInfoCollector();
		var networkedPlayerMock = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		networkedPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)1);

		// Act
		var moddedVotes = new List<VoteInfo>(role.GetModdedVoteInfo(infoCollector, networkedPlayerMock.Object));

		// Assert
		Assert.Empty(moddedVotes);
	}

	[Fact]
	public void UseAbility_SetVoteTarget_InvokesSetTargetVoteOnCaptain()
	{
		// Arrange
		byte playerId = 1;
		byte targetId = 3;
		SetLobbyMode(false);
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.AwakedDefaultVoteNum, out var defaultOpt) && defaultOpt != null)
		{
			defaultOpt.Selection = 10; // 1.0
		}
		InitializeCaptainRole(role, playerId);

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)Captain.AbilityType.SetVoteTarget)
			.Returns(playerId)
			.Returns(targetId);

		var readerObj = mockReader.Object;

		// Act
		Captain.UseAbility(ref readerObj);

		// Assert
		var infoCollector = new VoteInfoCollector();
		var networkedPlayerMock = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		networkedPlayerMock.SetupGet(p => p.PlayerId).Returns(playerId);

		var moddedVotes = new List<VoteInfo>(role.GetModdedVoteInfo(infoCollector, networkedPlayerMock.Object));

		Assert.Single(moddedVotes);
		Assert.Equal(targetId, moddedVotes[0].TargetId);
	}

	[Fact]
	public void UseAbility_ChargeVote_InvokesChargeVoteOnCaptain()
	{
		// Arrange
		byte playerId = 1;
		SetLobbyMode(false);
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.ChargeVoteWhenSkip, out var chargeOpt) && chargeOpt != null)
		{
			chargeOpt.Selection = 9; // 1.0 charge
		}
		InitializeCaptainRole(role, playerId);

		role.SetTargetVote(2);

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)Captain.AbilityType.ChargeVote)
			.Returns(playerId);

		var readerObj = mockReader.Object;

		// Act
		Captain.UseAbility(ref readerObj);

		// Assert
		var infoCollector = new VoteInfoCollector();
		var networkedPlayerMock = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		networkedPlayerMock.SetupGet(p => p.PlayerId).Returns(playerId);

		var moddedVotes = new List<VoteInfo>(role.GetModdedVoteInfo(infoCollector, networkedPlayerMock.Object));

		Assert.Single(moddedVotes);
		Assert.Equal(1, moddedVotes[0].Count);
	}

	[Fact]
	public void UseAbility_WithInvalidRolePlayer_DoesNotThrow()
	{
		// Arrange
		byte invalidPlayerId = 99;
		SetLobbyMode(false);

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)Captain.AbilityType.ChargeVote)
			.Returns(invalidPlayerId);

		var readerObj = mockReader.Object;

		// Act & Assert
		Captain.UseAbility(ref readerObj);
	}

	[Fact]
	public void Update_WhenTaskGageReached_AwakesRole()
	{
		// Arrange
		SetLobbyMode(false);

		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 5; // 50%
		}

		role.Initialize();

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);

		var task1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		task1.SetupGet(t => t.Complete).Returns(true);

		mockTasksList.SetupGet(l => l.Count).Returns(1);
		mockTasksList.Setup(l => l[0]).Returns(task1.Object);

		mockPlayerInfo.SetupGet(p => p.Tasks).Returns(mockTasksList.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockPlayerInfo.Object);

		// Act
		role.Update(mockPlayer.Object);

		// Assert
		Assert.True(role.IsAwake);
	}

	[Theory]
	[InlineData(0.0f, true)] // Cannot use special vote if vote < 1.0
	[InlineData(1.0f, false)] // Can use special vote if awake and vote >= 1.0
	public void IsBlockMeetingButtonAbility_ReturnsExpected(float defaultVote, bool expectedBlocked)
	{
		// Arrange
		SetLobbyMode(true); // Lobby mode so IsAwake is true
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.AwakedDefaultVoteNum, out var defaultOpt) && defaultOpt != null)
		{
			defaultOpt.Selection = (int)(defaultVote * 10);
		}

		role.Initialize();

		var mockPlayerArea = new Mock<PlayerVoteArea>(IntPtr.Zero);

		// Act
		bool isBlocked = role.IsBlockMeetingButtonAbility(mockPlayerArea.Object);

		// Assert
		Assert.Equal(expectedBlocked, isBlocked);
	}

	[Fact]
	public void GetFakeOptionString_ReturnsEmptyString()
	{
		// Arrange
		var role = new Captain();

		// Act
		string result = role.GetFakeOptionString();

		// Assert
		Assert.Equal("", result);
	}

	[Theory]
	[InlineData(true, true)]
	[InlineData(true, false)]
	[InlineData(false, true)]
	public void GetColoredRoleName_WhenAwakeOrTruthColor_ReturnsColoredRoleName(bool isTruthColor, bool isAwake)
	{
		// Arrange
		SetLobbyMode(isAwake);
		var role = new Captain();

		// Act
		string roleName = role.GetColoredRoleName(isTruthColor);

		// Assert
		Assert.NotNull(roleName);
		Assert.Contains("Captain", roleName);
	}

	[Fact]
	public void GetColoredRoleName_WhenNotAwakeAndNotTruthColor_ReturnsWhiteCrewmateName()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		role.Initialize();

		// Act
		string roleName = role.GetColoredRoleName(false);

		// Assert
		Assert.NotNull(roleName);
		Assert.Contains("Crewmate", roleName);
	}

	[Fact]
	public void GetFullDescription_WhenAwake_ReturnsCaptainFullDescription()
	{
		// Arrange
		SetLobbyMode(true);
		var role = new Captain();

		// Act
		string description = role.GetFullDescription();

		// Assert
		Assert.NotNull(description);
		Assert.Equal("CaptainFullDescription", description);
	}

	[Fact]
	public void GetFullDescription_WhenNotAwake_ReturnsCrewmateFullDescription()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		role.Initialize();

		// Act
		string description = role.GetFullDescription();

		// Assert
		Assert.NotNull(description);
		Assert.Equal("CrewmateFullDescription", description);
	}

	[Fact]
	public void GetImportantText_WhenAwake_ReturnsBaseImportantText()
	{
		// Arrange
		SetLobbyMode(true);
		var role = new Captain();

		// Act
		string text = role.GetImportantText(true);

		// Assert
		Assert.NotNull(text);
		Assert.Contains("Captain", text);
	}

	[Fact]
	public void GetImportantText_WhenNotAwake_ReturnsCrewmateImportantText()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		role.Initialize();

		// Act
		string text = role.GetImportantText(true);

		// Assert
		Assert.NotNull(text);
		Assert.Contains("crewImportantText", text);
	}

	[Fact]
	public void GetIntroDescription_WhenAwake_ReturnsBaseIntroDescription()
	{
		// Arrange
		SetLobbyMode(true);
		var role = new Captain();

		// Act
		string desc = role.GetIntroDescription();

		// Assert
		Assert.NotNull(desc);
		Assert.Contains("CaptainIntroDescription", desc);
	}

	[Fact]
	public void GetIntroDescription_WhenNotAwake_ReturnsCrewmateIntroDescription()
	{
		// Arrange
		SetLobbyMode(false);
		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();

		var mockRole = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRole.SetupGet(r => r.Blurb).Returns("Crewmate Intro Blurb");

		var mockInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockInfo.SetupGet(i => i.Role).Returns(mockRole.Object);
		localPlayerMock.SetupGet(p => p.Data).Returns(mockInfo.Object);

		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		role.Initialize();

		// Act
		string desc = role.GetIntroDescription();

		// Assert
		Assert.NotNull(desc);
		Assert.Contains("Crewmate Intro Blurb", desc);
	}

	[Theory]
	[InlineData(true, true)]
	[InlineData(true, false)]
	[InlineData(false, true)]
	public void GetNameColor_WhenAwakeOrTruthColor_ReturnsRoleColor(bool isTruthColor, bool isAwake)
	{
		// Arrange
		SetLobbyMode(isAwake);
		var role = new Captain();

		// Act
		Color color = role.GetNameColor(isTruthColor);

		// Assert
		Assert.Equal(ColorPalette.CaptainLightKonjou, color);
	}

	[Fact]
	public void GetNameColor_WhenNotAwakeAndNotTruthColor_ReturnsWhite()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Captain();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Captain.CaptainOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		role.Initialize();

		// Act
		Color color = role.GetNameColor(false);

		// Assert
		Assert.Equal(Palette.White, color);
	}

	[Fact]
	public void CreateRoleAllOption_CreatesExpectedOptions()
	{
		// Arrange
		int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Captain);
		var role = new Captain();

		// Act
		role.CreateRoleAllOption();

		// Assert
		Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
		Assert.NotNull(category);
	}
}
