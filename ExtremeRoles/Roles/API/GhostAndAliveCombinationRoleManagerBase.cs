using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Module.CustomOption.Factory.Old;

namespace ExtremeRoles.Roles.API;

public abstract class GhostAndAliveCombinationRoleManagerBase :
    ConstCombinationRoleManagerBase
{
    public Dictionary<ExtremeRoleId, GhostRoleBase> CombGhostRole = new
        Dictionary<ExtremeRoleId, GhostRoleBase> ();

    public GhostAndAliveCombinationRoleManagerBase(
		CombinationRoleType roleType,
        string roleName,
        Color optionColor,
        int setPlayerNum,
        int maxSetNum = int.MaxValue) : base(
			roleType,
			roleName,
            optionColor,
            setPlayerNum,
            maxSetNum)
    {
        this.CombGhostRole.Clear();
    }

    public abstract void InitializeGhostRole(
        byte rolePlayerId, GhostRoleBase role, SingleRoleBase aliveRole);

    public GhostRoleBase GetGhostRole(ExtremeRoleId id) =>
        this.CombGhostRole[id];

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {

        base.CreateSpecificOption(factory);

        IEnumerable<GhostRoleBase> collection = this.CombGhostRole.Values;

		foreach (var item in collection.Select(
            (Value, Index) => new { Value, Index }))
        {
			var role = item.Value; ;

			int offset = (item.Index + 1) * ExtremeGhostRoleManager.IdOffset;
			factory.IdOffset = offset;
			factory.OptionPrefix = role.Name;

			role.CreateRoleSpecificOption(factory);
			if (role is ICombination combGhost)
			{
				combGhost.OffsetInfo = new MultiAssignRoleBase.OptionOffsetInfo(
					this.RoleType, offset);
			}
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
