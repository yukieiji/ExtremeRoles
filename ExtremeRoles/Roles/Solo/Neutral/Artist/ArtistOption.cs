using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using static ExtremeRoles.Roles.Solo.Neutral.Artist.ArtistRole;

namespace ExtremeRoles.Roles.Solo.Neutral.Artist
{
    public readonly record struct ArtistSpecificOption(
        bool CanUseVent,
        int WinAreaSize,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class ArtistOptionLoader : ISpecificOptionLoader<ArtistSpecificOption>
    {
        public ArtistSpecificOption Load(IOptionLoader loader)
        {
            return new ArtistSpecificOption(
                loader.GetValue<ArtistOption, bool>(
                    ArtistOption.CanUseVent),
                loader.GetValue<ArtistOption, int>(
                    ArtistOption.WinAreaSize),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class ArtistOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                ArtistOption.CanUseVent,
                false);
            factory.CreateIntOption(
                ArtistOption.WinAreaSize,
                15, 1, 100, 1);
            IRoleAbility.CreateCommonAbilityOption(
                factory, 3.0f);
        }
    }
}
