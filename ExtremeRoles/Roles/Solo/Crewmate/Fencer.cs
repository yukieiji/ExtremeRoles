using UnityEngine;
using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class Fencer : SingleRoleBase, IRoleAbility, IRoleUpdate
    {
        public enum FencerOption
        {
            ResetTime
        }

        public enum FencerAbility : byte
        {
            CounterOn,
            CounterOff,
            ActivateKillButton
        }

        public ExtremeAbilityButton Button
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

        private ExtremeAbilityButton takeTaskButton;

        public Fencer() : base(
            ExtremeRoleId.Fencer,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Fencer.ToString(),
            ColorPalette.FencerPin,
            false, true, false, false)
        { }

        public static void Ability(ref MessageReader reader)
        {
            byte rolePlayerId = reader.ReadByte();
            FencerAbility abilityType = (FencerAbility)reader.ReadByte();

            var fencer = ExtremeRoleManager.GetSafeCastedRole<Fencer>(rolePlayerId);
            if (fencer == null) { return; }

            switch (abilityType)
            {
                case FencerAbility.CounterOn:
                    counterOn(fencer);
                    break;
                case FencerAbility.CounterOff:
                    counterOff(fencer);
                    break;
                case FencerAbility.ActivateKillButton:
                    enableKillButton(fencer, rolePlayerId);
                    break;
                default:
                    break;
            }

        }

        private static void counterOn(Fencer fencer)
        {
            fencer.IsCounter = true;
        }

        public static void counterOff(Fencer fencer)
        {
            fencer.IsCounter = false;
        }

        private static void enableKillButton(
            Fencer fencer, byte rolePlayerId)
        {
            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            if (localPlayer.PlayerId != rolePlayerId) { return; }

            if (MapBehaviour.Instance)
            {
                MapBehaviour.Instance.Close();
            }
            if (Minigame.Instance)
            {
                Minigame.Instance.ForceClose();
            }

            fencer.CanKill = true;
            localPlayer.killTimer = 0.1f;

            fencer.Timer = fencer.MaxTime;
        }

        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                "counter",
                Loader.CreateSpriteFromResources(
                    Path.FencerCounter),
                abilityOff: this.CleanUp,
                isReduceOnActive: true);
            this.Button.SetLabelToCrewmate();
        }
        public void CleanUp()
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.FencerAbility))
            {
                caller.WriteByte(
                    CachedPlayerControl.LocalPlayer.PlayerId);
                caller.WriteByte((byte)FencerAbility.CounterOff);
            }
            counterOff(this);
        }

        public bool UseAbility()
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.FencerAbility))
            {
                caller.WriteByte(
                    CachedPlayerControl.LocalPlayer.PlayerId);
                caller.WriteByte((byte)FencerAbility.CounterOn);
            }
            counterOn(this);
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
                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.FencerAbility))
                {
                    caller.WriteByte(
                        rolePlayer.PlayerId);
                    caller.WriteByte((byte)FencerAbility.ActivateKillButton);
                }
                enableKillButton(this, rolePlayer.PlayerId);
                Sound.PlaySound(Sound.SoundType.GuardianAngleGuard, 0.85f);
                return false;
            }

            return true;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            this.CreateAbilityCountOption(
                parentOps, 2, 5, 3.0f);
            CreateFloatOption(
                FencerOption.ResetTime,
                5.0f, 2.5f, 30.0f, 0.5f,
                parentOps, format: OptionUnit.Second);
        }

        protected override void RoleSpecificInit()
        {
            this.Timer = 0.0f;
            this.MaxTime = OptionHolder.AllOption[
                GetRoleOptionId(FencerOption.ResetTime)].GetValue();

            this.RoleAbilityInit();
        }
    }
}
