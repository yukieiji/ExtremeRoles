﻿using System.Collections.Generic;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;

namespace ExtremeRoles.GameMode.RoleSelector;

public sealed class ClassicGameModeRoleSelector : IRoleSelector
{
    public bool IsAdjustImpostorNum => false;

    public bool CanUseXion => true;

    public bool IsVanillaRoleToMultiAssign => false;

    public IEnumerable<ExtremeRoleId> UseNormalRoleId
    {
        get
        {
            foreach (ExtremeRoleId id in getUseNormalRoleId())
            {
                yield return id;
            }
        }
    }
    public IEnumerable<CombinationRoleType> UseCombRoleType
    {
        get
        {
            foreach (CombinationRoleType id in getUseCombRoleType())
            {
                yield return id;
            }
        }
    }
    public IEnumerable<ExtremeGhostRoleId> UseGhostRoleId
    {
        get
        {
            foreach (ExtremeGhostRoleId id in getUseGhostRoleId())
            {
                yield return id;
            }
        }
    }

    private readonly HashSet<int> roleCategoryGroup = new HashSet<int>();

    public ClassicGameModeRoleSelector()
    {
        foreach (ExtremeRoleId roleId in getUseNormalRoleId())
        {
            this.roleCategoryGroup.Add(
				ExtremeRoleManager.GetRoleGroupId(roleId));
        }
        foreach (CombinationRoleType roleId in getUseCombRoleType())
        {
			this.roleCategoryGroup.Add(
				ExtremeRoleManager.GetCombRoleGroupId(roleId));
		}
        foreach (ExtremeGhostRoleId roleId in getUseGhostRoleId())
        {
			this.roleCategoryGroup.Add(
				ExtremeGhostRoleManager.GetRoleGroupId(roleId));
		}
    }

	public bool IsValidCategory(int categoryId)
		=> this.roleCategoryGroup.Contains(categoryId);

    private static ExtremeRoleId[] getUseNormalRoleId() =>
		[
            ExtremeRoleId.SpecialCrew,
            ExtremeRoleId.Sheriff,
            ExtremeRoleId.Maintainer,
            ExtremeRoleId.Neet,
            ExtremeRoleId.Watchdog,
            ExtremeRoleId.Supervisor,
            ExtremeRoleId.BodyGuard,
            ExtremeRoleId.Whisper,
            ExtremeRoleId.TimeMaster,
            ExtremeRoleId.Agency,
            ExtremeRoleId.Bakary,
            ExtremeRoleId.CurseMaker,
            ExtremeRoleId.Fencer,
            ExtremeRoleId.Opener,
            ExtremeRoleId.Carpenter,
            ExtremeRoleId.Survivor,
            ExtremeRoleId.Captain,
            ExtremeRoleId.Photographer,
            ExtremeRoleId.Delusioner,
            ExtremeRoleId.Resurrecter,
            ExtremeRoleId.Gambler,
            ExtremeRoleId.Teleporter,
			ExtremeRoleId.Moderator,
			ExtremeRoleId.Psychic,
			ExtremeRoleId.Bait,
			ExtremeRoleId.Jailer,
			ExtremeRoleId.Summoner,

			ExtremeRoleId.SpecialImpostor,
            ExtremeRoleId.Evolver,
            ExtremeRoleId.Carrier,
            ExtremeRoleId.PsychoKiller,
            ExtremeRoleId.BountyHunter,
            ExtremeRoleId.Painter,
            ExtremeRoleId.Faker,
            ExtremeRoleId.OverLoader,
            ExtremeRoleId.Cracker,
            ExtremeRoleId.Bomber,
            ExtremeRoleId.Mery,
            ExtremeRoleId.SlaveDriver,
            ExtremeRoleId.SandWorm,
            ExtremeRoleId.Smasher,
            ExtremeRoleId.AssaultMaster,
            ExtremeRoleId.Shooter,
            ExtremeRoleId.LastWolf,
            ExtremeRoleId.Commander,
            ExtremeRoleId.Hypnotist,
            ExtremeRoleId.UnderWarper,
            ExtremeRoleId.Magician,
            ExtremeRoleId.Slime,
            ExtremeRoleId.Zombie,
			ExtremeRoleId.Thief,
			ExtremeRoleId.Crewshroom,
			ExtremeRoleId.Terorist,
			ExtremeRoleId.Raider,
			ExtremeRoleId.Glitch,
			ExtremeRoleId.Hijacker,
			ExtremeRoleId.TimeBreaker,

			ExtremeRoleId.Alice,
            ExtremeRoleId.Jackal,
            ExtremeRoleId.TaskMaster,
            ExtremeRoleId.Missionary,
            ExtremeRoleId.Jester,
            ExtremeRoleId.Yandere,
            ExtremeRoleId.Yoko,
            ExtremeRoleId.Totocalcio,
            ExtremeRoleId.Miner,
            ExtremeRoleId.Eater,
            ExtremeRoleId.Queen,
            ExtremeRoleId.Madmate,
            ExtremeRoleId.Umbrer,
			ExtremeRoleId.Hatter,
			ExtremeRoleId.Artist,
			ExtremeRoleId.Tucker,
			ExtremeRoleId.IronMate,
			ExtremeRoleId.Monika,
			ExtremeRoleId.Heretic,
		];

    private CombinationRoleType[] getUseCombRoleType() =>
		[
            CombinationRoleType.Avalon,
            CombinationRoleType.HeroAca,
            CombinationRoleType.DetectiveOffice,
            CombinationRoleType.Kids,

            CombinationRoleType.Lover,
            CombinationRoleType.Buddy,

            CombinationRoleType.Sharer,

            CombinationRoleType.Supporter,
            CombinationRoleType.Guesser,
            CombinationRoleType.Mover,
			CombinationRoleType.Accelerator,
			CombinationRoleType.Skater,

			CombinationRoleType.Traitor,
        ];

    private ExtremeGhostRoleId[] getUseGhostRoleId() =>
		[
            ExtremeGhostRoleId.Poltergeist,
            ExtremeGhostRoleId.Faunus,
            ExtremeGhostRoleId.Shutter,

            ExtremeGhostRoleId.Ventgeist,
            ExtremeGhostRoleId.SaboEvil,
            ExtremeGhostRoleId.Igniter,
			ExtremeGhostRoleId.Doppelganger,

			ExtremeGhostRoleId.Foras,
        ];
}

