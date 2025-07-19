using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Neutral.Miner
{
    public class MinerSpecificOption : IRoleSpecificOption
    {
        public bool LinkingAllVent { get; set; }
        public float MineKillRange { get; set; }
        public bool CanShowMine { get; set; }
        public int RolePlayerShowMode { get; set; }
        public int AnotherPlayerShowMode { get; set; }
        public bool CanShowNoneActiveAnotherPlayer { get; set; }
        public float NoneActiveTime { get; set; }
        public bool ShowKillLog { get; set; }
        public float AbilityCoolTime { get; set; }
    }

    public class MinerOptionLoader : ISpecificOptionLoader<MinerSpecificOption>
    {
        public MinerSpecificOption Load(IOptionLoader loader)
        {
            return new MinerSpecificOption
            {
                LinkingAllVent = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption.LinkingAllVent),
                MineKillRange = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption, float>(
                    ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption.MineKillRange),
                CanShowMine = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption.CanShowMine),
                RolePlayerShowMode = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption.RolePlayerShowMode),
                AnotherPlayerShowMode = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption.AnotherPlayerShowMode),
                CanShowNoneActiveAnotherPlayer = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption.CanShowNoneActiveAnotherPlayer),
                NoneActiveTime = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption, float>(
                    ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption.NoneActiveTime),
                ShowKillLog = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption.ShowKillLog),
                AbilityCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            };
        }
    }

    public class MinerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption.LinkingAllVent,
                false);
            IRoleAbility.CreateCommonAbilityOption(
                factory, 2.0f);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption.MineKillRange,
                1.8f, 0.5f, 5f, 0.1f);
            var showOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption.CanShowMine,
                false);
            factory.CreateSelectionOption(
                ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption.RolePlayerShowMode,
                new[]
                {
                    ExtremeRoles.Roles.Solo.Neutral.Miner.ShowMode.MineSeeOnlySe.ToString(),
                    ExtremeRoles.Roles.Solo.Neutral.Miner.ShowMode.MineSeeOnlyImg.ToString(),
                    ExtremeRoles.Roles.Solo.Neutral.Miner.ShowMode.MineSeeBoth.ToString(),
                }, showOpt);
            var anotherPlayerShowMode = factory.CreateSelectionOption<ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption, ExtremeRoles.Roles.Solo.Neutral.Miner.ShowMode>(
                ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption.AnotherPlayerShowMode, showOpt);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption.CanShowNoneActiveAnotherPlayer,
                false, anotherPlayerShowMode);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption.NoneActiveTime,
                20.0f, 1.0f, 45f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Miner.MinerOption.ShowKillLog,
                true);
        }
    }
}
