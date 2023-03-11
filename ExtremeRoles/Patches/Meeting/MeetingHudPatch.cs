using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using UnityEngine;

using Il2CppInterop.Runtime.InteropTypes.Arrays;

using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;


namespace ExtremeRoles.Patches.Meeting
{

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
    public static class MeetingHudBloopAVoteIconPatch
    {
        public static bool Prefix(
            MeetingHud __instance,
            [HarmonyArgument(0)] GameData.PlayerInfo voterPlayer,
            [HarmonyArgument(1)] int index, [HarmonyArgument(2)] Transform parent)
        {

            if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

            SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(
                __instance.PlayerVotePrefab);

            var role = ExtremeRoleManager.GetLocalPlayerRole();
            
            bool canSeeVote = false;
            
            var mariln = role as Roles.Combination.Marlin;
            var assassin = role as Roles.Combination.Assassin;

            if (mariln != null)
            {
                canSeeVote = mariln.CanSeeVote;
            }
            if (assassin != null)
            {
                canSeeVote = assassin.CanSeeVote;
            }


            if (!GameManager.Instance.LogicOptions.GetAnonymousVotes() || 
                canSeeVote ||
                (
                    CachedPlayerControl.LocalPlayer.Data.IsDead && 
                    OptionHolder.Client.GhostsSeeVote &&
                    !isVoteSeeBlock(role)
                ))
            {
                PlayerMaterial.SetColors(voterPlayer.DefaultOutfit.ColorId, spriteRenderer);
            }
            else
            {
                PlayerMaterial.SetColors(Palette.DisabledGrey, spriteRenderer);
            }

            spriteRenderer.transform.SetParent(parent);
            spriteRenderer.transform.localScale = Vector3.zero;

            __instance.StartCoroutine(
                Effects.Bloop((float)index * 0.3f,
                spriteRenderer.transform, 1f, 0.5f));
            parent.GetComponent<VoteSpreader>().AddVote(spriteRenderer);

            return false;
        }

