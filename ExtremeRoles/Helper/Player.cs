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

            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];

                if (!playerInfo.Disconnected &&
                    playerInfo.PlayerId != PlayerControl.LocalPlayer.PlayerId &&
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
            foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(
                PlayerControl.LocalPlayer.GetTruePosition(),
                range,
                Constants.PlayersOnlyMask))
            {
                if (collider2D.tag == "DeadBody")
                {
                    DeadBody component = collider2D.GetComponent<DeadBody>();

                    if (component && !component.Reported)
                    {
                        Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                        Vector2 truePosition2 = component.TruePosition;
                        if ((Vector2.Distance(truePosition2, truePosition) <= range) &&
                            (PlayerControl.LocalPlayer.CanMove) &&
                            (!PhysicsHelpers.AnythingBetween(
                                truePosition, truePosition2, Constants.ShipAndObjectsMask, false)))
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
            if (prevTarget != null && prevTarget.MyRend != null)
            {
                prevTarget.MyRend.material.SetFloat("_Outline", 0f);
            }

            if (target == null || target.MyRend == null) { return; }

            target.MyRend.material.SetFloat("_Outline", 1f);
            target.MyRend.material.SetColor("_OutlineColor", color);
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
                    player.Data, PlayerOutfitType.Default);
                poolPlayer.NameText.text = player.Data.DefaultOutfit.PlayerName;
                poolPlayer.SetFlipX(true);
                poolPlayer.gameObject.SetActive(false);
                playerIcon.Add(player.PlayerId, poolPlayer);
            }

            return playerIcon;

        }

    }
}
