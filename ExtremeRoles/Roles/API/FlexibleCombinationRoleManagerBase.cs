using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Roles.API;

public abstract class FlexibleCombinationRoleManagerBase : CombinationRoleManagerBase
{

    public MultiAssignRoleBase BaseRole;
    private int minimumRoleNum = 0;
    private bool canAssignImposter = true;

    public FlexibleCombinationRoleManagerBase(
        MultiAssignRoleBase role,
        int minimumRoleNum = 2,
        bool canAssignImposter = true) : 
            base(role.Id.ToString(), role.GetNameColor(true))
    {
        this.BaseRole = role;
        this.minimumRoleNum = minimumRoleNum;
        this.canAssignImposter = canAssignImposter;
    }

    public sealed override string GetOptionName()
        => Design.ColoedString(
            this.OptionColor,
            Translation.GetString(this.RoleName));

    public int GetOptionIdOffset() => this.OptionIdOffset;

    public string GetBaseRoleFullDescription() => 
        Translation.GetString($"{BaseRole.Id}FullDescription");

    public override void AssignSetUpInit(int curImpNum)
    {

        var allOption = OptionManager.Instance;

        foreach (var role in this.Roles)
        {
            role.CanHasAnotherRole = allOption.GetValue<bool>(
                GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign));

            if (!allOption.TryGet<bool>(
                    GetRoleOptionId(
                        CombinationRoleCommonOption.IsAssignImposter),
                    out var impOpt)) { continue; }

            bool isEvil = impOpt.GetValue();

            var spawnOption = allOption.Get<int>(
                GetRoleOptionId(CombinationRoleCommonOption.ImposterSelectedRate),
                OptionManager.ValueType.Int);
            isEvil = isEvil && 
                (UnityEngine.Random.RandomRange(0, 110) < (int)decimal.Multiply(
                    spawnOption.GetValue(), spawnOption.ValueCount)) &&
                curImpNum < GameOptionsManager.Instance.CurrentGameOptions.GetInt(
                    Int32OptionNames.NumImpostors);

            if (isEvil)
            {
                role.Team = ExtremeRoleType.Impostor;
                role.SetNameColor(Palette.ImpostorRed);
                role.CanKill = true;
                role.UseVent = true;
                role.UseSabotage = true;
                role.HasTask = false;
                ++curImpNum;
            }
            else
            {
                role.Team = ExtremeRoleType.Crewmate;
                role.CanKill = false;
                role.UseVent = false;
                role.UseSabotage = false;
                role.HasTask = true;
            }
            role.Initialize();
        }
    }

    public override MultiAssignRoleBase GetRole(
        int roleId, RoleTypes playerRoleType)
    {

        MultiAssignRoleBase role = null;
        
        if (this.BaseRole.Id != (ExtremeRoleId)roleId) { return role; }

        this.BaseRole.CanHasAnotherRole = OptionManager.Instance.GetValue<bool>(
            GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign));

        role = (MultiAssignRoleBase)this.BaseRole.Clone();

        switch (playerRoleType)
        {
            case RoleTypes.Impostor:
            case RoleTypes.Shapeshifter:
                role.Team = ExtremeRoleType.Impostor;
                role.SetNameColor(Palette.ImpostorRed);
                role.CanKill = true;
                role.UseVent = true;
                role.UseSabotage = true;
                role.HasTask = false;
                return role;
            default:
                return role;
        }


    }

    protected override IOptionInfo CreateSpawnOption()
    {
        // ExtremeRolesPlugin.Instance.Log.LogInfo($"Color: {this.optionColor}");
        var roleSetOption = new SelectionCustomOption(
            GetRoleOptionId(RoleCommonOption.SpawnRate),
            Design.ColoedString(
                this.OptionColor,
                string.Concat(
                    this.RoleName,
                    RoleCommonOption.SpawnRate.ToString())),
            OptionCreator.SpawnRate, null, true,
            tab: OptionTab.Combination);

        int roleAssignNum = this.BaseRole.IsImpostor() ? 
            GameSystem.MaxImposterNum : 
            GameSystem.VanillaMaxPlayerNum - 1;

        var roleAssignNumOption = new IntCustomOption(
            GetRoleOptionId(CombinationRoleCommonOption.AssignsNum),
            string.Concat(
                this.RoleName,
                CombinationRoleCommonOption.AssignsNum.ToString()),
            this.minimumRoleNum, this.minimumRoleNum,
            roleAssignNum, 1,
            roleSetOption, isHidden: this.minimumRoleNum <= 1,
            tab: OptionTab.Combination);


        int maxSetNum = this.BaseRole.IsImpostor() ?
            GameSystem.MaxImposterNum:
            (GameSystem.VanillaMaxPlayerNum - 1);

        var roleSetNumOption = new IntCustomOption(
            GetRoleOptionId(RoleCommonOption.RoleNum),
            string.Concat(
                this.RoleName,
                RoleCommonOption.RoleNum.ToString()),
            1, 1, maxSetNum, 1,
            roleSetOption,
            tab: OptionTab.Combination);

        roleAssignNumOption.SetUpdateOption(roleSetNumOption);

        new IntCustomOption(
            GetRoleOptionId(RoleCommonOption.AssignWeight),
            RoleCommonOption.AssignWeight.ToString(),
            1000, 1, 1000, 1,
            roleSetOption,
            tab: OptionTab.Combination);

        if (this.canAssignImposter)
        {
            var isImposterAssignOps = new BoolCustomOption(
                GetRoleOptionId(CombinationRoleCommonOption.IsAssignImposter),
                string.Concat(
                    this.RoleName,
                    CombinationRoleCommonOption.IsAssignImposter.ToString()),
                false, roleSetOption,
            tab: OptionTab.Combination);

            new SelectionCustomOption(
                GetRoleOptionId(CombinationRoleCommonOption.ImposterSelectedRate),
                string.Concat(
                    this.RoleName,
                    CombinationRoleCommonOption.ImposterSelectedRate.ToString()),
                OptionCreator.SpawnRate, isImposterAssignOps,
            tab: OptionTab.Combination);
        }

        new BoolCustomOption(
            GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign),
            string.Concat(
                this.RoleName,
                CombinationRoleCommonOption.IsMultiAssign.ToString()),
            false, roleSetOption,
            isHidden: this.minimumRoleNum <= 1,
            tab: OptionTab.Combination);

        return roleSetOption;
    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {

        int optionOffset = this.OptionIdOffset + ExtremeRoleManager.OptionOffsetPerRole;
        this.BaseRole.SetManagerOptionOffset(this.OptionIdOffset);
        this.BaseRole.CreateRoleSpecificOption(
            parentOps,
            optionOffset);
    }

    protected override void CommonInit()
    {
        this.Roles.Clear();
        int roleAssignNum = 1;
        var allOptions = OptionManager.Instance;

        this.BaseRole.CanHasAnotherRole = allOptions.GetValue<bool>(
            GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign));

        if (allOptions.TryGet<int>(
                GetRoleOptionId(CombinationRoleCommonOption.AssignsNum),
                out var opt))
        {
            roleAssignNum = opt.GetValue();
        }

        for (int i = 0; i < roleAssignNum; ++i)
        {
            this.Roles.Add((MultiAssignRoleBase)this.BaseRole.Clone());
        }
    }

}
