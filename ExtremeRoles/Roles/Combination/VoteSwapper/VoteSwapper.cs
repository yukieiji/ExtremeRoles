using ExtremeRoles.Core;
using ExtremeRoles.Core.Roles;
using ExtremeRoles.Core.Tabs;
using ExtremeRoles.Enums;
using ExtremeRoles.Helpers;
using UnityEngine;
using ExtremeRoles.Module.CustomOption.Factory; // Needed for AutoParentSetOptionCategoryFactory
using ExtremeRoles.Module.CustomOption.Interfaces; // Needed for IOption (parent option)
using ExtremeRoles.Roles.API; // Needed for CombinationRoleCommonOption, MultiAssignRoleBase
using ExtremeRoles.Roles.API.Interface; // For IRoleMeetingButtonAbility, IRoleResetMeeting
using ExtremeRoles.Performance; // For PlayerVoteArea, NetworkedPlayerInfo, PlayerControl
using ExtremeRoles.Module; // For UnityObjectLoader
using System; // For Action
using System.Linq; // For FirstOrDefault

// Removed duplicate namespace. Assuming the outer one is correct if this file is in specific VoteSwapper folder.
// If VoteSwapper.cs is directly in Roles/Combination/, then the outer namespace is not needed.
// For now, assuming the structure `ExtremeRoles/Roles/Combination/VoteSwapper/VoteSwapper.cs` means the inner namespace is the one to keep.
// Corrected: The file is in ExtremeRoles/Roles/Combination/VoteSwapper/, so the namespace should be ExtremeRoles.Roles.Combination.VoteSwapper
// The using for Core (for PlayerControl) was duplicated by the one for Performance.Consolidating.
// Removed: using ExtremeRoles.Module.Ability;
// Removed: using ExtremeRoles.Module.Ability.Behavior;

namespace ExtremeRoles.Roles.Combination.VoteSwapper
{
    public class VoteSwapper : MultiAssignRoleBase, IRoleMeetingButtonAbility, IRoleResetMeeting
    {
        // Removed old ability fields:
        // internal CastlingAbility castlingAbility;
        // private ExtremeAbilityButton castlingButton;

        // Option fields - make internal for testing access
        internal bool optionEnableRandomShuffling;
        internal bool optionRevealSwapTarget;
        internal int optionMaxUsesPerMeeting;
        // CanCallMeeting is already a public field in base
        // Team property reflects IsAssignImposter

        internal int currentUsesThisMeeting; // Changed to internal for testing
        private VoteSwapperAnimator voteSwapperAnimator; // Added

        // Define role-specific options
        private enum VoteSwapperOption
        {
            EnableRandomShuffling,
            RevealSwapTarget,
            MaxUsesPerMeeting,
            CanPressEmergencyButton, // Added
            // CanUseInEmergencyMeeting // Example if needed
        }

        public VoteSwapper()
        {
            Id = ExtremeRoleId.VoteSwapper;
            Team = ExtremeRoleType.Neutral; // Default team
            RawRoleName = "VoteSwapper";
            NameColor = ColorPalette.ImpostorRed;
            CanKill = false;
            HasTask = true;
            UseVent = false;
            UseSabotage = false;
            Tab = OptionTab.CombinationTab;
            CanCallMeeting = true; // Default value for CanCallMeeting
            // Removed old ability initialization from constructor
        }

        // Removed SetupAbilities method
        // Removed Update method (assuming it was only for the old button logic)

        // IRoleMeetingButtonAbility Implementations
        public Sprite AbilityImage => UnityObjectLoader.LoadFromResources(ExtremeRoleId.Guesser); // Placeholder icon

        public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance)
        {
            if (instance == null) return true;
            byte targetPlayerId = instance.TargetPlayerId;

            if (targetPlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                return true; // Cannot swap with self
            }
            if (this.currentUsesThisMeeting >= this.optionMaxUsesPerMeeting)
            {
                return true; // Max uses reached
            }
            // Example of further condition: only allow swap if target is alive
            // PlayerControl targetControl = PlayerControl.AllPlayerControls.FirstOrDefault(p => p.PlayerId == targetPlayerId);
            // if (targetControl == null || targetControl.Data.IsDead) return true;

            return false;
        }

        public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
        {
            // Leave empty for now
        }

