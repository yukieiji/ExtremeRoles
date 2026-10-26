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

public class RetaliatorStatusModel(
	int awakeKillCount,
	int maxAbilityUses,
	int maxTargetPerMeeting,
	RetaliatorTargetTeam targetTeam,
	bool notifyTargetNextMeeting) : IStatusModel
{
	public bool IsAwake { get; private set; } = awakeKillCount <= 0;
	public bool CanUseAbility =>
		IsAwake &&
		RemainingAbilityUses > 0 &&
		CurrentMeetingTargetCount > 0;

	public int AwakeKillCount { get; private set; } = awakeKillCount;
	public int RemainingAbilityUses { get; private set; } = maxAbilityUses;
	public int CurrentMeetingTargetCount { get; private set; } = maxTargetPerMeeting;
	public RetaliatorTargetTeam TargetTeam { get; } = targetTeam;
	public bool NotifyTargetNextMeeting { get; } = notifyTargetNextMeeting;

	private readonly int maxTargetPerMeeting = maxTargetPerMeeting;

	private readonly HashSet<byte> targetedPlayerIds = new HashSet<byte>();
    public IReadOnlyCollection<byte> TargetedPlayerIds => this.targetedPlayerIds;

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
