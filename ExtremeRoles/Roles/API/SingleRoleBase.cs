using UnityEngine;

using ExtremeRoles.Module;


namespace ExtremeRoles.Roles.API
{
    public abstract partial class SingleRoleBase : RoleOptionBase
    {
        public virtual bool IsAssignGhostRole => true;

        public bool CanCallMeeting = true;
        public bool CanRepairSabotage = true;

        public bool CanUseAdmin = true;
        public bool CanUseSecurity = true;
        public bool CanUseVital = true;

        public bool HasTask = true;
        public bool UseVent = false;
        public bool UseSabotage = false;
        public bool HasOtherVison = false;
        public bool HasOtherKillCool = false;
        public bool HasOtherKillRange = false;
        public bool IsApplyEnvironmentVision = true;
        public bool IsWin = false;

        public bool FakeImposter = false;
        public bool IsBoost = false;
        public float MoveSpeed = 1.0f;

        public float Vison = 0f;
        public float KillCoolTime = 0f;
        public int KillRange = 1;

        public string RoleName;

        public ExtremeRoleId Id;
        public ExtremeRoleType Team;


        public OptionTab Tab => this.tab;

        protected Color NameColor;

        private OptionTab tab = OptionTab.General;

        public SingleRoleBase()
        { }
        public SingleRoleBase(
            ExtremeRoleId id,
            ExtremeRoleType team,
            string roleName,
            Color roleColor,
            bool canKill,
            bool hasTask,
            bool useVent,
            bool useSabotage,
            bool canCallMeeting = true,
            bool canRepairSabotage = true,
            bool canUseAdmin = true,
            bool canUseSecurity = true,
            bool canUseVital = true,
            OptionTab tab = OptionTab.General)
        {
            this.Id = id;
            this.Team = team;
            this.RoleName = roleName;
            this.NameColor = roleColor;
            this.CanKill = canKill;
            this.HasTask = hasTask;
            this.UseVent = useVent;
            this.UseSabotage = useSabotage;

            this.CanCallMeeting = canCallMeeting;
            this.CanRepairSabotage = canRepairSabotage;

            this.CanUseAdmin = canUseAdmin;
            this.CanUseSecurity = canUseSecurity;
            this.CanUseVital = canUseVital;

            if (tab == OptionTab.General)
            {
                switch (this.Team)
                {
                    case ExtremeRoleType.Crewmate:
                        this.tab = OptionTab.Crewmate;
                        break;
                    case ExtremeRoleType.Impostor:
                        this.tab = OptionTab.Impostor;
                        break;
                    case ExtremeRoleType.Neutral:
                        this.tab = OptionTab.Neutral;
                        break;
                    default:
                        this.tab = OptionTab.General;
                        break;
                }
            }
            else
            {
                this.tab = tab;
            }
        }

        public virtual SingleRoleBase Clone()
        {
            SingleRoleBase copy = (SingleRoleBase)this.MemberwiseClone();
            Color baseColor = this.NameColor;

            copy.NameColor = new Color(
                baseColor.r,
                baseColor.g,
                baseColor.b,
                baseColor.a);

            return copy;
        }

        public virtual bool IsTeamsWin() => this.IsWin;

        protected sealed override void CommonInit()
        {
            var baseOption = PlayerControl.GameOptions;
            var allOption = OptionHolder.AllOption;

            this.Vison = this.IsImpostor() ? baseOption.ImpostorLightMod : baseOption.CrewLightMod;
            
            this.KillCoolTime = baseOption.KillCooldown;
            this.KillRange = baseOption.KillDistance;

            this.IsApplyEnvironmentVision = !this.IsImpostor();


            this.HasOtherVison = allOption[
                GetRoleOptionId(RoleCommonOption.HasOtherVison)].GetValue();
            if (this.HasOtherVison)
            {
                this.Vison = allOption[
                    GetRoleOptionId(RoleCommonOption.Vison)].GetValue();
                this.IsApplyEnvironmentVision = allOption[
                    GetRoleOptionId(RoleCommonOption.ApplyEnvironmentVisionEffect)].GetValue();
            }

            if (this.CanKill)
            {
                this.HasOtherKillCool = allOption[
                    GetRoleOptionId(KillerCommonOption.HasOtherKillCool)].GetValue();
                if (this.HasOtherKillCool)
                {
                    this.KillCoolTime = allOption[
                        GetRoleOptionId(KillerCommonOption.KillCoolDown)].GetValue();
                }

                this.HasOtherKillRange = allOption[
                    GetRoleOptionId(KillerCommonOption.HasOtherKillRange)].GetValue();

                if (this.HasOtherKillRange)
                {
                    this.KillRange = allOption[
                        GetRoleOptionId(KillerCommonOption.KillRange)].GetValue();
                }
            }
        }
    }
}
