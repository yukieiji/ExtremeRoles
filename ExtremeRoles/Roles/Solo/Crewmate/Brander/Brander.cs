using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Attributes;
using ExtremeRoles.Module.CustomOption.Enums;
using ExtremeRoles.Module.CustomOption.Objects;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using System.Collections.Generic;
using UnityEngine;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.ExtremeShipStatus; // For checking roles if needed for brand effect
using ExtremeRoles.GameMode; // For game state checks

namespace ExtremeRoles.Roles.Solo.Crewmate
{

    public sealed class Brander : SingleRoleBase, IRoleUpdate, IRoleSpecialSetUp, IRoleResetMeeting
    {
        private readonly BrandingManager brandingManager;

        private enum BranderOption
        {
            ProximityRange,
            TimeToBrand,
            ImpostorBrandEffectDuration,
            RebrandCooldown,
            MaxBrandedPlayers,
            NotifyOnBrandEffect
        }

        private float proximityRange; // Loaded from options
        private float timeToBrand; // Loaded from options
        private float impostorBrandEffectDuration; // Loaded from options
        private float rebrandCooldown; // Loaded from options
        private bool notifyOnBrandEffect; // Loaded from options
        private int maxBrandedPlayers; // Loaded from options

        public Brander() : base(
            ExtremeRoleId.Brander, // Changed here
            ExtremeRoleType.Crewmate,
            "Brander", // Placeholder name, will be updated
            Palette.CrewmateBlue, // Placeholder color, will be updated
            true, false, false, false) // Adjust flags as per role nature (e.g. canVent, canSabotage etc.)
        {
            this.brandingManager = new BrandingManager();
        }

        // IRoleUpdate implementation
        public void Update(PlayerControl rolePlayer)
        {
            if (rolePlayer == null || rolePlayer.Data == null || rolePlayer.Data.IsDead || rolePlayer.Data.Disconnected)
            {
                return;
            }

            if (MeetingHud.Instance != null || ShipStatus.Instance == null || GameData.Instance == null || !ShipStatus.Instance.enabled)
            {
                return;
            }

            // Update all active brand effect timers and rebrand cooldowns
            this.brandingManager.UpdateTimers(Time.deltaTime);

            // Iterate through all players to check proximity for new brands
            foreach (var pc in PlayerCache.AllPlayers) // Or GameData.Instance.AllPlayers.GetFastEnumerator()
            {
                if (pc == null || pc.Data == null || pc.PlayerId == rolePlayer.PlayerId || pc.Data.IsDead || pc.Data.Disconnected)
                {
                    continue;
                }

                // Pass necessary parameters from Brander (options, etc.) to the manager method
                bool brandApplied = this.brandingManager.ProcessPlayerProximity(
                    rolePlayer,
                    pc,
                    this.proximityRange,
                    this.timeToBrand,
                    this.impostorBrandEffectDuration, // This is the general duration from options
                    this.rebrandCooldown,
                    this.maxBrandedPlayers,
                    Time.deltaTime
                );

                if (brandApplied && this.notifyOnBrandEffect)
                {
                    // TODO: Notify Brander (e.g., text message, sound about pc.Data.PlayerName)
                    // Example: rolePlayer.RpcShowSystemMessage("Player " + pc.Data.PlayerName + " has been branded.");
                }
            }
        }

        // IRoleSpecialSetUp implementation
        public void IntroBeginSetUp()
        {
            this.brandingManager.Initialize();

            // Note: Option values (proximityRange, timeToBrand, etc.) are loaded in RoleSpecificInit,
            // which is called before IntroBeginSetUp. So, no need to reload them here.
        }

        public void IntroEndSetUp()
        {
            // No specific actions required for Brander at the end of the intro sequence for now.
            // This method is available for future enhancements if needed.
        }

