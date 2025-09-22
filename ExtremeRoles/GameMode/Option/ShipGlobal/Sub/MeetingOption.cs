
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;

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
	public MeetingHudOption(in IOptionLoader loader)
	{
		this.MaxMeetingCount = loader.GetValue<int>((int)MeetingOption.NumMeating);
		this.UseRaiseHand = loader.GetValue<bool>((int)MeetingOption.UseRaiseHand);
		this.IsChangeVoteAreaButtonSortArg = loader.GetValue<bool>((int)MeetingOption.ChangeMeetingVoteAreaSort);
		this.IsFixedVoteAreaPlayerLevel = loader.GetValue<bool>((int)MeetingOption.FixedMeetingPlayerLevel);
		this.IsBlockSkipInMeeting = loader.GetValue<bool>((int)MeetingOption.DisableSkipInEmergencyMeeting);
		this.DisableSelfVote = loader.GetValue<bool>((int)MeetingOption.DisableSelfVote);
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
