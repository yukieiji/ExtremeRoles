using System.Collections;
using System.Collections.Generic;


using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Module.AbilityButton.Mode;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;

using BepInEx.IL2CPP.Utils.Collections;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public sealed class Eater : SingleRoleBase, IRoleAbility, IRoleMurderPlayerHook, IRoleUpdate
    {
        public enum EaterOption
        {
            CanUseVent,
            EatRange,
            DeadBodyEatActiveCoolTimePenalty,
            KillEatCoolTimePenalty,
            KillEatActiveCoolTimeReduceRate,
            IsResetCoolTimeWhenMeeting,
            IsShowArrowForDeadBody
        }

        public enum EaterAbilityMode : byte
        {
            Kill,
            DeadBody
        }

        public RoleAbilityButtonBase Button
        { 
            get => this.eatButton;
            set
            {
                this.eatButton = value;
            }
        }

        private RoleAbilityButtonBase eatButton;
        private PlayerControl tmpTarget;
        private PlayerControl targetPlayer;
        private GameData.PlayerInfo targetDeadBody;
        
        private float range;
        private float deadBodyEatActiveCoolTimePenalty;
        private float killEatCoolTimePenalty;
        private float killEatActiveCoolTimeReduceRate;

        private float defaultCoolTime;
        private bool isResetCoolTimeWhenMeeting;
        private bool isShowArrow;
        private bool isActivated;
        private Dictionary<byte, Arrow> deadBodyArrow;

        private AbilityCountButtonMode<EaterAbilityMode> modeFactory;

        public Eater() : base(
           ExtremeRoleId.Eater,
           ExtremeRoleType.Neutral,
           ExtremeRoleId.Eater.ToString(),
           ColorPalette.EaterMaroon,
           false, false, false, false)
        { }

        public void CreateAbility()
        {
            var allOpt = OptionHolder.AllOption;

            AbilityCountButtonMode deadBodyMode = new AbilityCountButtonMode()
            {
                Text = Translation.GetString("deadBodyEat"),
                Img = Loader.CreateSpriteFromResources(
                    Path.EaterDeadBodyEat),
                ActiveTime = 0.1f,
            };

            this.CreateAbilityCountButton(
                deadBodyMode.Text, deadBodyMode.Img,
                CleanUp, IsAbilityCheck);
            
            if (this.Button is AbilityCountButton button)
            {
                int abilityNum = (int)allOpt[GetRoleOptionId(
                RoleAbilityCommonOption.AbilityCount)].GetValue();
                int halfPlayerNum = GameData.Instance.PlayerCount / 2;

                button.UpdateAbilityCount(
                    halfPlayerNum < abilityNum ? halfPlayerNum : abilityNum);

                this.modeFactory = new AbilityCountButtonMode<EaterAbilityMode>(button);
                this.modeFactory.AddMode(EaterAbilityMode.DeadBody, deadBodyMode);
                this.modeFactory.AddMode(
                    EaterAbilityMode.Kill,
                    new AbilityCountButtonMode()
                    {
                        Text = Translation.GetString("eatKill"),
                        Img = Loader.CreateSpriteFromResources(
                            Path.EaterEatKill),
                        ActiveTime = button.ActiveTime,
                    }
                );

            }
        }

        public void HookMuderPlayer(
            PlayerControl source, PlayerControl target)
        {
            if (MeetingHud.Instance || 
                source.PlayerId == CachedPlayerControl.LocalPlayer.PlayerId ||
                !this.isShowArrow) { return; }

            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; ++i)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == target.PlayerId)
                {
                    Arrow arr = new Arrow(this.NameColor);
                    arr.UpdateTarget(array[i].transform.position);

                    this.deadBodyArrow.Add(target.PlayerId, arr);
                    break;
                }
            }
        }

        public bool IsAbilityUse()
        {

            if (this.eatButton == null ||
                this.modeFactory == null) { return false; }

            this.tmpTarget = Player.GetClosestPlayerInRange(
                CachedPlayerControl.LocalPlayer, this, this.range);

            this.targetDeadBody = Player.GetDeadBodyInfo(
                this.range);

            bool hasPlayerTarget = this.tmpTarget != null;
            bool hasDedBodyTarget = this.targetDeadBody != null;

            this.modeFactory.SwithMode(
                !hasDedBodyTarget && hasPlayerTarget ? 
                EaterAbilityMode.Kill : EaterAbilityMode.DeadBody);

            return this.IsCommonUse() && 
                (hasPlayerTarget || hasDedBodyTarget);
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            if (this.eatButton != null)
            {
                if (isResetCoolTimeWhenMeeting)
                {
                    this.eatButton.SetCoolTime(this.defaultCoolTime);
                    this.eatButton.ResetCoolTimer();
                }
                if (!this.isActivated)
                {
                    var mode = this.modeFactory.GetMode(EaterAbilityMode.Kill);
                    mode.ActiveTime *= this.killEatActiveCoolTimeReduceRate;
                    this.modeFactory.AddMode(EaterAbilityMode.Kill, mode);
                }
            }
            this.isActivated = false;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            foreach (Arrow arrow in this.deadBodyArrow.Values)
            {
                arrow.Clear();
            }
            this.deadBodyArrow.Clear();
        }

        public bool UseAbility()
        {
            this.targetPlayer = this.tmpTarget;
            return true;
        }

        public void Update(PlayerControl rolePlayer)
        {

            if (CachedShipStatus.Instance == null ||
                GameData.Instance == null ||
                this.IsWin) { return; }
            if (!CachedShipStatus.Instance.enabled ||
                ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return; }

            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            HashSet<byte> existDeadBodyPlayerId = new HashSet<byte>();
            for (int i = 0; i < array.Length; ++i)
            {
                byte playerId = GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId;

                if (this.deadBodyArrow.TryGetValue(playerId, out Arrow arrow))
                {
                    arrow.Update();
                    existDeadBodyPlayerId.Add(playerId);
                }
            }

            HashSet<byte> removePlayerId = new HashSet<byte>();
            foreach (byte playerId in this.deadBodyArrow.Keys)
            {
                if (!existDeadBodyPlayerId.Contains(playerId))
                {
                    removePlayerId.Add(playerId);
                }
            }

            foreach (byte playerId in removePlayerId)
            {
                this.deadBodyArrow[playerId].Clear();
                this.deadBodyArrow.Remove(playerId);
            }

            if (this.Button is AbilityCountButton button &&
                button.CurAbilityNum != 0) { return; }

            ExtremeRolesPlugin.ShipState.RpcRoleIsWin(rolePlayer.PlayerId);
            this.IsWin = true;
        }

        public void CleanUp()
        {
            if (this.targetDeadBody != null)
            {
                Player.RpcCleanDeadBody(this.targetDeadBody.PlayerId);

                if (this.deadBodyArrow.ContainsKey(this.targetDeadBody.PlayerId))
                {
                    this.deadBodyArrow[this.targetDeadBody.PlayerId].Clear();
                    this.deadBodyArrow.Remove(this.targetDeadBody.PlayerId);
                }

                this.targetDeadBody = null;

                if (this.eatButton == null) { return; }

                var mode = this.modeFactory.GetMode(EaterAbilityMode.Kill);
                mode.ActiveTime *= this.deadBodyEatActiveCoolTimePenalty;
                this.modeFactory.AddMode(EaterAbilityMode.Kill, mode);
            }
            else if (this.targetPlayer != null)
            {
                Player.RpcUncheckMurderPlayer(
                    CachedPlayerControl.LocalPlayer.PlayerId,
                    this.targetPlayer.PlayerId, 0);

                ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
                    this.targetPlayer.PlayerId,
                    Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus.Eatting);

                if (!this.targetPlayer.Data.IsDead) { return; }

                FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(
                    this.cleanDeadBodyOps(
                        this.targetPlayer.PlayerId).WrapToIl2Cpp());
                
                this.isActivated = true;
            }
            
        }

        public bool IsAbilityCheck()
        {
            if (this.targetDeadBody != null) { return true; }

            return Player.IsPlayerInRangeAndDrawOutLine(
                CachedPlayerControl.LocalPlayer,
                this.targetPlayer, this, this.range);
        }

        public override bool IsSameTeam(SingleRoleBase targetRole) =>
            this.IsNeutralSameTeam(targetRole);

        protected override void CreateSpecificOption(
            IOption parentOps)
        {

            CreateBoolOption(
                EaterOption.CanUseVent,
                true, parentOps);
            this.CreateAbilityCountOption(
                parentOps, 5, 7, 7.5f);
            CreateFloatOption(
                EaterOption.EatRange,
                1.0f, 0.0f, 2.0f, 0.1f,
                parentOps);
            CreateIntOption(
                EaterOption.DeadBodyEatActiveCoolTimePenalty,
                10, 0, 25, 1, parentOps,
                format: OptionUnit.Percentage);
            CreateIntOption(
                EaterOption.KillEatCoolTimePenalty,
                10, 0, 25, 1, parentOps,
                format: OptionUnit.Percentage);
            CreateIntOption(
                EaterOption.KillEatActiveCoolTimeReduceRate,
                10, 0, 50, 1, parentOps,
                format: OptionUnit.Percentage);
            CreateBoolOption(
                EaterOption.IsResetCoolTimeWhenMeeting,
                false, parentOps);
            CreateBoolOption(
                EaterOption.IsShowArrowForDeadBody,
                true, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.targetDeadBody = null;
            this.targetPlayer = null;

            var allOps = OptionHolder.AllOption;

            this.UseVent = allOps[
                GetRoleOptionId(EaterOption.CanUseVent)].GetValue();
            this.range = allOps[
                GetRoleOptionId(EaterOption.EatRange)].GetValue();
            this.deadBodyEatActiveCoolTimePenalty = (float)allOps[
               GetRoleOptionId(EaterOption.DeadBodyEatActiveCoolTimePenalty)].GetValue() / 100.0f + 1.0f;
            this.killEatCoolTimePenalty = (float)allOps[
               GetRoleOptionId(EaterOption.KillEatCoolTimePenalty)].GetValue() / 100.0f + 1.0f;
            this.killEatActiveCoolTimeReduceRate = 1.0f - (float)allOps[
               GetRoleOptionId(EaterOption.KillEatCoolTimePenalty)].GetValue() / 100.0f;
            this.isResetCoolTimeWhenMeeting = allOps[
               GetRoleOptionId(EaterOption.IsResetCoolTimeWhenMeeting)].GetValue();
            this.isShowArrow = allOps[
               GetRoleOptionId(EaterOption.IsShowArrowForDeadBody)].GetValue();

            this.deadBodyArrow = new Dictionary<byte, Arrow>();
            this.isActivated = false;

            this.RoleAbilityInit();
        }

        private IEnumerator cleanDeadBodyOps(byte targetPlayerId)
        {
            DeadBody checkDeadBody = null;

            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; ++i)
            {
                if (GameData.Instance.GetPlayerById(
                    array[i].ParentId).PlayerId == targetPlayerId)
                {
                    checkDeadBody = array[i];
                    break;
                }
            }

            if (checkDeadBody == null) { yield break; }

            while(!checkDeadBody.enabled)
            {
                yield return null;
            }

            yield return null;

            Player.RpcCleanDeadBody(targetPlayerId);
            
            this.targetPlayer = null;

            if (this.Button == null) { yield break; }

            this.Button.SetCoolTime(this.Button.CoolTime * this.killEatCoolTimePenalty);
        }

    }
}
