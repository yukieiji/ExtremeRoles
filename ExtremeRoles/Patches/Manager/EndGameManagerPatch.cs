using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using HarmonyLib;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Patches.Manager
{

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public class EndGameManagerSetUpPatch
    {
        private static List<Roles.API.SingleRoleBase> winNeutral = new List<Roles.API.SingleRoleBase>();

        public static void Postfix(EndGameManager __instance)
        {
            setPlayerNameAndRole(__instance);
            setWinBonusText(__instance);
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

                foreach (var data in ExtremeRolesPlugin.GameDataStore.FinalSummary)
                {
                    if (data.PlayerName != winningPlayerData.PlayerName) { continue; }
                    poolablePlayer.cosmetics.nameText.text +=
                        $"\n\n<size=80%>{string.Join("\n", data.Role.GetColoredRoleName(true))}</size>";

                    if(data.Role.IsNeutral())
                    {
                        winNeutral.Add(data.Role);
                    }

                }
            }
        }

        private static void setRoleSummary(EndGameManager manager)
        {
            if (!OptionHolder.Client.ShowRoleSummary) { return; }

            var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
            GameObject summary = UnityEngine.Object.Instantiate(
                manager.WinText.gameObject);
            summary.transform.position = new Vector3(
                manager.Navigation.ExitButton.transform.position.x + 0.1f,
                position.y - 0.1f, -14f);
            summary.transform.localScale = new Vector3(1f, 1f, 1f);

            var summaryText = new StringBuilder();
            summaryText.AppendLine(Translation.GetString("summaryText"));
            summaryText.AppendLine(Translation.GetString("summaryInfo"));

            var summaryData = ExtremeRolesPlugin.GameDataStore.FinalSummary;

            summaryData.Sort((x, y) =>
            {
                if (x.StatusInfo != y.StatusInfo)
                {
                    return x.StatusInfo.CompareTo(y.StatusInfo);
                }

                if (x.Role.Id != y.Role.Id)
                {
                    return x.Role.Id.CompareTo(y.Role.Id);
                }
                if (x.Role.Id == ExtremeRoleId.VanillaRole)
                {
                    var xVanillaRole = (Roles.Solo.VanillaRoleWrapper)x.Role;
                    var yVanillaRole = (Roles.Solo.VanillaRoleWrapper)y.Role;

                    return xVanillaRole.VanilaRoleId.CompareTo(
                        yVanillaRole.VanilaRoleId);
                }

                return x.PlayerName.CompareTo(y.PlayerName);

            });

            List<Color> tagColor = new List<Color>();

            for (int i = 0; i < OptionHolder.VanillaMaxPlayerNum; ++i)
            {
                tagColor.Add(
                    UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f, 1f, 1f));
            }

            List<string> randomTag = new List<string> {
                    "γ", "ζ", "δ", "ε", "η",
                    "θ", "λ", "μ", "π", "ρ",
                    "σ", "φ", "ψ", "χ", "ω" }.OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();


            foreach (var playerSummary in summaryData)
            {
                string taskInfo = playerSummary.TotalTask > 0 ? 
                    $"<color=#FAD934FF>{playerSummary.CompletedTask}/{playerSummary.TotalTask}</color>" : "";
                string aliveDead = Translation.GetString(
                    playerSummary.StatusInfo.ToString());

                string roleName = playerSummary.Role.GetColoredRoleName(true);
                string tag = playerSummary.Role.GetRoleTag();
                
                int id = playerSummary.Role.GameControlId;
                int index = id % OptionHolder.VanillaMaxPlayerNum;
                if (tag != string.Empty)
                {
                    tag = Design.ColoedString(
                        tagColor[index], tag);
                }
                else
                {
                    tag = Design.ColoedString(
                        tagColor[index], randomTag[index]);
                }

                var mutiAssignRole = playerSummary.Role as Roles.API.MultiAssignRoleBase;
                if (mutiAssignRole != null)
                {
                    if (mutiAssignRole.AnotherRole != null)
                    {
                        string anotherTag = mutiAssignRole.AnotherRole.GetRoleTag();
                        id = mutiAssignRole.AnotherRole.GameControlId;
                        index = id % OptionHolder.VanillaMaxPlayerNum;

                        if (anotherTag != string.Empty)
                        {
                            anotherTag = Design.ColoedString(
                                tagColor[index], anotherTag);
                        }
                        else
                        {
                            anotherTag = Design.ColoedString(
                                tagColor[index], randomTag[index]);
                        }

                        tag = string.Concat(
                            tag, " + ", anotherTag);

                    }
                }

                summaryText.AppendLine(
                    $"{playerSummary.PlayerName}<pos=18%>{taskInfo}<pos=27%>{aliveDead}<pos=35%>{tag}:{roleName}");
            }


            
            TMPro.TMP_Text summaryTextMesh = summary.GetComponent<TMPro.TMP_Text>();
            summaryTextMesh.alignment = TMPro.TextAlignmentOptions.TopLeft;
            summaryTextMesh.color = Color.white;
            summaryTextMesh.outlineWidth *= 1.2f;
            summaryTextMesh.fontSizeMin = 1.25f;
            summaryTextMesh.fontSizeMax = 1.25f;
            summaryTextMesh.fontSize = 1.25f;

            var roleSummaryTextMeshRectTransform = summaryTextMesh.GetComponent<RectTransform>();
            roleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
            summaryTextMesh.text = summaryText.ToString();

        }

        private static void setWinBonusText(
            EndGameManager manager)
        {

            GameObject bonusTextObject = UnityEngine.Object.Instantiate(manager.WinText.gameObject);
            bonusTextObject.transform.position = new Vector3(
                manager.WinText.transform.position.x,
                manager.WinText.transform.position.y - 0.8f,
                manager.WinText.transform.position.z);
            bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

            TMPro.TMP_Text textRenderer = bonusTextObject.GetComponent<TMPro.TMP_Text>();
            textRenderer.text = string.Empty;

            string bonusText = string.Empty;

            var gameData = ExtremeRolesPlugin.GameDataStore;

            switch (gameData.EndReason)
            {
                case GameOverReason.HumansByTask:
                case GameOverReason.HumansByVote:
                    bonusText = Translation.GetString(
                        RoleTypes.Crewmate.ToString());
                    textRenderer.color = Palette.White;
                    break;
                case GameOverReason.ImpostorByKill:
                case GameOverReason.ImpostorByVote:
                case GameOverReason.ImpostorBySabotage:
                case (GameOverReason)RoleGameOverReason.AssassinationMarin:
                    bonusText = Translation.GetString(
                        RoleTypes.Impostor.ToString());
                    textRenderer.color = Palette.ImpostorRed;
                    break;
                case (GameOverReason)RoleGameOverReason.AliceKilledByImposter:
                case (GameOverReason)RoleGameOverReason.AliceKillAllOther:
                    bonusText = Translation.GetString(
                        ExtremeRoleId.Alice.ToString());
                    textRenderer.color = ColorPalette.AliceGold;
                    manager.BackgroundBar.material.SetColor("_Color", ColorPalette.AliceGold);
                    break;
                case (GameOverReason)RoleGameOverReason.JackalKillAllOther:
                    bonusText = Translation.GetString(
                        ExtremeRoleId.Jackal.ToString());
                    textRenderer.color = ColorPalette.JackalBlue;
                    manager.BackgroundBar.material.SetColor("_Color", ColorPalette.JackalBlue);
                    break;
                case (GameOverReason)RoleGameOverReason.TaskMasterGoHome:
                    bonusText = Translation.GetString(
                        ExtremeRoleId.TaskMaster.ToString());
                    textRenderer.color = ColorPalette.NeutralColor;
                    manager.BackgroundBar.material.SetColor("_Color", ColorPalette.NeutralColor);
                    break;
                case (GameOverReason)RoleGameOverReason.MissionaryAllAgainstGod:
                    bonusText = Translation.GetString(
                        ExtremeRoleId.Missionary.ToString());
                    textRenderer.color = ColorPalette.MissionaryBlue;
                    manager.BackgroundBar.material.SetColor("_Color", ColorPalette.MissionaryBlue);
                    break;
                case (GameOverReason)RoleGameOverReason.JesterMeetingFavorite:
                    bonusText = Translation.GetString(
                       ExtremeRoleId.Jester.ToString());
                    textRenderer.color = ColorPalette.JesterPink;
                    manager.BackgroundBar.material.SetColor("_Color", ColorPalette.JesterPink);
                    break;
                case (GameOverReason)RoleGameOverReason.LoverKillAllOther:
                case (GameOverReason)RoleGameOverReason.ShipFallInLove:
                    bonusText = Translation.GetString(
                        ExtremeRoleId.Lover.ToString());
                    textRenderer.color = ColorPalette.LoverPink;
                    manager.BackgroundBar.material.SetColor("_Color", ColorPalette.LoverPink);
                    break;
                case (GameOverReason)RoleGameOverReason.YandereKillAllOther:
                    bonusText = Translation.GetString(
                        ExtremeRoleId.Yandere.ToString());
                    textRenderer.color = ColorPalette.YandereVioletRed;
                    manager.BackgroundBar.material.SetColor("_Color", ColorPalette.YandereVioletRed);
                    break;
                case (GameOverReason)RoleGameOverReason.YandereShipJustForTwo:
                    bonusText = Translation.GetString(
                        RoleGameOverReason.YandereShipJustForTwo.ToString());
                    textRenderer.color = ColorPalette.YandereVioletRed;
                    manager.BackgroundBar.material.SetColor("_Color", ColorPalette.YandereVioletRed);
                    break;
                case (GameOverReason)RoleGameOverReason.VigilanteKillAllOther:
                case (GameOverReason)RoleGameOverReason.VigilanteNewIdealWorld:
                    bonusText = Translation.GetString(
                        ExtremeRoleId.Vigilante.ToString());
                    textRenderer.color = ColorPalette.VigilanteFujiIro;
                    manager.BackgroundBar.material.SetColor("_Color", ColorPalette.VigilanteFujiIro);
                    break;
                case (GameOverReason)RoleGameOverReason.YokoAllDeceive:
                    bonusText = Translation.GetString(
                        ExtremeRoleId.Yoko.ToString());
                    textRenderer.color = ColorPalette.YokoShion;
                    manager.BackgroundBar.material.SetColor("_Color", ColorPalette.YokoShion);
                    break;
                case (GameOverReason)RoleGameOverReason.MinerExplodeEverything:
                    bonusText = Translation.GetString(
                        ExtremeRoleId.Miner.ToString());
                    textRenderer.color = ColorPalette.MinerIvyGreen;
                    manager.BackgroundBar.material.SetColor("_Color", ColorPalette.MinerIvyGreen);
                    break;
                case (GameOverReason)RoleGameOverReason.EaterAllEatInTheShip:
                case (GameOverReason)RoleGameOverReason.EaterAliveAlone:
                    bonusText = Translation.GetString(
                        ExtremeRoleId.Eater.ToString());
                    textRenderer.color = ColorPalette.EaterMaroon;
                    manager.BackgroundBar.material.SetColor("_Color", ColorPalette.EaterMaroon);
                    break;
                case (GameOverReason)RoleGameOverReason.TraitorKillAllOther:
                    bonusText = Translation.GetString(
                        ExtremeRoleId.Traitor.ToString());
                    textRenderer.color = ColorPalette.TraitorLightShikon;
                    manager.BackgroundBar.material.SetColor("_Color", ColorPalette.TraitorLightShikon);
                    break;
                case (GameOverReason)RoleGameOverReason.QueenKillAllOther:
                    bonusText = Translation.GetString(
                        ExtremeRoleId.Queen.ToString());
                    textRenderer.color = ColorPalette.QueenWhite;
                    manager.BackgroundBar.material.SetColor("_Color", ColorPalette.QueenWhite);
                    break;
                default:
                    break;
            }

            HashSet<ExtremeRoleId> added = new HashSet<ExtremeRoleId>();

            if (OptionHolder.Ship.DisableNeutralSpecialForceEnd && winNeutral.Count != 0)
            {
                switch (gameData.EndReason)
                {
                    case GameOverReason.HumansByTask:
                    case GameOverReason.HumansByVote:
                    case GameOverReason.ImpostorByKill:
                    case GameOverReason.ImpostorByVote:
                    case GameOverReason.ImpostorBySabotage:

                        for (int i=0; i < winNeutral.Count; ++i)
                        {
                            if (added.Contains(winNeutral[i].Id)) { continue; }

                            if(added.Count == 0)
                            {
                                bonusText = string.Concat(
                                    bonusText, Translation.GetString("andFirst"));
                            }
                            else
                            {
                                bonusText = string.Concat(
                                    bonusText, Translation.GetString("and"));
                            }

                            bonusText = string.Concat(
                                bonusText, Translation.GetString(
                                    winNeutral[i].GetColoredRoleName(true)));
                            added.Add(winNeutral[i].Id);
                        }
                        break;
                    default:
                        break;
                }
                winNeutral.Clear();
            }

            foreach (var player in gameData.PlusWinner)
            {

                var role = ExtremeRoleManager.GameRole[player.PlayerId];

                if (!role.IsNeutral()) { continue; }

                if (added.Contains(role.Id)) { continue; }

                if (added.Count == 0)
                {
                    bonusText = string.Concat(bonusText, Translation.GetString("andFirst"));
                }
                else
                {
                    bonusText = string.Concat(bonusText, Translation.GetString("and"));
                }

                bonusText = string.Concat(bonusText, Translation.GetString(
                    role.GetColoredRoleName(true)));
                added.Add(role.Id);
            }

            textRenderer.text = string.Concat(bonusText, Translation.GetString("win"));
        }

    }
}
