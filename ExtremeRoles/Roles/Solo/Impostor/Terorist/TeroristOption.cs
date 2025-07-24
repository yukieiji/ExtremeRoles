using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.Terorist.TeroristRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Terorist
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
                loader.GetValue<TeroristOption, bool>(
                    TeroristOption.CanActiveOtherSabotage),
                loader.GetValue<TeroristOption, float>(
                    TeroristOption.ExplosionTime),
                loader.GetValue<TeroristOption, int>(
                    TeroristOption.BombNum),
                loader.GetValue<TeroristOption, float>(
                    TeroristOption.PlayerActivateTime),
                loader.GetValue<TeroristOption, bool>(
                    TeroristOption.CanUseDeadPlayer),
                loader.GetValue<TeroristOption, float>(
                    TeroristOption.DeadPlayerActivateTime),
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
                TeroristOption.CanActiveOtherSabotage,
                false);
            factory.CreateFloatOption(
                TeroristOption.ExplosionTime,
                45.0f, 10.0f, 240.0f, 1.0f,
                format: OptionUnit.Second);
            factory.CreateIntOption(
                TeroristOption.BombNum,
                3, 1, 6, 1);
            factory.CreateFloatOption(
                TeroristOption.PlayerActivateTime,
                3.0f, 0.25f, 10.0f, 0.25f,
                format: OptionUnit.Second);
            var deadPlayerOpt = factory.CreateBoolOption(
                TeroristOption.CanUseDeadPlayer,
                false);
            factory.CreateFloatOption(
                TeroristOption.DeadPlayerActivateTime,
                10.0f, 3.0f, 45.0f, 1.0f,
                deadPlayerOpt, format: OptionUnit.Second);
        }
    }
}
