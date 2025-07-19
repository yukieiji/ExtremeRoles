using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.AssaultMaster
{
    public class AssaultMasterSpecificOption : IRoleSpecificOption
    {
        public int StockLimit { get; set; }
        public int StockNumWhenReport { get; set; }
        public int StockNumWhenMeetingButton { get; set; }
        public float CockingKillCoolReduceTime { get; set; }
        public float ReloadReduceKillCoolTimePerStock { get; set; }
        public bool IsResetReloadCoolTimeWhenKill { get; set; }
        public int ReloadCoolTimeReduceRatePerHideStock { get; set; }
        public float AbilityCoolTime { get; set; }
    }

    public class AssaultMasterOptionLoader : ISpecificOptionLoader<AssaultMasterSpecificOption>
    {
        public AssaultMasterSpecificOption Load(IOptionLoader loader)
        {
            return new AssaultMasterSpecificOption
            {
                StockLimit = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption.StockLimit),
                StockNumWhenReport = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption.StockNumWhenReport),
                StockNumWhenMeetingButton = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption.StockNumWhenMeetingButton),
                CockingKillCoolReduceTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption.CockingKillCoolReduceTime),
                ReloadReduceKillCoolTimePerStock = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption.ReloadReduceKillCoolTimePerStock),
                IsResetReloadCoolTimeWhenKill = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption.IsResetReloadCoolTimeWhenKill),
                ReloadCoolTimeReduceRatePerHideStock = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption.ReloadCoolTimeReduceRatePerHideStock),
                AbilityCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            };
        }
    }

    public class AssaultMasterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateCommonAbilityOption(factory);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption.StockLimit,
                2, 1, 10, 1,
                format: OptionUnit.ScrewNum);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption.StockNumWhenReport,
                1, 1, 5, 1,
                format: OptionUnit.ScrewNum);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption.StockNumWhenMeetingButton,
                3, 1, 10, 1,
                format: OptionUnit.ScrewNum);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption.CockingKillCoolReduceTime,
                2.0f, 1.0f, 5.0f, 0.1f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption.ReloadReduceKillCoolTimePerStock,
                5.0f, 2.0f, 10.0f, 0.1f,
                format: OptionUnit.Second);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption.IsResetReloadCoolTimeWhenKill,
                true);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.AssaultMaster.AssaultMasterOption.ReloadCoolTimeReduceRatePerHideStock,
                75, 30, 90, 1,
                format: OptionUnit.Percentage);
        }
    }
}
