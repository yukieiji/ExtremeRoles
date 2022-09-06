using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class Resurrecter : SingleRoleBase, IRoleAwake<RoleTypes>
    {
        public override bool IsAssignGhostRole
        {
            get => false;
        }

        public bool IsAwake
        {
            get
            {
                return GameSystem.IsLobby || this.awakeRole;
            }
        }

        public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

        public enum ResurrecterOption
        {
            AwakeTaskGage,
            ResurrectTaskGage,
            ResurrectDelayTime,
            CanResurrectOnExil
        }

        private bool awakeRole;
        private float awakeTaskGage;
        private bool awakeHasOtherVision;
        private float resurrectTaskGage;

        private bool canResurrect;
        private bool canResurrectOnExil;
        private bool isResurrected;
        private bool isExild;

        private bool activateResurrectTimer;
        private float resurrectTimer;

        public Resurrecter() : base(
            ExtremeRoleId.Resurrecter,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Resurrecter.ToString(),
            ColorPalette.SurvivorYellow,
            false, true, false, false)
        { }

        public string GetFakeOptionString() => "";


        public void Update(PlayerControl rolePlayer)
        {
            if (!this.awakeRole || !this.canResurrect)
            {
                float taskGage = Player.GetPlayerTaskGage(rolePlayer);

                if (taskGage >= this.awakeTaskGage && !this.awakeRole)
                {
                    this.awakeRole = true;
                    this.HasOtherVison = this.awakeHasOtherVision;
                }
                if (taskGage >= this.resurrectTaskGage && !this.canResurrect)
                {
                    this.canResurrect = true;
                }
            }

            if (this.isResurrected) { return; }

            if (!this.activateResurrectTimer &&
                this.canResurrect &&
                (!this.isExild || this.canResurrectOnExil) &&
                rolePlayer.Data.IsDead)
            {

                return;
            }

            if (this.activateResurrectTimer &&
                !this.isExild &&
                MeetingHud.Instance != null &&
                ExileController.Instance != null)
            {
                this.resurrectTimer -= Time.fixedDeltaTime;
                if (this.resurrectTimer <= 0.0f)
                {

                }
            }
        }

        public override string GetColoredRoleName(bool isTruthColor = false)
        {
            if (isTruthColor || IsAwake)
            {
                return base.GetColoredRoleName();
            }
            else
            {
                return Design.ColoedString(
                    Palette.White, Translation.GetString(RoleTypes.Crewmate.ToString()));
            }
        }
        public override string GetFullDescription()
        {
            if (IsAwake)
            {
                return Translation.GetString(
                    $"{this.Id}FullDescription");
            }
            else
            {
                return Translation.GetString(
                    $"{RoleTypes.Crewmate}FullDescription");
            }
        }

        public override string GetImportantText(bool isContainFakeTask = true)
        {
            if (IsAwake)
            {
                return base.GetImportantText(isContainFakeTask);

            }
            else
            {
                return Design.ColoedString(
                    Palette.White,
                    $"{this.GetColoredRoleName()}: {Translation.GetString("crewImportantText")}");
            }
        }

        public override string GetIntroDescription()
        {
            if (IsAwake)
            {
                return base.GetIntroDescription();
            }
            else
            {
                return Design.ColoedString(
                    Palette.CrewmateBlue,
                    CachedPlayerControl.LocalPlayer.Data.Role.Blurb);
            }
        }

        public override Color GetNameColor(bool isTruthColor = false)
        {
            if (isTruthColor || IsAwake)
            {
                return base.GetNameColor(isTruthColor);
            }
            else
            {
                return Palette.White;
            }
        }

        public override void ExiledAction(
            GameData.PlayerInfo rolePlayer)
        {
            if (this.canResurrectOnExil &&
                this.canResurrect && 
                !this.isResurrected)
            {
                this.activateResurrectTimer = true;
                this.isExild = false;
            }
            else
            {
                this.isExild = true;
            }
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer,
            PlayerControl killerPlayer)
        {
            this.isExild = false;
            if (this.canResurrect && 
                !this.isResurrected)
            {
                this.activateResurrectTimer = true;
            }
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateIntOption(
                ResurrecterOption.AwakeTaskGage,
                100, 0, 100, 10,
                parentOps,
                format: OptionUnit.Percentage);
            CreateIntOption(
                ResurrecterOption.ResurrectTaskGage,
                100, 70, 100, 10,
                parentOps,
                format: OptionUnit.Percentage);

            CreateFloatOption(
                ResurrecterOption.ResurrectDelayTime,
                3.0f, 0.0f, 10.0f, 0.1f,
                parentOps);

            CreateBoolOption(
                ResurrecterOption.CanResurrectOnExil,
                false, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.awakeTaskGage = (float)OptionHolder.AllOption[
                GetRoleOptionId(ResurrecterOption.AwakeTaskGage)].GetValue() / 100.0f;
            this.resurrectTaskGage = (float)OptionHolder.AllOption[
                GetRoleOptionId(ResurrecterOption.ResurrectTaskGage)].GetValue() / 100.0f;

            this.awakeHasOtherVision = this.HasOtherVison;
            this.canResurrect = false;
            this.isResurrected = false;

            if (this.awakeTaskGage <= 0.0f)
            {
                this.awakeRole = true;
                this.HasOtherVison = this.awakeHasOtherVision;
            }
            else
            {
                this.awakeRole = false;
                this.HasOtherVison = false;
            }
        }
    }
}
