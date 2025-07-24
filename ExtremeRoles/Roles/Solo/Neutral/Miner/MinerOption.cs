using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Neutral.Miner.MinerRole;

namespace ExtremeRoles.Roles.Solo.Neutral.Miner
{
    public readonly record struct MinerSpecificOption(
        bool LinkingAllVent,
        float MineKillRange,
        bool CanShowMine,
        int RolePlayerShowMode,
        int AnotherPlayerShowMode,
        bool CanShowNoneActiveAnotherPlayer,
        float NoneActiveTime,
        bool ShowKillLog,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class MinerOptionLoader : ISpecificOptionLoader<MinerSpecificOption>
    {
        public MinerSpecificOption Load(IOptionLoader loader)
        {
            return new MinerSpecificOption(
                loader.GetValue<MinerOption, bool>(
                    MinerOption.LinkingAllVent),
                loader.GetValue<MinerOption, float>(
                    MinerOption.MineKillRange),
                loader.GetValue<MinerOption, bool>(
                    Miner.MinerOption.CanShowMine),
                loader.GetValue<MinerOption, int>(
                    MinerOption.RolePlayerShowMode),
                loader.GetValue<MinerOption, int>(
                    MinerOption.AnotherPlayerShowMode),
                loader.GetValue<MinerOption, bool>(
                    MinerOption.CanShowNoneActiveAnotherPlayer),
                loader.GetValue<MinerOption, float>(
                    MinerOption.NoneActiveTime),
                loader.GetValue<MinerOption, bool>(
                    MinerOption.ShowKillLog),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class MinerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                MinerOption.LinkingAllVent,
                false);
            IRoleAbility.CreateCommonAbilityOption(
                factory, 2.0f);
            factory.CreateFloatOption(
                MinerOption.MineKillRange,
                1.8f, 0.5f, 5f, 0.1f);
            var showOpt = factory.CreateBoolOption(
                MinerOption.CanShowMine,
                false);
            factory.CreateSelectionOption(
                MinerOption.RolePlayerShowMode,
                new[]
                {
                    ShowMode.MineSeeOnlySe.ToString(),
                    ShowMode.MineSeeOnlyImg.ToString(),
                    ShowMode.MineSeeBoth.ToString(),
                }, showOpt);
            var anotherPlayerShowMode = factory.CreateSelectionOption<MinerOption, ShowMode>(
                MinerOption.AnotherPlayerShowMode, showOpt);
            factory.CreateBoolOption(
                MinerOption.CanShowNoneActiveAnotherPlayer,
                false, anotherPlayerShowMode);
            factory.CreateFloatOption(
                MinerOption.NoneActiveTime,
                20.0f, 1.0f, 45f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateBoolOption(
                MinerOption.ShowKillLog,
                true);
        }
    }
}
