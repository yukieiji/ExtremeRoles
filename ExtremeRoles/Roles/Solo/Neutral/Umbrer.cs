using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public sealed class Umbrer : SingleRoleBase, IRoleAbility, IRoleSpecialSetUp, IRoleUpdate
    {
        private sealed class InfectedContainer
        {
            public HashSet<byte> FirstStage => this.firstStage;
            public HashSet<byte> FinalStage => this.finalStage;

            private HashSet<byte> firstStage = new HashSet<byte>();
            private HashSet<byte> finalStage = new HashSet<byte>();

            public InfectedContainer()
            {
                this.firstStage.Clear();
                this.finalStage.Clear();
            }

            public void Fetch(SingleRoleBase umbrer)
            {
                foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
                {
                    if (umbrer.IsSameTeam(role))
                    {
                        this.firstStage.Add(playerId);
                    }
                }
            }

            public bool IsAllPlayerInfected()
            {
                if (this.firstStage.Count <= 0) { return false; }

                foreach (GameData.PlayerInfo player in
                    GameData.Instance.AllPlayers.GetFastEnumerator())
                {
                    if (player == null || player?.Object == null) { continue; }
                    if (player.IsDead || player.Disconnected) { continue; }

                    if (!this.firstStage.Contains(player.PlayerId))
                    {
                        return false;
                    }

                }
                return true;
            }

            public bool IsContain(byte playerId) =>
                this.firstStage.Contains(playerId) || this.finalStage.Contains(playerId);

            public bool IsFirstStage(byte playerId) => 
                this.firstStage.Contains(playerId);

            public bool IsFinalStage(byte playerId) =>
                this.finalStage.Contains(playerId);

            public void Update()
            {
                removeToHashSet(ref this.firstStage);
                removeToHashSet(ref this.finalStage);
            }

            private void removeToHashSet(ref HashSet<byte> cont)
            {
                List<byte> remove = new List<byte>();

                foreach (byte playerId in this.firstStage)
                {

                    GameData.PlayerInfo player = GameData.Instance.GetPlayerById(playerId);

                    if (player == null ||
                        player.IsDead ||
                        player.Disconnected)
                    {
                        remove.Add(playerId);
                    }
                }
                foreach (byte playerId in remove)
                {
                    cont.Remove(playerId);
                }
            }

        }

        private sealed class UmbrerVirusAbility : RoleAbilityButtonBase
        {
            public bool IsUpgradeMode => this.isUpgradeVirus;

            private Sprite setVirusSprite;
            private string setVirusButtonText;
            private float setVirusTime;

            private Sprite upgradeVirusSprite;
            private string upgradeVirusButtonText;
            private float upgradeVirusTime;

            private bool isUpgradeVirus;
            private Func<bool> upgradeVirusFunc;

            public UmbrerVirusAbility(
                string setVirusButtonText,
                string upgradeVirusButtonText,
                Sprite setVirusSprite,
                Sprite upgradeVirusSprite,
                float setVirusTime,
                float upgradeVirusTime,
                Func<bool> upgradeVirusModeCheck,
                Func<bool> ability,
                Func<bool> canUse,
                Vector3 positionOffset,
                Action abilityCleanUp = null,
                Func<bool> abilityCheck = null,
                KeyCode hotkey = KeyCode.F,
                bool mirror = false) : base(
                    setVirusButtonText,
                    ability, canUse,
                    setVirusSprite,
                    positionOffset,
                    abilityCleanUp,
                    abilityCheck,
                    hotkey, mirror)
            {

                this.setVirusSprite = setVirusSprite;
                this.setVirusButtonText = setVirusButtonText;
                this.setVirusTime = setVirusTime;

                this.upgradeVirusSprite = upgradeVirusSprite;
                this.upgradeVirusButtonText = upgradeVirusButtonText;
                this.upgradeVirusTime = upgradeVirusTime;

                this.isUpgradeVirus = false;
                this.upgradeVirusFunc = upgradeVirusModeCheck;
            }

            protected override void AbilityButtonUpdate()
            {
                this.isUpgradeVirus = this.upgradeVirusFunc();
                if (this.isUpgradeVirus)
                {
                    this.ButtonSprite = this.upgradeVirusSprite;
                    this.ButtonText = this.upgradeVirusButtonText;
                    this.AbilityActiveTime = this.upgradeVirusTime;
                }
                else
                {
                    this.ButtonSprite = this.setVirusSprite;
                    this.ButtonText = this.setVirusButtonText;
                    this.AbilityActiveTime = this.setVirusTime;
                }

                if (this.CanUse())
                {
                    this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.EnabledColor;
                    this.Button.graphic.material.SetFloat("_Desat", 0f);
                }
                else
                {
                    this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.DisabledClear;
                    this.Button.graphic.material.SetFloat("_Desat", 1f);
                }

                if (this.Timer >= 0)
                {
                    bool abilityOn = this.IsHasCleanUp() && IsAbilityOn;

                    if (abilityOn || (
                            !CachedPlayerControl.LocalPlayer.PlayerControl.inVent &&
                            CachedPlayerControl.LocalPlayer.PlayerControl.moveable))
                    {
                        this.Timer -= Time.deltaTime;
                    }
                    if (abilityOn)
                    {
                        if (!this.AbilityCheck())
                        {
                            this.Timer = 0;
                            this.IsAbilityOn = false;
                        }
                    }
                }

                if (this.Timer <= 0 && this.IsHasCleanUp() && IsAbilityOn)
                {
                    this.IsAbilityOn = false;
                    this.Button.cooldownTimerText.color = Palette.EnabledColor;
                    this.CleanUp();
                    this.ResetCoolTimer();
                }

                Button.SetCoolDown(
                    this.Timer,
                    (this.IsHasCleanUp() && this.IsAbilityOn) ? this.AbilityActiveTime : this.CoolTime);
            }

            protected override void OnClickEvent()
            {
                if (this.CanUse() &&
                    this.Timer < 0f &&
                    !this.IsAbilityOn)
                {
                    Button.graphic.color = this.DisableColor;

                    if (this.UseAbility())
                    {
                        if (this.IsHasCleanUp())
                        {
                            this.Timer = this.AbilityActiveTime;
                            Button.cooldownTimerText.color = this.TimerOnColor;
                            this.IsAbilityOn = true;
                        }
                        else
                        {
                            this.ResetCoolTimer();
                        }
                    }
                }
            }
        }
        public enum UmbrerOption
        {
            Range,
            UpgradeVirusTime,
            InfectRange,
            KeepUpgradedVirus
        }

        public RoleAbilityButtonBase Button
        {
            get => this.madmateAbilityButton;
            set
            {
                this.madmateAbilityButton = value;
            }
        }

        private RoleAbilityButtonBase madmateAbilityButton;
        private InfectedContainer container;
        private PlayerControl tmpTarget;
        private PlayerControl target;
        private float range;
        private float infectRange;
        private float maxTimer;
        private bool isFetch = false;
        private Dictionary<byte, float> timer = new Dictionary<byte, float>();
        private Dictionary<byte, PoolablePlayer> playerIcon = new Dictionary<byte, PoolablePlayer>();


        public Umbrer() : base(
            ExtremeRoleId.Umbrer,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Umbrer.ToString(),
            ColorPalette.UmbrerRed,
            false, false, false, false)
        { }

        public void CreateAbility()
        {
            var allOpt = OptionHolder.AllOption;

            this.Button = new UmbrerVirusAbility(
                Helper.Translation.GetString("featVirus"),
                Helper.Translation.GetString("upgradeVirus"),
                Loader.CreateSpriteFromResources(
                    Path.UmbrerFeatVirus),
                Loader.CreateSpriteFromResources(
                    Path.UmbrerUpgradeVirus),
                (float)allOpt[GetRoleOptionId(RoleAbilityCommonOption.AbilityActiveTime)].GetValue(),
                (float)allOpt[GetRoleOptionId(UmbrerOption.UpgradeVirusTime)].GetValue(),
                IsUpgrade,
                UseAbility,
                IsAbilityUse,
                new Vector3(-1.8f, -0.06f, 0),
                CleanUp,
                IsAbilityCheck);
            abilityInit();
        }

        public bool UseAbility()
        {
            this.target = this.tmpTarget;
            return true;
        }

        public void IntroBeginSetUp()
        {
            return;
        }

        public void IntroEndSetUp()
        {
            this.playerIcon = Helper.Player.CreatePlayerIcon();
            this.showIcon();
        }


        public bool IsAbilityCheck()
        {
            PlayerControl checkPlayer = Helper.Player.GetPlayerTarget(
                CachedPlayerControl.LocalPlayer, this, this.range);

            if (checkPlayer == null) { return false; }

            return checkPlayer.PlayerId == this.target.PlayerId;
        }

        public void CleanUp()
        {
            if (this.container.IsFirstStage(this.target.PlayerId))
            {
                this.container.FinalStage.Add(this.target.PlayerId);
                this.timer.Add(this.target.PlayerId, this.maxTimer);
            }
            else
            {
                this.container.FirstStage.Add(this.target.PlayerId);
            }
            this.target = null;
        }


        public bool IsUpgrade() => this.container.IsFirstStage(
            this.tmpTarget == null ? byte.MaxValue : this.tmpTarget.PlayerId);

        public bool IsAbilityUse()
        {
            this.tmpTarget = Helper.Player.GetPlayerTarget(
                CachedPlayerControl.LocalPlayer, this, this.range);
            if (this.tmpTarget == null) { return false; }

            return this.IsCommonUse() && !this.container.IsFinalStage(this.tmpTarget.PlayerId);
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            foreach (var (_, poolPlayer) in this.playerIcon)
            {
                poolPlayer.gameObject.SetActive(false);
            }
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (MeetingHud.Instance != null ||
                CachedShipStatus.Instance == null ||
                this.IsWin ||
                GameData.Instance == null ||
                this.container == null) { return; }
            if (!CachedShipStatus.Instance.enabled) { return; }

            if (ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd && !isFetch)
            {
                this.isFetch = true;
                this.container.Fetch(this);
            }

            if (this.container.IsAllPlayerInfected())
            {
                this.IsWin = true;
                RPCOperator.RoleIsWin(rolePlayer.PlayerId);
                return;
            }

            HashSet<byte> remove = new HashSet<byte> ();

            foreach (byte playerId in this.container.FinalStage)
            {
                this.timer[playerId] = this.timer[playerId] - Time.fixedDeltaTime;
                if (this.timer[playerId] <= 0.0f ||
                    isInfectOtherPlayer(
                        Helper.Player.GetPlayerControlById(playerId)))
                {
                    remove.Add(playerId);
                }
            }

            foreach (byte playerId in remove)
            {
                this.container.FinalStage.Remove(playerId);
            }

            this.container.Update();
            this.showIcon();
        }

        public override bool IsSameTeam(SingleRoleBase targetRole)
        {
            if (this.Id == targetRole.Id)
            {
                if (OptionHolder.Ship.IsSameNeutralSameWin)
                {
                    return true;
                }
                else
                {
                    return this.IsSameControlId(targetRole);
                }
            }
            else
            {
                return base.IsSameTeam(targetRole);
            }
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateFloatOption(
                UmbrerOption.Range,
                1.0f, 0.1f, 4.0f, 0.1f,
                parentOps);

            this.CreateCommonAbilityOption(
                parentOps, 3.0f);

            CreateFloatOption(
                UmbrerOption.UpgradeVirusTime,
                3.5f, 0.5f, 10.0f, 0.1f,
                parentOps,
                format: OptionUnit.Second);

            CreateFloatOption(
                UmbrerOption.InfectRange,
                0.8f, 0.1f, 3.0f, 0.1f,
                parentOps);

            CreateFloatOption(
                UmbrerOption.KeepUpgradedVirus,
                10.0f, 2.5f, 30.0f, 0.1f,
                parentOps,
                format: OptionUnit.Second);
        }

        protected override void RoleSpecificInit()
        {
            this.container = new InfectedContainer();

            var allOpt = OptionHolder.AllOption;

            this.range = allOpt[GetRoleOptionId(UmbrerOption.Range)].GetValue();
            this.infectRange = allOpt[GetRoleOptionId(UmbrerOption.InfectRange)].GetValue();
            this.maxTimer = allOpt[GetRoleOptionId(UmbrerOption.KeepUpgradedVirus)].GetValue();

            this.isFetch = false;

            abilityInit();
        }
        private bool isInfectOtherPlayer(PlayerControl sourcePlayer)
        {
            if (sourcePlayer == null) { return false; }
            Vector2 pos = sourcePlayer.GetTruePosition();
            byte sourcePlayerId = sourcePlayer.PlayerId;
            byte rolePlayerId = CachedPlayerControl.LocalPlayer.PlayerId;

            foreach (GameData.PlayerInfo playerInfo in
                    GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (playerInfo == null) { continue; }

                if (!playerInfo.Disconnected &&
                    !playerInfo.IsDead &&
                    playerInfo.Object != null &&
                    !playerInfo.Object.inVent &&
                    (playerInfo.PlayerId != sourcePlayerId || playerInfo.PlayerId != rolePlayerId))
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object)
                    {
                        Vector2 vector = @object.GetTruePosition() - pos;
                        float magnitude = vector.magnitude;
                        if (magnitude <= this.infectRange &&
                            !PhysicsHelpers.AnyNonTriggersBetween(
                                pos, vector.normalized,
                                magnitude, Constants.ShipAndObjectsMask))
                        {
                            this.container.FirstStage.Add(playerInfo.PlayerId);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void abilityInit()
        {
            if (this.Button == null) { return; }

            var allOps = OptionHolder.AllOption;
            this.Button.SetAbilityCoolTime(
                allOps[GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)].GetValue());
            this.Button.SetAbilityActiveTime(1.0f);
            this.Button.ResetCoolTimer();
        }

        private void showIcon()
        {
            int visibleCounter = 0;
            Vector3 bottomLeft = FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition;
            bottomLeft.x *= -1;
            bottomLeft += new Vector3(-0.25f, -0.25f, 0);

            foreach (var (playerId, poolPlayer) in this.playerIcon)
            {
                if (this.container.IsContain(playerId))
                {
                    poolPlayer.gameObject.SetActive(false);
                }
                else
                {
                    poolPlayer.gameObject.SetActive(true);
                    poolPlayer.transform.localScale = Vector3.one * 0.275f;
                    poolPlayer.transform.localPosition = bottomLeft + Vector3.right * visibleCounter * 0.45f;
                    ++visibleCounter;
                }
            }
        }
    }
}
