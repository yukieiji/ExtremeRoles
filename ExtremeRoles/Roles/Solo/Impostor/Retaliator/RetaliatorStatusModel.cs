using System.Collections.Generic;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor.Retaliator;

public enum RetaliatorTargetTeam
{
	AllImpostorOther,
	CrewmateOnly,
    NeutralOnly,
	LiberalOnly,
    BothCrewAndNeutral,
	BothCrewAndLiberal,
	BothLiberalAndNeutral,
}

public class RetaliatorStatusModel : IStatusModel
{
    public bool IsAwake { get; private set; }
	public bool CanUseAbility =>
		IsAwake &&
		RemainingAbilityUses > 0 &&
		CurrentMeetingTargetCount > 0;

    public int AwakeKillCount { get; private set; }
    public int RemainingAbilityUses { get; private set; }
    public int CurrentMeetingTargetCount { get; private set; }
    public RetaliatorTargetTeam TargetTeam { get; }
    public bool NotifyTargetNextMeeting { get; }

	private readonly int maxTargetPerMeeting;

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
        this.RemainingAbilityUses = maxAbilityUses;
        this.maxTargetPerMeeting = maxTargetPerMeeting;
        this.TargetTeam = targetTeam;
        this.NotifyTargetNextMeeting = notifyTargetNextMeeting;
        this.IsAwake = awakeKillCount <= 0;
        this.CurrentMeetingTargetCount = this.maxTargetPerMeeting;
    }

    public void TargetPlayer(byte playerId)
    {
        if (this.targetedPlayerIds.Add(playerId))
        {
            this.RemainingAbilityUses--;
            this.CurrentMeetingTargetCount--;
        }
    }

    public bool IsTargeted(byte playerId) => this.targetedPlayerIds.Contains(playerId);

    public void IncrementKill()
    {
        if (this.IsAwake)
        {
            return;
        }

		this.AwakeKillCount--;
        if (this.AwakeKillCount <= 0)
        {
            this.IsAwake = true;
        }
    }

    public void OnMeetingStart()
    {
        this.CurrentMeetingTargetCount = this.maxTargetPerMeeting;
		this.targetedPlayerIds.Clear();
	}
}
