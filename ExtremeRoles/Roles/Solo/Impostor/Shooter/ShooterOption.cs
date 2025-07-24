using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Shooter
{
    public readonly record struct ShooterSpecificOption(
        int AwakeKillNum,
        int AwakeImpNum,
        bool IsInitAwake,
        bool NoneAwakeWhenShoot,
        float ShootKillCoolPenalty,
        bool CanCallMeeting,
        bool CanShootSelfCallMeeting,
        int MaxShootNum,
        int InitShootNum,
        int MaxMeetingShootNum,
        float ShootChargeTime,
        int ShootKillNum
    ) : IRoleSpecificOption;

    public class ShooterOptionLoader : ISpecificOptionLoader<ShooterSpecificOption>
    {
        public ShooterSpecificOption Load(IOptionLoader loader)
        {
            return new ShooterSpecificOption(
                loader.GetValue<ShooterOption, int>(
                    ShooterOption.AwakeKillNum),
                loader.GetValue<ShooterOption, int>(
                    ShooterOption.AwakeImpNum),
                loader.GetValue<ShooterOption, bool>(
                    ShooterOption.IsInitAwake),
                loader.GetValue<ShooterOption, bool>(
                    ShooterOption.NoneAwakeWhenShoot),
                loader.GetValue<ShooterOption, float>(
                    ShooterOption.ShootKillCoolPenalty),
                loader.GetValue<ShooterOption, bool>(
                    ShooterOption.CanCallMeeting),
                loader.GetValue<ShooterOption, bool>(
                    ShooterOption.CanShootSelfCallMeeting),
                loader.GetValue<ShooterOption, int>(
                    ShooterOption.MaxShootNum),
                loader.GetValue<ShooterOption, int>(
                    ShooterOption.InitShootNum),
                loader.GetValue<ShooterOption, int>(
                    ShooterOption.MaxMeetingShootNum),
                loader.GetValue<ShooterOption, float>(
                    ShooterOption.ShootChargeTime),
                loader.GetValue<ShooterOption, int>(
                    ShooterOption.ShootKillNum)
            );
        }
    }

    public class ShooterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                ShooterOption.IsInitAwake,
                false);
            factory.CreateIntOption(
                ShooterOption.AwakeKillNum,
                1, 0, 5, 1,
                format: OptionUnit.Shot);
            factory.CreateIntOption(
                ShooterOption.AwakeImpNum,
                1, 1, GameSystem.MaxImposterNum, 1);

            factory.CreateBoolOption(
                ShooterOption.NoneAwakeWhenShoot,
                true);
            factory.CreateFloatOption(
                ShooterOption.ShootKillCoolPenalty,
                5.0f, 0.0f, 30.0f, 0.5f,
                format: OptionUnit.Second);

            var meetingOps = factory.CreateBoolOption(
                ShooterOption.CanCallMeeting,
                true);

            factory.CreateBoolOption(
                ShooterOption.CanShootSelfCallMeeting,
                true, meetingOps,
                invert: true);

            var maxShootOps = factory.CreateIntOption(
               ShooterOption.MaxShootNum,
               1, 1, 14, 1,
               format: OptionUnit.Shot);

            var initShootOps = factory.CreateIntDynamicOption(
                ShooterOption.InitShootNum,
                0, 0, 1,
                format: OptionUnit.Shot,
                tempMaxValue: 14);

            var maxMeetingShootOps = factory.CreateIntDynamicOption(
                ShooterOption.MaxMeetingShootNum,
                1, 1, 1,
                format: OptionUnit.Shot,
                tempMaxValue: 14);

            factory.CreateFloatOption(
                ShooterOption.ShootChargeTime,
                90.0f, 30.0f, 120.0f, 5.0f,
                format: OptionUnit.Second);
            factory.CreateIntOption(
                ShooterOption.ShootKillNum,
                1, 0, 5, 1,
                format: OptionUnit.Shot);

            maxShootOps.AddWithUpdate(initShootOps);
            maxShootOps.AddWithUpdate(maxMeetingShootOps);
        }
    }
}
