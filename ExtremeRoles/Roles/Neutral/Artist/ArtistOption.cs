using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Neutral.Artist
{
    public class ArtistSpecificOption : IRoleSpecificOption
    {
        public bool CanUseVent { get; set; }
        public int WinAreaSize { get; set; }
        public float AbilityCoolTime { get; set; }
    }

    public class ArtistOptionLoader : ISpecificOptionLoader<ArtistSpecificOption>
    {
        public ArtistSpecificOption Load(IOptionLoader loader)
        {
            return new ArtistSpecificOption
            {
                CanUseVent = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Artist.ArtistOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Artist.ArtistOption.CanUseVent),
                WinAreaSize = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Artist.ArtistOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Artist.ArtistOption.WinAreaSize),
                AbilityCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            };
        }
    }

    public class ArtistOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Artist.ArtistOption.CanUseVent,
                false);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Neutral.Artist.ArtistOption.WinAreaSize,
                15, 1, 100, 1);
            IRoleAbility.CreateCommonAbilityOption(
                factory, 3.0f);
        }
    }
}
