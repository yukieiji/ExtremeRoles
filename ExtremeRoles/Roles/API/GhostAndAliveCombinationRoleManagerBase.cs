using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module.NewOption.Factory;

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
			item.Value.CreateRoleSpecificOption(factory);
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
