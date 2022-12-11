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
                poolablePlayer.cosmetics.nameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
                poolablePlayer.cosmetics.nameText.transform.localPosition = new Vector3(
                    poolablePlayer.cosmetics.nameText.transform.localPosition.x,
                    poolablePlayer.cosmetics.nameText.transform.localPosition.y, -15f);
                poolablePlayer.cosmetics.nameText.text = winningPlayerData.PlayerName;

                foreach (var data in FinalSummary.GetSummary())
                {
                    if (data.PlayerName != winningPlayerData.PlayerName) { continue; }
                    poolablePlayer.cosmetics.nameText.text +=
                        $"\n\n<size=80%>{string.Join("\n", data.Role.GetColoredRoleName(true))}</size>";

                    if(data.Role.IsNeutral())
                    {
                        winNeutral.Add((data.Role, data.PlayerId));
                    }

                }
            }
        }

        private static void setRoleSummary(EndGameManager manager)
        {
            if (!OptionHolder.Client.ShowRoleSummary) { return; }

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
            switch (state.EndReason)
            {
                case GameOverReason.HumansByTask:
                case GameOverReason.HumansByVote:
                    winDetailText.Add(Translation.GetString(
                        RoleTypes.Crewmate.ToString()));
                    textRenderer.color = Palette.White;
                    break;
                case GameOverReason.ImpostorByKill:
                case GameOverReason.ImpostorByVote:
                case GameOverReason.ImpostorBySabotage:
                case (GameOverReason)RoleGameOverReason.AssassinationMarin:
                    winDetailText.Add(Translation.GetString(
                        RoleTypes.Impostor.ToString()));
                    textRenderer.color = Palette.ImpostorRed;
                    break;
                case (GameOverReason)RoleGameOverReason.AliceKilledByImposter:
                case (GameOverReason)RoleGameOverReason.AliceKillAllOther:
                    winDetailText.Add(Translation.GetString(
                        ExtremeRoleId.Alice.ToString()));
                    textRenderer.color = ColorPalette.AliceGold;
                    manager.BackgroundBar.material.SetColor(
                        "_Color", ColorPalette.AliceGold);
                    break;
                case (GameOverReason)RoleGameOverReason.JackalKillAllOther:
                    winDetailText.Add(Translation.GetString(
                        ExtremeRoleId.Jackal.ToString()));
                    textRenderer.color = ColorPalette.JackalBlue;
                    manager.BackgroundBar.material.SetColor(
                        "_Color", ColorPalette.JackalBlue);
                    break;
                case (GameOverReason)RoleGameOverReason.TaskMasterGoHome:
                    winDetailText.Add(Translation.GetString(
                        ExtremeRoleId.TaskMaster.ToString()));
                    textRenderer.color = ColorPalette.NeutralColor;
                    manager.BackgroundBar.material.SetColor(
                        "_Color", ColorPalette.NeutralColor);
                    break;
                case (GameOverReason)RoleGameOverReason.MissionaryAllAgainstGod:
                    winDetailText.Add(Translation.GetString(
                        ExtremeRoleId.Missionary.ToString()));
                    textRenderer.color = ColorPalette.MissionaryBlue;
                    manager.BackgroundBar.material.SetColor(
                        "_Color", ColorPalette.MissionaryBlue);
                    break;
                case (GameOverReason)RoleGameOverReason.JesterMeetingFavorite:
                    winDetailText.Add(Translation.GetString(
                       ExtremeRoleId.Jester.ToString()));
                    textRenderer.color = ColorPalette.JesterPink;
                    manager.BackgroundBar.material.SetColor(
                        "_Color", ColorPalette.JesterPink);
                    break;
                case (GameOverReason)RoleGameOverReason.LoverKillAllOther:
                case (GameOverReason)RoleGameOverReason.ShipFallInLove:
                    winDetailText.Add(Translation.GetString(
                        ExtremeRoleId.Lover.ToString()));
                    textRenderer.color = ColorPalette.LoverPink;
                    manager.BackgroundBar.material.SetColor(
                        "_Color", ColorPalette.LoverPink);
                    break;
                case (GameOverReason)RoleGameOverReason.YandereKillAllOther:
                    winDetailText.Add(Translation.GetString(
                        ExtremeRoleId.Yandere.ToString()));
                    textRenderer.color = ColorPalette.YandereVioletRed;
                    manager.BackgroundBar.material.SetColor(
                        "_Color", ColorPalette.YandereVioletRed);
                    break;
                case (GameOverReason)RoleGameOverReason.YandereShipJustForTwo:
                    winDetailText.Add(Translation.GetString(
                        RoleGameOverReason.YandereShipJustForTwo.ToString()));
                    textRenderer.color = ColorPalette.YandereVioletRed;
                    manager.BackgroundBar.material.SetColor(
                        "_Color", ColorPalette.YandereVioletRed);
                    break;
                case (GameOverReason)RoleGameOverReason.VigilanteKillAllOther:
                case (GameOverReason)RoleGameOverReason.VigilanteNewIdealWorld:
                    winDetailText.Add(Translation.GetString(
                        ExtremeRoleId.Vigilante.ToString()));
                    textRenderer.color = ColorPalette.VigilanteFujiIro;
                    manager.BackgroundBar.material.SetColor(
                        "_Color", ColorPalette.VigilanteFujiIro);
                    break;
                case (GameOverReason)RoleGameOverReason.YokoAllDeceive:
                    winDetailText.Add(Translation.GetString(
                        ExtremeRoleId.Yoko.ToString()));
                    textRenderer.color = ColorPalette.YokoShion;
                    manager.BackgroundBar.material.SetColor(
                        "_Color", ColorPalette.YokoShion);
                    break;
                case (GameOverReason)RoleGameOverReason.MinerExplodeEverything:
                    winDetailText.Add(Translation.GetString(
                        ExtremeRoleId.Miner.ToString()));
                    textRenderer.color = ColorPalette.MinerIvyGreen;
                    manager.BackgroundBar.material.SetColor(
                        "_Color", ColorPalette.MinerIvyGreen);
                    break;
                case (GameOverReason)RoleGameOverReason.EaterAllEatInTheShip:
                case (GameOverReason)RoleGameOverReason.EaterAliveAlone:
                    winDetailText.Add(Translation.GetString(
                        ExtremeRoleId.Eater.ToString()));
                    textRenderer.color = ColorPalette.EaterMaroon;
                    manager.BackgroundBar.material.SetColor(
                        "_Color", ColorPalette.EaterMaroon);
                    break;
                case (GameOverReason)RoleGameOverReason.TraitorKillAllOther:
                    winDetailText.Add(Translation.GetString(
                        ExtremeRoleId.Traitor.ToString()));
                    textRenderer.color = ColorPalette.TraitorLightShikon;
                    manager.BackgroundBar.material.SetColor(
                        "_Color", ColorPalette.TraitorLightShikon);
                    break;
                case (GameOverReason)RoleGameOverReason.QueenKillAllOther:
                    winDetailText.Add(Translation.GetString(
                        ExtremeRoleId.Queen.ToString()));
                    textRenderer.color = ColorPalette.QueenWhite;
                    manager.BackgroundBar.material.SetColor(
                        "_Color", ColorPalette.QueenWhite);
                    break;
                case (GameOverReason)RoleGameOverReason.UmbrerBiohazard:
                    winDetailText.Add(Translation.GetString(
                        ExtremeRoleId.Umbrer.ToString()));
                    textRenderer.color = ColorPalette.UmbrerRed;
                    manager.BackgroundBar.material.SetColor(
                        "_Color", ColorPalette.UmbrerRed);
                    break;
                case (GameOverReason)RoleGameOverReason.KidsTooBigHomeAlone:
                case (GameOverReason)RoleGameOverReason.KidsAliveAlone:
                    winDetailText.Add(Translation.GetString(
                        ExtremeRoleId.Delinquent.ToString()));
                    textRenderer.color = ColorPalette.KidsYellowGreen;
                    manager.BackgroundBar.material.SetColor(
                        "_Color", ColorPalette.KidsYellowGreen);
                    break;
                default:
                    break;
            }

            // 幽霊役職の勝者テキスト追加処理
            HashSet<ExtremeRoleId> textAddedRole = new HashSet<ExtremeRoleId>();
            HashSet<ExtremeGhostRoleId> textAddedGhostRole = new HashSet<ExtremeGhostRoleId>();
            HashSet<byte> winPlayer = new HashSet<byte>();

            foreach (var player in state.GetPlusWinner())
            {
                var ghostRole = ExtremeGhostRoleManager.GameRole[player.PlayerId];

                if (!ghostRole.IsNeutral() ||
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

            if (OptionHolder.Ship.DisableNeutralSpecialForceEnd && winNeutral.Count != 0)
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
    }
}
