using System.Collections.Generic;

using ExtremeRoles.Module.CustomOption.OLDS;

using ExtremeRoles.Roles;

using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;
using ExtremeRoles.GameMode.RoleSelector;

namespace ExtremeRoles.Module.CustomOption.Migrator;

public sealed class V10toV11 : MigratorBase
{
	public override int TargetVersion => 11;

	protected override IReadOnlyDictionary<string, string> ChangeOption => new Dictionary<string, string>()
	{
		{ S(CommonOptionKey.UseRaiseHand)      , $"{ShipGlobalOptionCategory.MeetingOption}{MeetingOption.UseRaiseHand}" },
		{ S(CommonOptionKey.UseStrongRandomGen), $"{OptionCreator.CommonOption.RandomOption}{OptionCreator.RandomOptionKey.UseStrong}" },
		{ S(CommonOptionKey.UsePrngAlgorithm)  , $"{OptionCreator.CommonOption.RandomOption}{OptionCreator.RandomOptionKey.Algorithm}" },

		{ S(RoleGlobalOption.MinCrewmateRoles), $"{SpawnOptionCategory.RoleSpawnCategory}{RoleSpawnOption.MinCrewmate}" },
		{ S(RoleGlobalOption.MaxCrewmateRoles), $"{SpawnOptionCategory.RoleSpawnCategory}{RoleSpawnOption.MaxCrewmate}" },
		{ S(RoleGlobalOption.MinNeutralRoles) , $"{SpawnOptionCategory.RoleSpawnCategory}{RoleSpawnOption.MinNeutral}" },
		{ S(RoleGlobalOption.MaxNeutralRoles) , $"{SpawnOptionCategory.RoleSpawnCategory}{RoleSpawnOption.MaxNeutral}" },
		{ S(RoleGlobalOption.MinImpostorRoles), $"{SpawnOptionCategory.RoleSpawnCategory}{RoleSpawnOption.MinImpostor}" },
		{ S(RoleGlobalOption.MaxImpostorRoles), $"{SpawnOptionCategory.RoleSpawnCategory}{RoleSpawnOption.MaxImpostor}" },

		{ S(RoleGlobalOption.MinCrewmateGhostRoles), $"{SpawnOptionCategory.GhostRoleSpawnCategory}{RoleSpawnOption.MinCrewmate}" },
		{ S(RoleGlobalOption.MaxCrewmateGhostRoles), $"{SpawnOptionCategory.GhostRoleSpawnCategory}{RoleSpawnOption.MaxCrewmate}" },
		{ S(RoleGlobalOption.MinNeutralGhostRoles) , $"{SpawnOptionCategory.GhostRoleSpawnCategory}{RoleSpawnOption.MinNeutral}" },
		{ S(RoleGlobalOption.MaxNeutralGhostRoles) , $"{SpawnOptionCategory.GhostRoleSpawnCategory}{RoleSpawnOption.MaxNeutral}" },
		{ S(RoleGlobalOption.MinImpostorGhostRoles), $"{SpawnOptionCategory.GhostRoleSpawnCategory}{RoleSpawnOption.MinImpostor}" },
		{ S(RoleGlobalOption.MaxImpostorGhostRoles), $"{SpawnOptionCategory.GhostRoleSpawnCategory}{RoleSpawnOption.MaxImpostor}" },

		{ S(RoleGlobalOption.UseXion), $"{ExtremeRoleId.Xion}{XionOption.UseXion}" },

		{ S(GlobalOption.NumMeating)                   , $"{ShipGlobalOptionCategory.MeetingOption}{MeetingOption.NumMeating}" },
		{ S(GlobalOption.ChangeMeetingVoteAreaSort)    , $"{ShipGlobalOptionCategory.MeetingOption}{MeetingOption.ChangeMeetingVoteAreaSort}" },
		{ S(GlobalOption.FixedMeetingPlayerLevel)      , $"{ShipGlobalOptionCategory.MeetingOption}{MeetingOption.FixedMeetingPlayerLevel}" },
		{ S(GlobalOption.DisableSkipInEmergencyMeeting), $"{ShipGlobalOptionCategory.MeetingOption}{MeetingOption.DisableSkipInEmergencyMeeting}" },
		{ S(GlobalOption.DisableSelfVote)              , $"{ShipGlobalOptionCategory.MeetingOption}{MeetingOption.DisableSelfVote}" },

		{ S(GlobalOption.ConfirmExilMode), $"{ShipGlobalOptionCategory.ExiledOption}{ExiledOption.ConfirmExilMode}" },
		{ S(GlobalOption.IsConfirmRole)  , $"{ShipGlobalOptionCategory.ExiledOption}{ExiledOption.IsConfirmRole}" },

		{ S(GlobalOption.DisableVent)             , $"{ShipGlobalOptionCategory.VentOption}{VentOption.Disable}" },
		{ S(GlobalOption.EngineerUseImpostorVent) , $"{ShipGlobalOptionCategory.VentOption}{VentOption.EngineerUseImpostor}" },
		{ S(GlobalOption.CanKillVentInPlayer)     , $"{ShipGlobalOptionCategory.VentOption}{VentOption.CanKillInPlayer}" },
		{ S(GlobalOption.VentAnimationModeInVison), $"{ShipGlobalOptionCategory.VentOption}{VentOption.AnimationModeInVison}" },

		{ S(GlobalOption.IsFixWallHaskTask), $"{ShipGlobalOptionCategory.TaskOption}{TaskOption.IsFixWallHaskTask}" },
		{ S(GlobalOption.GarbageTask)      , $"{ShipGlobalOptionCategory.TaskOption}{TaskOption.GarbageTask}" },
		{ S(GlobalOption.ShowerTask)       , $"{ShipGlobalOptionCategory.TaskOption}{TaskOption.ShowerTask}" },
		{ S(GlobalOption.DevelopPhotosTask), $"{ShipGlobalOptionCategory.TaskOption}{TaskOption.DevelopPhotosTask}" },
		{ S(GlobalOption.DivertPowerTask)  , $"{ShipGlobalOptionCategory.TaskOption}{TaskOption.DivertPowerTask}" },

		{ S(GlobalOption.EnableSpecialSetting)   , $"{ShipGlobalOptionCategory.RandomSpawnOption}{RandomSpawnOption.Enable}" },
		{ S(GlobalOption.SkeldRandomSpawn)       , $"{ShipGlobalOptionCategory.RandomSpawnOption}{RandomSpawnOption.Skeld}" },
		{ S(GlobalOption.MiraHqRandomSpawn)      , $"{ShipGlobalOptionCategory.RandomSpawnOption}{RandomSpawnOption.MiraHq}" },
		{ S(GlobalOption.PolusRandomSpawn)       , $"{ShipGlobalOptionCategory.RandomSpawnOption}{RandomSpawnOption.Polus}" },
		{ S(GlobalOption.AirShipRandomSpawn)     , $"{ShipGlobalOptionCategory.RandomSpawnOption}{RandomSpawnOption.AirShip}" },
		{ S(GlobalOption.FungleRandomSpawn)      , $"{ShipGlobalOptionCategory.RandomSpawnOption}{RandomSpawnOption.Fungle}" },
		{ S(GlobalOption.IsAutoSelectRandomSpawn), $"{ShipGlobalOptionCategory.RandomSpawnOption}{RandomSpawnOption.IsAutoSelect}" },


		{ S(GlobalOption.IsRemoveAdmin)     , $"{ShipGlobalOptionCategory.AdminOption}{DeviceOptionType.IsRemove}" },
		{ S(GlobalOption.EnableAdminLimit)  , $"{ShipGlobalOptionCategory.AdminOption}{DeviceOptionType.EnableLimit}" },
		{ S(GlobalOption.AdminLimitTime)    , $"{ShipGlobalOptionCategory.AdminOption}{DeviceOptionType.LimitTime}" },
		{ S(GlobalOption.AirShipEnableAdmin), $"{ShipGlobalOptionCategory.AdminOption}{AdminSpecialOption.AirShipEnable}" },

		{ S(GlobalOption.IsRemoveSecurity)     , $"{ShipGlobalOptionCategory.SecurityOption}{DeviceOptionType.IsRemove}" },
		{ S(GlobalOption.EnableSecurityLimit)  , $"{ShipGlobalOptionCategory.SecurityOption}{DeviceOptionType.EnableLimit}" },
		{ S(GlobalOption.SecurityLimitTime)    , $"{ShipGlobalOptionCategory.SecurityOption}{DeviceOptionType.LimitTime}" },

		{ S(GlobalOption.IsRemoveVital)     , $"{ShipGlobalOptionCategory.VitalOption}{DeviceOptionType.IsRemove}" },
		{ S(GlobalOption.EnableVitalLimit)  , $"{ShipGlobalOptionCategory.VitalOption}{DeviceOptionType.EnableLimit}" },
		{ S(GlobalOption.VitalLimitTime)    , $"{ShipGlobalOptionCategory.VitalOption}{DeviceOptionType.LimitTime}" },

		{ S(GlobalOption.RandomMap), $"{ShipGlobalOptionCategory.RandomMapOption}{RandomMap.Enable}" },

		{ S(GlobalOption.DisableTaskWinWhenNoneTaskCrew), $"{ShipGlobalOptionCategory.TaskWinOption}{TaskWinOption.DisableWhenNoneTaskCrew}" },
		{ S(GlobalOption.DisableTaskWin)                , $"{ShipGlobalOptionCategory.TaskWinOption}{TaskWinOption.DisableAll}" },

		{ S(GlobalOption.IsSameNeutralSameWin)         , $"{ShipGlobalOptionCategory.NeutralWinOption}{NeutralWinOption.IsSame}" },
		{ S(GlobalOption.DisableNeutralSpecialForceEnd), $"{ShipGlobalOptionCategory.NeutralWinOption}{NeutralWinOption.DisableSpecialEnd}" },

		{ S(GlobalOption.IsAssignNeutralToVanillaCrewGhostRole), $"{ShipGlobalOptionCategory.GhostRoleGlobalOption}{GhostRoleGlobalOption.IsAssignNeutralToVanillaCrewGhostRole}" },
		{ S(GlobalOption.IsRemoveAngleIcon)                    , $"{ShipGlobalOptionCategory.GhostRoleGlobalOption}{GhostRoleGlobalOption.IsRemoveAngleIcon}" },
		{ S(GlobalOption.IsBlockGAAbilityReport)               , $"{ShipGlobalOptionCategory.GhostRoleGlobalOption}{GhostRoleGlobalOption.IsBlockGAAbilityReport}" },
	};
}