using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
    {

        base.CreateSpecificOption(categoryScope);

        IEnumerable<GhostRoleBase> collection = this.CombGhostRole.Values;

		var innerCategoryBuilder = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<AutoRoleOptionCategoryFactory>();
		foreach (var item in collection.Select(
            (Value, Index) => new { Value, Index }))
        {
			var role = item.Value;
			var inner = innerCategoryBuilder.CreateInnnerRoleCategory(role.Id, categoryScope);
			role.CreateRoleSpecificOption(inner.Builder);
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
