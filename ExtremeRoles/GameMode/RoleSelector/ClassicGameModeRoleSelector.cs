using System.Collections.Generic;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination;

namespace ExtremeRoles.GameMode.RoleSelector
{
    public sealed class ClassicGameModeRoleSelector : IRoleSelector
    {
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
        public IEnumerable<int> GhostRoleSpawnOptionId
        {
            get
            {
                foreach (int id in this.useGhostRoleSpawnOption)
                {
                    yield return id;
                }
            }
        }

        private readonly HashSet<int> useNormalRoleSpawnOption = new HashSet<int>();
        private readonly HashSet<int> useCombRoleSpawnOption = new HashSet<int>();
        private readonly HashSet<int> useGhostRoleSpawnOption = new HashSet<int>();

        public ClassicGameModeRoleSelector()
        {
            foreach (ExtremeRoleId roleId in getUseNormalRoleId())
            {
                var role = ExtremeRoleManager.NormalRole[(int)roleId];
                this.useNormalRoleSpawnOption.Add(
                    role.GetRoleOptionId(RoleCommonOption.SpawnRate));
            }
            foreach (CombinationRoleType roleId in getUseCombRoleType())
            {
                var role = ExtremeRoleManager.CombRole[(byte)roleId];
                this.useCombRoleSpawnOption.Add(
                    role.GetRoleOptionId(RoleCommonOption.SpawnRate));
            }
            foreach (GhostRoleBase role in ExtremeGhostRoleManager.AllGhostRole.Values)
            {
                this.useGhostRoleSpawnOption.Add(
                    role.GetRoleOptionId(RoleCommonOption.SpawnRate));
            }
        }

        public bool IsValidRoleOption(IOption option)
        {
            while (option.Parent != null)
            {
                option = option.Parent;
            }

            int id = option.Id;

            return
                this.useNormalRoleSpawnOption.Contains(id) ||
                this.useCombRoleSpawnOption.Contains(id) ||
                this.useGhostRoleSpawnOption.Contains(id);
        }

        private static ExtremeRoleId[] getUseNormalRoleId() =>
            new ExtremeRoleId[]
            {
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
            };
        private CombinationRoleType[] getUseCombRoleType() =>
            new CombinationRoleType[]
            {
                CombinationRoleType.Avalon,
                CombinationRoleType.HeroAca,
                CombinationRoleType.DetectiveOffice,
                CombinationRoleType.Kids,

                CombinationRoleType.Lover,
                CombinationRoleType.Buddy,

                CombinationRoleType.Sharer,

                CombinationRoleType.Supporter,
                CombinationRoleType.Guesser,

                CombinationRoleType.Traitor,
            };
    }
}
