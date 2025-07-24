using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.Captain.CaptainRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Captain
{
    public readonly record struct CaptainSpecificOption(
        int AwakeTaskGage,
        float ChargeVoteWhenSkip,
        float AwakedDefaultVoteNum
    ) : IRoleSpecificOption;

    public class CaptainOptionLoader : ISpecificOptionLoader<CaptainSpecificOption>
    {
        public CaptainSpecificOption Load(IOptionLoader loader)
        {
            return new CaptainSpecificOption(
                loader.GetValue<CaptainOption, int>(
                    CaptainOption.AwakeTaskGage),
                loader.GetValue<CaptainOption, float>(
                    CaptainOption.ChargeVoteWhenSkip),
                loader.GetValue<CaptainOption, float>(
                    CaptainOption.AwakedDefaultVoteNum)
            );
        }
    }

    public class CaptainOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                CaptainOption.AwakeTaskGage,
                70, 0, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateFloatOption(
                CaptainOption.ChargeVoteWhenSkip,
                0.7f, 0.1f, 100.0f, 0.1f,
                format: OptionUnit.VoteNum);
            factory.CreateFloatOption(
                CaptainOption.AwakedDefaultVoteNum,
                0.0f, 0.0f, 100.0f, 0.1f,
                format: OptionUnit.VoteNum);
        }
    }
}
