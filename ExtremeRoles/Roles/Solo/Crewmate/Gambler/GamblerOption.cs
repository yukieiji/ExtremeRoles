using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.Gambler.GamblerRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Gambler
{
    public readonly record struct GamblerSpecificOption(
        int NormalVoteRate
    ) : IRoleSpecificOption;

    public class GamblerOptionLoader : ISpecificOptionLoader<GamblerSpecificOption>
    {
        public GamblerSpecificOption Load(IOptionLoader loader)
        {
            return new GamblerSpecificOption(
                loader.GetValue<GamblerOption, int>(
                    GamblerOption.NormalVoteRate)
            );
        }
    }

    public class GamblerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                GamblerOption.NormalVoteRate,
                50, 0, 90, 5,
                format: OptionUnit.Percentage);
        }
    }
}
