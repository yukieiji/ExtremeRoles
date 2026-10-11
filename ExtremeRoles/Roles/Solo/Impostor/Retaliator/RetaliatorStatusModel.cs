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
    public bool IsAwake { get; set; }
    public int KillCount { get; set; }
    public int AwakeKillCount { get; set; }
    public int RemainingAbilityUses { get; set; }
    public int MaxAbilityUses { get; set; }
    public int MaxTargetPerMeeting { get; set; }
    public int CurrentMeetingTargetCount { get; set; }
    public RetaliatorTargetTeam TargetTeam { get; set; }
    public bool NotifyTargetNextMeeting { get; set; }

    public HashSet<byte> TargetedPlayerIds { get; } = new HashSet<byte>();
    public HashSet<byte> NotifiedTargetPlayerIds { get; } = new HashSet<byte>();

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
        if (this.TargetedPlayerIds.Add(playerId))
        {
            this.RemainingAbilityUses--;
            this.CurrentMeetingTargetCount++;
        }
    }

    public bool IsTargeted(byte playerId) => this.TargetedPlayerIds.Contains(playerId);

    public void OnMeetingStart()
    {
        this.CurrentMeetingTargetCount = 0;
        this.NotifiedTargetPlayerIds.Clear();
        foreach (byte id in this.TargetedPlayerIds)
        {
            this.NotifiedTargetPlayerIds.Add(id);
        }
        this.TargetedPlayerIds.Clear();
    }
}
