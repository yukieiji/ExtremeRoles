using System;
using System.Runtime.CompilerServices;

using ExtremeRoles.Helper;

namespace ExtremeRoles.Roles.API;

public abstract partial class SingleRoleBase
{
    protected sealed override void CreateKillerOption(
        IOptionInfo parentOps)
    {
        var killCoolOption = CreateBoolOption(
            KillerCommonOption.HasOtherKillCool,
            false, parentOps);
        CreateFloatOption(
            KillerCommonOption.KillCoolDown,
            30f, 1.0f, 120f, 0.5f,
            killCoolOption, format: OptionUnit.Second);

        var killRangeOption = CreateBoolOption(
            KillerCommonOption.HasOtherKillRange,
            false, parentOps);
        CreateSelectionOption(
            KillerCommonOption.KillRange,
            OptionCreator.Range,
            killRangeOption);
    }
    protected sealed override IOptionInfo CreateSpawnOption()
    {
        var roleSetOption = CreateSelectionOption(
            RoleCommonOption.SpawnRate,
            OptionCreator.SpawnRate, null, true,
            colored: true);

        int spawnNum = this.IsImpostor() ? 
            GameSystem.MaxImposterNum : GameSystem.VanillaMaxPlayerNum - 1;

        CreateIntOption(
            RoleCommonOption.RoleNum,
            1, 1, spawnNum, 1, roleSetOption);

        new IntCustomOption(
            GetRoleOptionId(RoleCommonOption.AssignWeight),
            RoleCommonOption.AssignWeight.ToString(),
            500, 1, 1000, 1,
            roleSetOption, tab:this.Tab);

        return roleSetOption;
    }

    protected sealed override void CreateVisionOption(
        IOptionInfo parentOps)
    {
        var visionOption = CreateBoolOption(
            RoleCommonOption.HasOtherVision,
            false, parentOps);
        CreateFloatOption(RoleCommonOption.Vision,
            2f, 0.25f, 5.0f, 0.25f,
            visionOption, format: OptionUnit.Multiplier);

        CreateBoolOption(
            RoleCommonOption.ApplyEnvironmentVisionEffect,
            this.IsCrewmate(), visionOption);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected FloatCustomOption CreateFloatOption<T>(
        T option,
        float defaultValue,
        float min, float max, float step,
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null,
        bool colored = false) where T : struct, IConvertible
    {
        EnumCheck(option);

        return new FloatCustomOption(
            GetRoleOptionId(option),
            createAutoOptionString(option, colored),
            defaultValue,
            min, max, step,
            parent, isHeader, isHidden,
            format, invert, enableCheckOption, this.Tab);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected FloatDynamicCustomOption CreateFloatDynamicOption<T>(
        T option,
        float defaultValue,
        float min, float step,
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null,
        bool colored = false,
        float tempMaxValue = 0.0f) where T : struct, IConvertible
    {
        EnumCheck(option);

        return new FloatDynamicCustomOption(
            GetRoleOptionId(option),
            createAutoOptionString(option, colored),
            defaultValue,
            min, step,
            parent, isHeader, isHidden,
            format, invert, enableCheckOption,
            this.Tab, tempMaxValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected IntCustomOption CreateIntOption<T>(
        T option,
        int defaultValue,
        int min, int max, int step,
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null,
        bool colored = false) where T : struct, IConvertible
    {
        EnumCheck(option);

        return new IntCustomOption(
            GetRoleOptionId(option),
            createAutoOptionString(option, colored),
            defaultValue,
            min, max, step,
            parent, isHeader, isHidden,
            format, invert, enableCheckOption, this.Tab);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected IntDynamicCustomOption CreateIntDynamicOption<T>(
        T option,
        int defaultValue,
        int min, int step,
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null,
        bool colored = false,
        int tempMaxValue = 0) where T : struct, IConvertible
    {
        EnumCheck(option);

        return new IntDynamicCustomOption(
            GetRoleOptionId(option),
            createAutoOptionString(option, colored),
            defaultValue,
            min, step,
            parent, isHeader, isHidden,
            format, invert, enableCheckOption,
            this.Tab, tempMaxValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected BoolCustomOption CreateBoolOption<T>(
        T option,
        bool defaultValue,
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null,
        bool colored = false) where T : struct, IConvertible
    {
        EnumCheck(option);

        return new BoolCustomOption(
            GetRoleOptionId(option),
            createAutoOptionString(option, colored),
            defaultValue,
            parent, isHeader, isHidden,
            format, invert, enableCheckOption, this.Tab);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected SelectionCustomOption CreateSelectionOption<T>(
        T option,
        string[] selections,
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null,
        bool colored = false) where T : struct, IConvertible
    {
        EnumCheck(option);

        return new SelectionCustomOption(
            GetRoleOptionId(option),
            createAutoOptionString(option, colored),
            selections,
            parent, isHeader, isHidden,
            format, invert, enableCheckOption, this.Tab);
    }

    private string createAutoOptionString<T>(
        T option, bool colored) where T : struct, IConvertible
    {
        if (!colored)
        {
            return string.Concat(
                this.RawRoleName, option.ToString());
        }
        else
        {
            return Design.ColoedString(
                this.NameColor,
                string.Concat(
                    this.RawRoleName,
                    RoleCommonOption.SpawnRate.ToString()));
        }
    }
}
