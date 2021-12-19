using ExtremeRoles.Helper;
using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.Combination
{
    public class LoverManager : FlexibleCombRoleManagerBase
    {
        public enum LoverOption
        {
            IsNeutral,
            BecomeKiller,
            WhenKilledAllDeath,
            DeathWhenOnlyOne
        }

        public LoverManager() : base(
            ExtremeRoleId.Lover.ToString(),
            ColorPalette.LoverPink,
            new Lover())
        { }

        protected override void CreateKillerOption(
            CustomOption parentOps)
        {
            var becomeKillerOps = CustomOption.Create(
                GetRoleSettingId((int)LoverOption.BecomeKiller),
                Design.ConcatString(
                    this.BaseRole.RoleName,
                    LoverOption.BecomeKiller.ToString()),
                false, parentOps);

            var killCoolSetting = CustomOption.Create(
                GetRoleSettingId(KillerCommonSetting.HasOtherKillCool),
                Design.ConcatString(
                    this.BaseRole.RoleName,
                    KillerCommonSetting.HasOtherKillCool.ToString()),
                false, becomeKillerOps);
            CustomOption.Create(
                GetRoleSettingId(KillerCommonSetting.KillCoolDown),
                Design.ConcatString(
                    this.BaseRole.RoleName,
                    KillerCommonSetting.KillCoolDown.ToString()),
                30f, 2.5f, 120f, 2.5f,
                killCoolSetting, format: "unitSeconds");

            var killRangeSetting = CustomOption.Create(
                GetRoleSettingId(KillerCommonSetting.HasOtherKillRange),
                Design.ConcatString(
                    this.BaseRole.RoleName,
                    KillerCommonSetting.HasOtherKillRange.ToString()),
                false, becomeKillerOps);
            CustomOption.Create(
                GetRoleSettingId(KillerCommonSetting.KillRange),
                Design.ConcatString(
                    this.BaseRole.RoleName,
                    KillerCommonSetting.KillRange.ToString()),
                OptionsHolder.KillRange,
                killRangeSetting);
        }

        protected override void CreateSpecificOption(
            CustomOption parentOps)
        {
            CustomOption.Create(
                GetRoleSettingId((int)LoverOption.IsNeutral),
                Design.ConcatString(
                    this.BaseRole.RoleName,
                    LoverOption.IsNeutral.ToString()),
                true, parentOps);

            var deathOps = CustomOption.Create(
                GetRoleSettingId((int)LoverOption.WhenKilledAllDeath),
                Design.ConcatString(
                    this.BaseRole.RoleName,
                    LoverOption.WhenKilledAllDeath.ToString()),
                true, parentOps);
            CustomOption.Create(
                GetRoleSettingId((int)LoverOption.DeathWhenOnlyOne),
                Design.ConcatString(
                    this.BaseRole.RoleName,
                    LoverOption.DeathWhenOnlyOne.ToString()),
                true, deathOps);
        }

        protected override void RoleSpecificInit()
        {
            var allOption = OptionsHolder.AllOptions;

            foreach (Lover lover in Roles)
            {

                lover.Teams = allOption[
                    GetRoleSettingId((int)LoverOption.IsNeutral)].GetBool() ? ExtremeRoleType.Neutral : ExtremeRoleType.Crewmate;
                lover.BecomeKiller = allOption[
                    GetRoleSettingId((int)LoverOption.BecomeKiller)].GetBool();
                lover.DeathWhenOnlyOne = allOption[
                    GetRoleSettingId((int)LoverOption.DeathWhenOnlyOne)].GetBool();
                lover.WhenKilledAllDeath = allOption[
                    GetRoleSettingId((int)LoverOption.WhenKilledAllDeath)].GetBool();

                lover.HasOtherKillCool = allOption[
                    GetRoleSettingId(KillerCommonSetting.HasOtherKillCool)].GetBool();
                lover.KillCoolTime = allOption[
                    GetRoleSettingId(KillerCommonSetting.KillCoolDown)].GetFloat();
                lover.HasOtherKillRange = allOption[
                    GetRoleSettingId(KillerCommonSetting.HasOtherKillRange)].GetBool();
                lover.KillRange = allOption[
                    GetRoleSettingId(KillerCommonSetting.KillRange)].GetSelection();
            };
        }
    }
    public class Lover : MultiAssignRoleAbs
    {

        public bool BecomeKiller = false;
        public bool DeathWhenOnlyOne = true;
        public bool WhenKilledAllDeath = true;

        public Lover() : base(
            ExtremeRoleId.Lover,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Lover.ToString(),
            ColorPalette.LoverPink,
            false, true, false, false)
        {}

        protected override void CreateSpecificOption(
            CustomOption parentOps)
        {}

        protected override void RoleSpecificInit()
        {
            this.BecomeKiller = false;
            this.DeathWhenOnlyOne = true;
            this.WhenKilledAllDeath = true;
        }
    }
}
