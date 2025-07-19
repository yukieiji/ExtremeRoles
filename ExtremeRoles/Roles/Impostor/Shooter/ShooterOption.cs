using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Shooter
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.AwakeKillNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.AwakeImpNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.IsInitAwake),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.NoneAwakeWhenShoot),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.ShootKillCoolPenalty),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.CanCallMeeting),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.CanShootSelfCallMeeting),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.MaxShootNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.InitShootNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.MaxMeetingShootNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.ShootChargeTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.ShootKillNum)
            );
        }
    }

    public class ShooterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.IsInitAwake,
                false);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.AwakeKillNum,
                1, 0, 5, 1,
                format: OptionUnit.Shot);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.AwakeImpNum,
                1, 1, GameSystem.MaxImposterNum, 1);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.NoneAwakeWhenShoot,
                true);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.ShootKillCoolPenalty,
                5.0f, 0.0f, 30.0f, 0.5f,
                format: OptionUnit.Second);

            var meetingOps = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.CanCallMeeting,
                true);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.CanShootSelfCallMeeting,
                true, meetingOps,
                invert: true);

            var maxShootOps = factory.CreateIntOption(
               ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.MaxShootNum,
               1, 1, 14, 1,
               format: OptionUnit.Shot);

            var initShootOps = factory.CreateIntDynamicOption(
                ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.InitShootNum,
                0, 0, 1,
                format: OptionUnit.Shot,
                tempMaxValue: 14);

            var maxMeetingShootOps = factory.CreateIntDynamicOption(
                ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.MaxMeetingShootNum,
                1, 1, 1,
                format: OptionUnit.Shot,
                tempMaxValue: 14);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.ShootChargeTime,
                90.0f, 30.0f, 120.0f, 5.0f,
                format: OptionUnit.Second);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Shooter.ShooterOption.ShootKillNum,
                1, 0, 5, 1,
                format: OptionUnit.Shot);

            maxShootOps.AddWithUpdate(initShootOps);
            maxShootOps.AddWithUpdate(maxMeetingShootOps);
        }
    }
}
