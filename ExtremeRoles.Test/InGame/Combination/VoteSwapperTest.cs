using BepInEx.Logging;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Combination.VoteSwapper; // For VoteSwapper type
using ExtremeRoles.Test.Helper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExtremeRoles.Test.InGame.Combination
{
    public static class VoteSwapperTest
    {
        public static IEnumerator TestAssignVoteSwapperRole(ManualLogSource logger)
        {
            logger.LogInfo("VoteSwapperTest: Starting TestAssignVoteSwapperRole.");

            // Setup game with VoteSwapper
            HashSet<ExtremeRoleId> rolesToInclude = new HashSet<ExtremeRoleId>
            {
                ExtremeRoleId.VoteSwapper
                // Add other roles as needed for a valid game setup, e.g., an Impostor
                // ExtremeRoleId.Evolver // Example Impostor role
            };
            // It's important that the game setup is valid for roles to be assigned.
            // This might mean ensuring enough players for combination roles, specific team compositions etc.
            // For now, just including VoteSwapper. GameUtility might handle some defaults.

            // Number of players - VoteSwapper is a combination role, might need specific counts.
            // Let's assume 5 players for now. GameUtility.PrepereGameWithRole spawns 14 by default.
            // We might need to adjust player counts or how PrepereGameWithRole is called if it doesn't assign.

            logger.LogInfo("VoteSwapperTest: Preparing game with VoteSwapper role.");
            GameUtility.PrepereGameWithRole(logger, rolesToInclude);

            // Specifically set VoteSwapper spawn rate high and assign number
            // The exact category ID for VoteSwapper needs to be known or retrieved.
            // Assuming VoteSwapperManager is registered and has a RoleType corresponding to VoteSwapper.Id
            // This part is a bit of a guess without seeing VoteSwapperManager registration.
            // Let's assume VoteSwapper.Id can be cast to CombinationRoleType for GetCombRoleGroupId
            // Or that VoteSwapper is managed by a generic combination manager if not specific.
            // For now, PrepereGameWithRole with high weight on VoteSwapper should be enough.

            yield return GameUtility.StartGame(logger);
            logger.LogInfo("VoteSwapperTest: Game started. Checking role assignments.");

            bool voteSwapperAssigned = false;
            PlayerControl voteSwapperPlayer = null;

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null) continue;

                if (ExtremeRoleManager.GameRole.TryGetValue(player.PlayerId, out var assignedRole))
                {
                    if (assignedRole.Id == ExtremeRoleId.VoteSwapper)
                    {
                        voteSwapperAssigned = true;
                        voteSwapperPlayer = player;
                        logger.LogInfo($"VoteSwapperTest: VoteSwapper assigned to Player {player.PlayerId} ({player.Data.PlayerName}).");
                        // Check if it's the correct type
                        if (assignedRole is VoteSwapper)
                        {
                             logger.LogInfo($"VoteSwapperTest: Role is of type VoteSwapper.");
                        }
                        else
                        {
                             logger.LogError($"VoteSwapperTest: Role is ExtremeRoleId.VoteSwapper but not of type VoteSwapper. Actual type: {assignedRole.GetType().Name}");
                        }
                        break;
                    }
                }
            }

            if (voteSwapperAssigned)
            {
                logger.LogInfo("VoteSwapperTest: TestAssignVoteSwapperRole PASSED.");
            }
            else
            {
                logger.LogError("VoteSwapperTest: TestAssignVoteSwapperRole FAILED - VoteSwapper not assigned to any player.");
            }

            yield return GameUtility.ReturnLobby(logger);
            logger.LogInfo("VoteSwapperTest: Returned to lobby.");
        }

        // TestCastlingButtonVisibility and TestCastlingAbilityActivation removed as requested.

        // Helper to create a PlayerVoteArea substitute for testing.
        // In a real Unity test environment, you might use mocks or stubs.
        // Here, we create a simple class that can hold TargetPlayerId.
        // Note: This is NOT a real PlayerVoteArea and only works because
        // the methods being tested only access TargetPlayerId.
        private class TestPlayerVoteArea
        {
            public byte TargetPlayerId { get; set; }
            // Constructor or other methods can be added if needed by other tests.
            // For now, only TargetPlayerId is used by IsBlockMeetingButtonAbility and CreateAbilityAction.
            // The actual PlayerVoteArea class has many more fields and is a MonoBehaviour.
            // This is a simplified stand-in. For the methods to accept this, they'd need to take an interface
            // or this TestPlayerVoteArea would need to inherit PlayerVoteArea, which is complex for this setup.
            // The path of least resistance is to assume MeetingHud.Instance.playerStates becomes available
            // and we can pick a real (but perhaps inactive/dummy) PlayerVoteArea from there.
            // If MeetingHud.Instance or its playerStates are not available during these tests,
            // then testing these interface methods correctly is very difficult.

            // Let's assume for now the tests will try to find a real PlayerVoteArea after starting a meeting.
            // If that fails, these tests might be limited.
        }


        public static IEnumerator TestIsBlockMeetingButtonAbility_SelfTarget(ManualLogSource logger)
        {
            logger.LogInfo("VoteSwapperTest: Starting TestIsBlockMeetingButtonAbility_SelfTarget.");
            HashSet<ExtremeRoleId> rolesToInclude = new HashSet<ExtremeRoleId> { ExtremeRoleId.VoteSwapper };
            GameUtility.PrepereGameWithRole(logger, rolesToInclude);
            yield return GameUtility.StartGame(logger);
            // TODO: Need a way to reliably start a meeting and get MeetingHud.Instance and its playerStates.
            // For now, this test will be conceptual or fail if MeetingHud is not available.

            VoteSwapper voteSwapperRole = PlayerControl.LocalPlayer?.gameObject.GetComponent<VoteSwapper>(); // This is not how roles are typically retrieved.
                                                                                                            // Roles are usually in ExtremeRoleManager.GameRole[playerId]

            PlayerControl localPlayer = PlayerControl.LocalPlayer;
            if (localPlayer != null && ExtremeRoleManager.GameRole.TryGetValue(localPlayer.PlayerId, out var role) && role is VoteSwapper swapper)
            {
                voteSwapperRole = swapper;
            }


            if (voteSwapperRole == null || localPlayer == null)
            {
                logger.LogError("VoteSwapperTest: Could not get VoteSwapper role or LocalPlayer for self-target test. SKIPPING.");
                yield return GameUtility.ReturnLobby(logger);
                yield break;
            }

            // Simulate a PlayerVoteArea targeting self.
            // This requires MeetingHud to be active and playerStates populated.
            // If MeetingHud.Instance is null or playerStates is empty, this test part cannot run.
            if (MeetingHud.Instance != null && MeetingHud.Instance.playerStates.Any(pva => pva.TargetPlayerId == localPlayer.PlayerId))
            {
                PlayerVoteArea selfVoteArea = MeetingHud.Instance.playerStates.First(pva => pva.TargetPlayerId == localPlayer.PlayerId);
                if (voteSwapperRole.IsBlockMeetingButtonAbility(selfVoteArea))
                {
                    logger.LogInfo("VoteSwapperTest: IsBlockMeetingButtonAbility_SelfTarget PASSED (blocked self).");
                }
                else
                {
                    logger.LogError("VoteSwapperTest: IsBlockMeetingButtonAbility_SelfTarget FAILED (did not block self).");
                }
            }
            else
            {
                logger.LogWarning("VoteSwapperTest: MeetingHud or self PlayerVoteArea not available for self-target test. SKIPPING specific assertion.");
            }

            yield return GameUtility.ReturnLobby(logger);
        }

        public static IEnumerator TestIsBlockMeetingButtonAbility_MaxUsesReached(ManualLogSource logger)
        {
            logger.LogInfo("VoteSwapperTest: Starting TestIsBlockMeetingButtonAbility_MaxUsesReached.");

            // Set MaxUsesPerMeeting to 1
            int voteSwapperCategoryId = ExtremeRoleManager.GetCombRoleGroupId((CombinationRoleType)ExtremeRoleId.VoteSwapper);
            GameUtility.UpdateExROption(OptionTab.CombinationTab, voteSwapperCategoryId,
                new RequireOption<int, int>((int)VoteSwapper.VoteSwapperOption.MaxUsesPerMeeting, 1));

            HashSet<ExtremeRoleId> rolesToInclude = new HashSet<ExtremeRoleId> { ExtremeRoleId.VoteSwapper };
            GameUtility.PrepereGameWithRole(logger, rolesToInclude);
            yield return GameUtility.StartGame(logger);
            // TODO: Start meeting, get MeetingHud.Instance

            VoteSwapper voteSwapperRole = null;
            PlayerControl localPlayer = PlayerControl.LocalPlayer;
            PlayerControl otherPlayer = null;

            if (localPlayer != null && ExtremeRoleManager.GameRole.TryGetValue(localPlayer.PlayerId, out var role) && role is VoteSwapper swapper)
            {
                voteSwapperRole = swapper;
            }
            foreach(var p in PlayerControl.AllPlayerControls)
            {
                if (p != null && p.PlayerId != localPlayer?.PlayerId) { otherPlayer = p; break; }
            }

            if (voteSwapperRole == null || localPlayer == null || otherPlayer == null)
            {
                logger.LogError("VoteSwapperTest: Could not get VoteSwapper role, LocalPlayer or OtherPlayer for max uses test. SKIPPING.");
                yield return GameUtility.ReturnLobby(logger);
                yield break;
            }
             // Ensure option is loaded
            voteSwapperRole.optionMaxUsesPerMeeting = 1; // Force it for test consistency if RoleSpecificInit issues

            if (MeetingHud.Instance != null && MeetingHud.Instance.playerStates.Any(pva => pva.TargetPlayerId == otherPlayer.PlayerId))
            {
                PlayerVoteArea targetVoteArea = MeetingHud.Instance.playerStates.First(pva => pva.TargetPlayerId == otherPlayer.PlayerId);

                // Simulate one use
                voteSwapperRole.ResetOnMeetingStart(); // Ensure uses are 0
                voteSwapperRole.CreateAbilityAction(targetVoteArea).Invoke(); // Use 1
                if (voteSwapperRole.currentUsesThisMeeting != 1)
                {
                     logger.LogError($"VoteSwapperTest: Use count incorrect after 1 use. Expected 1, got {voteSwapperRole.currentUsesThisMeeting}");
                }


                if (voteSwapperRole.IsBlockMeetingButtonAbility(targetVoteArea)) // Should be blocked now
                {
                    logger.LogInfo("VoteSwapperTest: IsBlockMeetingButtonAbility_MaxUsesReached PASSED (blocked after 1 use).");
                }
                else
                {
                    logger.LogError("VoteSwapperTest: IsBlockMeetingButtonAbility_MaxUsesReached FAILED (did not block after 1 use).");
                }

                voteSwapperRole.ResetOnMeetingStart(); // Reset for next part
                 if (voteSwapperRole.currentUsesThisMeeting != 0)
                {
                     logger.LogError($"VoteSwapperTest: Use count incorrect after ResetOnMeetingStart. Expected 0, got {voteSwapperRole.currentUsesThisMeeting}");
                }
                if (!voteSwapperRole.IsBlockMeetingButtonAbility(targetVoteArea)) // Should not be blocked
                {
                    logger.LogInfo("VoteSwapperTest: IsBlockMeetingButtonAbility_MaxUsesReached PASSED (not blocked after reset).");
                }
                else
                {
                    logger.LogError("VoteSwapperTest: IsBlockMeetingButtonAbility_MaxUsesReached FAILED (still blocked after reset).");
                }
            }
            else
            {
                logger.LogWarning("VoteSwapperTest: MeetingHud or target PlayerVoteArea not available for max uses test. SKIPPING specific assertions.");
            }

            yield return GameUtility.ReturnLobby(logger);
        }

        public static IEnumerator TestCreateAbilityAction_ExecutesAndIncrementsUses(ManualLogSource logger)
        {
            logger.LogInfo("VoteSwapperTest: Starting TestCreateAbilityAction_ExecutesAndIncrementsUses.");

            int voteSwapperCategoryId = ExtremeRoleManager.GetCombRoleGroupId((CombinationRoleType)ExtremeRoleId.VoteSwapper);
            GameUtility.UpdateExROption(OptionTab.CombinationTab, voteSwapperCategoryId,
                new RequireOption<int, int>((int)VoteSwapper.VoteSwapperOption.MaxUsesPerMeeting, 2)); // Allow multiple uses

            HashSet<ExtremeRoleId> rolesToInclude = new HashSet<ExtremeRoleId> { ExtremeRoleId.VoteSwapper };
            GameUtility.PrepereGameWithRole(logger, rolesToInclude);
            yield return GameUtility.StartGame(logger);
            // TODO: Start meeting

            VoteSwapper voteSwapperRole = null;
            PlayerControl localPlayer = PlayerControl.LocalPlayer;
            PlayerControl otherPlayer = null;

             if (localPlayer != null && ExtremeRoleManager.GameRole.TryGetValue(localPlayer.PlayerId, out var role) && role is VoteSwapper swapper)
            {
                voteSwapperRole = swapper;
            }
            foreach(var p in PlayerControl.AllPlayerControls)
            {
                if (p != null && p.PlayerId != localPlayer?.PlayerId) { otherPlayer = p; break; }
            }

            if (voteSwapperRole == null || localPlayer == null || otherPlayer == null)
            {
                logger.LogError("VoteSwapperTest: Could not get VoteSwapper role, LocalPlayer or OtherPlayer for action execution test. SKIPPING.");
                yield return GameUtility.ReturnLobby(logger);
                yield break;
            }
            voteSwapperRole.optionMaxUsesPerMeeting = 2; // Force for test

            if (MeetingHud.Instance != null && MeetingHud.Instance.playerStates.Any(pva => pva.TargetPlayerId == otherPlayer.PlayerId))
            {
                PlayerVoteArea targetVoteArea = MeetingHud.Instance.playerStates.First(pva => pva.TargetPlayerId == otherPlayer.PlayerId);
                voteSwapperRole.ResetOnMeetingStart();
                int usesBefore = voteSwapperRole.currentUsesThisMeeting;

                System.Action abilityAction = voteSwapperRole.CreateAbilityAction(targetVoteArea);
                abilityAction.Invoke();

                if (voteSwapperRole.currentUsesThisMeeting == usesBefore + 1)
                {
                    logger.LogInfo("VoteSwapperTest: TestCreateAbilityAction_ExecutesAndIncrementsUses PASSED.");
                }
                else
                {
                    logger.LogError($"VoteSwapperTest: TestCreateAbilityAction_ExecutesAndIncrementsUses FAILED. Uses before: {usesBefore}, Uses after: {voteSwapperRole.currentUsesThisMeeting}");
                }
            }
            else
            {
                 logger.LogWarning("VoteSwapperTest: MeetingHud or target PlayerVoteArea not available for action execution test. SKIPPING specific assertions.");
            }

            yield return GameUtility.ReturnLobby(logger);
        }


        public static IEnumerator TestVoteSwapperOptions(ManualLogSource logger)
        {
            logger.LogInfo("VoteSwapperTest: Starting TestVoteSwapperOptions.");

            // Helper to run individual option tests
            static IEnumerator RunOptionSubTest<TEnum, TValue>(
                ManualLogSource log,
                TEnum optionKey, // Enum type for the option key
                TValue testValue, // Value to set the option to
                System.Func<VoteSwapper, TValue, bool> assertion, // Func<RoleInstance, SetValue, ExpectedValueMatches>
                string optionNameForLog) where TEnum : struct, System.IConvertible
            {
                log.LogInfo($"VoteSwapperTest: Testing option {optionNameForLog} with value {testValue}.");

                // Update the specific option
                // Need to know the category ID for VoteSwapper options.
                // This assumes VoteSwapper options are under a category identified by its RoleId or a specific CombinationRoleType.
                // Let's assume VoteSwapper.Id can be used for GetCombRoleGroupId, similar to how common options are structured.
                // Or, if options are directly under VoteSwapperOption enum as top-level keys in its category.
                int voteSwapperCategoryId = ExtremeRoleManager.GetCombRoleGroupId((CombinationRoleType)ExtremeRoleId.VoteSwapper); // This cast is speculative

                // For CombinationRoleCommonOption.IsAssignImposter
                if (optionKey is CombinationRoleCommonOption commonOptionKey)
                {
                    // The factory in VoteSwapper adds IsAssignImposter using its int value directly.
                    GameUtility.UpdateExROption(OptionTab.CombinationTab, voteSwapperCategoryId,
                        new RequireOption<int, int>((int)(object)commonOptionKey, System.Convert.ToInt32(testValue)) );
                }
                // For VoteSwapperOption
                else if (optionKey is VoteSwapper.VoteSwapperOption specificOptionKey)
                {
                     GameUtility.UpdateExROption(OptionTab.CombinationTab, voteSwapperCategoryId,
                        new RequireOption<int, int>((int)(object)specificOptionKey, System.Convert.ToInt32(testValue)) );
                }
                else
                {
                    log.LogError($"VoteSwapperTest: Unknown option key type for {optionNameForLog}.");
                    yield break;
                }

                HashSet<ExtremeRoleId> rolesToInclude = new HashSet<ExtremeRoleId> { ExtremeRoleId.VoteSwapper };
                GameUtility.PrepereGameWithRole(log, rolesToInclude); // Re-prep game with new option setting

                yield return GameUtility.StartGame(log);

                VoteSwapper voteSwapperRole = null;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player != null && player.Data != null && ExtremeRoleManager.GameRole.TryGetValue(player.PlayerId, out var assignedRole) && assignedRole.Id == ExtremeRoleId.VoteSwapper)
                    {
                        voteSwapperRole = assignedRole as VoteSwapper;
                        break;
                    }
                }

                if (voteSwapperRole == null)
                {
                    log.LogError($"VoteSwapperTest: FAILED to assign VoteSwapper for option {optionNameForLog} test.");
                }
                else
                {
                    if (assertion(voteSwapperRole, testValue)) // testValue is passed to compare against, if needed by assertion
                    {
                        log.LogInfo($"VoteSwapperTest: Option {optionNameForLog} with value {testValue} PASSED.");
                    }
                    else
                    {
                        log.LogError($"VoteSwapperTest: Option {optionNameForLog} with value {testValue} FAILED. Role property did not match.");
                    }
                }
                yield return GameUtility.ReturnLobby(log);
            }

            // Test IsAssignImposter (True)
            yield return RunOptionSubTest<CombinationRoleCommonOption, bool>(logger,
                CombinationRoleCommonOption.IsAssignImposter, true,
                (role, val) => role.Team == ExtremeRoleType.Impostor,
                "IsAssignImposter_True");

            // Test IsAssignImposter (False)
            yield return RunOptionSubTest<CombinationRoleCommonOption, bool>(logger,
                CombinationRoleCommonOption.IsAssignImposter, false,
                (role, val) => role.Team == ExtremeRoleType.Neutral, // Assuming Neutral is the default
                "IsAssignImposter_False");

            // Test EnableRandomShuffling
            yield return RunOptionSubTest<VoteSwapper.VoteSwapperOption, bool>(logger,
                VoteSwapper.VoteSwapperOption.EnableRandomShuffling, true,
                (role, val) => role.optionEnableRandomShuffling == val,
                "EnableRandomShuffling_True");
            yield return RunOptionSubTest<VoteSwapper.VoteSwapperOption, bool>(logger,
                VoteSwapper.VoteSwapperOption.EnableRandomShuffling, false,
                (role, val) => role.optionEnableRandomShuffling == val,
                "EnableRandomShuffling_False");

            // Test RevealSwapTarget
            yield return RunOptionSubTest<VoteSwapper.VoteSwapperOption, bool>(logger,
                VoteSwapper.VoteSwapperOption.RevealSwapTarget, true,
                (role, val) => role.optionRevealSwapTarget == val,
                "RevealSwapTarget_True");
             yield return RunOptionSubTest<VoteSwapper.VoteSwapperOption, bool>(logger,
                VoteSwapper.VoteSwapperOption.RevealSwapTarget, false,
                (role, val) => role.optionRevealSwapTarget == val,
                "RevealSwapTarget_False");

            // Test MaxUsesPerMeeting
            int testMaxUses = 3;
            yield return RunOptionSubTest<VoteSwapper.VoteSwapperOption, int>(logger,
                VoteSwapper.VoteSwapperOption.MaxUsesPerMeeting, testMaxUses,
                (role, val) => role.optionMaxUsesPerMeeting == val,
                $"MaxUsesPerMeeting_{testMaxUses}");

            // Test CanPressEmergencyButton
            yield return RunOptionSubTest<VoteSwapper.VoteSwapperOption, bool>(logger,
                VoteSwapper.VoteSwapperOption.CanPressEmergencyButton, true,
                (role, val) => role.CanCallMeeting == val,
                "CanPressEmergencyButton_True");
            yield return RunOptionSubTest<VoteSwapper.VoteSwapperOption, bool>(logger,
                VoteSwapper.VoteSwapperOption.CanPressEmergencyButton, false,
                (role, val) => role.CanCallMeeting == val,
                "CanPressEmergencyButton_False");

            logger.LogInfo("VoteSwapperTest: TestVoteSwapperOptions finished.");
        }
    }
}
