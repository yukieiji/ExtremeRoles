using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Neutral.Alice
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption.CanUseSabotage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption.RevartCommonTaskNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption.RevartLongTaskNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption.RevartNormalTaskNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption.WinTaskRate),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption.WinKillNum),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class AliceOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption.CanUseSabotage,
                true);

            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 100);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption.RevartLongTaskNum,
                1, 0, 15, 1);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption.RevartCommonTaskNum,
                1, 0, 15, 1);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption.RevartNormalTaskNum,
                1, 0, 15, 1);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption.WinTaskRate,
                0, 0, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Neutral.Alice.AliceOption.WinKillNum,
                0, 0, 5, 1);
        }
    }
}
