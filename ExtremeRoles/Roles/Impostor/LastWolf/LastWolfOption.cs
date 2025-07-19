using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.LastWolf
{
    public class LastWolfSpecificOption : IRoleSpecificOption
    {
        public int AwakeImpostorNum { get; set; }
        public float DeadPlayerNumBonus { get; set; }
        public float KillPlayerNumBonus { get; set; }
        public float LightOffVision { get; set; }
        public float AbilityActiveTime { get; set; }
    }

    public class LastWolfOptionLoader : ISpecificOptionLoader<LastWolfSpecificOption>
    {
        public LastWolfSpecificOption Load(IOptionLoader loader)
        {
            return new LastWolfSpecificOption
            {
                AwakeImpostorNum = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption.AwakeImpostorNum),
                DeadPlayerNumBonus = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption.DeadPlayerNumBonus),
                KillPlayerNumBonus = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption.KillPlayerNumBonus),
                LightOffVision = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption.LightOffVision),
                AbilityActiveTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            };
        }
    }

    public class LastWolfOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption.AwakeImpostorNum,
                1, 1, GameSystem.MaxImposterNum, 1);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption.DeadPlayerNumBonus,
                1.0f, 2.0f, 6.5f, 0.1f,
                format: OptionUnit.Percentage);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption.KillPlayerNumBonus,
                2.5f, 4.0f, 10.0f, 0.1f,
                format: OptionUnit.Percentage);

            IRoleAbility.CreateCommonAbilityOption(
                factory, 10.0f);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.LastWolf.LastWolfOption.LightOffVision,
                0.1f, 0.0f, 1.0f, 0.1f);
        }
    }
}
