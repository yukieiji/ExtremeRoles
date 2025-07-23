using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Terorist
{
    public readonly record struct TeroristSpecificOption(
        bool CanActiveOtherSabotage,
        float ExplosionTime,
        int BombNum,
        float PlayerActivateTime,
        bool CanUseDeadPlayer,
        float DeadPlayerActivateTime,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class TeroristOptionLoader : ISpecificOptionLoader<TeroristSpecificOption>
    {
        public TeroristSpecificOption Load(IOptionLoader loader)
        {
            return new TeroristSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption.CanActiveOtherSabotage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption.ExplosionTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption.BombNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption.PlayerActivateTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption.CanUseDeadPlayer),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption.DeadPlayerActivateTime),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class TeroristOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 5, 100);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption.CanActiveOtherSabotage,
                false);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption.ExplosionTime,
                45.0f, 10.0f, 240.0f, 1.0f,
                format: OptionUnit.Second);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption.BombNum,
                3, 1, 6, 1);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption.PlayerActivateTime,
                3.0f, 0.25f, 10.0f, 0.25f,
                format: OptionUnit.Second);
            var deadPlayerOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption.CanUseDeadPlayer,
                false);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristOption.DeadPlayerActivateTime,
                10.0f, 3.0f, 45.0f, 1.0f,
                deadPlayerOpt, format: OptionUnit.Second);
        }
    }
}
