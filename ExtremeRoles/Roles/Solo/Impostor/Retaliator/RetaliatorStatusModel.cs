using System.Collections.Generic;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor.Retaliator;

public enum RetaliatorTargetTeam
{
    CrewmateOnly,
    NeutralOnly,
    BothCrewAndNeutral,
}

public class RetaliatorStatusModel : IStatusModel
{
    public bool IsAwake { get; private set; }
    public int KillCount { get; private set; }
    public int AwakeKillCount { get; }
    public int RemainingAbilityUses { get; private set; }
    public int MaxAbilityUses { get; }
    public int MaxTargetPerMeeting { get; }
    public int CurrentMeetingTargetCount { get; private set; }
    public RetaliatorTargetTeam TargetTeam { get; }
    public bool NotifyTargetNextMeeting { get; }

    private readonly HashSet<byte> targetedPlayerIds = new HashSet<byte>();
    public IReadOnlyCollection<byte> TargetedPlayerIds => this.targetedPlayerIds;

    public RetaliatorStatusModel(
        int awakeKillCount,
        int maxAbilityUses,
        int maxTargetPerMeeting,
        RetaliatorTargetTeam targetTeam,
        bool notifyTargetNextMeeting)
    {
        this.AwakeKillCount = awakeKillCount;
        this.MaxAbilityUses = maxAbilityUses;
        this.RemainingAbilityUses = maxAbilityUses;
        this.MaxTargetPerMeeting = maxTargetPerMeeting;
        this.TargetTeam = targetTeam;
        this.NotifyTargetNextMeeting = notifyTargetNextMeeting;
        this.IsAwake = awakeKillCount <= 0;
        this.KillCount = 0;
        this.CurrentMeetingTargetCount = 0;
    }

    public void TargetPlayer(byte playerId)
    {
        if (this.targetedPlayerIds.Add(playerId))
        {
            this.RemainingAbilityUses--;
            this.CurrentMeetingTargetCount++;
        }
    }

    public bool IsTargeted(byte playerId) => this.targetedPlayerIds.Contains(playerId);

    public void IncrementKill()
    {
        if (this.IsAwake)
        {
            return;
        }

        this.KillCount++;
        if (this.KillCount >= this.AwakeKillCount)
        {
            this.IsAwake = true;
        }
    }

    public void OnMeetingStart()
    {
        this.CurrentMeetingTargetCount = 0;
    }

    public void ClearTargetedPlayers()
    {
        this.targetedPlayerIds.Clear();
    }
}
