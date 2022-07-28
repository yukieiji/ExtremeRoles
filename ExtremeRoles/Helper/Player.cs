using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Helper
{
    public static class Player
    {
        private static PlayerControl prevTarget;

        public static void ResetTarget()
        {
            prevTarget = null;
        }

        public static bool ShowButtons
        {
            get
            {
                return (
                    !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) &&
                    !MeetingHud.Instance &&
                    !ExileController.Instance &&
                    !ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger);
            }
        }

        public static PlayerControl GetPlayerControlById(byte id)
        {
            foreach (PlayerControl player in CachedPlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == id) { return player; }
            }
            return null;
        }

        public static PlayerControl GetClosestKillRangePlayer()
        {
            var playersInAbilityRangeSorted = 
                CachedPlayerControl.LocalPlayer.Data.Role.GetPlayersInAbilityRangeSorted(
                    RoleBehaviour.GetTempPlayerList());
            if (playersInAbilityRangeSorted.Count <= 0)
            {
                return null;
            }
            return playersInAbilityRangeSorted[0];
        }

        public static PlayerControl GetClosestKillRangePlayer(PlayerControl player)
        {
            var playersInAbilityRangeSorted =
                player.Data.Role.GetPlayersInAbilityRangeSorted(
                    RoleBehaviour.GetTempPlayerList());
            if (playersInAbilityRangeSorted.Count <= 0)
            {
                return null;
            }
            return playersInAbilityRangeSorted[0];
        }

        public static PlayerControl GetPlayerTarget(
            PlayerControl sourcePlayer,
            Roles.API.SingleRoleBase role,
            float range)
        {
            PlayerControl result = null;
            float num = range;

            if (!ShipStatus.Instance)
            {
                return null;
            }

            Vector2 truePosition = sourcePlayer.GetTruePosition();

            foreach (GameData.PlayerInfo playerInfo in 
                GameData.Instance.AllPlayers.GetFastEnumerator())
            {

                if (!playerInfo.Disconnected &&
                    playerInfo.PlayerId != CachedPlayerControl.LocalPlayer.PlayerId &&
                    !playerInfo.IsDead &&
                    !playerInfo.Object.inVent)
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object)
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num &&
                            !PhysicsHelpers.AnyNonTriggersBetween(
                                truePosition, vector.normalized,
                                magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }

            if (result)
            {
                if (role.IsSameTeam(Roles.ExtremeRoleManager.GameRole[result.PlayerId]))
                {
                    result = null;
                }
            }

            SetPlayerOutLine(result, role.GetNameColor());

            return result;
        }

        public static float GetPlayerTaskGage(PlayerControl player)
        {
            return GetPlayerTaskGage(player.Data);
        }

        public static float GetPlayerTaskGage(GameData.PlayerInfo player)
        {
            int taskNum = 0;
            int compNum = 0;

            foreach (GameData.TaskInfo task in player.Tasks.GetFastEnumerator())
            {

                ++taskNum;

                if (task.Complete)
                {
                    ++compNum;
                }
            }

            return (float)compNum / (float)taskNum;

        }

        public static GameData.PlayerInfo GetDeadBodyInfo(float range)
        {

            Vector2 playerPos = CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();

            foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(
                playerPos, range,
                Constants.PlayersOnlyMask))
            {
                if (collider2D.tag == "DeadBody")
                {
                    DeadBody component = collider2D.GetComponent<DeadBody>();

                    if (component && !component.Reported)
                    {
                        Vector2 truePosition = component.TruePosition;
                        if ((Vector2.Distance(truePosition, playerPos) <= range) &&
                            (CachedPlayerControl.LocalPlayer.PlayerControl.CanMove) &&
                            (!PhysicsHelpers.AnythingBetween(
                                playerPos, truePosition, Constants.ShipAndObjectsMask, false)))
                        {
                            return GameData.Instance.GetPlayerById(component.ParentId);
                        }
                    }
                }
            }
            return null;
        }


        public static void SetPlayerOutLine(PlayerControl target, Color color)
        {
            if (prevTarget != null &&
                prevTarget.cosmetics.currentBodySprite.BodySprite != null)
            {
                prevTarget.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 0f);
            }

            if (target == null || target.cosmetics.currentBodySprite.BodySprite == null) { return; }

            target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
            target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
            prevTarget = target;
        }

        public static Dictionary<byte, PoolablePlayer> CreatePlayerIcon()
        {

            Dictionary<byte, PoolablePlayer> playerIcon = new Dictionary<byte, PoolablePlayer>();

            foreach (PlayerControl player in CachedPlayerControl.AllPlayerControls)
            {
                PoolablePlayer poolPlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(
                    Module.Prefab.PlayerPrefab,
                    FastDestroyableSingleton<HudManager>.Instance.transform);

                poolPlayer.gameObject.SetActive(true);
                poolPlayer.UpdateFromPlayerData(
                    player.Data, PlayerOutfitType.Default,
                    PlayerMaterial.MaskType.None, true);
                poolPlayer.cosmetics.SetName(player.Data.DefaultOutfit.PlayerName);
                poolPlayer.name = $"poolable_{player.PlayerId}";
                poolPlayer.SetFlipX(true);
                poolPlayer.gameObject.SetActive(false);
                playerIcon.Add(player.PlayerId, poolPlayer);
            }

            return playerIcon;

        }

    }
}
