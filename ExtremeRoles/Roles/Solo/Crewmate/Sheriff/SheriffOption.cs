using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Sheriff
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
                loader.GetValue<SheriffOption, int>(
                    SheriffOption.ShootNum),
                loader.GetValue<SheriffOption, bool>(
                    SheriffOption.CanShootAssassin),
                loader.GetValue<SheriffOption, bool>(
                    SheriffOption.CanShootNeutral),
                loader.GetValue<SheriffOption, bool>(
                    SheriffOption.EnableTaskRelated),
                loader.GetValue<SheriffOption, float>(
                    SheriffOption.ReduceCurKillCool),
                loader.GetValue<SheriffOption, bool>(
                    SheriffOption.IsPerm),
                loader.GetValue<SheriffOption, bool>(
                    SheriffOption.IsSyncTaskAndShootNum),
                loader.GetValue<SheriffOption, int>(
                    SheriffOption.SyncShootTaskGage)
            );
        }
    }

    public class SheriffOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                SheriffOption.CanShootAssassin,
                false);

            factory.CreateBoolOption(
                SheriffOption.CanShootNeutral,
                true);

            factory.CreateIntOption(
                SheriffOption.ShootNum,
                1, 1, GameSystem.VanillaMaxPlayerNum - 1, 1,
                format: OptionUnit.Shot);

            var enableTaskRelatedOps = factory.CreateBoolOption(
                SheriffOption.EnableTaskRelated,
                false);

            factory.CreateFloatOption(
                SheriffOption.ReduceCurKillCool,
                2.0f, 1.0f, 5.0f,
                0.1f, enableTaskRelatedOps,
                format: OptionUnit.Second);

            factory.CreateBoolOption(
                SheriffOption.IsPerm,
                false, enableTaskRelatedOps);

            var syncOpt = factory.CreateBoolOption(
                SheriffOption.IsSyncTaskAndShootNum,
                false, enableTaskRelatedOps); ;
            factory.CreateIntOption(
                SheriffOption.SyncShootTaskGage,
                5, 5, 100, 1,
                syncOpt, format: OptionUnit.Percentage);
        }
    }
}
