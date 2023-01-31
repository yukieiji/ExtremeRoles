using System.Collections.Generic;

using ExtremeRoles.Module;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.GameMode.RoleSelector
{
    public sealed class HideNSeekGameModeRoleSelector : IRoleSelector
    {
        public bool CanUseXion => false;
        public bool IsVanillaRoleToMultiAssign => true;

        public IEnumerable<ExtremeRoleId> UseNormalRoleId
        {
            get
            {
                foreach (ExtremeRoleId id in getUseNormalId())
                {
                    yield return id;
                }
            }
        }
        public IEnumerable<CombinationRoleType> UseCombRoleType
        {
            get
            {
                yield break;
            }
        }
        public IEnumerable<int> GhostRoleSpawnOptionId
        {
            get
            {
                yield break;
            }
        }

        private readonly HashSet<int> useNormalRoleSpawnOption = new HashSet<int>();

        public HideNSeekGameModeRoleSelector()
        {
            foreach (ExtremeRoleId id in getUseNormalId())
            {
                this.useNormalRoleSpawnOption.Add(
                    ExtremeRoleManager.NormalRole[(int)id].GetRoleOptionId(RoleCommonOption.SpawnRate));
            }
        }

        public bool IsValidRoleOption(IOption option)
        {
            while (option.Parent != null)
            {
                option = option.Parent;
            }

            int id = option.Id;

            return this.useNormalRoleSpawnOption.Contains(id);
        }

        // TODO: ラストウルフ
        private static ExtremeRoleId[] getUseNormalId() => 
            new ExtremeRoleId[]
            {
                ExtremeRoleId.SpecialCrew,
                ExtremeRoleId.Neet,
                ExtremeRoleId.Watchdog,
                ExtremeRoleId.Supervisor,
                ExtremeRoleId.Survivor,
                ExtremeRoleId.Resurrecter,

                ExtremeRoleId.BountyHunter,
                ExtremeRoleId.Bomber,
                ExtremeRoleId.Hypnotist,
            };
    }
}
