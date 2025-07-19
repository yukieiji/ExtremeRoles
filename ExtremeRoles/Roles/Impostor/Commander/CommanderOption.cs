using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Commander
{
    public class CommanderSpecificOption : IRoleSpecificOption
    {
        public float KillCoolReduceTime { get; set; }
        public float KillCoolReduceImpBonus { get; set; }
        public int IncreaseKillNum { get; set; }
        public int AbilityUseCount { get; set; }
    }

    public class CommanderOptionLoader : ISpecificOptionLoader<CommanderSpecificOption>
    {
        public CommanderSpecificOption Load(IOptionLoader loader)
        {
            return new CommanderSpecificOption
            {
                KillCoolReduceTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Commander.CommanderOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Commander.CommanderOption.KillCoolReduceTime),
                KillCoolReduceImpBonus = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Commander.CommanderOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Commander.CommanderOption.KillCoolReduceImpBonus),
                IncreaseKillNum = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Commander.CommanderOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Commander.CommanderOption.IncreaseKillNum),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            };
        }
    }

    public class CommanderOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Commander.CommanderOption.KillCoolReduceTime,
                2.0f, 0.5f, 5.0f, 0.1f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Commander.CommanderOption.KillCoolReduceImpBonus,
                1.5f, 0.1f, 3.0f, 0.1f,
                format: OptionUnit.Second);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Commander.CommanderOption.IncreaseKillNum,
                2, 1, 3, 1,
                format: OptionUnit.Shot);
            IRoleAbility.CreateAbilityCountOption(factory, 1, 3);
        }
    }
}
