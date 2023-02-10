using System.Collections;

using UnityEngine;
using PowerTools;
using TMPro;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;
using ExtremeRoles.Helper;

namespace ExtremeRoles.GameMode.IntroRunner
{
    public sealed class HideNSeekIntroRunner : IIntroRunner
    {
        public IEnumerator CoRunModeIntro(
            IntroCutscene instance, GameObject roleAssignText)
        {
            CachedPlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            roleAssignText.gameObject.SetActive(false);
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

            IntroCutscene.SelectTeamToShow(
                (Il2CppSystem.Func<GameData.PlayerInfo, bool>)(
                    (GameData.PlayerInfo pcd) =>
                        localPlayer.Data.Role.IsImpostor != pcd.Role.IsImpostor
                ));

            PlayerControl impostor =
                CachedPlayerControl.AllPlayerControls.Find(
                    (CachedPlayerControl pc) => pc.Data.Role.IsImpostor);

            instance.ImpostorName.gameObject.SetActive(true);
            instance.ImpostorTitle.gameObject.SetActive(true);
            instance.BackgroundBar.enabled = false;
            instance.TeamTitle.gameObject.SetActive(false);
            instance.ImpostorName.text = impostor.Data.PlayerName;

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

            PoolablePlayer playerSlot = instance.CreatePlayer(0, 1, impostor.Data, false);
            playerSlot.transform.localPosition = instance.impostorPos;
            playerSlot.transform.localScale = Vector3.one * instance.impostorScale;

            yield return CachedShipStatus.Instance.CosmeticsCache.PopulateFromPlayers();
            yield return new WaitForSecondsRealtime(6f);

            playerSlot.gameObject.SetActive(false);
            instance.HideAndSeekPanels.SetActive(false);
            instance.CrewmateRules.SetActive(false);
            instance.ImpostorRules.SetActive(false);
            roleText.gameObject.SetActive(false);

            LogicOptionsHnS logicOptionsHnS = GameManager.Instance.LogicOptions.Cast<LogicOptionsHnS>();
            LogicHnSMusic logicHnSMusic =
                GameManager.Instance.GetLogicComponent<LogicHnSMusic>() as LogicHnSMusic;

            if (logicHnSMusic != null)
            {
                logicHnSMusic.StartMusicWithIntro();
            }

            float crewmateLeadTime = (float)logicOptionsHnS.GetCrewmateLeadTime();

            if (localPlayer.Data.Role.IsImpostor)
            {
                instance.HideAndSeekTimerText.gameObject.SetActive(true);
                instance.HideAndSeekPlayerVisual.gameObject.SetActive(true);
                instance.HideAndSeekPlayerVisual.SetBodyType(PlayerBodyTypes.Seeker);
                SpriteAnim component = instance.HideAndSeekPlayerVisual.GetComponent<SpriteAnim>();
                instance.HideAndSeekPlayerVisual.UpdateFromPlayerData(
                    localPlayer.Data,
                    localPlayer.PlayerControl.CurrentOutfitType,
                    PlayerMaterial.MaskType.None, false);
                component.Play(instance.HnSSeekerSpawnAnim, 1f);
                instance.HideAndSeekPlayerVisual.SetBodyCosmeticsVisible(false);
                instance.HideAndSeekPlayerVisual.ToggleName(false);

                while (crewmateLeadTime > 0f)
                {
                    instance.HideAndSeekTimerText.text = Mathf.RoundToInt(crewmateLeadTime).ToString();
                    crewmateLeadTime -= Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                CachedShipStatus.Instance.HideCountdown = crewmateLeadTime;
                impostor.AnimateCustom(instance.HnSSeekerSpawnAnim);
                impostor.cosmetics.SetBodyCosmeticsVisible(false);
            }
            impostor = null;
            playerSlot = null;
            yield break;
        }
    }
}
