using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.API
{
    public abstract class GhostAndAliveCombinationRoleManagerBase : 
        ConstCombinationRoleManagerBase
    {
        public Dictionary<ExtremeRoleId, GhostRoles.API.GhostRoleBase> CombGhostRole = new 
            Dictionary<ExtremeRoleId, GhostRoles.API.GhostRoleBase> ();

        public GhostAndAliveCombinationRoleManagerBase(
            string roleName,
            Color optionColor,
            int setPlayerNum,
            int maxSetNum = int.MaxValue) : base(
                roleName,
                optionColor,
                setPlayerNum,
                maxSetNum)
        {
            this.CombGhostRole.Clear();
        }

        public int GetOptionIdOffset() => this.OptionIdOffset;

        public GhostRoles.API.GhostRoleBase GetGhostRole(ExtremeRoleId id) => 
            this.CombGhostRole[id];

        protected override void CreateSpecificOption(
            IOption parentOps)
        {

            IEnumerable<GhostRoles.API.GhostRoleBase> collection = this.CombGhostRole.Values;

            foreach (var item in collection.Select(
                (Value, Index) => new { Value, Index }))
            {
                int optionOffset = this.OptionIdOffset + (
                    ExtremeRoleManager.OptionOffsetPerRole * (
                    item.Index + 1 + this.Roles.Count));
                item.Value.CreateRoleSpecificOption(
                    parentOps, optionOffset);
            }
        }
        protected override void CommonInit()
        {
            base.CommonInit();

            foreach (var role in this.CombGhostRole.Values)
            {
                role.Initialize();
            }
        }
    }
}
