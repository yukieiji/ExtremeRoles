using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Module.ExtremeShipStatus;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public sealed class Miner : SingleRoleBase, IRoleAbility, IRoleUpdate, IRoleSpecialReset
    {
        public enum MinerOption
        {
            MineKillRange,
            NoneActiveTime,
            ShowKillLog
        }

        public RoleAbilityButtonBase Button
        { 
            get => this.setMine;
            set
            {
                this.setMine = value;
            }
        }

        private RoleAbilityButtonBase setMine;

        private List<Vector2> mines;
        private float killRange;
        private float nonActiveTime;
        private float timer;
        private bool isShowKillLog;
        private Vector2? setPos;
        private TextPopUpper killLogger = null;

        public Miner() : base(
            ExtremeRoleId.Miner,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Miner.ToString(),
            ColorPalette.MinerIvyGreen,
            false, false, true, false)
        { }

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Helper.Translation.GetString("setMine"),
                Loader.CreateSpriteFromResources(
                    Path.MinerSetMine),
                abilityCleanUp: CleanUp);
        }

        public bool UseAbility()
        {

            this.setPos = CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();
            return true;
        }

        public void CleanUp()
        {
            if (this.setPos.HasValue)
            {
                this.mines.Add(this.setPos.Value);
            }
            this.setPos = null;
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public void AllReset(PlayerControl rolePlayer)
        {
            this.mines.Clear();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            if (this.killLogger != null)
            {
                this.killLogger.Clear();
            }
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (rolePlayer.Data.IsDead || rolePlayer.Data.Disconnected) { return; }
            
            if (CachedShipStatus.Instance == null ||
                GameData.Instance == null) { return; }
            if (!CachedShipStatus.Instance.enabled ||
                ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return; }
            if (MeetingHud.Instance || ExileController.Instance)
            {
                this.timer = this.nonActiveTime;
                return;
            }

            if (this.timer > 0.0f)
            {
                this.timer -= Time.fixedDeltaTime;
                return;
            }
            
            if (this.mines.Count == 0) { return; }

            HashSet<int> activateMine = new HashSet<int>();
            HashSet<byte> killedPlayer = new HashSet<byte>();

            for (int i = 0; i < this.mines.Count; ++i)
            {
                Vector2 pos = this.mines[i];

                foreach (GameData.PlayerInfo playerInfo in
                    GameData.Instance.AllPlayers.GetFastEnumerator())
                {
                    if (playerInfo == null) { continue; }

                    if (killedPlayer.Contains(playerInfo.PlayerId)) { continue; }
                    
                    var assassin = ExtremeRoleManager.GameRole[
                        playerInfo.PlayerId] as Combination.Assassin;

                    if (assassin != null)
                    {
                        if (!assassin.CanKilled || !assassin.CanKilledFromNeutral)
                        {
                            continue;
                        }
                    }

                    if (!playerInfo.Disconnected &&
                        !playerInfo.IsDead &&
                        playerInfo.Object != null &&
                        !playerInfo.Object.inVent)
                    {
                        PlayerControl @object = playerInfo.Object;
                        if (@object)
                        {
                            Vector2 vector = @object.GetTruePosition() - pos;
                            float magnitude = vector.magnitude;
                            if (magnitude <= this.killRange &&
                                !PhysicsHelpers.AnyNonTriggersBetween(
                                    pos, vector.normalized,
                                    magnitude, Constants.ShipAndObjectsMask))
                            {
                                activateMine.Add(i);
                                killedPlayer.Add(playerInfo.PlayerId);
                                break;
                            }
                        }
                    }
                }
            }

            foreach (int index in activateMine)
            {
                this.mines.RemoveAt(index);
            }
            foreach (byte player in killedPlayer)
            {
                Helper.Player.RpcUncheckMurderPlayer(
                    rolePlayer.PlayerId,
                    player, 0);
                ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
                    player, ExtremeShipStatus.PlayerStatus.Explosion);

                if (this.isShowKillLog)
                {

                    GameData.PlayerInfo killPlayer = GameData.Instance.GetPlayerById(player);

                    if (killPlayer != null)
                    {
                        // 以下のテキスト表示処理
                        // [AUER32-ACM] {プレイヤー名} 100↑
                        // AmongUs ExtremeRoles v3.2.0.0 - AntiCrewmateMine
                        this.killLogger.AddText(
                            $"[AUER32-ACM] {Helper.Design.ColoedString(new Color32(255, 153, 51, byte.MaxValue), killPlayer.DefaultOutfit.PlayerName)} 100↑");
                    }
                }
            }

        }

        public override void ExiledAction(PlayerControl rolePlayer)
        {
            this.mines.Clear();
        }
        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer, PlayerControl killerPlayer)
        {
            this.mines.Clear();
        }

        public override bool IsSameTeam(SingleRoleBase targetRole) =>
            this.IsNeutralSameTeam(targetRole);

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            
            this.CreateCommonAbilityOption(
                parentOps, 2.0f);
            CreateFloatOption(
                MinerOption.MineKillRange,
                1.8f, 0.5f, 5f, 0.1f, parentOps);
            CreateFloatOption(
                MinerOption.NoneActiveTime,
                20.0f, 1.0f, 45f, 0.5f,
                parentOps, format: OptionUnit.Second);
            CreateBoolOption(
                MinerOption.ShowKillLog,
                true, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            var allOpt = OptionHolder.AllOption;

            this.killRange = allOpt[GetRoleOptionId(
                MinerOption.MineKillRange)].GetValue();
            this.nonActiveTime = allOpt[GetRoleOptionId(
                MinerOption.NoneActiveTime)].GetValue();
            this.isShowKillLog = allOpt[GetRoleOptionId(
                MinerOption.ShowKillLog)].GetValue();

            this.mines = new List<Vector2>();
            this.timer = this.nonActiveTime;
            this.setPos = null;
            this.killLogger = new TextPopUpper(
                2, 3.5f, new Vector3(0, -1.2f, 0.0f),
                TMPro.TextAlignmentOptions.Center);
        }
    }
}
