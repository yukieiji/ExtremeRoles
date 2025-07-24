using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterRole;

namespace ExtremeRoles.Roles.Solo.Impostor.AssaultMaster
{
    public readonly record struct AssaultMasterSpecificOption(
        int StockLimit,
        int StockNumWhenReport,
        int StockNumWhenMeetingButton,
        float CockingKillCoolReduceTime,
        float ReloadReduceKillCoolTimePerStock,
        bool IsResetReloadCoolTimeWhenKill,
        int ReloadCoolTimeReduceRatePerHideStock,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class AssaultMasterOptionLoader : ISpecificOptionLoader<AssaultMasterSpecificOption>
    {
        public AssaultMasterSpecificOption Load(IOptionLoader loader)
        {
            return new AssaultMasterSpecificOption(
                loader.GetValue<AssaultMasterOption, int>(
                    AssaultMasterOption.StockLimit),
                loader.GetValue<AssaultMasterOption, int>(
                    AssaultMasterOption.StockNumWhenReport),
                loader.GetValue<AssaultMasterOption, int>(
                    AssaultMasterOption.StockNumWhenMeetingButton),
                loader.GetValue<AssaultMasterOption, float>(
                    AssaultMasterOption.CockingKillCoolReduceTime),
                loader.GetValue<AssaultMasterOption, float>(
                    AssaultMasterOption.ReloadReduceKillCoolTimePerStock),
                loader.GetValue<AssaultMasterOption, bool>(
                    AssaultMasterOption.IsResetReloadCoolTimeWhenKill),
                loader.GetValue<AssaultMasterOption, int>(
                    AssaultMasterOption.ReloadCoolTimeReduceRatePerHideStock),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class AssaultMasterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateCommonAbilityOption(factory);

            factory.CreateIntOption(
                AssaultMasterOption.StockLimit,
                2, 1, 10, 1,
                format: OptionUnit.ScrewNum);
            factory.CreateIntOption(
                AssaultMasterOption.StockNumWhenReport,
                1, 1, 5, 1,
                format: OptionUnit.ScrewNum);
            factory.CreateIntOption(
                AssaultMasterOption.StockNumWhenMeetingButton,
                3, 1, 10, 1,
                format: OptionUnit.ScrewNum);
            factory.CreateFloatOption(
                AssaultMasterOption.CockingKillCoolReduceTime,
                2.0f, 1.0f, 5.0f, 0.1f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                AssaultMasterOption.ReloadReduceKillCoolTimePerStock,
                5.0f, 2.0f, 10.0f, 0.1f,
                format: OptionUnit.Second);
            factory.CreateBoolOption(
                AssaultMasterOption.IsResetReloadCoolTimeWhenKill,
                true);
            factory.CreateIntOption(
                AssaultMasterOption.ReloadCoolTimeReduceRatePerHideStock,
                75, 30, 90, 1,
                format: OptionUnit.Percentage);
        }
    }
}
