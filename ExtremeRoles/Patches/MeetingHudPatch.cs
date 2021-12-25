using System;
using System.Linq;

using HarmonyLib;
using UnityEngine;

using UnhollowerBaseLib;

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

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Confirm))]
    class MeetingHudConfirmPatch
    {
        public static bool Prefix(
            MeetingHud __instance,
            [HarmonyArgument(0)] byte suspectStateIdx)
        {
            if (!AssassinMeeting.AssassinMeetingTrigger) { return true; }

            if (PlayerControl.LocalPlayer.PlayerId != __instance.reporterId)
            {
                return false;
            }
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                playerVoteArea.ClearButtons();
                playerVoteArea.voteComplete = true;
            }
            __instance.SkipVoteButton.ClearButtons();
            __instance.SkipVoteButton.voteComplete = true;
            __instance.SkipVoteButton.gameObject.SetActive(false);
            MeetingHud.VoteStates voteStates = __instance.state;
            if (voteStates != MeetingHud.VoteStates.NotVoted)
            {
                return false;
            }
            __instance.state = MeetingHud.VoteStates.Voted;
            __instance.CmdCastVote(PlayerControl.LocalPlayer.PlayerId, suspectStateIdx);

            return false;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
    class MeetingHudCheckForEndVotingPatch
    {
        public static bool Prefix(
            MeetingHud __instance)
        {
            if (!AssassinMeeting.AssassinMeetingTrigger) { return true; }

            var (isVoteEnd, voteFor) = assassinVoteState(__instance);

            if (isVoteEnd)
            {
                //GameData.PlayerInfo exiled = Helper.Player.GetPlayerControlById(voteFor).Data;
                Il2CppStructArray<MeetingHud.VoterState> array = 
                    new Il2CppStructArray<MeetingHud.VoterState>(
                        __instance.playerStates.Length);


                Helper.Logging.Debug($"IsSuccess?:{ExtremeRoleManager.GameRole[voteFor].Id == ExtremeRoleId.Marlin}");

                AssassinMeeting.AssassinateMarin = ExtremeRoleManager.GameRole[
                    voteFor].Id == ExtremeRoleId.Marlin;

                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.TargetPlayerId != __instance.reporterId)
                    {
                        playerVoteArea.VotedFor = 254;
                        __instance.SetDirtyBit(1U);
                    }

                    array[i] = new MeetingHud.VoterState
                    {
                        VoterId = playerVoteArea.TargetPlayerId,
                        VotedForId = playerVoteArea.VotedFor
                    };

                }
                __instance.RpcVotingComplete(array, null, true);
            }

            return false;
        }

        private static Tuple<bool, byte> assassinVoteState(MeetingHud __instance)
        {
            bool isVoteEnd = false;
            byte voteFor = byte.MaxValue;

            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                if (playerVoteArea.TargetPlayerId == __instance.reporterId)
                {
                    isVoteEnd = playerVoteArea.DidVote;
                    voteFor = playerVoteArea.VotedFor;
                    break;
                }
            }

            return Tuple.Create(isVoteEnd, voteFor);

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
                    Module.PlayerDataContainer.DeadedAssassin.Add(id);
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

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.SetForegroundForDead))]
    class MeetingHudSetForegroundForDeadPatch
    {
        public static bool Prefix(
            MeetingHud __instance)
        {

            if (!AssassinMeeting.AssassinMeetingTrigger) { return true; }

            if (ExtremeRoleManager.GameRole[
                PlayerControl.LocalPlayer.PlayerId].Id != ExtremeRoleId.Assassin)
            { 
                return true; 
            }
            else
            {
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class MeetingHudStartPatch
    {
        public static void Postfix(
            MeetingHud __instance)
        {
            if (!AssassinMeeting.AssassinMeetingTrigger) { return; }

            DestroyableSingleton<HudManager>.Instance.Chat.gameObject.SetActive(false);
            __instance.TimerText.text = "WhoIsMarine";
        }
    }


    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    class MeetingHudUpdatePatch
    {

        public static void Postfix(MeetingHud __instance)
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
        public static bool PreFix(MeetingHud __instance)
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

}
