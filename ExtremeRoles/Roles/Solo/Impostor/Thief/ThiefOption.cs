using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Thief
{
    public readonly record struct ThiefSpecificOption(
        float Range,
        int SetTimeOffset,
        int SetNum,
        int PickUpTimeOffset,
        bool IsAddEffect,
        int AbilityUseCount,
        float AbilityActiveTime
    ) : IRoleSpecificOption;

    public class ThiefOptionLoader : ISpecificOptionLoader<ThiefSpecificOption>
    {
        public ThiefSpecificOption Load(IOptionLoader loader)
        {
            return new ThiefSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Thief.ThiefOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Thief.ThiefOption.Range),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Thief.ThiefOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Thief.ThiefOption.SetTimeOffset),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Thief.ThiefOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Thief.ThiefOption.SetNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Thief.ThiefOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Thief.ThiefOption.PickUpTimeOffset),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Thief.ThiefOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Thief.ThiefOption.IsAddEffect),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            );
        }
    }

    public class ThiefOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 5, 2.0f);
            factory.CreateFloatOption(ExtremeRoles.Roles.Solo.Impostor.Thief.ThiefOption.Range, 0.1f, 1.8f, 3.6f, 0.1f);
            factory.CreateIntOption(ExtremeRoles.Roles.Solo.Impostor.Thief.ThiefOption.SetTimeOffset, 30, 10, 360, 5, format: OptionUnit.Second);
            factory.CreateIntOption(ExtremeRoles.Roles.Solo.Impostor.Thief.ThiefOption.SetNum, 5, 1, 10, 1);
            factory.CreateIntOption(ExtremeRoles.Roles.Solo.Impostor.Thief.ThiefOption.PickUpTimeOffset, 6, 1, 60, 1, format: OptionUnit.Second);
            factory.CreateBoolOption(ExtremeRoles.Roles.Solo.Impostor.Thief.ThiefOption.IsAddEffect, true);
        }
    }
}
