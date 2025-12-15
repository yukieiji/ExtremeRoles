using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using AmongUs.GameOptions;

using HarmonyLib;

using ExtremeRoles.GameMode;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

using Il2CppArray = Il2CppSystem.Array;
using Il2CppObject = Il2CppSystem.Object;

namespace ExtremeRoles.Patches.Manager;


[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
public static class EndGameManagerSetUpPatch
{
    public static void Postfix(EndGameManager __instance)
    {
		var gameResult = ExtremeGameResultManager.Instance;
		gameResult.CreateEndGameManagerResult();

		var winner = gameResult.Winner;
		var winNeutral = setPlayerNameAndRole(
			__instance,
			gameResult.PlayerSummaries,
			winner.Winner);
        setWinDetailText(__instance, winNeutral, winner.PlusedWinner);
        setRoleSummary(__instance, gameResult.PlayerSummaries);
        RPCOperator.Initialize();
    }

    private static List<(SingleRoleBase, byte)> setPlayerNameAndRole(
        in EndGameManager manager,
		in IReadOnlyList<FinalSummary.PlayerSummary> summaries,
		in IReadOnlyList<CachedPlayerData> winner)
    {
		List<(SingleRoleBase, byte)> winNeutral = new List<(SingleRoleBase, byte)>(winner.Count);

		// Delete and readd PoolablePlayers always showing the name and role of the player
		foreach (PoolablePlayer pb in manager.transform.GetComponentsInChildren<PoolablePlayer>())
        {
            Object.Destroy(pb.gameObject);
        }
        int num = Mathf.CeilToInt(7.5f);
        IReadOnlyList<CachedPlayerData> winnerList = winner.OrderBy(
            delegate (CachedPlayerData b)
            {
                if (!b.IsYou)
                {
                    return 0;
                }
                return -1;
            }).ToList();


		// 色とテキストが上書きされてる可能性があるためこっちで指定する
		// テキスト色のデフォルトは(0, 0.5490196, 1, 1)
		// 背景デフォルトは(0.6415, 0, 0, 1)
		var (overrideText, textColor, bkColor) = winnerList.Any(x => x.IsYou) ?
			(StringNames.Victory, new Color32(0, 140, byte.MaxValue, byte.MaxValue), Palette.CrewmateBlue) :
			(StringNames.Defeat , new Color32(byte.MaxValue, 0, 0, byte.MaxValue)  , new Color(0.6415f, 0, 0, 1));

		manager.WinText.text = TranslationController.Instance.GetString(
			overrideText, Il2CppArray.Empty<Il2CppObject>());
		manager.WinText.color = textColor;
		manager.BackgroundBar.material.SetColor("_Color", bkColor);

		for (int i = 0; i < winnerList.Count; i++)
        {
            CachedPlayerData playerData = winnerList[i];
            int num2 = (i % 2 == 0) ? -1 : 1;
            int num3 = (i + 1) / 2;
            float num4 = (float)num3 / (float)num;
            float num5 = Mathf.Lerp(1f, 0.75f, num4);
            float num6 = (float)((i == 0) ? -8 : -1);

            PoolablePlayer poolablePlayer = Object.Instantiate<PoolablePlayer>(
                manager.PlayerPrefab, manager.transform);
            poolablePlayer.transform.localPosition = new Vector3(
                1f * (float)num2 * (float)num3 * num5,
                FloatRange.SpreadToEdges(-1.125f, 0f, num3, num),
                num6 + (float)num3 * 0.01f) * 0.9f;

            float num7 = Mathf.Lerp(1f, 0.65f, num4) * 0.9f;
            Vector3 vector = new Vector3(num7, num7, 1f);

			bool isDead = playerData.IsDead;
			poolablePlayer.transform.localScale = vector;

            if (isDead)
            {
                poolablePlayer.SetBodyAsGhost();
                poolablePlayer.SetDeadFlipX(i % 2 == 0);
            }
            else
            {
                poolablePlayer.SetFlipX(i % 2 == 0);
            }

			poolablePlayer.UpdateFromPlayerOutfit(
				playerData.Outfit,
				PlayerMaterial.MaskType.None,
				isDead, true, null);

			var text = poolablePlayer.cosmetics.nameText;
			text.color = Color.white;
			text.lineSpacing *= 0.7f;
            text.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
			text.transform.localPosition = new Vector3(
                text.transform.localPosition.x,
				text.transform.localPosition.y - 1.0f, -15f);

			string name = playerData.PlayerName;
			text.text = name;

            foreach (var data in summaries)
            {
                if (data.PlayerName != name)
				{
					continue;
				}
				text.text +=
                    $"\n\n<size=80%>{string.Join("\n", data.Role.GetColoredRoleName(true))}</size>";

                if (data.Role.IsNeutral())
                {
                    winNeutral.Add((data.Role, data.PlayerId));
                }

            }
        }
		return winNeutral;
    }

    private static void setRoleSummary(
		EndGameManager manager,
		IReadOnlyList<FinalSummary.PlayerSummary> summaries)
    {
        if (!ClientOption.Instance.ShowRoleSummary.Value) { return; }

        var position = Camera.main.ViewportToWorldPoint(
			new Vector3(0f, 1f, Camera.main.nearClipPlane));
        GameObject summaryObj = Object.Instantiate(
            manager.WinText.gameObject);
        summaryObj.transform.position = new Vector3(
            manager.Navigation.ExitButton.transform.position.x + 0.1f,
            position.y - 0.1f, -14f);
        summaryObj.transform.localScale = new Vector3(1f, 1f, 1f);

        FinalSummary summary = summaryObj.AddComponent<FinalSummary>();
        summary.SetAnchorPoint(position);
        summary.Create(summaries);
    }

    private static void setWinDetailText(
        in EndGameManager manager,
		in List<(SingleRoleBase, byte)> winNeutral,
		in IReadOnlyList<NetworkedPlayerInfo> plusWinner)
    {
		var winText = manager.WinText;

		GameObject bonusTextObject = Object.Instantiate(winText.gameObject);
        bonusTextObject.transform.position = new Vector3(
			winText.transform.position.x,
			winText.transform.position.y - 0.8f,
			winText.transform.position.z);
        bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

        TMPro.TMP_Text textRenderer = bonusTextObject.GetComponent<TMPro.TMP_Text>();
        textRenderer.text = string.Empty;

		var winDetailTextBuilder = new StringBuilder();
        var state = ExtremeRolesPlugin.ShipState;

        // 背景とベースのテキスト追加
        var info = createWinTextInfo((RoleGameOverReason)state.EndReason);

		winDetailTextBuilder.Append(info.Text);

        textRenderer.color = info.Color;

        if (info.IsChangeBk)
        {
            manager.BackgroundBar.material.SetColor("_Color", info.Color);
        }

        // 幽霊役職の勝者テキスト追加処理
        HashSet<ExtremeRoleId> textAddedRole = new HashSet<ExtremeRoleId>();
        HashSet<ExtremeGhostRoleId> textAddedGhostRole = new HashSet<ExtremeGhostRoleId>();
        HashSet<byte> winPlayer = new HashSet<byte>();

        foreach (var player in plusWinner)
        {
            if (!ExtremeGhostRoleManager.GameRole.TryGetValue(
					player.PlayerId, out GhostRoleBase ghostRole) ||
                !ghostRole.IsNeutral() ||
                !(ghostRole is IGhostRoleWinable ghostWin)) { continue; }

            winPlayer.Add(player.PlayerId);

            if (textAddedGhostRole.Contains(ghostRole.Id)) { continue; }

            AddPrefixs(
                ref winDetailTextBuilder,
                textAddedRole.Count == 0 && textAddedGhostRole.Count == 0);

			winDetailTextBuilder.Append(Tr.GetString(
                ghostRole.GetColoredRoleName()));
            textAddedGhostRole.Add(ghostRole.Id);
        }

        if (ExtremeGameModeManager.Instance.ShipOption.DisableNeutralSpecialForceEnd &&
			winNeutral.Count != 0)
        {
            switch (state.EndReason)
            {
                case GameOverReason.CrewmatesByTask:
                case GameOverReason.CrewmatesByVote:
                case GameOverReason.ImpostorsByKill:
                case GameOverReason.ImpostorsByVote:
                case GameOverReason.ImpostorsBySabotage:

                    foreach (var (role, playerId) in winNeutral)
                    {
                        ExtremeRoleId id = role.Core.Id;

                        if (textAddedRole.Contains(id) ||
                            winPlayer.Contains(playerId))
						{ 
							continue;
						}

                        AddPrefixs(ref winDetailTextBuilder, textAddedRole.Count == 0);

						winDetailTextBuilder.Append(Tr.GetString(
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
        foreach (var player in plusWinner)
        {
            if (!ExtremeRoleManager.TryGetRole(player.PlayerId, out var role))
            {
                // If role is not found for a player in plusWinner, this is unexpected.
                // Log an error and skip this player.
                ExtremeRolesPlugin.Logger.LogError($"Role not found for PlayerId: {player.PlayerId} in EndGameManagerPatch.setWinDetailText");
                continue;
            }

			var id = role.Core.Id;

            if (!role.IsNeutral() ||
                textAddedRole.Contains(id) ||
                winPlayer.Contains(player.PlayerId)) { continue; }

            winPlayer.Add(player.PlayerId);

            AddPrefixs(
                ref winDetailTextBuilder,
                textAddedRole.Count == 0);

			winDetailTextBuilder.Append(Tr.GetString(
                role.GetColoredRoleName(true)));
            textAddedRole.Add(id);
        }

		winDetailTextBuilder.Append(
			TranslationController.Instance.GetString(StringNames.Victory));

		textRenderer.text = winDetailTextBuilder.ToString();
	}

    private static void AddPrefixs(ref StringBuilder baseStrings, in bool condition)
    {
        baseStrings.Append(
            condition ? Tr.GetString("andFirst") : Tr.GetString("and"));
    }

    private static WinTextInfo createWinTextInfo(in RoleGameOverReason reason)
		=> reason switch
        {
            (RoleGameOverReason)GameOverReason.CrewmatesByTask or
            (RoleGameOverReason)GameOverReason.CrewmatesByVote or
            (RoleGameOverReason)GameOverReason.HideAndSeek_CrewmatesByTimer =>
                new(RoleTypes.Crewmate, Palette.White, false),

            (RoleGameOverReason)GameOverReason.ImpostorsByKill or
            (RoleGameOverReason)GameOverReason.ImpostorsByVote or
            (RoleGameOverReason)GameOverReason.ImpostorsBySabotage or
            (RoleGameOverReason)GameOverReason.HideAndSeek_ImpostorsByKills or
            RoleGameOverReason.AssassinationMarin or
			RoleGameOverReason.TeroristoTeroWithShip =>
                new(RoleTypes.Impostor, Palette.ImpostorRed, false),

            RoleGameOverReason.AliceKilledByImposter or
            RoleGameOverReason.AliceKillAllOther =>
                new(ExtremeRoleId.Alice, ColorPalette.AliceGold),

            RoleGameOverReason.JackalKillAllOther =>
                new(ExtremeRoleId.Jackal, ColorPalette.JackalBlue),

            RoleGameOverReason.TaskMasterGoHome =>
                new(ExtremeRoleId.TaskMaster, ColorPalette.NeutralColor),

            RoleGameOverReason.MissionaryAllAgainstGod =>
                new(ExtremeRoleId.Missionary, ColorPalette.MissionaryBlue),

            RoleGameOverReason.JesterMeetingFavorite =>
                new(ExtremeRoleId.Jester, ColorPalette.JesterPink),

            RoleGameOverReason.LoverKillAllOther or
            RoleGameOverReason.ShipFallInLove =>
                new(ExtremeRoleId.Lover, ColorPalette.LoverPink),

            RoleGameOverReason.YandereKillAllOther =>
                new(ExtremeRoleId.Yandere, ColorPalette.YandereVioletRed),
            RoleGameOverReason.YandereShipJustForTwo =>
                new(RoleGameOverReason.YandereShipJustForTwo, ColorPalette.YandereVioletRed),

            RoleGameOverReason.VigilanteKillAllOther or
            RoleGameOverReason.VigilanteNewIdealWorld =>
                new(ExtremeRoleId.Vigilante, ColorPalette.VigilanteFujiIro),

            RoleGameOverReason.YokoAllDeceive =>
                new(ExtremeRoleId.Yoko, ColorPalette.YokoShion),

            RoleGameOverReason.MinerExplodeEverything =>
                new(ExtremeRoleId.Miner, ColorPalette.MinerIvyGreen),

            RoleGameOverReason.EaterAllEatInTheShip or
            RoleGameOverReason.EaterAliveAlone =>
                new(ExtremeRoleId.Eater, ColorPalette.EaterMaroon),
				
            RoleGameOverReason.TraitorKillAllOther =>
                new(ExtremeRoleId.Traitor, ColorPalette.TraitorLightShikon),

            RoleGameOverReason.QueenKillAllOther =>
                new(ExtremeRoleId.Queen, ColorPalette.QueenWhite),

            RoleGameOverReason.UmbrerBiohazard =>
                new(ExtremeRoleId.Umbrer, ColorPalette.UmbrerRed),

            RoleGameOverReason.KidsTooBigHomeAlone or
            RoleGameOverReason.KidsAliveAlone =>
                new(ExtremeRoleId.Delinquent, ColorPalette.KidsYellowGreen),

			RoleGameOverReason.HatterEndlessTeaTime or
			RoleGameOverReason.HatterTeaPartyTime =>
				new(ExtremeRoleId.Hatter, ColorPalette.HatterYanagizome),

			RoleGameOverReason.ArtistShipToArt =>
				new(ExtremeRoleId.Artist, ColorPalette.ArtistChenChuWhowan),

			RoleGameOverReason.TuckerShipIsExperimentStation =>
				new(ExtremeRoleId.Tucker, ColorPalette.TuckerMerdedoie),

			RoleGameOverReason.MonikaIamTheOnlyOne or
				RoleGameOverReason.MonikaThisGameIsMine =>
				new(ExtremeRoleId.Monika, ColorPalette.MonikaRoseSaumon),

			RoleGameOverReason.AllJackalWin =>
				new(RoleGameOverReason.AllJackalWin, ColorPalette.JackalBlue),
			RoleGameOverReason.AllYandereWin =>
				new(RoleGameOverReason.AllYandereWin, ColorPalette.YandereVioletRed),
			RoleGameOverReason.AllQueenWin =>
				new(RoleGameOverReason.AllQueenWin, ColorPalette.QueenWhite),


			RoleGameOverReason.LiberalRevolution =>
				new(ExtremeRoleType.Liberal, ColorPalette.LiberalColor),

			_ => new(RoleGameOverReason.UnKnown, Color.black)
        };

	private readonly record struct WinTextInfo(string Text, Color Color, bool IsChangeBk)
	{
		public WinTextInfo(
			System.Enum textEnum, Color color, bool isChangeBk = true) :
			this(Tr.GetString(textEnum.ToString()), color, isChangeBk)
		{
		}
	}
}
