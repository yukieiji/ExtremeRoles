namespace ExtremeRoles.Module.NewOption;

public enum OptionTab : byte
{
	General,
	Combination,

	Crewmate,
	GhostCrewmate,

	Impostor,
	GhostImpostor,

	Neutral,
	GhostNeutral,
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
