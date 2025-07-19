using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Captain
{
    public class CaptainSpecificOption : IRoleSpecificOption
    {
        public int AwakeTaskGage { get; set; }
        public float ChargeVoteWhenSkip { get; set; }
        public float AwakedDefaultVoteNum { get; set; }
    }

    public class CaptainOptionLoader : ISpecificOptionLoader<CaptainSpecificOption>
    {
        public CaptainSpecificOption Load(IOptionLoader loader)
        {
            return new CaptainSpecificOption
            {
                AwakeTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Captain.CaptainOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Captain.CaptainOption.AwakeTaskGage),
                ChargeVoteWhenSkip = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Captain.CaptainOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Captain.CaptainOption.ChargeVoteWhenSkip),
                AwakedDefaultVoteNum = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Captain.CaptainOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Captain.CaptainOption.AwakedDefaultVoteNum)
            };
        }
    }

    public class CaptainOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Captain.CaptainOption.AwakeTaskGage,
                70, 0, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Captain.CaptainOption.ChargeVoteWhenSkip,
                0.7f, 0.1f, 100.0f, 0.1f,
                format: OptionUnit.VoteNum);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Captain.CaptainOption.AwakedDefaultVoteNum,
                0.0f, 0.0f, 100.0f, 0.1f,
                format: OptionUnit.VoteNum);
        }
    }
}
