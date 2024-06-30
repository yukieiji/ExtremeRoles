namespace ExtremeRoles.Module.CustomOption;

public enum OptionTab : byte
{
	GeneralTab,
	CombinationTab,

	CrewmateTab,
	GhostCrewmateTab,

	ImpostorTab,
	GhostImpostorTab,

	NeutralTab,
	GhostNeutralTab,
}

public enum OptionUnit : byte
{
	None,
	Preset,
	Second,
	Minute,
	Shot,
	Multiplier,
	Percentage,
	ScrewNum,
	VoteNum,
}
