using UnityEngine;

using ExtremeRoles.Module;
using AmongUs.GameOptions;

namespace ExtremeRoles.Roles.API
{
    public abstract partial class SingleRoleBase : RoleOptionBase
    {
        public virtual bool IsAssignGhostRole => true;

        public OptionTab Tab { get; } = OptionTab.General;
        public virtual string RoleName => this.RawRoleName;

        public bool CanCallMeeting = true;
        public bool CanRepairSabotage = true;

        public bool CanUseAdmin = true;
        public bool CanUseSecurity = true;
        public bool CanUseVital = true;

        public bool HasTask = true;
        public bool UseVent = false;
        public bool UseSabotage = false;
        public bool HasOtherVision = false;
        public bool HasOtherKillCool = false;
        public bool HasOtherKillRange = false;
        public bool IsApplyEnvironmentVision = true;
        public bool IsWin = false;

        public bool FakeImposter = false;
        public bool IsBoost = false;
        public float MoveSpeed = 1.0f;

        public float Vision = 0f;
        public float KillCoolTime = 0f;
        public int KillRange = 1;

        public ExtremeRoleId Id;
        public ExtremeRoleType Team;

        protected Color NameColor;

        protected string RawRoleName;

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
            this.RawRoleName = roleName;
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
                        this.Tab = OptionTab.Crewmate;
                        break;
                    case ExtremeRoleType.Impostor:
                        this.Tab = OptionTab.Impostor;
                        break;
                    case ExtremeRoleType.Neutral:
                        this.Tab = OptionTab.Neutral;
                        break;
                    default:
                        this.Tab = OptionTab.General;
                        break;
                }
            }
            else
            {
                this.Tab = tab;
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

        protected override void CommonInit()
        {
            var baseOption = GameOptionsManager.Instance.CurrentGameOptions;
            var allOption = OptionHolder.AllOption;

            this.Vision = this.IsImpostor() ? 
                baseOption.GetFloat(FloatOptionNames.ImpostorLightMod) : 
                baseOption.GetFloat(FloatOptionNames.CrewLightMod);
            
            this.KillCoolTime = baseOption.GetFloat(FloatOptionNames.KillCooldown);
            this.KillRange = baseOption.GetInt(Int32OptionNames.KillDistance);

            this.IsApplyEnvironmentVision = !this.IsImpostor();


            this.HasOtherVision = allOption[
                GetRoleOptionId(RoleCommonOption.HasOtherVision)].GetValue();
            if (this.HasOtherVision)
            {
                this.Vision = allOption[
                    GetRoleOptionId(RoleCommonOption.Vision)].GetValue();
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
