using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Sheriff
{
    public readonly record struct SheriffSpecificOption(
        int ShootNum,
        bool CanShootAssassin,
        bool CanShootNeutral,
        bool EnableTaskRelated,
        float ReduceCurKillCool,
        bool IsPerm,
        bool IsSyncTaskAndShootNum,
        int SyncShootTaskGage
    ) : IRoleSpecificOption;

    public class SheriffOptionLoader : ISpecificOptionLoader<SheriffSpecificOption>
    {
        public SheriffSpecificOption Load(IOptionLoader loader)
        {
            return new SheriffSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.ShootNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.CanShootAssassin),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.CanShootNeutral),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.EnableTaskRelated),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.ReduceCurKillCool),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.IsPerm),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.IsSyncTaskAndShootNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.SyncShootTaskGage)
            );
        }
    }

    public class SheriffOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.CanShootAssassin,
                false);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.CanShootNeutral,
                true);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.ShootNum,
                1, 1, GameSystem.VanillaMaxPlayerNum - 1, 1,
                format: OptionUnit.Shot);

            var enableTaskRelatedOps = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.EnableTaskRelated,
                false);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.ReduceCurKillCool,
                2.0f, 1.0f, 5.0f,
                0.1f, enableTaskRelatedOps,
                format: OptionUnit.Second);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.IsPerm,
                false, enableTaskRelatedOps);

            var syncOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.IsSyncTaskAndShootNum,
                false, enableTaskRelatedOps); ;
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.SyncShootTaskGage,
                5, 5, 100, 1,
                syncOpt, format: OptionUnit.Percentage);
        }
    }
}
