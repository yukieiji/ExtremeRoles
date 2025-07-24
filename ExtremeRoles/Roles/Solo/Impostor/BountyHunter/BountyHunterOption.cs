using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.BountyHunter.BountyHunterRole;

namespace ExtremeRoles.Roles.Solo.Impostor.BountyHunter
{
    public readonly record struct BountyHunterSpecificOption(
        float TargetUpdateTime,
        float TargetKillCoolTime,
        float NoneTargetKillCoolTime,
        bool IsShowArrow,
        float ArrowUpdateCycle
    ) : IRoleSpecificOption;

    public class BountyHunterOptionLoader : ISpecificOptionLoader<BountyHunterSpecificOption>
    {
        public BountyHunterSpecificOption Load(IOptionLoader loader)
        {
            return new BountyHunterSpecificOption(
                loader.GetValue<BountyHunterOption, float>(
                    BountyHunterOption.TargetUpdateTime),
                loader.GetValue<BountyHunterOption, float>(
                    BountyHunterOption.TargetKillCoolTime),
                loader.GetValue<BountyHunterOption, float>(
                    BountyHunterOption.NoneTargetKillCoolTime),
                loader.GetValue<BountyHunterOption, bool>(
                    BountyHunterOption.IsShowArrow),
                loader.GetValue<BountyHunterOption, float>(
                    BountyHunterOption.ArrowUpdateCycle)
            );
        }
    }

    public class BountyHunterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateFloatOption(
                BountyHunterOption.TargetUpdateTime,
                60f, 30.0f, 120f, 0.5f,
                format: OptionUnit.Second);

            factory.CreateFloatOption(
                BountyHunterOption.TargetKillCoolTime,
                5f, 1.0f, 60f, 0.5f,
                format: OptionUnit.Second);

            factory.CreateFloatOption(
                BountyHunterOption.NoneTargetKillCoolTime,
                45f, 1.0f, 120f, 0.5f,
                format: OptionUnit.Second);

            var arrowOption = factory.CreateBoolOption(
                BountyHunterOption.IsShowArrow,
                false);

            factory.CreateFloatOption(
                BountyHunterOption.ArrowUpdateCycle,
                10f, 1.0f, 120f, 0.5f,
                arrowOption, format: OptionUnit.Second);
        }
    }
}