        // IRoleResetMeeting implementation
        public void ResetOnMeetingStart()
        {
            this.brandingManager.Initialize(); // Clears proximity timers among other things.

            // TODO: Display information to the Brander about significant events related to branded players
            // that occurred during the previous round (e.g., branded Impostor kill blocked, branded player killed/ejected).
            // This will require further logic once brand effects are fully defined.
            // Example:
            // foreach (byte playerId in brandingManager.brandedPlayers) // Access via manager
            // {
            //     PlayerControl pc = Player.GetPlayerControlById(playerId);
            //     if (pc != null && pc.Data.IsDead)
            //     {
            //         // Notify Brander that a branded player died.
            //     }
            // }

            // Note: Active brand effect timers (`brandEffectTimers`) and `rebrandCooldownTimers`
            // will continue to be managed by the Update method (via brandingManager.UpdateTimers).
            // The Update method's conditions prevent it from running during meetings, effectively pausing timers.
        }

        public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
        {
            // Called when a meeting ends. exiledPlayer is null if skip, or contains info of the exiled player.

            if (exiledPlayer != null)
            {
                this.brandingManager.HandleExiledPlayer(exiledPlayer.PlayerId);
            }

            // No specific global cooldowns for the Brander role itself need resetting here,
            // as re-brand cooldowns are player-specific and managed by BrandingManager.
            // Proximity timers are already cleared in ResetOnMeetingStart.
            // Brand effect timers resume ticking once the meeting ends and Update runs again.
        }

        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
        {
            // Proximity Range (float, meters)
            factory.CreateFloatOption(
                BranderOption.ProximityRange, // Enum key
                2.0f,                         // Default value
                0.5f,                         // Min value
                5.0f,                         // Max value
                0.1f,                         // Increment
                OptionUnit.Meter             // Unit
            );

            // Time to Brand (float, seconds)
            factory.CreateFloatOption(
                BranderOption.TimeToBrand,
                3.0f,  // Default
                1.0f,  // Min
                10.0f, // Max
                0.5f,  // Increment
                OptionUnit.Second
            );

            // Impostor Brand Effect Duration (float, seconds)
            factory.CreateFloatOption(
                BranderOption.ImpostorBrandEffectDuration,
                15.0f, // Default
                5.0f,  // Min
                30.0f, // Max
                1.0f,  // Increment
                OptionUnit.Second
            );

            // Re-brand Cooldown (float, seconds)
            factory.CreateFloatOption(
                BranderOption.RebrandCooldown,
                60.0f, // Default
                15.0f, // Min
                120.0f,// Max
                5.0f,  // Increment
                OptionUnit.Second
            );

            // Max Active Brands (int)
            factory.CreateIntOption(
                BranderOption.MaxBrandedPlayers,
                1,     // Default
                1,     // Min
                5,     // Max
                1      // Increment
            );

            // Notify on Brand Effect (bool)
            factory.CreateBoolOption(
                BranderOption.NotifyOnBrandEffect,
                true // Default
            );
        }

        protected override void RoleSpecificInit()
        {
            // this.Loader is an instance of RoleOptionLoader, initialized by the base class.
            // Use it to get the configured values for the options defined with BranderOption enum.

            this.proximityRange = this.Loader.GetValue<BranderOption, float>(BranderOption.ProximityRange);
            this.timeToBrand = this.Loader.GetValue<BranderOption, float>(BranderOption.TimeToBrand);
            this.impostorBrandEffectDuration = this.Loader.GetValue<BranderOption, float>(BranderOption.ImpostorBrandEffectDuration);
            this.rebrandCooldown = this.Loader.GetValue<BranderOption, float>(BranderOption.RebrandCooldown);

            // For integer options, specify int as the TValueType.
            this.maxBrandedPlayers = this.Loader.GetValue<BranderOption, int>(BranderOption.MaxBrandedPlayers);

            // For boolean options, specify bool as the TValueType.
            this.notifyOnBrandEffect = this.Loader.GetValue<BranderOption, bool>(BranderOption.NotifyOnBrandEffect);

            // The fallback logic (if an option wasn't found) is handled internally by RoleOptionLoader
            // or would have been set by default values in CreateSpecificOption.
            // No need for explicit null checks and fallbacks here as in the previous attempt,
            // as this.Loader.GetValue should return the default value if not configured.
        }
    }
}
