using System;
using System.Collections.Generic;

using ExtremeRoles.Helper;
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
        ChangeVoteChance,
        MaxVoteNum,
        MinVoteNum
    }

    public int Order => (int)IRoleVoteModifier.ModOrder.GamblerAddVote;
    
	private int normalVoteIndex;

	private int minVoteNum;
	private int maxVoteNum;

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
        if (!(
				voteTarget.TryGetValue(rolePlayerId, out byte votedFor) && 
				voteResult.TryGetValue(votedFor, out int curVoteNum)
			))
        {
            return;
        }

		int index = RandomGenerator.Instance.Next(1, 101);
		int voteCount = 1;
		if (index <= this.normalVoteIndex)
		{
			voteCount = 1;
		}
		else
		{
			do
			{
				voteCount = RandomGenerator.Instance.Next(this.minVoteNum, this.maxVoteNum);
			}
			while (voteCount == 1);
		}

		if (voteCount == 1)
        {
            return;
        }

		// 自分の票数がもともと入っていて票数=curVoteNumなので、入っていた票数を消して足す
		int newVotedNum = curVoteNum + voteCount - 1;
		Logging.Debug($"New vote num : {newVotedNum}");
		voteResult[votedFor] = UnityEngine.Mathf.Clamp(newVotedNum, 0, int.MaxValue);
    }

    public IEnumerable<VoteInfo> GetModdedVoteInfo(NetworkedPlayerInfo rolePlayer)
    {
		// Gamblerは見た目は変更しないのでそのままにする
		yield break;
    }

    public void ResetModifier()
    {

    }

    protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateIntOption(
            GamblerOption.ChangeVoteChance,
            50, 10, 100, 5,
            format: OptionUnit.Percentage);
		factory.CreateIntOption(
            GamblerOption.MinVoteNum,
            0, -100, 0, 1,
            format: OptionUnit.VoteNum);
		factory.CreateIntOption(
            GamblerOption.MaxVoteNum,
            2, 2, 100, 1,
            format: OptionUnit.VoteNum);
    }

    protected override void RoleSpecificInit()
    {
        int changeIndex = this.Loader.GetValue<GamblerOption, int>(GamblerOption.ChangeVoteChance);

		this.normalVoteIndex = 100 - changeIndex;

        this.minVoteNum = this.Loader.GetValue<GamblerOption, int>(GamblerOption.MinVoteNum);
        this.maxVoteNum = this.Loader.GetValue<GamblerOption, int>(GamblerOption.MaxVoteNum);
    }
}
