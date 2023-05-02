using System;
using System.Collections.Generic;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Gambler : 
    SingleRoleBase, 
    IRoleVoteModifier
{
    public enum GamblerOption
    {
        NormalVoteRate
    }

    public int Order => (int)IRoleVoteModifier.ModOrder.GamblerAddVote;
    private int normalVoteRate;

    public Gambler() : base(
        ExtremeRoleId.Gambler,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Gambler.ToString(),
        ColorPalette.GamblerYellowGold,
        false, true, false, false)
    { }

    public void ModifiedVote(
        byte rolePlayerId,
        ref Dictionary<byte, byte> voteTarget,
        ref Dictionary<byte, int> voteResult)
    {
        if (!voteTarget.TryGetValue(rolePlayerId, out byte voteTo) ||
            !voteResult.TryGetValue(voteTo, out int curVoteNum)) { return; }

        int[] voteArray = new int[100];
        int dualVoteRate = (int)Math.Floor((100 - this.normalVoteRate) / 2.0d);
        int zeroVoteRate = 100 - dualVoteRate - this.normalVoteRate;

        Array.Fill(voteArray, 1, 0, this.normalVoteRate);
        Array.Fill(voteArray, 0, this.normalVoteRate, zeroVoteRate);
        Array.Fill(voteArray, 2, this.normalVoteRate + zeroVoteRate, dualVoteRate);

        int playerVoteNum = voteArray[RandomGenerator.Instance.Next(100)];
        
        if (playerVoteNum == 1) { return; }
     
        int newVotedNum = playerVoteNum == 0 ? curVoteNum - 1 : curVoteNum + 1;
        voteResult[voteTo] = newVotedNum;
    }

    public void ModifiedVoteAnime(
        MeetingHud instance,
        GameData.PlayerInfo rolePlayer,
        ref Dictionary<byte, int> voteIndex)
    { }

    public void ResetModifier()
    { }

    protected override void CreateSpecificOption(IOption parentOps)
    {
        CreateIntOption(
            GamblerOption.NormalVoteRate,
            50, 0, 90, 5, parentOps,
            format: OptionUnit.Percentage);
    }

    protected override void RoleSpecificInit()
    {
        this.normalVoteRate = OptionHolder.AllOption[
            GetRoleOptionId(GamblerOption.NormalVoteRate)].GetValue();
    }
}