        public Action CreateAbilityAction(PlayerVoteArea instance)
        {
            byte targetPlayerId = instance.TargetPlayerId;
            return () => {
                ExtremeRolesPlugin.Logger.LogInfo($"VoteSwapper: Castling ability activated on player {targetPlayerId}. Uses this meeting: {this.currentUsesThisMeeting + 1}/{this.optionMaxUsesPerMeeting}");
                this.currentUsesThisMeeting++;

                ExtremeRolesPlugin.Logger.LogInfo($"VoteSwapper: (Placeholder) Actual vote data would be swapped now between LocalPlayer and player {targetPlayerId}.");

                // Trigger Animation
                PlayerControl localPlayer = PlayerControl.LocalPlayer;
                PlayerControl targetPlayerControl = PlayerControl.AllPlayerControls.FirstOrDefault(p => p.PlayerId == targetPlayerId);

                if (localPlayer != null && targetPlayerControl != null)
                {
                    if (this.voteSwapperAnimator != null)
                    {
                        this.voteSwapperAnimator.PlaySwapAnimation(localPlayer, targetPlayerControl);
                    }
                    else
                    {
                        ExtremeRolesPlugin.Logger.LogWarning("VoteSwapper: voteSwapperAnimator is null. Cannot play animation.");
                    }
                }
                else
                {
                    ExtremeRolesPlugin.Logger.LogWarning($"VoteSwapper: LocalPlayer or TargetPlayerControl for animation is null. Local: {localPlayer?.PlayerId}, Target: {targetPlayerControl?.PlayerId}");
                }
            };
        }

        // IRoleResetMeeting Implementations
        public void ResetOnMeetingStart()
        {
            this.currentUsesThisMeeting = 0;
            if (MeetingHud.Instance != null)
            {
                this.voteSwapperAnimator = new VoteSwapperAnimator(MeetingHud.Instance);
            }
            else
            {
                this.voteSwapperAnimator = null;
                ExtremeRolesPlugin.Logger.LogWarning("VoteSwapper: MeetingHud.Instance is null during ResetOnMeetingStart. voteSwapperAnimator could not be initialized.");
            }
        }

        public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
        {
            // Leave empty for now
        }

