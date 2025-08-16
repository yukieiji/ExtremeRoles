﻿namespace ExtremeRoles.Module.CustomOption;

public enum OptionTab : byte
{
	GeneralTab,

	CrewmateTab,
	ImpostorTab,
	NeutralTab,
	LiberalTab,
	CombinationTab,

	GhostCrewmateTab,
	GhostImpostorTab,
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
