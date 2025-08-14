using System.Collections;

using UnityEngine;

using ExtremeRoles.Performance;

namespace ExtremeRoles.GameMode.IntroRunner;

public sealed class ClassicIntroRunner : IIntroRunner
{
    public IEnumerator CoRunModeIntro(
        IntroCutscene instance, IntroText introText)
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;

        Logger.GlobalInstance.Info("IntroCutscene :: CoBegin() :: Game Mode: Normal", null);
        instance.LogPlayerRoleData();

        instance.HideAndSeekPanels.SetActive(false);
        instance.CrewmateRules.SetActive(false);
        instance.ImpostorRules.SetActive(false);
        instance.ImpostorName.gameObject.SetActive(false);
        instance.ImpostorTitle.gameObject.SetActive(false);

        Il2CppSystem.Collections.Generic.List<PlayerControl> teamToShow = IntroCutscene.SelectTeamToShow(
            (Il2CppSystem.Func<NetworkedPlayerInfo, bool>)(
                (NetworkedPlayerInfo pcd) =>
                    !localPlayer.Data.Role.IsImpostor ||
                    pcd.Role.TeamType == localPlayer.Data.Role.TeamType
            ));

        if (localPlayer.Data.Role.IsImpostor)
        {
            instance.ImpostorText.gameObject.SetActive(false);
        }
        else
        {
            int adjustedNumImpostors = GameOptionsManager.Instance.CurrentGameOptions.GetAdjustedNumImpostors(
                GameData.Instance.PlayerCount);
            if (adjustedNumImpostors == 1)
            {
                instance.ImpostorText.text = TranslationController.Instance.GetString(
                    StringNames.NumImpostorsS, System.Array.Empty<Il2CppSystem.Object>());
            }
            else
            {
                instance.ImpostorText.text = TranslationController.Instance.GetString(
                    StringNames.NumImpostorsP, [adjustedNumImpostors.ToString()]);
            }
            instance.ImpostorText.text = instance.ImpostorText.text.Replace("[FF1919FF]", "<color=#FF1919FF>");
            instance.ImpostorText.text = instance.ImpostorText.text.Replace("[]", "</color>");
        }

		introText.Destroy();

		yield return instance.ShowTeam(teamToShow, 3.0f);
        yield return instance.ShowRole();
        yield break;
    }
}
