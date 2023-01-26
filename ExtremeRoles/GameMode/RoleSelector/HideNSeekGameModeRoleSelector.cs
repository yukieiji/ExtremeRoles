using System;
using System.Collections.Generic;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.GameMode.RoleSelector
{
    public sealed class HideNSeekGameModeRoleSelector : IRoleSelector
    {
        public bool CanUseXion => false;
        public bool IsVanillaRoleToMultiAssign => true;

        public IEnumerable<int> NormalRoleSpawnOptionId
        {
            get
            {
                foreach (int id in this.useNormalRoleSpawnOption)
                {
                    yield return id;
                }
            }
        }
        public IEnumerable<int> CombRoleSpawnOptionId
        {
            get
            {
                foreach (int id in this.useCombRoleSpawnOption)
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

        public HideNSeekGameModeRoleSelector()
        {
            foreach (ExtremeRoleId id in new ExtremeRoleId[]
            {
                ExtremeRoleId.SpecialCrew,
                ExtremeRoleId.Watchdog,
                ExtremeRoleId.Supervisor,
                ExtremeRoleId.Survivor,
            })
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

            return
                this.useNormalRoleSpawnOption.Contains(id);
        }
    }
}
