using System;
using System.Linq;

using HarmonyLib;
using UnityEngine;

using ExtremeRoles.Roles;

namespace ExtremeRoles.Patches
{
    public class AssassinMeeting
    {
        public static byte ExiledAssassinId = Byte.MaxValue;
        public static bool AssassinMeetingTrigger = false;
        public static bool AssassinateMarin = false;
        public static void Reset()
        {
            AssassinMeetingTrigger = false;
            AssassinateMarin = false;
            ExiledAssassinId = Byte.MaxValue;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
    class MeetingHudBloopAVoteIconPatch
    {
        public static bool Prefix(
            MeetingHud __instance,
            [HarmonyArgument(0)] GameData.PlayerInfo voterPlayer,
            [HarmonyArgument(1)] int index, [HarmonyArgument(2)] Transform parent)
        {
            SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(
                __instance.PlayerVotePrefab);

            var role = ExtremeRoleManager.GameRole[PlayerControl.LocalPlayer.PlayerId];
            bool canSeeVote = role.Id == ExtremeRoleId.Marlin;

            if (canSeeVote)
            {
                canSeeVote = ((Roles.Combination.Marlin)role).CanSeeVote;
            }

            if (!PlayerControl.GameOptions.AnonymousVotes || canSeeVote ||
                (PlayerControl.LocalPlayer.Data.IsDead && MapOption.GhostsSeeVotes))
            {
                PlayerControl.SetPlayerMaterialColors(voterPlayer.DefaultOutfit.ColorId, spriteRenderer);
            }
            else
            {
                PlayerControl.SetPlayerMaterialColors(Palette.DisabledGrey, spriteRenderer);
            }

            spriteRenderer.transform.SetParent(parent);
            spriteRenderer.transform.localScale = Vector3.zero;

            __instance.StartCoroutine(
                Effects.Bloop((float)index * 0.3f, spriteRenderer.transform, 1f, 0.5f));
            parent.GetComponent<VoteSpreader>().AddVote(spriteRenderer);

            return false;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Select))]
    class MeetingHudSelectPatch
    {
        public static bool Prefix(
            MeetingHud __instance,
            ref bool __result,
            [HarmonyArgument(0)] int suspectStateIdx)
        {
            if (!AssassinMeeting.AssassinMeetingTrigger) { return true; }

            __result = false;

            if (__instance.discussionTimer < (float)PlayerControl.GameOptions.DiscussionTime)
            {
                return __result;
            }
            if (PlayerControl.LocalPlayer.PlayerId != __instance.reporterId)
            {
                return __result;
            }
            SoundManager.Instance.PlaySound(__instance.VoteSound, false, 1f).volume = 0.8f;
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                if (suspectStateIdx != (int)playerVoteArea.TargetPlayerId)
                {
                    playerVoteArea.ClearButtons();
                }
            }
            if (suspectStateIdx != -1)
            {
                __instance.SkipVoteButton.ClearButtons();
            }

