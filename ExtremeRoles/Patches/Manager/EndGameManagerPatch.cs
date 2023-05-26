using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using AmongUs.GameOptions;

using HarmonyLib;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Patches.Manager
{

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public static class EndGameManagerSetUpPatch
    {
        private static List<(SingleRoleBase, byte)> winNeutral = new List<(SingleRoleBase, byte)>();

        public static void Postfix(EndGameManager __instance)
        {
            setPlayerNameAndRole(__instance);
            setWinDetailText(__instance);
            setRoleSummary(__instance);
            RPCOperator.Initialize();
        }

        private static void setPlayerNameAndRole(
            EndGameManager manager)
        {

            winNeutral.Clear();

            // Delete and readd PoolablePlayers always showing the name and role of the player
            foreach (PoolablePlayer pb in manager.transform.GetComponentsInChildren<PoolablePlayer>())
            {
                UnityEngine.Object.Destroy(pb.gameObject);
            }
            int num = Mathf.CeilToInt(7.5f);
            List<WinningPlayerData> winnerList = TempData.winners.ToArray().ToList().OrderBy(
                delegate (WinningPlayerData b)
                {
                    if (!b.IsYou)
                    {
                        return 0;
                    }
                    return -1;
                }
                ).ToList<WinningPlayerData>();

            for (int i = 0; i < winnerList.Count; i++)
            {
                WinningPlayerData winningPlayerData = winnerList[i];
                int num2 = (i % 2 == 0) ? -1 : 1;
                int num3 = (i + 1) / 2;
                float num4 = (float)num3 / (float)num;
                float num5 = Mathf.Lerp(1f, 0.75f, num4);
                float num6 = (float)((i == 0) ? -8 : -1);

                PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(
                    manager.PlayerPrefab, manager.transform);
                poolablePlayer.transform.localPosition = new Vector3(
                    1f * (float)num2 * (float)num3 * num5,
                    FloatRange.SpreadToEdges(-1.125f, 0f, num3, num),
                    num6 + (float)num3 * 0.01f) * 0.9f;

                float num7 = Mathf.Lerp(1f, 0.65f, num4) * 0.9f;
                Vector3 vector = new Vector3(num7, num7, 1f);

                poolablePlayer.transform.localScale = vector;
                poolablePlayer.UpdateFromPlayerOutfit(
                    winningPlayerData,
                    PlayerMaterial.MaskType.None,
                    winningPlayerData.IsDead, true);

                if (winningPlayerData.IsDead)
                {
                    poolablePlayer.SetBodyAsGhost();
                    poolablePlayer.SetDeadFlipX(i % 2 == 0);
                }
                else
                {
                    poolablePlayer.SetFlipX(i % 2 == 0);
                }

                poolablePlayer.cosmetics.nameText.color = Color.white;
                poolablePlayer.cosmetics.nameText.lineSpacing *= 0.7f;
                poolablePlayer.cosmetics.nameText.transform.localScale = new Vector3(
                    1f / vector.x, 1f / vector.y, 1f / vector.z);
                poolablePlayer.cosmetics.nameText.transform.localPosition = new Vector3(
                    poolablePlayer.cosmetics.nameText.transform.localPosition.x,
                    poolablePlayer.cosmetics.nameText.transform.localPosition.y - 1.0f, -15f);
                poolablePlayer.cosmetics.nameText.text = winningPlayerData.PlayerName;

                foreach (var data in FinalSummary.GetSummary())
                {
                    if (data.PlayerName != winningPlayerData.PlayerName) { continue; }
                    poolablePlayer.cosmetics.nameText.text +=
                        $"\n\n<size=80%>{string.Join("\n", data.Role.GetColoredRoleName(true))}</size>";

                    if (data.Role.IsNeutral())
                    {
                        winNeutral.Add((data.Role, data.PlayerId));
                    }

                }
            }
        }

        private static void setRoleSummary(EndGameManager manager)
        {
            if (!ClientOption.Instance.ShowRoleSummary.Value) { return; }

            var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
            GameObject summaryObj = Object.Instantiate(
                manager.WinText.gameObject);
            summaryObj.transform.position = new Vector3(
                manager.Navigation.ExitButton.transform.position.x + 0.1f,
                position.y - 0.1f, -14f);
            summaryObj.transform.localScale = new Vector3(1f, 1f, 1f);

            FinalSummary summary = summaryObj.AddComponent<FinalSummary>();
            summary.SetAnchorPoint(position);
            summary.Create();
        }

        private static void setWinDetailText(
            EndGameManager manager)
        {
            GameObject bonusTextObject = Object.Instantiate(manager.WinText.gameObject);
            bonusTextObject.transform.position = new Vector3(
                manager.WinText.transform.position.x,
                manager.WinText.transform.position.y - 0.8f,
                manager.WinText.transform.position.z);
            bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

            TMPro.TMP_Text textRenderer = bonusTextObject.GetComponent<TMPro.TMP_Text>();
            textRenderer.text = string.Empty;

            List<string> winDetailText = new List<string>();

            var state = ExtremeRolesPlugin.ShipState;

            // 背景とベースのテキスト追加
            var info = CreateWinTextInfo((RoleGameOverReason)state.EndReason);

            winDetailText.Add(info.Text);

            textRenderer.color = info.Color;

            if (info.IsChangeBk)
            {
                manager.BackgroundBar.material.SetColor("_Color", info.Color);
            }

            // 幽霊役職の勝者テキスト追加処理
            HashSet<ExtremeRoleId> textAddedRole = new HashSet<ExtremeRoleId>();
            HashSet<ExtremeGhostRoleId> textAddedGhostRole = new HashSet<ExtremeGhostRoleId>();
            HashSet<byte> winPlayer = new HashSet<byte>();

            foreach (var player in state.GetPlusWinner())
            {
                bool isGhost = ExtremeGhostRoleManager.GameRole.TryGetValue(
                    player.PlayerId, out GhostRoleBase ghostRole);

                if (!isGhost ||
                    !ghostRole.IsNeutral() ||
                    !(ghostRole is IGhostRoleWinable ghostWin)) { continue; }

                winPlayer.Add(player.PlayerId);

                if (textAddedGhostRole.Contains(ghostRole.Id)) { continue; }

                AddPrefixs(
                    ref winDetailText,
                    textAddedRole.Count == 0 && textAddedGhostRole.Count == 0);

                winDetailText.Add(Translation.GetString(
                    ghostRole.GetColoredRoleName()));
                textAddedGhostRole.Add(ghostRole.Id);
            }

            if (ExtremeGameModeManager.Instance.ShipOption.DisableNeutralSpecialForceEnd && winNeutral.Count != 0)
            {
                switch (state.EndReason)
                {
                    case GameOverReason.HumansByTask:
                    case GameOverReason.HumansByVote:
                    case GameOverReason.ImpostorByKill:
                    case GameOverReason.ImpostorByVote:
                    case GameOverReason.ImpostorBySabotage:

                        foreach (var (role, playerId) in winNeutral)
                        {
                            ExtremeRoleId id = role.Id;

                            if (textAddedRole.Contains(id) ||
                                winPlayer.Contains(playerId)) { continue; }

                            AddPrefixs(ref winDetailText, textAddedRole.Count == 0);

                            winDetailText.Add(Translation.GetString(
                                role.GetColoredRoleName(true)));
                            textAddedRole.Add(id);
                        }
                        break;
                    default:
                        break;
                }
                winNeutral.Clear();
            }

            // ニュートラルの追加処理
            foreach (var player in state.GetPlusWinner())
            {
                var role = ExtremeRoleManager.GameRole[player.PlayerId];

                if (!role.IsNeutral() ||
                    textAddedRole.Contains(role.Id) ||
                    winPlayer.Contains(player.PlayerId)) { continue; }

                winPlayer.Add(player.PlayerId);

                AddPrefixs(
                    ref winDetailText,
                    textAddedRole.Count == 0);

                winDetailText.Add(Translation.GetString(
                    role.GetColoredRoleName(true)));
                textAddedRole.Add(role.Id);
            }

            winDetailText.Add(Translation.GetString("win"));

            textRenderer.text = string.Concat(winDetailText);
        }

        private static void AddPrefixs(ref List<string> baseStrings, bool condition)
        {
            baseStrings.Add(
                condition ? Translation.GetString("andFirst") : Translation.GetString("and"));
        }

        private static WinTextInfo CreateWinTextInfo(RoleGameOverReason reason)
        {
            return reason switch
            {
                (RoleGameOverReason)GameOverReason.HumansByTask or
                (RoleGameOverReason)GameOverReason.HumansByVote or
                (RoleGameOverReason)GameOverReason.HideAndSeek_ByTimer =>
                    WinTextInfo.Create(RoleTypes.Crewmate, Palette.White, false),

                (RoleGameOverReason)GameOverReason.ImpostorByKill or
                (RoleGameOverReason)GameOverReason.ImpostorByVote or
                (RoleGameOverReason)GameOverReason.ImpostorBySabotage or
                (RoleGameOverReason)GameOverReason.HideAndSeek_ByKills or
                RoleGameOverReason.AssassinationMarin =>
                    WinTextInfo.Create(RoleTypes.Impostor, Palette.ImpostorRed, false),

                RoleGameOverReason.AliceKilledByImposter or
                RoleGameOverReason.AliceKillAllOther =>
                    WinTextInfo.Create(ExtremeRoleId.Alice, ColorPalette.AliceGold),

                RoleGameOverReason.JackalKillAllOther =>
                    WinTextInfo.Create(ExtremeRoleId.Jackal, ColorPalette.JackalBlue),

                RoleGameOverReason.TaskMasterGoHome =>
                    WinTextInfo.Create(ExtremeRoleId.TaskMaster, ColorPalette.NeutralColor),

                RoleGameOverReason.MissionaryAllAgainstGod =>
                    WinTextInfo.Create(ExtremeRoleId.Missionary, ColorPalette.MissionaryBlue),

                RoleGameOverReason.JesterMeetingFavorite =>
                    WinTextInfo.Create(ExtremeRoleId.Jester, ColorPalette.JesterPink),

                RoleGameOverReason.LoverKillAllOther or
                RoleGameOverReason.ShipFallInLove =>
                    WinTextInfo.Create(ExtremeRoleId.Lover, ColorPalette.LoverPink),

                RoleGameOverReason.YandereKillAllOther =>
                    WinTextInfo.Create(
                        ExtremeRoleId.Yandere, ColorPalette.YandereVioletRed),
                RoleGameOverReason.YandereShipJustForTwo =>
                    WinTextInfo.Create(
                        RoleGameOverReason.YandereShipJustForTwo, ColorPalette.YandereVioletRed),

                RoleGameOverReason.VigilanteKillAllOther or
                RoleGameOverReason.VigilanteNewIdealWorld =>
                    WinTextInfo.Create(ExtremeRoleId.Vigilante, ColorPalette.VigilanteFujiIro),

                RoleGameOverReason.YokoAllDeceive =>
                    WinTextInfo.Create(ExtremeRoleId.Yoko, ColorPalette.YokoShion),

                RoleGameOverReason.MinerExplodeEverything =>
                    WinTextInfo.Create(ExtremeRoleId.Miner, ColorPalette.MinerIvyGreen),

                RoleGameOverReason.EaterAllEatInTheShip or
                RoleGameOverReason.EaterAliveAlone =>
                    WinTextInfo.Create(ExtremeRoleId.Eater, ColorPalette.EaterMaroon),

                RoleGameOverReason.TraitorKillAllOther =>
                    WinTextInfo.Create(ExtremeRoleId.Traitor, ColorPalette.TraitorLightShikon),

                RoleGameOverReason.QueenKillAllOther =>
                    WinTextInfo.Create(ExtremeRoleId.Queen, ColorPalette.QueenWhite),

                RoleGameOverReason.UmbrerBiohazard =>
                    WinTextInfo.Create(ExtremeRoleId.Umbrer, ColorPalette.UmbrerRed),

                RoleGameOverReason.KidsTooBigHomeAlone or
                RoleGameOverReason.KidsAliveAlone =>
                    WinTextInfo.Create(ExtremeRoleId.Delinquent, ColorPalette.KidsYellowGreen),

                _ => WinTextInfo.Create(RoleGameOverReason.UnKnown, Color.black)
            };
        }

        private struct WinTextInfo
        {
            public string Text;
            public Color Color;
            public bool IsChangeBk;

            internal static WinTextInfo Create(
                System.Enum textEnum, Color color)
            {
                return new WinTextInfo
                {
                    Text = Translation.GetString(textEnum.ToString()),
                    Color = color,
                    IsChangeBk = true,
                };
            }

            internal static WinTextInfo Create(
                System.Enum textEnum, Color color, bool isChangeBk)
            {
                return new WinTextInfo
                {
                    Text = Translation.GetString(textEnum.ToString()),
                    Color = color,
                    IsChangeBk = isChangeBk,
                };
            }
        }
    }
}
