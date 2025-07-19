using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Sheriff
{
    public class SheriffSpecificOption : IRoleSpecificOption
    {
        public int ShootNum { get; set; }
        public bool CanShootAssassin { get; set; }
        public bool CanShootNeutral { get; set; }
        public bool EnableTaskRelated { get; set; }
        public float ReduceCurKillCool { get; set; }
        public bool IsPerm { get; set; }
        public bool IsSyncTaskAndShootNum { get; set; }
        public int SyncShootTaskGage { get; set; }
    }

    public class SheriffOptionLoader : ISpecificOptionLoader<SheriffSpecificOption>
    {
        public SheriffSpecificOption Load(IOptionLoader loader)
        {
            return new SheriffSpecificOption
            {
                ShootNum = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.ShootNum),
                CanShootAssassin = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.CanShootAssassin),
                CanShootNeutral = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.CanShootNeutral),
                EnableTaskRelated = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.EnableTaskRelated),
                ReduceCurKillCool = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.ReduceCurKillCool),
                IsPerm = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.IsPerm),
                IsSyncTaskAndShootNum = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.IsSyncTaskAndShootNum),
                SyncShootTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Sheriff.SheriffOption.SyncShootTaskGage)
            };
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
