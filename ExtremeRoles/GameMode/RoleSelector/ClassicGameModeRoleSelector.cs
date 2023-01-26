using System;
using System.Collections.Generic;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.GameMode.RoleSelector
{
    public sealed class ClassicGameModeRoleSelector : IRoleSelector
    {
        public bool CanUseXion => true;
        public bool IsVanillaRoleToMultiAssign => false;

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

        public ClassicGameModeRoleSelector()
        {
            foreach (SingleRoleBase role in ExtremeRoleManager.NormalRole.Values)
            {
                this.useNormalRoleSpawnOption.Add(
                    role.GetRoleOptionId(RoleCommonOption.SpawnRate));
            }
            foreach (CombinationRoleManagerBase role in ExtremeRoleManager.CombRole.Values)
            {
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
    }
}
