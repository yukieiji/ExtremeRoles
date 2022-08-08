using System;
using System.Runtime.CompilerServices;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.API
{
    public abstract partial class SingleRoleBase
    {
        protected sealed override void CreateKillerOption(
            IOption parentOps)
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
                OptionHolder.Range,
                killRangeOption);
        }
        protected sealed override IOption CreateSpawnOption()
        {
            var roleSetOption = CreateSelectionOption(
                RoleCommonOption.SpawnRate,
                OptionHolder.SpawnRate, null, true,
                colored: true);

            int spawnNum = this.IsImpostor() ? OptionHolder.MaxImposterNum : OptionHolder.VanillaMaxPlayerNum - 1;

            CreateIntOption(
                RoleCommonOption.RoleNum,
                1, 1, spawnNum, 1, roleSetOption);

            return roleSetOption;
        }

        protected sealed override void CreateVisonOption(
            IOption parentOps)
        {
            var visonOption = CreateBoolOption(
                RoleCommonOption.HasOtherVison,
                false, parentOps);
            CreateFloatOption(RoleCommonOption.Vison,
                2f, 0.25f, 5.0f, 0.25f,
                visonOption, format: OptionUnit.Multiplier);

            CreateBoolOption(
                RoleCommonOption.ApplyEnvironmentVisionEffect,
               this.IsCrewmate(), visonOption);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected  FloatCustomOption CreateFloatOption<T>(
            T option,
            float defaultValue,
            float min, float max, float step,
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
            bool colored = false) where T : struct, IConvertible
        {
            EnumCheck(option);

            return new FloatCustomOption(
                GetRoleOptionId(option),
                createAutoOptionString(option, colored),
                defaultValue,
                min, max, step,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption, this.tab);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected FloatDynamicCustomOption CreateFloatDynamicOption<T>(
            T option,
            float defaultValue,
            float min, float step,
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
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
                this.tab, tempMaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IntCustomOption CreateIntOption<T>(
            T option,
            int defaultValue,
            int min, int max, int step,
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
            bool colored = false) where T : struct, IConvertible
        {
            EnumCheck(option);

            return new IntCustomOption(
                GetRoleOptionId(option),
                createAutoOptionString(option, colored),
                defaultValue,
                min, max, step,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption, this.tab);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IntDynamicCustomOption CreateIntDynamicOption<T>(
            T option,
            int defaultValue,
            int min, int step,
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
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
                this.tab, tempMaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected BoolCustomOption CreateBoolOption<T>(
            T option,
            bool defaultValue,
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
            bool colored = false) where T : struct, IConvertible
        {
            EnumCheck(option);

            return new BoolCustomOption(
                GetRoleOptionId(option),
                createAutoOptionString(option, colored),
                defaultValue,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption, this.tab);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected SelectionCustomOption CreateSelectionOption<T>(
            T option,
            string[] selections,
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
            bool colored = false) where T : struct, IConvertible
        {
            EnumCheck(option);

            return new SelectionCustomOption(
                GetRoleOptionId(option),
                createAutoOptionString(option, colored),
                selections,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption, this.tab);
        }

        private string createAutoOptionString<T>(
            T option, bool colored) where T : struct, IConvertible
        {
            if (!colored)
            {
                return string.Concat(
                    this.RoleName, option.ToString());
            }
            else
            {
                return Design.ColoedString(
                    this.NameColor,
                    string.Concat(
                        this.RoleName,
                        RoleCommonOption.SpawnRate.ToString()));
            }
        }
    }
}
