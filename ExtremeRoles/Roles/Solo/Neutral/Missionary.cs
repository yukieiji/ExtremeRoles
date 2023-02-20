using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Performance;

using BepInEx.IL2CPP.Utils.Collections;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public sealed class Missionary : SingleRoleBase, IRoleAbility, IRoleUpdate
    {

        public enum MissionaryOption
        {
            TellDeparture,
            DepartureMinTime,
            DepartureMaxTime,
            PropagateRange
        }

        public ExtremeAbilityButton Button
        {
            get => this.propagate;
            set
            {
                this.propagate = value;
            }
        }

        public byte TargetPlayer = byte.MaxValue;

        private Queue<byte> lamb;
        private float timer;

        private float propagateRange;
        private float minTimerTime;
        private float maxTimerTime;
        private bool tellDeparture;

        private TMPro.TextMeshPro tellText;

        private ExtremeAbilityButton propagate;

        public Missionary() : base(
            ExtremeRoleId.Missionary,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Missionary.ToString(),
            ColorPalette.MissionaryBlue,
            false, false, false, false)
        { }

        public override bool IsSameTeam(SingleRoleBase targetRole) =>
            this.IsNeutralSameTeam(targetRole);

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateBoolOption(
                MissionaryOption.TellDeparture,
                true, parentOps);
            CreateFloatOption(
                MissionaryOption.DepartureMinTime,
                10f, 1.0f, 15f, 0.5f,
                parentOps, format: OptionUnit.Second);
            CreateFloatOption(
                MissionaryOption.DepartureMaxTime,
                30f, 15f, 120f, 0.5f,
                parentOps, format: OptionUnit.Second);
            CreateFloatOption(
                MissionaryOption.PropagateRange,
                1.2f, 0.0f, 2.0f, 0.1f,
                parentOps);

            this.CreateCommonAbilityOption(parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.lamb = new Queue<byte>();
            this.timer = 0;

            this.tellDeparture = OptionHolder.AllOption[
                GetRoleOptionId(MissionaryOption.TellDeparture)].GetValue();
            this.maxTimerTime = OptionHolder.AllOption[
                GetRoleOptionId(MissionaryOption.DepartureMaxTime)].GetValue();
            this.minTimerTime = OptionHolder.AllOption[
                GetRoleOptionId(MissionaryOption.DepartureMinTime)].GetValue();
            this.propagateRange = OptionHolder.AllOption[
                GetRoleOptionId(MissionaryOption.PropagateRange)].GetValue();

            resetTimer();
            this.RoleAbilityInit();

        }

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                "propagate", Loader.CreateSpriteFromResources(
                    Path.MissionaryPropagate));
        }

        public bool IsAbilityUse()
        {
            this.TargetPlayer = byte.MaxValue;
            PlayerControl target = Helper.Player.GetClosestPlayerInRange(
                CachedPlayerControl.LocalPlayer, this,
                this.propagateRange);
            
            if (target != null)
            {
                if (!this.lamb.Contains(target.PlayerId))
                {
                    this.TargetPlayer = target.PlayerId;
                }
            }
            
            return this.IsCommonUse() && this.TargetPlayer != byte.MaxValue;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            if (this.tellText != null)
            {
                this.tellText.gameObject.SetActive(false);
            }
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            if (this.tellText != null)
            {
                this.tellText.gameObject.SetActive(false);
            }
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (this.lamb.Count == 0) { return; }

            if (CachedShipStatus.Instance == null || 
                GameData.Instance == null) { return; }
            if (!CachedShipStatus.Instance.enabled || 
                ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return; }

            this.timer -= Time.deltaTime;
            if (this.timer > 0) { return; }

            resetTimer();

            byte targetPlayerId = this.lamb.Dequeue();
            PlayerControl targetPlayer = Helper.Player.GetPlayerControlById(targetPlayerId);

            if (targetPlayer == null) { return; }
            if (targetPlayer.Data.IsDead || targetPlayer.Data.Disconnected) { return; }

            Helper.Player.RpcUncheckMurderPlayer(
                targetPlayer.PlayerId,
                targetPlayer.PlayerId,
                byte.MaxValue);

            ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
                targetPlayer.PlayerId, ExtremeShipStatus.PlayerStatus.Departure);

            if (this.tellDeparture)
            {
                rolePlayer.StartCoroutine(showText().WrapToIl2Cpp());
            }
        }

        public bool UseAbility()
        {
            var assassin = ExtremeRoleManager.GameRole[this.TargetPlayer] as Combination.Assassin;

            if (assassin != null)
            {
                if (!assassin.CanKilled)
                {
                    return false;
                }
                if (!assassin.CanKilledFromNeutral)
                {
                    return false;
                }
            }
            this.lamb.Enqueue(this.TargetPlayer);
            this.TargetPlayer = byte.MaxValue;
            return true;
        }

        private void resetTimer()
        {
            this.timer = Random.RandomRange(
                this.minTimerTime, this.maxTimerTime);
        }

        private IEnumerator showText()
        {
            if (this.tellText == null)
            {
                this.tellText = Object.Instantiate(
                    Prefab.Text, Camera.main.transform, false);
                this.tellText.transform.localPosition = new Vector3(-4.0f, -2.75f, -250.0f);
                this.tellText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                this.tellText.gameObject.layer = 5;
                this.tellText.text = Helper.Translation.GetString("departureText");
            }
            this.tellText.gameObject.SetActive(true);

            yield return new WaitForSeconds(3.5f);

            this.tellText.gameObject.SetActive(false);

        }
    }
}
