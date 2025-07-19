using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Delusioner
{
    public class DelusionerSpecificOption : IRoleSpecificOption
    {
        public int AwakeVoteNum { get; set; }
        public bool IsOnetimeAwake { get; set; }
        public float Range { get; set; }
        public int VoteCoolTimeReduceRate { get; set; }
        public int DeflectDamagePenaltyRate { get; set; }
        public bool IsIncludeLocalPlayer { get; set; }
        public bool IsIncludeSpawnPoint { get; set; }
        public bool EnableCounter { get; set; }
        public int AbilityUseCount { get; set; }
        public float AbilityCoolTime { get; set; }
    }

    public class DelusionerOptionLoader : ISpecificOptionLoader<DelusionerSpecificOption>
    {
        public DelusionerSpecificOption Load(IOptionLoader loader)
        {
            return new DelusionerSpecificOption
            {
                AwakeVoteNum = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.AwakeVoteNum),
                IsOnetimeAwake = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.IsOnetimeAwake),
                Range = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.Range),
                VoteCoolTimeReduceRate = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.VoteCoolTimeReduceRate),
                DeflectDamagePenaltyRate = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.DeflectDamagePenaltyRate),
                IsIncludeLocalPlayer = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.IsIncludeLocalPlayer),
                IsIncludeSpawnPoint = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.IsIncludeSpawnPoint),
                EnableCounter = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.EnableCounter),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                AbilityCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            };
        }
    }

    public class DelusionerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.AwakeVoteNum,
                3, 0, 8, 1,
                format: OptionUnit.VoteNum);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.IsOnetimeAwake,
                false);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.Range,
                2.5f, 0.0f, 7.5f, 0.1f);

            IRoleAbility.CreateAbilityCountOption(
                factory, 3, 25);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.VoteCoolTimeReduceRate,
                5, 0, 100, 5,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.DeflectDamagePenaltyRate,
                10, 0, 100, 5,
                format: OptionUnit.Percentage);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.IsIncludeLocalPlayer,
                true);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.IsIncludeSpawnPoint,
                false);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.EnableCounter,
                false);
        }
    }
}
