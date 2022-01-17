using System.Collections.Generic;

using UnityEngine;

namespace ExtremeRoles.Helper
{
    public static class Player
    {
        public enum MurderAttemptResult
        {
            PerformKill,
            SuppressKill,
            BlankKill
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
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == id) { return player; }
            }
            return null;
        }

        public static void SetPlayerOutLine(PlayerControl target, Color color)
        {
            if (target == null || target.myRend == null) { return; }

            target.myRend.material.SetFloat("_Outline", 1f);
            target.myRend.material.SetColor("_OutlineColor", color);
        }

        public static Dictionary<byte, PoolablePlayer> CreatePlayerIcon()
        {

            Dictionary<byte, PoolablePlayer> playerIcon = new Dictionary<byte, PoolablePlayer>();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                PoolablePlayer poolPlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(
                    ExtremeRolesPlugin.GameDataStore.PlayerPrefab,
                    HudManager.Instance.transform);
                
                poolPlayer.gameObject.SetActive(true);
                poolPlayer.UpdateFromPlayerData(
                    player.Data, PlayerOutfitType.Default);
                poolPlayer.SetFlipX(true);
                poolPlayer.gameObject.SetActive(false);
                playerIcon.Add(player.PlayerId, poolPlayer);
            }

            return playerIcon;

        }

    }
}
