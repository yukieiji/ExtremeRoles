using System.Collections.Generic;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using Microsoft.Extensions.DependencyInjection;

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
		var gen = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IRoleParentOptionIdGenerator>();
        foreach (ExtremeRoleId roleId in getUseNormalRoleId())
        {
            this.roleCategoryGroup.Add(gen.Get(roleId));
        }
        foreach (CombinationRoleType roleId in getUseCombRoleType())
        {
			this.roleCategoryGroup.Add(gen.Get(roleId));
		}
        foreach (ExtremeGhostRoleId roleId in getUseGhostRoleId())
        {
			this.roleCategoryGroup.Add(gen.Get(roleId));
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
			ExtremeRoleId.Exorcist,
			ExtremeRoleId.Loner,
			ExtremeRoleId.CEO,
			ExtremeRoleId.Echo,

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
			ExtremeRoleId.Scavenger,
			ExtremeRoleId.Boxer,

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
			ExtremeRoleId.Shepherd,
			ExtremeRoleId.Furry,
			ExtremeRoleId.Intimate,
			ExtremeRoleId.Surrogator,
			ExtremeRoleId.Knight,
			ExtremeRoleId.Pawn,
		];

    private CombinationRoleType[] getUseCombRoleType() =>
		[
            CombinationRoleType.Avalon,
            CombinationRoleType.HeroAca,
            CombinationRoleType.InvestigatorOffice,
            CombinationRoleType.Kids,

            CombinationRoleType.Lover,
            CombinationRoleType.Buddy,

            CombinationRoleType.Sharer,

            CombinationRoleType.Supporter,
            CombinationRoleType.Guesser,
            CombinationRoleType.Mover,
			CombinationRoleType.Accelerator,
			CombinationRoleType.Skater,
			CombinationRoleType.Barter,

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