            __result = true;
            return false;

        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.ServerStart))]
    class MeetingHudServerStartPatch
    {
        static void Postfix(MeetingHud __instance)
        {
            DeadBody[] deadArray = UnityEngine.Object.FindObjectsOfType<DeadBody>();

            foreach (DeadBody deadBody in deadArray)
            {
                byte id = deadBody.ParentId;
                if (ExtremeRoleManager.GameRole[id].Id == ExtremeRoleId.Assassin)
                {
                    Modules.PlayerDataContainer.DeadedAssassin.Add(id);
                }
            }
            if (AssassinMeeting.AssassinMeetingTrigger)
            {
                int randomPlayerIndex = UnityEngine.Random.RandomRange(
                    0, __instance.playerStates.Length);
                __instance.SkipVoteButton.SetTargetPlayerId(
                    __instance.playerStates[randomPlayerIndex].TargetPlayerId);
                __instance.SkipVoteButton.Parent = __instance;
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class MeetingHudStartPatch
    {
        public static bool Prefix(
            MeetingHud __instance)
        {
            if (!AssassinMeeting.AssassinMeetingTrigger) { return true; }

            __instance.BlackBackground.sprite = ShipStatus.Instance.MeetingBackground;
            foreach (SpriteRenderer playerMaterialColors in __instance.PlayerColoredParts)
            {
                PlayerControl.LocalPlayer.SetPlayerMaterialColors(
                    playerMaterialColors);
            }
            DestroyableSingleton<HudManager>.Instance.Chat.gameObject.SetActive(true);
            DestroyableSingleton<HudManager>.Instance.Chat.SetPosition(__instance);
            DestroyableSingleton<HudManager>.Instance.StopOxyFlash();
            DestroyableSingleton<HudManager>.Instance.StopReactorFlash();
            Camera.main.GetComponent<FollowerCamera>().Locked = true;

            //AmongUsClient.Instance.DisconnectHandlers.AddUnique(__instance);

            foreach (PlayerVoteArea playerVoteArea in __instance.playerStates)
            {
                __instance.ControllerSelectable.Add(playerVoteArea.PlayerButton);
            }
            DestroyableSingleton<AchievementManager>.Instance.OnMeetingCalled();

            return false;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    class MeetingHudUpdatePatch
    {
        /*
        static void PreFix(MeetingHud __instance)
        {
            var role = ExtremeRoleManager.GameRole[PlayerControl.LocalPlayer.PlayerId];
            if(role is Roles.Combination.Marlin)
            {

            }
        }
        */
        static void Postfix(MeetingHud __instance)
        {
            if (__instance.state == MeetingHud.VoteStates.Animating) { return; }

            // Deactivate skip Button if skipping on emergency meetings is disabled
            if (OptionsHolder.AllOptions[
                (int)OptionsHolder.CommonOptionKey.DisableSkipInEmergencyMeeting].GetBool())
            {
                __instance.SkipVoteButton.gameObject.SetActive(false);
            }
            // From TOR
            // This fixes a bug with the original game where pressing the button and a kill happens simultaneously
            // results in bodies sometimes being created *after* the meeting starts, marking them as dead and
            // removing the corpses so there's no random corpses leftover afterwards

            if (AssassinMeeting.AssassinMeetingTrigger) { return; }

            foreach (DeadBody b in UnityEngine.Object.FindObjectsOfType<DeadBody>())
            {
                foreach (PlayerVoteArea pva in __instance.playerStates)
                {
                    if (pva.TargetPlayerId == b.ParentId && !pva.AmDead)
                    {
                        pva.SetDead(pva.DidReport, true);
                        pva.Overlay.gameObject.SetActive(true);
                    }
                }
                //UnityEngine.Object.Destroy(b.gameObject);
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
    class MeetingHudUpdateButtonsPatch
    {
        static bool PreFix(MeetingHud __instance)
        {
            if (!AssassinMeeting.AssassinMeetingTrigger) { return true; }

            if (AmongUsClient.Instance.AmHost)
            {
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(
                        playerVoteArea.TargetPlayerId);
                    if (playerById == null)
                    {
                        playerVoteArea.SetDisabled();
                    }
                }
            }

            return false;
        }
        
    }


    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    class MeetingHudVotingCompletePatch
    {

        static bool Prefix(
            MeetingHud __instance,
            [HarmonyArgument(0)] MeetingHud.VoterState[] states,
            [HarmonyArgument(1)] GameData.PlayerInfo exiled,
            [HarmonyArgument(2)] bool tie)
        {
            if (!AssassinMeeting.AssassinMeetingTrigger) { return true; }

            if (__instance.state == MeetingHud.VoteStates.Results)
            {
                return false;
            }
            AssassinMeeting.AssassinateMarin = Roles.ExtremeRoleManager.GameRole[
                exiled.PlayerId].Id == ExtremeRoleId.Marlin;
            __instance.state = MeetingHud.VoteStates.Results;
            __instance.resultsStartedAt = __instance.discussionTimer;
            __instance.exiledPlayer = null;
            __instance.wasTie = tie;
            __instance.SkipVoteButton.gameObject.SetActive(false);
            __instance.SkippedVoting.gameObject.SetActive(true);
            
            for (int i = 0; i < GameData.Instance.PlayerCount; i++)
            {
                PlayerControl @object = GameData.Instance.AllPlayers[i].Object;
                if (@object != null && @object.Data != null && @object.Data.Role)
                {
                    @object.Data.Role.OnVotingComplete();
                }
            }
            __instance.PopulateResults(states);
            __instance.SetupProceedButton();
            MeetingHud.VoterState voterState = states.FirstOrDefault(
                (MeetingHud.VoterState s) => s.VoterId == PlayerControl.LocalPlayer.PlayerId);
            GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(voterState.VotedForId);
            DestroyableSingleton<AchievementManager>.Instance.OnMeetingVote(PlayerControl.LocalPlayer.Data, playerById);
            
            if (DestroyableSingleton<HudManager>.Instance.Chat.IsOpen)
            {
                DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
                ControllerManager.Instance.CloseOverlayMenu(DestroyableSingleton<HudManager>.Instance.Chat.name);
            }
            ControllerManager.Instance.CloseOverlayMenu(__instance.name);
            ControllerManager.Instance.OpenOverlayMenu(__instance.name, null, __instance.ProceedButtonUi);

            return false;
        }
    }

}
