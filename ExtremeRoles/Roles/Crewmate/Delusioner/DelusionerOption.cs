using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Delusioner
{
    public readonly record struct DelusionerSpecificOption(
        int AwakeVoteNum,
        bool IsOnetimeAwake,
        float Range,
        int VoteCoolTimeReduceRate,
        int DeflectDamagePenaltyRate,
        bool IsIncludeLocalPlayer,
        bool IsIncludeSpawnPoint,
        bool EnableCounter,
        int AbilityUseCount,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class DelusionerOptionLoader : ISpecificOptionLoader<DelusionerSpecificOption>
    {
        public DelusionerSpecificOption Load(IOptionLoader loader)
        {
            return new DelusionerSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.AwakeVoteNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.IsOnetimeAwake),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.Range),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.VoteCoolTimeReduceRate),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.DeflectDamagePenaltyRate),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.IsIncludeLocalPlayer),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.IsIncludeSpawnPoint),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerOption.EnableCounter),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
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
