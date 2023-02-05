using System.Collections.Generic;

using ExtremeRoles.GhostRoles;
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
        public IEnumerable<ExtremeGhostRoleId> UseGhostRoleId
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
        
        // TODO: リザレク復活がHideNSeekでちゃんと計算されてないので要調査
        private static ExtremeRoleId[] getUseNormalId() => 
            new ExtremeRoleId[]
            {
                ExtremeRoleId.SpecialCrew,
                ExtremeRoleId.Neet,
                ExtremeRoleId.Watchdog,
                ExtremeRoleId.Supervisor,
                ExtremeRoleId.Survivor,

                ExtremeRoleId.BountyHunter,
                ExtremeRoleId.Bomber,
                ExtremeRoleId.LastWolf,
                ExtremeRoleId.Hypnotist,
            };
    }
}
