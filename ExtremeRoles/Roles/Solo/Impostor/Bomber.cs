using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class Bomber : SingleRoleBase, IRoleAbility, IRoleUpdate
    {
        public enum BomberOption
        {
            ExplosionRange,
            ExplosionKillChance,
            TimerMaxTime,
            TimerMinTime,
            TellExplosion
        }

        private float timer = 0f;
        private float timerMinTime = 0f;
        private float timerMaxTime = 0f;
        private int explosionKillChance;
        private float explosionRange;
        private bool tellExplosion;
        private PlayerControl setTargetPlayer;
        private PlayerControl bombSettingPlayer;

        private Queue<byte> bombPlayerId;
        private TMPro.TextMeshPro tellText;

        public ExtremeAbilityButton Button
        {
            get => this.bombButton;
            set
            {
                this.bombButton = value;
            }
        }
        private ExtremeAbilityButton bombButton;


        public Bomber() : base(
            ExtremeRoleId.Bomber,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Bomber.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }

        public void CreateAbility()
        {

            this.CreateAbilityCountButton(
                "setBomb",
                Loader.CreateSpriteFromResources(
                    Path.BomberSetBomb),
                CheckAbility, CleanUp);
        }

        public bool IsAbilityUse()
        {
            this.setTargetPlayer = Player.GetClosestPlayerInKillRange();
            return this.IsCommonUse() && this.setTargetPlayer != null;
        }

        public void CleanUp()
        {
            this.bombPlayerId.Enqueue(this.bombSettingPlayer.PlayerId);
            this.bombSettingPlayer = null;
        }

        public bool CheckAbility()
            => Player.IsPlayerInRangeAndDrawOutLine(
                CachedPlayerControl.LocalPlayer,
                this.bombSettingPlayer, this, this.KillRange);

        public bool UseAbility()
        {
            this.bombSettingPlayer = this.setTargetPlayer;
            return true;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            this.CreateAbilityCountOption(
                parentOps, 2, 5, 2.5f);
            CreateIntOption(
                BomberOption.ExplosionRange,
                2, 1, 5, 1,
                parentOps);
            CreateIntOption(
                BomberOption.ExplosionKillChance,
                50, 25, 75, 1,
                parentOps, format: OptionUnit.Percentage);
            CreateFloatOption(
                BomberOption.TimerMinTime,
                15f, 5.0f, 30f, 0.5f,
                parentOps, format: OptionUnit.Second);
            CreateFloatOption(
                BomberOption.TimerMaxTime,
                60f, 45f, 75f, 0.5f,
                parentOps, format: OptionUnit.Second);
            CreateBoolOption(
                BomberOption.TellExplosion,
                true, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();

            var allOption = OptionHolder.AllOption;

            this.timerMinTime = allOption[
                GetRoleOptionId(BomberOption.TimerMinTime)].GetValue();
            this.timerMaxTime = allOption[
                GetRoleOptionId(BomberOption.TimerMaxTime)].GetValue();
            this.explosionKillChance = allOption[
                GetRoleOptionId(BomberOption.ExplosionKillChance)].GetValue();
            this.explosionRange = allOption[
                GetRoleOptionId(BomberOption.ExplosionRange)].GetValue();
            this.tellExplosion = allOption[
                GetRoleOptionId(BomberOption.TellExplosion)].GetValue();

            this.bombPlayerId = new Queue<byte>();
            resetTimer();

        }

        public void ResetOnMeetingStart()
        {
            if (this.tellText != null)
            {
                this.tellText.gameObject.SetActive(false);
            }
        }

        public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
        {
            return;
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (rolePlayer.Data.IsDead || rolePlayer.Data.Disconnected) { return; }
            if (this.bombPlayerId.Count == 0) { return; }

            if (MeetingHud.Instance != null ||
                CachedShipStatus.Instance == null ||
                GameData.Instance == null) { return; }
            if (!CachedShipStatus.Instance.enabled ||
                ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return; }

            this.timer -= Time.deltaTime;
            if (this.timer > 0) { return; }

            resetTimer();

            byte bombTargetPlayerId = this.bombPlayerId.Dequeue();
            PlayerControl bombPlayer = Player.GetPlayerControlById(bombTargetPlayerId);

            if (bombPlayer == null) { return; }
            if (bombPlayer.Data.IsDead || bombPlayer.Data.Disconnected) { return; }
            
            HashSet<PlayerControl> target = getAllPlayerInExplosion(
                rolePlayer, bombPlayer);
            foreach (PlayerControl player in target)
            {
                if (explosionKillChance > Random.RandomRange(0, 100))
                {
                    explosionKill(bombPlayer, player);
                }
            }
            explosionKill(bombPlayer, bombPlayer);
            if (this.tellExplosion)
            {
                rolePlayer.StartCoroutine(
                    showText().WrapToIl2Cpp());
            }
        }

        private void resetTimer()
        {
            this.timer = Random.RandomRange(
                this.timerMinTime, this.timerMaxTime);
        }

        private HashSet<PlayerControl> getAllPlayerInExplosion(
            PlayerControl rolePlayer,
            PlayerControl sourcePlayer)
        {
            HashSet<PlayerControl> result = new HashSet<PlayerControl>();

            Vector2 truePosition = sourcePlayer.GetTruePosition();

            foreach (GameData.PlayerInfo playerInfo in 
                GameData.Instance.AllPlayers.GetFastEnumerator())
            {

                if (!playerInfo.Disconnected &&
                    !playerInfo.IsDead &&
                    (playerInfo.PlayerId != sourcePlayer.PlayerId) &&
                    (!playerInfo.Object.inVent || ExtremeGameModeManager.Instance.ShipOption.CanKillVentInPlayer) &&
                    (!ExtremeRoleManager.GameRole[playerInfo.PlayerId].IsImpostor() ||
                     playerInfo.PlayerId == rolePlayer.PlayerId))
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object)
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= this.explosionRange &&
                            !PhysicsHelpers.AnyNonTriggersBetween(
                                truePosition, vector.normalized,
                                magnitude, Constants.ShipAndObjectsMask))
                        {
                            result.Add(@object);
                        }
                    }
                }
            }

            return result;

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
                this.tellText.text = Helper.Translation.GetString("explosionText");
            }
            this.tellText.gameObject.SetActive(true);

            yield return new WaitForSeconds(3.5f);

            this.tellText.gameObject.SetActive(false);

        }

        private static void explosionKill(
            PlayerControl bombPlayer,
            PlayerControl target)
        {

            if (Crewmate.BodyGuard.TryRpcKillGuardedBodyGuard(
                    bombPlayer.PlayerId, target.PlayerId))
            {
                return;
            }

            Player.RpcUncheckMurderPlayer(
                bombPlayer.PlayerId,
                target.PlayerId,
                byte.MaxValue);

            ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
                target.PlayerId, ExtremeShipStatus.PlayerStatus.Explosion);
        }
    }
}
