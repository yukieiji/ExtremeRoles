using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Impostor.Cracker
{
    public class CrackerSpecificOption : IRoleSpecificOption
    {
        public bool RemoveDeadBody { get; set; }
        public float CanCrackDistance { get; set; }
        public int AbilityUseCount { get; set; }
    }

    public class CrackerOptionLoader : ISpecificOptionLoader<CrackerSpecificOption>
    {
        public CrackerSpecificOption Load(IOptionLoader loader)
        {
            return new CrackerSpecificOption
            {
                RemoveDeadBody = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Cracker.CrackerOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Cracker.CrackerOption.RemoveDeadBody),
                CanCrackDistance = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Cracker.CrackerOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Cracker.CrackerOption.CanCrackDistance),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            };
        }
    }

    public class CrackerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 5);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Cracker.CrackerOption.CanCrackDistance,
                1.0f, 1.0f, 5.0f, 0.5f);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Cracker.CrackerOption.RemoveDeadBody,
                false);
        }
    }
}
