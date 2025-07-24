using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Neutral.Alice.AliceRole;

namespace ExtremeRoles.Roles.Solo.Neutral.Alice
{
    public readonly record struct AliceSpecificOption(
        bool CanUseSabotage,
        int RevartCommonTaskNum,
        int RevartLongTaskNum,
        int RevartNormalTaskNum,
        int WinTaskRate,
        int WinKillNum,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class AliceOptionLoader : ISpecificOptionLoader<AliceSpecificOption>
    {
        public AliceSpecificOption Load(IOptionLoader loader)
        {
            return new AliceSpecificOption(
                loader.GetValue<AliceOption, bool>(
                    AliceOption.CanUseSabotage),
                loader.GetValue<AliceOption, int>(
                    AliceOption.RevartCommonTaskNum),
                loader.GetValue<AliceOption, int>(
                    AliceOption.RevartLongTaskNum),
                loader.GetValue<AliceOption, int>(
                    AliceOption.RevartNormalTaskNum),
                loader.GetValue<AliceOption, int>(
                    AliceOption.WinTaskRate),
                loader.GetValue<AliceOption, int>(
                    AliceOption.WinKillNum),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class AliceOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                AliceOption.CanUseSabotage,
                true);

            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 100);
            factory.CreateIntOption(
                AliceOption.RevartLongTaskNum,
                1, 0, 15, 1);
            factory.CreateIntOption(
                AliceOption.RevartCommonTaskNum,
                1, 0, 15, 1);
            factory.CreateIntOption(
                AliceOption.RevartNormalTaskNum,
                1, 0, 15, 1);
            factory.CreateIntOption(
                AliceOption.WinTaskRate,
                0, 0, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                AliceOption.WinKillNum,
                0, 0, 5, 1);
        }
    }
}
