namespace ExtremeRoles.Module.NewOption.OLDS;

public enum GlobalOption : int
{
	NumMeating = 50,
	ChangeMeetingVoteAreaSort,
	FixedMeetingPlayerLevel,
	DisableSkipInEmergencyMeeting,
	DisableSelfVote,

	ConfirmExilMode,
	IsConfirmRole,

	DisableVent,
	EngineerUseImpostorVent,
	CanKillVentInPlayer,
	VentAnimationModeInVison,

	ParallelMedBayScans,

	IsFixWallHaskTask,
	GarbageTask,
	ShowerTask,
	DevelopPhotosTask,
	DivertPowerTask,

	EnableSpecialSetting,
	SkeldRandomSpawn,
	MiraHqRandomSpawn,
	PolusRandomSpawn,
	AirShipRandomSpawn,
	FungleRandomSpawn,

	IsAutoSelectRandomSpawn,

	IsRemoveAdmin,
	AirShipEnableAdmin,
	EnableAdminLimit,
	AdminLimitTime,

	IsRemoveSecurity,
	EnableSecurityLimit,
	SecurityLimitTime,

	IsRemoveVital,
	EnableVitalLimit,
	VitalLimitTime,

	RandomMap,

	DisableTaskWinWhenNoneTaskCrew,
	DisableTaskWin,
	IsSameNeutralSameWin,
	DisableNeutralSpecialForceEnd,

	IsAssignNeutralToVanillaCrewGhostRole,
	IsRemoveAngleIcon,
	IsBlockGAAbilityReport,

	// ウマングアスを一時的もしくは恒久的に無効化
	// EnableHorseMode
}
