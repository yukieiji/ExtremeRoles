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
    public class Sorcery : SingleRoleBase, IRoleAbility, IRoleMurderPlayerHock, IRoleUpdate
    {
        public enum SorceryOption
        {
            CursingRange,
        }


        public GameData.PlayerInfo targetBody;
        public byte deadBodyId;

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

        public Sorcery() : base(
            ExtremeRoleId.Sorcery,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Sorcery.ToString(),
            Palette.ImpostorRed,
            false, true, false, false)
        { }

        public void CreateAbility()
        {
            this.defaultButtonText = Translation.GetString("evolve");

            this.CreateAbilityCountButton(
                this.defaultButtonText,
                Loader.CreateSpriteFromResources(
                    Path.EvolverEvolved),
                checkAbility: CheckAbility,
                abilityCleanUp: CleanUp);
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

        protected override void CreateSpecificOption(CustomOptionBase parentOps)
        {
            throw new System.NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            throw new System.NotImplementedException();
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
