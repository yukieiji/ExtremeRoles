using System.Collections;

using UnityEngine;
using PowerTools;
using TMPro;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;
using ExtremeRoles.Helper;
using ExtremeRoles.Patches;

namespace ExtremeRoles.GameMode.IntroRunner;

public sealed class HideNSeekIntroRunner : IIntroRunner
{
    public IEnumerator CoRunModeIntro(
        IntroCutscene instance, GameObject roleAssignText)
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;

        Logger.GlobalInstance.Info(
            "IntroCutscene :: CoBegin() :: Game Mode: Hide and Seek", null);
        instance.LogPlayerRoleData();

        roleAssignText.SetActive(false);
        Object.Destroy(roleAssignText);
        roleAssignText = null;

        instance.HideAndSeekPanels.SetActive(true);
        if (localPlayer.Data.Role.IsImpostor)
        {
            instance.CrewmateRules.SetActive(false);
            instance.ImpostorRules.SetActive(true);
        }
        else
        {
            instance.CrewmateRules.SetActive(true);
            instance.ImpostorRules.SetActive(false);
        }

        var teams = IntroCutscene.SelectTeamToShow(
            (Il2CppSystem.Func<NetworkedPlayerInfo, bool>)(
                (NetworkedPlayerInfo pcd) =>
                    localPlayer.Data.Role.IsImpostor != pcd.Role.IsImpostor
            ));
		if (teams == null || teams.Count < 1)
		{
			Logger.GlobalInstance.Error(
				"IntroCutscene :: CoBegin() :: teamToShow is EMPTY or NULL", null);
		}

		PlayerControl impostor =
            AmongUsCache.AllPlayerControl.Find(
				(PlayerControl pc) => pc.Data.Role.IsImpostor);

        if (impostor == null)
        {
            Logger.GlobalInstance.Error(
                "IntroCutscene :: CoBegin() :: impostor is NULL", null);
        }

        GameManager.Instance.SetSpecialCosmetics(impostor);
        instance.ImpostorName.gameObject.SetActive(true);
        instance.ImpostorTitle.gameObject.SetActive(true);
        instance.BackgroundBar.enabled = false;
        instance.TeamTitle.gameObject.SetActive(false);
		instance.ImpostorName.text =
			impostor != null ?
			impostor.Data.PlayerName : "???";

		// HnSではイントロの初期化周りが呼ばれないのでここで呼ぶ
		IntroCutscenceHelper.SetupRole();
		IntroCutscenceHelper.SetupPlayerPrefab(instance);

		SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
        TextMeshPro roleText = Object.Instantiate(
            instance.ImpostorName,
            instance.ImpostorName.gameObject.transform);
        roleText.gameObject.SetActive(true);
        roleText.color = role.GetNameColor();
        roleText.text =
            $"{Translation.GetString("youAreRoleIntro")}\n{role.GetColoredRoleName()}\n{role.GetIntroDescription()}";
        roleText.gameObject.transform.localPosition =
            new Vector3(-2.5f, -5.15f, 0.0f);

        yield return new WaitForSecondsRealtime(0.1f);

        PoolablePlayer playerSlot = null;
        if (impostor != null)
        {
            playerSlot = instance.CreatePlayer(1, 1, impostor.Data, false);
            playerSlot.SetBodyType(PlayerBodyTypes.Normal);
            playerSlot.SetFlipX(false);
            playerSlot.transform.localPosition = instance.impostorPos;
            playerSlot.transform.localScale = Vector3.one * instance.impostorScale;
        }

        yield return CachedShipStatus.Instance.CosmeticsCache.PopulateFromPlayers();
        yield return new WaitForSecondsRealtime(6f);

        if (playerSlot != null)
        {
            playerSlot.gameObject.SetActive(false);
        }

        instance.HideAndSeekPanels.SetActive(false);
        instance.CrewmateRules.SetActive(false);
        instance.ImpostorRules.SetActive(false);
        roleText.gameObject.SetActive(false);

        LogicOptionsHnS logicOptionsHnS =
            GameManager.Instance.LogicOptions.Cast<LogicOptionsHnS>();
        LogicHnSMusic logicHnSMusic =
            GameManager.Instance.GetLogicComponent<LogicHnSMusic>().TryCast<LogicHnSMusic>();

        if (logicHnSMusic != null)
        {
            logicHnSMusic.StartMusicWithIntro();
        }

        float crewmateLeadTime = (float)logicOptionsHnS.GetCrewmateLeadTime();
        bool enableHorse = AprilFoolsMode.ShouldHorseAround();
		bool enableLongAround = AprilFoolsMode.ShouldLongAround();

        if (localPlayer.Data.Role.IsImpostor)
        {
            instance.HideAndSeekTimerText.gameObject.SetActive(true);

            PoolablePlayer poolablePlayer;
            AnimationClip animationClip;
            if (enableHorse)
            {
                poolablePlayer = instance.HorseWrangleVisualSuit;
                poolablePlayer.gameObject.SetActive(true);
                poolablePlayer.SetBodyType(PlayerBodyTypes.Seeker);
                animationClip = instance.HnSSeekerSpawnHorseAnim;
                instance.HorseWrangleVisualPlayer.SetBodyType(PlayerBodyTypes.Normal);
                instance.HorseWrangleVisualPlayer.UpdateFromPlayerData(
                    localPlayer.Data,
                    localPlayer.CurrentOutfitType, PlayerMaterial.MaskType.None, false);
            }
			else if (enableLongAround)
			{
				poolablePlayer = instance.HideAndSeekPlayerVisual;
				poolablePlayer.gameObject.SetActive(true);
				poolablePlayer.SetBodyType(PlayerBodyTypes.LongSeeker);
				animationClip = instance.HnSSeekerSpawnLongAnim;
			}
			else
            {
                poolablePlayer = instance.HideAndSeekPlayerVisual;
                poolablePlayer.gameObject.SetActive(true);
                poolablePlayer.SetBodyType(PlayerBodyTypes.Seeker);
                animationClip = instance.HnSSeekerSpawnAnim;
            }

			poolablePlayer.SetBodyCosmeticsVisible(false);

			poolablePlayer.UpdateFromPlayerData(
                localPlayer.Data,
                localPlayer.CurrentOutfitType,
                PlayerMaterial.MaskType.None, false);

            SpriteAnim component = poolablePlayer.GetComponent<SpriteAnim>();
            poolablePlayer.gameObject.SetActive(true);

            poolablePlayer.ToggleName(false);

            component.Play(animationClip, 1f);

            while (crewmateLeadTime > 0f)
            {
                instance.HideAndSeekTimerText.text =
                    Mathf.RoundToInt(crewmateLeadTime).ToString();
                crewmateLeadTime -= Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            CachedShipStatus.Instance.HideCountdown = crewmateLeadTime;
            if (impostor != null)
            {
                if (enableHorse)
                {
                    impostor.AnimateCustom(instance.HnSSeekerSpawnHorseInGameAnim);
                }
				else if (enableLongAround)
				{
					if (impostor != null)
					{
						impostor.AnimateCustom(instance.HnSSeekerSpawnLongInGameAnim);
					}
				}
				else
                {
                    impostor.AnimateCustom(instance.HnSSeekerSpawnAnim);
                    impostor.cosmetics.SetBodyCosmeticsVisible(false);
                }
            }
        }
        impostor = null;
        playerSlot = null;
        yield break;
    }
}
