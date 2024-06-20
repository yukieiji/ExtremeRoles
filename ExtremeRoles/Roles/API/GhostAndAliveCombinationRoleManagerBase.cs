using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factories;

namespace ExtremeRoles.Roles.API;

public abstract class GhostAndAliveCombinationRoleManagerBase :
    ConstCombinationRoleManagerBase
{
    public Dictionary<ExtremeRoleId, GhostRoleBase> CombGhostRole = new
        Dictionary<ExtremeRoleId, GhostRoleBase> ();

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

    public abstract void InitializeGhostRole(
        byte rolePlayerId, GhostRoleBase role, SingleRoleBase aliveRole);

    public int GetOptionIdOffset() => this.OptionIdOffset;

    public GhostRoleBase GetGhostRole(ExtremeRoleId id) =>
        this.CombGhostRole[id];

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {

        base.CreateSpecificOption(parentOps);

        IEnumerable<GhostRoleBase> collection = this.CombGhostRole.Values;
		var factory = new AutoParentSetFactory(tab: OptionTab.Combination, parent: parentOps);

		foreach (var item in collection.Select(
            (Value, Index) => new { Value, Index }))
        {
            int optionOffset = this.OptionIdOffset + (
                ExtremeRoleManager.OptionOffsetPerRole * (
                item.Index + 1 + this.Roles.Count));
			factory.IdOffset = optionOffset;
			factory.NamePrefix = item.Value.Name;
			item.Value.CreateRoleSpecificOption(factory, optionOffset);
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