        private static bool isVoteSeeBlock(SingleRoleBase role)
        {
            if (ExtremeGhostRoleManager.GameRole.ContainsKey(
                    CachedPlayerControl.LocalPlayer.PlayerId) ||
                CachedPlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel)
            {
                return true;
            }
            else if (
                (role.IsImpostor() && role.Id != ExtremeRoleId.Assassin) ||
                role.Id == ExtremeRoleId.Madmate ||
                role.Id == ExtremeRoleId.Doll)
            {
                return ExtremeRolesPlugin.ShipState.IsAssassinAssign;
            }
            return role.IsBlockShowMeetingRoleInfo();
        }

    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Confirm))]
    public static class MeetingHudConfirmPatch
    {
        public static bool Prefix(
            MeetingHud __instance,
            [HarmonyArgument(0)] byte suspectStateIdx)
        {
            if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return true; }

            if (CachedPlayerControl.LocalPlayer.PlayerId != ExtremeRolesPlugin.ShipState.ExiledAssassinId)
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
            __instance.CmdCastVote(
                CachedPlayerControl.LocalPlayer.PlayerId, suspectStateIdx);

            return false;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
    public static class MeetingHudCheckForEndVotingPatch
    {
        public static bool Prefix(
            MeetingHud __instance)
        {
            if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

            if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger)
            {
                normalMeetingVote(__instance);
            }
            else
            {
                assassinMeetingVote(__instance);
            }

            return false;
        }

        private static void assassinMeetingVote(MeetingHud instance)
        {
            var (isVoteEnd, voteFor) = assassinVoteState(instance);

            if (isVoteEnd)
            {
                //GameData.PlayerInfo exiled = Helper.Player.GetPlayerControlById(voteFor).Data;
                Il2CppStructArray<MeetingHud.VoterState> array =
                    new Il2CppStructArray<MeetingHud.VoterState>(
                        instance.playerStates.Length);

                if (voteFor == 254 || voteFor == byte.MaxValue)
                {
                    bool targetImposter;
                    do
                    {
                        int randomPlayerIndex = UnityEngine.Random.RandomRange(
                            0, instance.playerStates.Length);
                        voteFor = instance.playerStates[randomPlayerIndex].TargetPlayerId;

                        targetImposter = ExtremeRoleManager.GameRole[voteFor].IsImpostor();

                    }
                    while (targetImposter);
                }

                Helper.Logging.Debug($"IsSuccess?:{ExtremeRoleManager.GameRole[voteFor].Id == ExtremeRoleId.Marlin}");

                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.AssasinVoteFor))
                {
                    caller.WriteByte(voteFor);
                }
                RPCOperator.AssasinVoteFor(voteFor);

                for (int i = 0; i < instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = instance.playerStates[i];
                    if (playerVoteArea.TargetPlayerId == ExtremeRolesPlugin.ShipState.ExiledAssassinId)
                    {
                        playerVoteArea.VotedFor = voteFor;
                    }
                    else
                    {
                        playerVoteArea.VotedFor = 254;
                    }
                    instance.SetDirtyBit(1U);

                    array[i] = new MeetingHud.VoterState
                    {
                        VoterId = playerVoteArea.TargetPlayerId,
                        VotedForId = playerVoteArea.VotedFor
                    };

                }
                instance.RpcVotingComplete(array, null, true);
            }
        }

        private static (bool, byte) assassinVoteState(MeetingHud instance)
        {
            bool isVoteEnd = false;
            byte voteFor = byte.MaxValue;

            foreach (PlayerVoteArea playerVoteArea in instance.playerStates)
            {
                if (playerVoteArea.TargetPlayerId == ExtremeRolesPlugin.ShipState.ExiledAssassinId)
                {
                    isVoteEnd = playerVoteArea.DidVote;
                    voteFor = playerVoteArea.VotedFor;
                    break;
                }
            }

            return (isVoteEnd, voteFor);
        }

        private static void addVoteModRole(
            IRoleVoteModifier role, byte rolePlayerId,
            ref SortedList<int, (IRoleVoteModifier, byte)> voteModifier)
        {
            if (role != null)
            {
                int order = role.Order;
                // 同じ役職は同じ優先度になるので次の優先度になるようにセット
                while (voteModifier.ContainsKey(order))
                {
                    ++order;
                }
                voteModifier.Add(order, (role, rolePlayerId));
            }
        }

        private static Dictionary<byte, int> calculateVote(MeetingHud instance)
        {

            RPCOperator.Call(RPCOperator.Command.CloseMeetingVoteButton);
            RPCOperator.CloseMeetingButton();

            Dictionary<byte, int> voteResult = new Dictionary<byte, int>();
            Dictionary<byte, byte> voteTarget = new Dictionary<byte, byte>();

            SortedList<int, (IRoleVoteModifier, byte)> voteModifier = new SortedList<int, (IRoleVoteModifier, byte)>();

            foreach (PlayerVoteArea playerVoteArea in instance.playerStates)
            {
                
                byte playerId = playerVoteArea.TargetPlayerId;

                // 切断されたプレイヤーは残っている状態で役職を持たない状態になるのでキーチェックはしておく
                if (ExtremeRoleManager.GameRole.ContainsKey(playerId))
                {
                    // 投票をいじる役職か？
                    var (voteModRole, voteAnotherRole) = 
                        ExtremeRoleManager.GetInterfaceCastedRole<IRoleVoteModifier>(playerId);
                    addVoteModRole(voteModRole, playerId, ref voteModifier);
                    addVoteModRole(voteAnotherRole, playerId, ref voteModifier);
                }

                // 投票先を全格納
                voteTarget.Add(playerId, playerVoteArea.VotedFor);

                if (playerVoteArea.VotedFor != 252 && 
                    playerVoteArea.VotedFor != 255 && 
                    playerVoteArea.VotedFor != 254)
                {
                    int currentVotes;
                    if (voteResult.TryGetValue(playerVoteArea.VotedFor, out currentVotes))
                    {
                        voteResult[playerVoteArea.VotedFor] = currentVotes + 1;
                    }
                    else
                    {
                        voteResult[playerVoteArea.VotedFor] = 1;
                    }
                }
            }

            foreach (var (role, playerId) in voteModifier.Values)
            {
                role.ModifiedVote(playerId, ref voteTarget, ref voteResult);
            }
            return voteResult;

        }

        private static void normalMeetingVote(MeetingHud instance)
        {
            if (instance.playerStates.All((PlayerVoteArea ps) => ps.AmDead || ps.DidVote))
            {
                Dictionary<byte, int> result = calculateVote(instance);

                bool isExiled = true;
                
                KeyValuePair<byte, int> exiledResult = new KeyValuePair<byte, int>(
                    byte.MaxValue, int.MinValue);
                foreach (KeyValuePair<byte, int> item in result)
                {
                    if (item.Value > exiledResult.Value)
                    {
                        exiledResult = item;
                        isExiled = false;
                    }
                    else if (item.Value == exiledResult.Value)
                    {
                        isExiled = true;
                    }
                }

                GameData.PlayerInfo exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(
                    (GameData.PlayerInfo v) => !isExiled && v.PlayerId == exiledResult.Key);
                
                MeetingHud.VoterState[] array = new MeetingHud.VoterState[instance.playerStates.Length];
                for (int i = 0; i < instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = instance.playerStates[i];
                    array[i] = new MeetingHud.VoterState
                    {
                        VoterId = playerVoteArea.TargetPlayerId,
                        VotedForId = playerVoteArea.VotedFor
                    };
                }
                
                instance.RpcVotingComplete(array, exiled, isExiled);
            }
        }
    }


    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Select))]
    public static class MeetingHudSelectPatch
    {
        private static bool isBlock = false;

        public static void SetSelectBlock(bool isBlockActive)
        {
            isBlock = isBlockActive;
        }

        public static bool Prefix(
            MeetingHud __instance,
            ref bool __result,
            [HarmonyArgument(0)] int suspectStateIdx)
        {
            __result = false;

            if (isBlock) { return false; }

            var shipOpt = ExtremeGameModeManager.Instance.ShipOption;

            if (shipOpt.DisableSelfVote &&
                CachedPlayerControl.LocalPlayer.PlayerId == suspectStateIdx)
            {
                return false;
            }
            if (shipOpt.IsBlockSkipInMeeting && suspectStateIdx == -1)
            {
                return false;
            }

            if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return true; }

            LogicOptionsNormal logicOptionsNormal = GameManager.Instance.LogicOptions.Cast<
                LogicOptionsNormal>();

            if (__instance.discussionTimer < (float)logicOptionsNormal.GetDiscussionTime())
            {
                return __result;
            }
            if (CachedPlayerControl.LocalPlayer.PlayerId != ExtremeRolesPlugin.ShipState.ExiledAssassinId)
            {
                return __result;
            }

            SoundManager.Instance.PlaySound(
                __instance.VoteSound, false, 1f, null).volume = 0.8f;
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

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.SetForegroundForDead))]
    public static class MeetingHudSetForegroundForDeadPatch
    {
        public static bool Prefix(
            MeetingHud __instance)
        {

            if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return true; }

            if (CachedPlayerControl.LocalPlayer.PlayerId != 
                ExtremeRolesPlugin.ShipState.ExiledAssassinId)
            { 
                return true; 
            }
            else
            {
                __instance.amDead = false;
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    public static class MeetingHudClosePatch
    {
        public static void Prefix(
            MeetingHud __instance)
        {
            ExtremeRolesPlugin.Info.HideInfoOverlay();
        }
    }


    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CoIntro))]
    public static class MeetingHudCoIntroPatch
    {
        public static void Postfix(
            MeetingHud __instance,
            [HarmonyArgument(0)] GameData.PlayerInfo reporter,
            [HarmonyArgument(1)] GameData.PlayerInfo reportedBody)
        {
            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger)
            {
                var player = CachedPlayerControl.LocalPlayer;
                var hookRole = ExtremeRoleManager.GetLocalPlayerRole() as IRoleReportHook;
                var multiAssignRole = ExtremeRoleManager.GetLocalPlayerRole() as MultiAssignRoleBase;

                if (hookRole != null)
                {
                    if (reportedBody == null)
                    {
                        hookRole.HookReportButton(
                            player, reporter);
                    }
                    else
                    {
                        hookRole.HookBodyReport(
                            player, reporter, reportedBody);
                    }
                }
                if (multiAssignRole != null)
                {
                    hookRole = multiAssignRole.AnotherRole as IRoleReportHook;
                    if (hookRole != null)
                    {

                        if (reportedBody == null)
                        {
                            hookRole.HookReportButton(
                                player, reporter);
                        }
                        else
                        {
                            hookRole.HookBodyReport(
                                player, reporter, reportedBody);
                        }
                    }
                }

            }
            else
            {
                __instance.TitleText.text = Helper.Translation.GetString("whoIsMarine");
            }
        }
    }


    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public static class MeetingHudStartPatch
    {
        public static void Postfix(
            MeetingHud __instance)
        {
            ExtremeRolesPlugin.ShipState.ClearMeetingResetObject();
            Helper.Player.ResetTarget();
            MeetingHudSelectPatch.SetSelectBlock(false);

            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            var role = ExtremeRoleManager.GetLocalPlayerRole();

            if (role is IRoleAbility abilityRole)
            {
                abilityRole.Button.OnMeetingStart();
            }
            if (role is IRoleResetMeeting resetRole)
            {
                resetRole.ResetOnMeetingStart();
            }

            var multiAssignRole = role as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole is IRoleAbility multiAssignAbilityRole)
                {
                    multiAssignAbilityRole.Button.OnMeetingStart();
                }
                if (multiAssignRole.AnotherRole is IRoleResetMeeting multiAssignResetRole)
                {
                    multiAssignResetRole.ResetOnMeetingStart();
                }
            }

            var ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
            if (ghostRole != null)
            {
                ghostRole.ResetOnMeetingStart();
            }

            if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return; }

            FastDestroyableSingleton<HudManager>.Instance.Chat.gameObject.SetActive(false);
        }
    }


    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    public static class MeetingHudUpdatePatch
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (ExtremeRolesPlugin.Info.IsBlock &&
                __instance.state != MeetingHud.VoteStates.Animating)
            {
                ExtremeRolesPlugin.Info.BlockShow(false);
            }

            if (NamePlateHelper.NameplateChange)
            {
                foreach (var pva in __instance.playerStates)
                {
                    NamePlateHelper.UpdateNameplate(pva);
                }
                NamePlateHelper.NameplateChange = false;
            }

            if (__instance.state == MeetingHud.VoteStates.Animating) { return; }

            // From TOR
            // This fixes a bug with the original game where pressing the button and a kill happens simultaneously
            // results in bodies sometimes being created *after* the meeting starts, marking them as dead and
            // removing the corpses so there's no random corpses leftover afterwards

            foreach (DeadBody b in UnityEngine.Object.FindObjectsOfType<DeadBody>())
            {
                if (b == null) { continue; }

                foreach (PlayerVoteArea pva in __instance.playerStates)
                {
                    if (pva == null ||
                        pva.TargetPlayerId != b.ParentId ||
                        pva.AmDead)
                    {
                        continue;
                    }
                    pva.SetDead(pva.DidReport, true);
                    pva.Overlay.gameObject.SetActive(true);
                }
                UnityEngine.Object.Destroy(b.gameObject);
            }

            // Deactivate skip Button if skipping on emergency meetings is disabled
            if (ExtremeGameModeManager.Instance.ShipOption.IsBlockSkipInMeeting)
            {
                __instance.SkipVoteButton.gameObject.SetActive(false);
            }

            if (ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger)
            {
                __instance.TitleText.text = Helper.Translation.GetString(
                    "whoIsMarine");
                __instance.SkipVoteButton.gameObject.SetActive(false);

                if (CachedPlayerControl.LocalPlayer.PlayerId == ExtremeRolesPlugin.ShipState.ExiledAssassinId ||
                    ExtremeRoleManager.GetLocalPlayerRole().IsImpostor())
                {
                    FastDestroyableSingleton<HudManager>.Instance.Chat.gameObject.SetActive(true);
                }
                else
                {
                    FastDestroyableSingleton<HudManager>.Instance.Chat.gameObject.SetActive(false);
                }

                return;
            }
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.SortButtons))]
    public static class MeetingHudSortButtonsPatch
    {
        public static bool Prefix(MeetingHud __instance)
        {
            if (!ExtremeGameModeManager.Instance.ShipOption.IsChangeVoteAreaButtonSortArg) { return true; }
            if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

            PlayerVoteArea[] array = __instance.playerStates.OrderBy(delegate (PlayerVoteArea p)
            {
                if (!p.AmDead)
                {
                    return 0;
                }
                return 50;
            }).ThenBy(x => playerName2Int(x)).ToArray();

            for (int i = 0; i < array.Length; i++)
            {
                int num = i % 3;
                int num2 = i / 3;
                array[i].transform.localPosition = __instance.VoteOrigin + new Vector3(
                    __instance.VoteButtonOffsets.x * (float)num,
                    __instance.VoteButtonOffsets.y * (float)num2, -0.9f - (float)num2 * 0.01f);
            }

            return false;
        }

        public static void Postfix(MeetingHud __instance)
        {
            if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return; }

            bool isHudOverrideTaskActive = PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(
                CachedPlayerControl.LocalPlayer);

            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                VoteAreaInfo playerInfoUpdater;
                if (playerVoteArea.TargetPlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
                {
                    playerInfoUpdater = 
                        __instance.gameObject.AddComponent<LocalPlayerVoteAreaInfo>();
                }
                else
                {
                    playerInfoUpdater = 
                        __instance.gameObject.AddComponent<OtherPlayerVoteAreaInfo>();
                }
                playerInfoUpdater.Init(playerVoteArea, isHudOverrideTaskActive);
            }
        }

        private static int playerName2Int(PlayerVoteArea pva)
        {
            var player = GameData.Instance.GetPlayerById(pva.TargetPlayerId);
            if (player == null) { return 0; }
            byte[] bytedPlayerName = System.Text.Encoding.UTF8.GetBytes(
                player.DefaultOutfit.PlayerName.Trim());
            
            return BitConverter.ToInt32(bytedPlayerName, 0);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
    public static class MeetingHudPopulateResultsPatch
    {
        public static bool Prefix(
            MeetingHud __instance,
            [HarmonyArgument(0)] Il2CppStructArray<MeetingHud.VoterState> states)
        {

            if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

            __instance.TitleText.text = 
                FastDestroyableSingleton<TranslationController>.Instance.GetString(
                    StringNames.MeetingVotingResults, Array.Empty<Il2CppSystem.Object>());

            Dictionary<byte, int> voteIndex = new Dictionary<byte, int>();
            SortedList<int, (IRoleVoteModifier, GameData.PlayerInfo)> voteModifier = new SortedList<
                int, (IRoleVoteModifier, GameData.PlayerInfo)>();

            int num = 0;
            // それぞれの人に対してどんな投票があったか
		    for (int i = 0; i < __instance.playerStates.Length; i++)
		    {
			    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
			    playerVoteArea.ClearForResults();

                byte checkPlayerId = playerVoteArea.TargetPlayerId;

                // 切断されたプレイヤーは残っている状態で役職を持たない状態になるのでキーチェックはしておく
                if (ExtremeRoleManager.GameRole.ContainsKey(checkPlayerId))
                {
                    // 投票をいじる役職か？
                    var (voteModRole, voteAnotherRole) = 
                        ExtremeRoleManager.GetInterfaceCastedRole<IRoleVoteModifier>(checkPlayerId);
                    addVoteModRole(voteModRole, checkPlayerId, ref voteModifier);
                    addVoteModRole(voteAnotherRole, checkPlayerId, ref voteModifier);
                }

                int num2 = 0;
			    foreach (MeetingHud.VoterState voterState in states)
			    {
                    GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(voterState.VoterId);
				    if (playerById == null)
				    {
					    Debug.LogError(
                            string.Format("Couldn't find player info for voter: {0}",
                            voterState.VoterId));
				    }
				    else if (i == 0 && voterState.SkippedVote)
				    {
                        // スキップのアニメーション
                        __instance.BloopAVoteIcon(playerById, num, __instance.SkippedVoting.transform);
					    num++;
                    }
				    else if (voterState.VotedForId == checkPlayerId)
				    {
                        // 投票された人のアニメーション
                        __instance.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
					    num2++;
                    }
			    }
                voteIndex.Add(playerVoteArea.TargetPlayerId, num2);
            }

            voteIndex[ __instance.playerStates[0].TargetPlayerId] = num;

            foreach (var (role, player) in voteModifier.Values)
            {
                role.ModifiedVoteAnime(
                    __instance, player, ref voteIndex);
                role.ResetModifier();
            }
            return false;
        }
        private static void addVoteModRole(
            IRoleVoteModifier role, byte rolePlayerId,
            ref SortedList<int, (IRoleVoteModifier, GameData.PlayerInfo)> voteModifier)
        {
            GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(rolePlayerId);
            if (role != null)
            {
                int order = role.Order;
                // 同じ役職は同じ優先度になるので次の優先度になるようにセット
                while (voteModifier.ContainsKey(order))
                {
                    ++order;
                }
                voteModifier.Add(order, (role, playerById));
            }
        }
    }


    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
    public static class MeetingHudUpdateButtonsPatch
    {
        public static bool Prefix(MeetingHud __instance)
        {
            if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return true; }

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
                    else
                    {
                        playerVoteArea.SetDead(
                            __instance.reporterId == playerById.PlayerId, false, false);
                        __instance.SetDirtyBit(1U);
                    }
                }
            }

            return false;
        }
        
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    public static class MeetingHudVotingCompletedPatch
    {
        public static void Postfix()
        {
            ExtremeRolesPlugin.Info.HideInfoOverlay();

            foreach (DeadBody b in UnityEngine.Object.FindObjectsOfType<DeadBody>())
            {
                UnityEngine.Object.Destroy(b?.gameObject);
            }
        }
    }
}
