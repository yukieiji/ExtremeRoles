using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Impostor.Hijacker
{
    public class HijackerSpecificOption : IRoleSpecificOption
    {
        public bool IsRandomPlayer { get; set; }
        public int AbilityUseCount { get; set; }
        public float AbilityActiveTime { get; set; }
    }

    public class HijackerOptionLoader : ISpecificOptionLoader<HijackerSpecificOption>
    {
        public HijackerSpecificOption Load(IOptionLoader loader)
        {
            return new HijackerSpecificOption
            {
                IsRandomPlayer = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hijacker.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Hijacker.Option.IsRandomPlayer),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                AbilityActiveTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            };
        }
    }

    public class HijackerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 3, 10, 10f);
            factory.CreateBoolOption(ExtremeRoles.Roles.Solo.Impostor.Hijacker.Option.IsRandomPlayer, true);
        }
    }
}
