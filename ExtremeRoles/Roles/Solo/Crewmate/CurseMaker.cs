using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class CurseMaker : SingleRoleBase, IRoleAbility, IRoleMurderPlayerHock, IRoleUpdate
    {
        public enum CurseMakerOption
        {
            CursingRange,
            AdditionalKillCool,
            SearchDeadBodyTime,
        }


        public GameData.PlayerInfo targetBody;
        public byte deadBodyId;

        private float additionalKillCool = 1.0f;
        private float searchDeadBodyTime = 1.0f;
        private float deadBodyCheckRange = 1.0f;

        private string defaultButtonText;
        private string cursingText;

        public RoleAbilityButtonBase Button
        {
            get => this.curseButton;
            set
            {
                this.curseButton = value;
            }
        }
        private RoleAbilityButtonBase curseButton;

        public CurseMaker() : base(
            ExtremeRoleId.CurseMaker,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.CurseMaker.ToString(),
            ColorPalette.CurseMakerViolet,
            false, true, false, false)
        { }

        public void CreateAbility()
        {
            this.defaultButtonText = Translation.GetString("curse");

            this.CreateAbilityCountButton(
                this.defaultButtonText,
                Loader.CreateSpriteFromResources(
                    Path.TestButton),
                checkAbility: CheckAbility,
                abilityCleanUp: CleanUp);
            this.Button.SetLabelToCrewmate();
        }

        public bool IsAbilityUse()
        {
            this.targetBody = Player.GetDeadBodyInfo(
                this.deadBodyCheckRange);
            return this.IsCommonUse() && this.targetBody != null;
        }

        public void CleanUp()
        {

            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.CleanDeadBody,
                new List<byte> { this.deadBodyId });

            RPCOperator.CleanDeadBody(this.deadBodyId);
        }

        public bool CheckAbility()
        {
            this.targetBody = Player.GetDeadBodyInfo(
                this.deadBodyCheckRange);

            bool result;

            if (this.targetBody == null)
            {
                result = false;
            }
            else
            {
                result = this.deadBodyCheckRange == this.targetBody.PlayerId;
            }

            this.Button.ButtonText = result ? this.cursingText : this.defaultButtonText;

            return result;
        }

        public bool UseAbility()
        {
            this.deadBodyCheckRange = this.targetBody.PlayerId;
            return true;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {

            CustomOption.Create(
                GetRoleOptionId((int)CurseMakerOption.CursingRange),
                string.Concat(
                    this.RoleName,
                    CurseMakerOption.CursingRange.ToString()),
                2.5f, 0.5f, 5.0f, 0.5f,
                parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)CurseMakerOption.AdditionalKillCool),
                string.Concat(
                    this.RoleName,
                    CurseMakerOption.AdditionalKillCool.ToString()),
                5.0f, 1.0f, 30.0f, 0.1f,
                parentOps, format: "unitSeconds");

            this.CreateAbilityCountOption(
                parentOps, 1, 3, 5.0f);

            CustomOption.Create(
                GetRoleOptionId((int)CurseMakerOption.SearchDeadBodyTime),
                string.Concat(
                    this.RoleName,
                    CurseMakerOption.SearchDeadBodyTime.ToString()),
                60.0f, 45.0f, 90.0f, 0.1f,
                parentOps, format: "unitSeconds");

        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();

            var allOption = OptionHolder.AllOption;

            this.additionalKillCool = allOption[
                GetRoleOptionId((int)CurseMakerOption.AdditionalKillCool)].GetValue();
            this.deadBodyCheckRange = allOption[
                GetRoleOptionId((int)CurseMakerOption.CursingRange)].GetValue();
            this.searchDeadBodyTime = allOption[
                GetRoleOptionId((int)CurseMakerOption.SearchDeadBodyTime)].GetValue();

            this.cursingText = Translation.GetString("cursing");

        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void HockMuderPlayer(PlayerControl source, PlayerControl target)
        {
            throw new System.NotImplementedException();
        }

        public void Update(PlayerControl rolePlayer)
        {
            throw new System.NotImplementedException();
        }
    }
}
