using System;
using System.Collections.Generic;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Meeting;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;


using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Gambler :
    SingleRoleBase,
    IRoleVoteModifier
{
    public enum GamblerOption
    {
        NormalVoteRate,
        MaxVoteNum,
        MinVoteNum
    }

    public int Order => (int)IRoleVoteModifier.ModOrder.GamblerAddVote;
    private int normalVoteRate;
    private int minVoteNum;
    private int maxVoteNum;

    private byte _votedFor = 255;
    private int _voteCount = 1;

    public Gambler() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Gambler,
			ColorPalette.GamblerYellowGold),
        false, true, false, false)
    { }

    public void ModifiedVote(
        byte rolePlayerId,
        ref Dictionary<byte, byte> voteTarget,
        ref Dictionary<byte, int> voteResult)
    {
        if (!voteTarget.TryGetValue(rolePlayerId, out _votedFor) ||
            !voteResult.TryGetValue(_votedFor, out int curVoteNum))
        {
            return;
        }

        int[] voteArray = new int[100];
        int dualVoteRate = (int)Math.Floor((100 - this.normalVoteRate) / 2.0d);
        int zeroVoteRate = 100 - dualVoteRate - this.normalVoteRate;

        Array.Fill(voteArray, 1, 0, this.normalVoteRate);
        Array.Fill(voteArray, this.minVoteNum, this.normalVoteRate, zeroVoteRate);
        Array.Fill(voteArray, this.maxVoteNum, this.normalVoteRate + zeroVoteRate, dualVoteRate);

        _voteCount = voteArray[RandomGenerator.Instance.Next(100)];

        if (_voteCount == 1)
        {
            return;
        }

        int newVotedNum = curVoteNum + _voteCount - 1;
        voteResult[_votedFor] = UnityEngine.Mathf.Clamp(newVotedNum, 0, int.MaxValue);
    }

    public IEnumerable<VoteInfo> GetVoteModifications(NetworkedPlayerInfo rolePlayer)
    {
        if (_voteCount != 1 && _votedFor != 255)
        {
            yield return new VoteInfo(rolePlayer.PlayerId, _votedFor, _voteCount - 1);
        }
    }

    public void ResetModifier()
    {
        _votedFor = 255;
        _voteCount = 1;
    }

    protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateIntOption(
            GamblerOption.NormalVoteRate,
            50, 0, 90, 5,
            format: OptionUnit.Percentage);
		factory.CreateIntOption(
            GamblerOption.MinVoteNum,
            0, -100, 0, 1,
            format: OptionUnit.Percentage);
		factory.CreateIntOption(
            GamblerOption.MaxVoteNum,
            2, 2, 100, 1,
            format: OptionUnit.Percentage);
    }

    protected override void RoleSpecificInit()
    {
        this.normalVoteRate = this.Loader.GetValue<GamblerOption, int>(GamblerOption.NormalVoteRate);
        this.minVoteNum = this.Loader.GetValue<GamblerOption, int>(GamblerOption.MinVoteNum);
        this.maxVoteNum = this.Loader.GetValue<GamblerOption, int>(GamblerOption.MaxVoteNum);
    }
}
