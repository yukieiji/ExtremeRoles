using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using HarmonyLib;

using ExtremeRoles.Module;


namespace ExtremeRoles.Patches.Manager
{

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public class EndGameManagerSetUpPatch
    {
        public static void Postfix(EndGameManager __instance)
        {
            // Delete and readd PoolablePlayers always showing the name and role of the player
            foreach (PoolablePlayer pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>())
            {
                UnityEngine.Object.Destroy(pb.gameObject);
            }
            int num = Mathf.CeilToInt(7.5f);
            List<WinningPlayerData> list = TempData.winners.ToArray().ToList().OrderBy(
                delegate (WinningPlayerData b)
                    {
                    if (!b.IsYou)
                    {
                        return 0;
                    }
                    return -1;
                    }
                ).ToList<WinningPlayerData>();
            for (int i = 0; i < list.Count; i++)
            {
                WinningPlayerData winningPlayerData2 = list[i];
                int num2 = (i % 2 == 0) ? -1 : 1;
                int num3 = (i + 1) / 2;
                float num4 = (float)num3 / (float)num;
                float num5 = Mathf.Lerp(1f, 0.75f, num4);
                float num6 = (float)((i == 0) ? -8 : -1);
                
                PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(
                    __instance.PlayerPrefab, __instance.transform);
                poolablePlayer.transform.localPosition = new Vector3(
                    1f * (float)num2 * (float)num3 * num5,
                    FloatRange.SpreadToEdges(-1.125f, 0f, num3, num),
                    num6 + (float)num3 * 0.01f) * 0.9f;
                
                float num7 = Mathf.Lerp(1f, 0.65f, num4) * 0.9f;
                Vector3 vector = new Vector3(num7, num7, 1f);
                
                poolablePlayer.transform.localScale = vector;
                poolablePlayer.UpdateFromPlayerOutfit(winningPlayerData2, winningPlayerData2.IsDead);
                
                if (winningPlayerData2.IsDead)
                {
                    poolablePlayer.Body.sprite = __instance.GhostSprite;
                    poolablePlayer.SetDeadFlipX(i % 2 == 0);
                }
                else
                {
                    poolablePlayer.SetFlipX(i % 2 == 0);
                }

                poolablePlayer.NameText.color = Color.white;
                poolablePlayer.NameText.lineSpacing *= 0.7f;
                poolablePlayer.NameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
                poolablePlayer.NameText.transform.localPosition = new Vector3(
                    poolablePlayer.NameText.transform.localPosition.x,
                    poolablePlayer.NameText.transform.localPosition.y, -15f);
                poolablePlayer.NameText.text = winningPlayerData2.PlayerName;

                foreach (var data in PlayerDataContainer.EndGamePlayerInfo)
                {
                    if (data.PlayerName != winningPlayerData2.PlayerName) continue;
                    poolablePlayer.NameText.text += 
                        $"\n<size=80%>{string.Join("\n", Helper.Design.Cs(data.Roles.NameColor, data.Roles.RoleName))}</size>";
                }
            }

            // Additional code
            GameObject bonusTextObject = UnityEngine.Object.Instantiate(
                __instance.WinText.gameObject);
            bonusTextObject.transform.position = new Vector3(
                __instance.WinText.transform.position.x,
                __instance.WinText.transform.position.y - 0.8f,
                __instance.WinText.transform.position.z);
            bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            TMPro.TMP_Text textRenderer = bonusTextObject.GetComponent<TMPro.TMP_Text>();
            textRenderer.text = "";
        }
    }
}
