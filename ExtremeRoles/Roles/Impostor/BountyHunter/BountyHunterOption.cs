using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.BountyHunter
{
    public class BountyHunterSpecificOption : IRoleSpecificOption
    {
        public float TargetUpdateTime { get; set; }
        public float TargetKillCoolTime { get; set; }
        public float NoneTargetKillCoolTime { get; set; }
        public bool IsShowArrow { get; set; }
        public float ArrowUpdateCycle { get; set; }
    }

    public class BountyHunterOptionLoader : ISpecificOptionLoader<BountyHunterSpecificOption>
    {
        public BountyHunterSpecificOption Load(IOptionLoader loader)
        {
            return new BountyHunterSpecificOption
            {
                TargetUpdateTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.BountyHunter.BountyHunterOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.BountyHunter.BountyHunterOption.TargetUpdateTime),
                TargetKillCoolTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.BountyHunter.BountyHunterOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.BountyHunter.BountyHunterOption.TargetKillCoolTime),
                NoneTargetKillCoolTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.BountyHunter.BountyHunterOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.BountyHunter.BountyHunterOption.NoneTargetKillCoolTime),
                IsShowArrow = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.BountyHunter.BountyHunterOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.BountyHunter.BountyHunterOption.IsShowArrow),
                ArrowUpdateCycle = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.BountyHunter.BountyHunterOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.BountyHunter.BountyHunterOption.ArrowUpdateCycle)
            };
        }
    }

    public class BountyHunterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.BountyHunter.BountyHunterOption.TargetUpdateTime,
                60f, 30.0f, 120f, 0.5f,
                format: OptionUnit.Second);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.BountyHunter.BountyHunterOption.TargetKillCoolTime,
                5f, 1.0f, 60f, 0.5f,
                format: OptionUnit.Second);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.BountyHunter.BountyHunterOption.NoneTargetKillCoolTime,
                45f, 1.0f, 120f, 0.5f,
                format: OptionUnit.Second);

            var arrowOption = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.BountyHunter.BountyHunterOption.IsShowArrow,
                false);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.BountyHunter.BountyHunterOption.ArrowUpdateCycle,
                10f, 1.0f, 120f, 0.5f,
                arrowOption, format: OptionUnit.Second);
        }
    }
}
