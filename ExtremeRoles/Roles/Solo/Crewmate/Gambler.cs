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
        NormalVoteRate,
        MaxVoteNum,
        MinVoteNum
    }

    public int Order => (int)IRoleVoteModifier.ModOrder.GamblerAddVote;
    
	private int normalVoteIndex;
	private int minVoteIndex;

	private int minVoteNum;
    private int maxVoteNum;

    private byte votedFor = PlayerVoteArea.HasNotVoted;
    private int voteCount = 1;

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
        if (!voteTarget.TryGetValue(rolePlayerId, out votedFor) ||
            !voteResult.TryGetValue(votedFor, out int curVoteNum))
        {
            return;
        }

		int index = RandomGenerator.Instance.Next(1, 101);

		if (index <= this.normalVoteIndex)
		{
			this.voteCount = 1;
		}
		else if (index <= this.minVoteIndex)
		{
			this.voteCount = this.minVoteNum;
		}
		else
		{
			this.voteCount = this.maxVoteNum;
		}

        if (this.voteCount == 1)
        {
            return;
        }

        int newVotedNum = curVoteNum + voteCount - 1;
		Logging.Debug($"New vote num : {newVotedNum}");
		voteResult[this.votedFor] = UnityEngine.Mathf.Clamp(newVotedNum, 0, int.MaxValue);
    }

    public IEnumerable<VoteInfo> GetModdedVoteInfo(NetworkedPlayerInfo rolePlayer)
    {
		// Gamblerは見た目は変更しないのでそのままにする
		yield break;
    }

    public void ResetModifier()
    {
        votedFor = PlayerVoteArea.HasNotVoted;
        voteCount = 1;
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
        this.normalVoteIndex = this.Loader.GetValue<GamblerOption, int>(GamblerOption.NormalVoteRate);
		
		int changeIndex = (100 - this.normalVoteIndex) / 2;
		this.minVoteIndex = this.normalVoteIndex + changeIndex;

        this.minVoteNum = this.Loader.GetValue<GamblerOption, int>(GamblerOption.MinVoteNum);
        this.maxVoteNum = this.Loader.GetValue<GamblerOption, int>(GamblerOption.MaxVoteNum);
    }
}
