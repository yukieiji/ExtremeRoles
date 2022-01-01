using UnityEngine;

namespace ExtremeRoles.Helper
{
    public class Player
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
                    !ExileController.Instance);
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
    }
}
