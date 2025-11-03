using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.OLDS;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub;

public enum MeetingOption : int
{
	UseRaiseHand,
	NumMeating,
	ChangeMeetingVoteAreaSort,
	FixedMeetingPlayerLevel,
	DisableSkipInEmergencyMeeting,
	DisableSelfVote,
}

public readonly struct MeetingHudOption
{
	public readonly int MaxMeetingCount;
	public readonly bool UseRaiseHand;
	public readonly bool IsChangeVoteAreaButtonSortArg;
	public readonly bool IsFixedVoteAreaPlayerLevel;
	public readonly bool IsBlockSkipInMeeting;
	public readonly bool DisableSelfVote;

	public MeetingHudOption()
	{
		this.MaxMeetingCount = 0;
		this.UseRaiseHand = false;
	}
	public MeetingHudOption(in OptionCategory category)
	{
		this.MaxMeetingCount = category.GetValue<int>((int)MeetingOption.NumMeating);
		this.UseRaiseHand = category.GetValue<bool>((int)MeetingOption.UseRaiseHand);
		this.IsChangeVoteAreaButtonSortArg = category.GetValue<bool>((int)MeetingOption.ChangeMeetingVoteAreaSort);
		this.IsFixedVoteAreaPlayerLevel = category.GetValue<bool>((int)MeetingOption.FixedMeetingPlayerLevel);
		this.IsBlockSkipInMeeting = category.GetValue<bool>((int)MeetingOption.DisableSkipInEmergencyMeeting);
		this.DisableSelfVote = category.GetValue<bool>((int)MeetingOption.DisableSelfVote);
	}

	public static void Create(in OptionCategoryFactory factory)
	{
		factory.CreateBoolOption(MeetingOption.UseRaiseHand, false);
		factory.CreateIntOption(MeetingOption.NumMeating, 10, 0, 100, 1);
		factory.CreateBoolOption(MeetingOption.ChangeMeetingVoteAreaSort, false);
		factory.CreateBoolOption(MeetingOption.FixedMeetingPlayerLevel, false);
		factory.CreateBoolOption(MeetingOption.DisableSkipInEmergencyMeeting, false);
		factory.CreateBoolOption(MeetingOption.DisableSelfVote, false);
	}
}