        protected override AutoParentSetOptionCategoryFactory CreateSpawnOption()
        {
            // This method needs to be implemented if it's abstract in a base class.
            // It should return a factory for the role's main option category.
            // Referring to Guesser or Lover, this is usually set up in the Manager class constructor for the role.
            // For now, assuming this is handled by the base or will be added if compilation fails.
            // GuesserManager or LoverManager likely calls something like:
            // OptionManager.Instance.CreateOptionCategory(OptionTab.CombinationTab, ExtremeRoleManager.GetCombRoleGroupId(this.RoleType), this.GetOptionName());
            // And then this method would retrieve it.
            // For the purpose of this subtask, I'll focus on CreateSpecificOption.
            // If this method is abstract, I'll need to provide a minimal implementation.
            if (OptionManager.Instance.TryGetCategoryFactory(
                this.Tab, ExtremeRoleManager.GetCombRoleGroupId(CombinationRoleType.Supporter), // Using a placeholder type for now
                this.RawRoleName, out var factory))
            {
                return factory;
            }
            // Fallback or error, this indicates an issue with category setup not done prior to this call.
            // This part is complex and depends on how RoleOptionBase expects categories to be pre-registered.
            // For now, let's assume the category is created elsewhere and this method can retrieve it.
            // The key part is that CreateSpecificOption receives this factory.
            // A more robust way would be to ensure the manager class for VoteSwapper sets up the category.
            // For now, to satisfy the method signature if it's abstract:
            return OptionManager.Instance.CreateOptionCategory(this.Tab, ExtremeRoleManager.GetCombRoleGroupId( (CombinationRoleType)this.Id ), this.RawRoleName); // This is a guess
        }

        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
        {
            // 1. Role alignment (Crewmate/Impostor)
            // Use CombinationRoleCommonOption.IsAssignImposter. This option should already exist if the base classes are set up correctly.
            // We don't create it here, but we might use it as a parent if other options depend on it.
            // IOption? impostorAssignOption = factory.Get((int)CombinationRoleCommonOption.IsAssignImposter);
            // If IsAssignImposter is not found, it means it's not added to this role's category by default.
            // Roles like Guesser add it explicitly if they need it:
            // factory.CreateBoolOption((int)CombinationRoleCommonOption.IsAssignImposter, false);
            // Let's assume for VoteSwapper, we want to add it if not present, or get it if it is.
            // For simplicity, let's assume the task means to *configure* it if it were applicable,
            // or that it's an implicitly available setting for combination roles.
            // The subtask asks to *use* existing types, not necessarily re-declare them in this method.
            // The factory is for *this role's specific category*. Common options are often inherited or globally available.
            // However, Guesser's example shows it calling factory.Get() on a common option *ID*, implying it's part of the same factory chain.

            // Let's try to add it to ensure it's part of VoteSwapper's settings page.
            // This makes it a setting for VoteSwapper rather than a global property of combination roles.
            IOption impostorAssignSetting = factory.CreateBoolOption(
                (int)CombinationRoleCommonOption.IsAssignImposter, // Key for "Can be Impostor"
                defaultValue: false, // Default to not being an Impostor-aligned role
                parent: null,
                ignorePrefix: true); // ignorePrefix is often true for these common options


            // 2. Random vote shuffling (boolean)
            factory.CreateBoolOption(
                VoteSwapperOption.EnableRandomShuffling,
                defaultValue: true, // Default to true
                parent: null); // No parent, general option for the role

            // 3. Revealing swap target (boolean)
            factory.CreateBoolOption(
                VoteSwapperOption.RevealSwapTarget,
                defaultValue: false, // Default to false (don't reveal)
                parent: null);

            // 4. Multiple uses per meeting (number)
            factory.CreateIntOption(
                VoteSwapperOption.MaxUsesPerMeeting,
                defaultValue: 1, // Default to 1 use
                minValue: 1,
                maxValue: 5, // Arbitrary max, adjust as needed
                step: 1,
                parent: null,
                format: OptionUnit.Shot); // "Shot" is often used for "uses"

            // 5. Emergency button usage:
            // As discussed, this is ambiguous. If it means "can the ability be used in meetings called by emergency button",
            // this is generally covered by the MeetingOnlyActivator.
            // If it's a specific count or toggle for *only* emergency meetings, it would need a dedicated option.
            // For now, I'm assuming no separate option is needed here beyond the ability being meeting-only.
            // If an option like "CanCallMeeting" (like Guesser) was required, it'd be:
            // factory.CreateBoolOption(VoteSwapperOption.CanCallMeeting, false);

            // Add option for CanPressEmergencyButton
            factory.CreateBoolOption(
                VoteSwapperOption.CanPressEmergencyButton,
                defaultValue: true, // Default to true
                parent: null);
        }

        protected override void CommonInit()
        {
            // Initialize options from loader if needed
            // Example: this.canEnableRandomShuffling = this.Loader.GetValue<VoteSwapperOption, bool>(VoteSwapperOption.EnableRandomShuffling);
        }

        protected override void RoleSpecificInit()
        {
            // Initialize role-specific properties based on options
            // This is where you'd read the values set by CreateSpecificOption
            var loader = this.Loader;
            var impostorAssignOptionKey = (CombinationRoleCommonOption)CombinationRoleCommonOption.IsAssignImposter;
            bool canBeImpostor = loader.GetValue<CombinationRoleCommonOption, bool>(impostorAssignOptionKey);

            if (canBeImpostor)
            {
                this.Team = ExtremeRoleType.Impostor;
            }
            else
            {
                this.Team = ExtremeRoleType.Neutral; // Remains Neutral (or other non-Impostor default) if option is false
            }

            this.optionEnableRandomShuffling = loader.GetValue<VoteSwapperOption, bool>(VoteSwapperOption.EnableRandomShuffling);
            this.optionRevealSwapTarget = loader.GetValue<VoteSwapperOption, bool>(VoteSwapperOption.RevealSwapTarget);
            this.optionMaxUsesPerMeeting = loader.GetValue<VoteSwapperOption, int>(VoteSwapperOption.MaxUsesPerMeeting);
            this.CanCallMeeting = loader.GetValue<VoteSwapperOption, bool>(VoteSwapperOption.CanPressEmergencyButton);
            this.currentUsesThisMeeting = 0; // Initialize here as well, though ResetOnMeetingStart covers it.
        }
    }
} // This was the end of the inner (duplicate) namespace
