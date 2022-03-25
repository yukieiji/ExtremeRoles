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
    public class Fencer : SingleRoleBase, IRoleAbility, IRoleUpdate
    {
        public enum FencerOption
        {
            ResetTime
        }

        public RoleAbilityButtonBase Button
        {
            get => this.takeTaskButton;
            set
            {
                this.takeTaskButton = value;
            }
        }

        public bool IsCounter = false;
        public float Timer = 0.0f;
        public float MaxTime = 120f;

        private RoleAbilityButtonBase takeTaskButton;

        public Fencer() : base(
            ExtremeRoleId.Fencer,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Fencer.ToString(),
            ColorPalette.FencerPin,
            false, true, false, false)
        { }


        public static void CounterOn(byte rolePlayerId)
        {
            var fencer = ExtremeRoleManager.GetSafeCastedRole<Fencer>(rolePlayerId);

            if (fencer != null)
            {
                fencer.IsCounter = true;
            }
        }

        public static void CounterOff(byte rolePlayerId)
        {
            var fencer = ExtremeRoleManager.GetSafeCastedRole<Fencer>(rolePlayerId);

            if (fencer != null)
            {
                fencer.IsCounter = false;
            }
        }

        public static void EnableKillButton(byte rolePlayerId)
        {
            if (PlayerControl.LocalPlayer.PlayerId != rolePlayerId) { return; }

            var fencer = ExtremeRoleManager.GetSafeCastedRole<Fencer>(rolePlayerId);

            if (fencer != null)
            {

                if (Minigame.Instance)
                {
                    Minigame.Instance.ForceClose();
                }

                PlayerControl.LocalPlayer.SetKillTimer(0.1f);

                fencer.CanKill = true;
                fencer.Timer = fencer.MaxTime;
            }
        }


        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Translation.GetString("counter"),
                Loader.CreateSpriteFromResources(
                    Path.TestButton),
                abilityCleanUp: this.CleanUp);
            this.Button.SetLabelToCrewmate();
        }
        public void CleanUp()
        {
            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.FencerCounterOff,
                new List<byte>
                {
                    PlayerControl.LocalPlayer.PlayerId,
                });
            CounterOff(PlayerControl.LocalPlayer.PlayerId);
        }

        public bool UseAbility()
        {
            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.FencerCounterOn,
                new List<byte>
                {
                    PlayerControl.LocalPlayer.PlayerId,
                });
            CounterOn(PlayerControl.LocalPlayer.PlayerId);
            return true;
        }

        public bool IsAbilityUse()
        {
            return this.IsCommonUse();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            this.CleanUp();
            this.CanKill = false;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (this.Timer <= 0.0f)
            {
                this.CanKill = false;
                return;
            }

            this.Timer -= Time.fixedDeltaTime;

        }

        public override bool TryRolePlayerKilledFrom(
            PlayerControl rolePlayer, PlayerControl fromPlayer)
        {

            if (this.IsCounter)
            {
                RPCOperator.Call(
                    PlayerControl.LocalPlayer.NetId,
                    RPCOperator.Command.FencerEnableKillButton,
                    new List<byte>
                    {
                        rolePlayer.PlayerId,
                    });
                EnableKillButton(rolePlayer.PlayerId);

                return false;
            }

            return true;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleOptionId((int)FencerOption.ResetTime),
                string.Concat(
                    this.RoleName,
                    FencerOption.ResetTime.ToString()),
                5.0f, 2.5f, 30.0f, 0.5f,
                parentOps, format: OptionUnit.Second);

            this.CreateAbilityCountOption(
                parentOps, 2, 5, 3.0f);
        }

        protected override void RoleSpecificInit()
        {
            this.Timer = 0.0f;
            this.MaxTime = OptionHolder.AllOption[
                GetRoleOptionId((int)FencerOption.ResetTime)].GetValue();

            this.RoleAbilityInit();
        }
    }
}
