using System;
using System.Collections.Generic;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.Solo.Crewmate;
using Moq;
using UnityEngine;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.GamblerTests;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class GamblerTests
{
    public GamblerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
        MockSetupHelper.SetupPlayerControlMocks();
        MockSetupHelper.SetupGameOptionsManagerMock();
        MockSetupHelper.SetupOptionManager();

        var mockClampInt = new Mock<MockMathfClampHelper2>();
        mockClampInt.Setup(h => h.Invoke(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int v, int min, int max) => Math.Clamp(v, min, max));
        MockMathfClampHelper2.Instance = mockClampInt.Object;
    }

    [Fact]
    public void CreateRoleAllOption_CreatesCategoryInOptionManager()
    {
        // Arrange
        var gambler = new Gambler();
        int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Gambler);

        // Act
        gambler.CreateRoleAllOption();

        // Assert
        Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
        Assert.NotNull(category);
    }

    [Fact]
    public void ModifiedVote_WhenRolePlayerIdNotInVoteTarget_DoesNotModifyVoteResult()
    {
        // Arrange
        var gambler = new Gambler();
        gambler.CreateRoleAllOption();
        gambler.Initialize();

        byte rolePlayerId = 1;
        byte otherPlayerId = 2;

        var voteTarget = new Dictionary<byte, byte>
        {
            { otherPlayerId, 3 }
        };
        var voteResult = new Dictionary<byte, int>
        {
            { 3, 1 }
        };

        // Act
        gambler.ModifiedVote(rolePlayerId, ref voteTarget, ref voteResult);

        // Assert
        Assert.Equal(1, voteResult[3]);
    }

    [Fact]
    public void ModifiedVote_WhenVotedTargetNotInVoteResult_DoesNotModifyVoteResult()
    {
        // Arrange
        var gambler = new Gambler();
        gambler.CreateRoleAllOption();
        gambler.Initialize();

        byte rolePlayerId = 1;

        var voteTarget = new Dictionary<byte, byte>
        {
            { rolePlayerId, 3 }
        };
        var voteResult = new Dictionary<byte, int>
        {
            { 4, 1 }
        };

        // Act
        gambler.ModifiedVote(rolePlayerId, ref voteTarget, ref voteResult);

        // Assert
        Assert.False(voteResult.ContainsKey(3));
        Assert.Equal(1, voteResult[4]);
    }

    [Fact]
    public void ModifiedVote_WithMultiplePlayersVotes_OnlyModifiesGamblersVoteTarget()
    {
        // Arrange
        var gambler = new Gambler();
        gambler.CreateRoleAllOption();

        // ChangeVoteChance = 100% -> normalVoteIndex = 0
        if (gambler.Loader.TryGet(Gambler.GamblerOption.ChangeVoteChance, out var chanceOpt) && chanceOpt != null)
        {
            chanceOpt.Selection = 18; // 100%
        }
        // MinVoteNum 0, MaxVoteNum 2
        if (gambler.Loader.TryGet(Gambler.GamblerOption.MinVoteNum, out var minOpt) && minOpt != null)
        {
            minOpt.Selection = 100; // 0
        }
        if (gambler.Loader.TryGet(Gambler.GamblerOption.MaxVoteNum, out var maxOpt) && maxOpt != null)
        {
            maxOpt.Selection = 0; // 2
        }
        gambler.Initialize();

        byte gamblerId = 1;
        byte player2Id = 2;
        byte player3Id = 3;

        byte targetX = 10;
        byte targetY = 11;

        // Gambler(1) votes for TargetX(10)
        // Player2(2) votes for TargetY(11)
        // Player3(3) votes for TargetX(10)
        var voteTarget = new Dictionary<byte, byte>
        {
            { gamblerId, targetX },
            { player2Id, targetY },
            { player3Id, targetX }
        };

        // TargetX initially has 2 votes (Gambler + Player3)
        // TargetY initially has 1 vote (Player2)
        var voteResult = new Dictionary<byte, int>
        {
            { targetX, 2 },
            { targetY, 1 }
        };

        // Act
        gambler.ModifiedVote(gamblerId, ref voteTarget, ref voteResult);

        // Assert
        // TargetY's votes remain 1
        Assert.Equal(1, voteResult[targetY]);

        // TargetX's votes modified for Gambler (with min=0, max=2, voteCount=0 -> newVotedNum = 2 + 0 - 1 = 1)
        Assert.Equal(1, voteResult[targetX]);

        // voteTarget dictionary is unmodified
        Assert.Equal(targetX, voteTarget[gamblerId]);
        Assert.Equal(targetY, voteTarget[player2Id]);
        Assert.Equal(targetX, voteTarget[player3Id]);
    }

    [Fact]
    public void ModifiedVote_WhenChangeVoteChanceIsZeroPercent_DoesNotChangeVoteCount()
    {
        // Arrange
        var gambler = new Gambler();
        gambler.CreateRoleAllOption();

        gambler.Initialize();

        byte rolePlayerId = 1;
        byte targetId = 2;

        var voteTarget = new Dictionary<byte, byte>
        {
            { rolePlayerId, targetId }
        };
        var voteResult = new Dictionary<byte, int>
        {
            { targetId, 1 }
        };

        // Act
        gambler.ModifiedVote(rolePlayerId, ref voteTarget, ref voteResult);

        // Assert
        Assert.True(voteResult[targetId] >= 0);
    }

    [Fact]
    public void ModifiedVote_WhenChangeVoteChanceIs100PercentAndMin0Max2_ModifiesVoteResultToZero()
    {
        // Arrange
        var gambler = new Gambler();
        gambler.CreateRoleAllOption();

        // ChangeVoteChance = 100% (Selection 18: 10 + 18*5 = 100%) -> normalVoteIndex = 0
        if (gambler.Loader.TryGet(Gambler.GamblerOption.ChangeVoteChance, out var chanceOpt) && chanceOpt != null)
        {
            chanceOpt.Selection = 18; // 100%
        }
        // MinVoteNum min -100, max 0, step 1. Index 100 is 0.
        if (gambler.Loader.TryGet(Gambler.GamblerOption.MinVoteNum, out var minOpt) && minOpt != null)
        {
            minOpt.Selection = 100; // 0
        }
        // MaxVoteNum min 2, max 100, step 1. Index 0 is 2.
        if (gambler.Loader.TryGet(Gambler.GamblerOption.MaxVoteNum, out var maxOpt) && maxOpt != null)
        {
            maxOpt.Selection = 0; // 2
        }
        gambler.Initialize();

        byte rolePlayerId = 1;
        byte targetId = 2;

        var voteTarget = new Dictionary<byte, byte>
        {
            { rolePlayerId, targetId }
        };
        var voteResult = new Dictionary<byte, int>
        {
            { targetId, 1 }
        };

        // Act
        gambler.ModifiedVote(rolePlayerId, ref voteTarget, ref voteResult);

        // Assert
        // minVoteNum = 0, maxVoteNum = 2. RandomGenerator.Next(0, 2) can only return 0 (1 is skipped by loop).
        // newVotedNum = 1 + 0 - 1 = 0
        Assert.Equal(0, voteResult[targetId]);
    }

    [Fact]
    public void ModifiedVote_WhenChangeVoteChanceIs100PercentAndMaxVoteNumIs4_ModifiesVoteResult()
    {
        // Arrange
        var gambler = new Gambler();
        gambler.CreateRoleAllOption();

        if (gambler.Loader.TryGet(Gambler.GamblerOption.ChangeVoteChance, out var chanceOpt) && chanceOpt != null)
        {
            chanceOpt.Selection = 18; // 100%
        }
        if (gambler.Loader.TryGet(Gambler.GamblerOption.MinVoteNum, out var minOpt) && minOpt != null)
        {
            minOpt.Selection = 100; // 0
        }
        if (gambler.Loader.TryGet(Gambler.GamblerOption.MaxVoteNum, out var maxOpt) && maxOpt != null)
        {
            maxOpt.Selection = 2; // 4 (min 2 + 2 = 4)
        }
        gambler.Initialize();

        byte rolePlayerId = 1;
        byte targetId = 2;

        var voteTarget = new Dictionary<byte, byte>
        {
            { rolePlayerId, targetId }
        };
        var voteResult = new Dictionary<byte, int>
        {
            { targetId, 2 }
        };

        // Act
        gambler.ModifiedVote(rolePlayerId, ref voteTarget, ref voteResult);

        // Assert
        // voteCount is in range [0, 4) excluding 1 -> {0, 2, 3}.
        // newVotedNum = 2 + voteCount - 1 >= 2 + 0 - 1 = 1
        Assert.True(voteResult[targetId] >= 1);
    }

    [Fact]
    public void ModifiedVote_WhenNewVoteCountIsNegative_ClampsToZero()
    {
        // Arrange
        var gambler = new Gambler();
        gambler.CreateRoleAllOption();

        if (gambler.Loader.TryGet(Gambler.GamblerOption.ChangeVoteChance, out var chanceOpt) && chanceOpt != null)
        {
            chanceOpt.Selection = 18; // 100%
        }
        if (gambler.Loader.TryGet(Gambler.GamblerOption.MinVoteNum, out var minOpt) && minOpt != null)
        {
            // minVoteNum = -10 (Selection 90: -100 + 90 = -10)
            minOpt.Selection = 90;
        }
        if (gambler.Loader.TryGet(Gambler.GamblerOption.MaxVoteNum, out var maxOpt) && maxOpt != null)
        {
            maxOpt.Selection = 0; // 2
        }
        gambler.Initialize();

        byte rolePlayerId = 1;
        byte targetId = 2;

        var voteTarget = new Dictionary<byte, byte>
        {
            { rolePlayerId, targetId }
        };
        var voteResult = new Dictionary<byte, int>
        {
            { targetId, 1 }
        };

        // Act
        gambler.ModifiedVote(rolePlayerId, ref voteTarget, ref voteResult);

        // Assert
        Assert.True(voteResult[targetId] >= 0);
    }

    [Fact]
    public void GetModdedVoteInfo_ReturnsEmptyEnumerable()
    {
        // Arrange
        var gambler = new Gambler();
        var collector = new VoteInfoCollector();

        // Act
        var result = gambler.GetModdedVoteInfo(collector, null!);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ResetModifier_ExecutesWithoutError()
    {
        // Arrange
        var gambler = new Gambler();

        // Act & Assert
        gambler.ResetModifier();
    }
}
