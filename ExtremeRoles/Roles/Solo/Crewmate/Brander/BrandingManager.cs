using System.Collections.Generic;
using UnityEngine; // For Vector2, Time.deltaTime (though deltaTime will be passed in)
using ExtremeRoles.Performance; // For PlayerCache if used directly, or pass PlayerControl
using ExtremeRoles.Helper; // For PlayerControl extensions like GetTruePosition

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class BrandingManager
    {
        // Fields moved from Brander.cs
        public Dictionary<byte, float> proximityTimers;
        public HashSet<byte> brandedPlayers;
        public Dictionary<byte, float> brandEffectTimers;
        public Dictionary<byte, float> rebrandCooldownTimers;

        public BrandingManager()
        {
            proximityTimers = new Dictionary<byte, float>();
            brandedPlayers = new HashSet<byte>();
            brandEffectTimers = new Dictionary<byte, float>();
            rebrandCooldownTimers = new Dictionary<byte, float>();
        }

        public void Initialize()
        {
            proximityTimers.Clear();
            brandedPlayers.Clear();
            brandEffectTimers.Clear();
            rebrandCooldownTimers.Clear();
        }

        public void UpdateTimers(float deltaTime)
        {
            List<byte> keysToRemove = new List<byte>();

            // Update brand effect timers
            foreach (byte playerId in new List<byte>(brandEffectTimers.Keys)) // Iterate over a copy
            {
                brandEffectTimers[playerId] -= deltaTime;
                if (brandEffectTimers[playerId] <= 0)
                {
                    keysToRemove.Add(playerId);
                    // TODO: Logic when brand effect wears off (e.g., notify Brander, specific state change)
                }
            }
            foreach (byte key in keysToRemove)
            {
                brandEffectTimers.Remove(key);
                brandedPlayers.Remove(key); // Assuming brand is removed when effect timer ends
            }
            keysToRemove.Clear();

            // Update rebrand cooldown timers
            foreach (byte playerId in new List<byte>(rebrandCooldownTimers.Keys)) // Iterate over a copy
            {
                rebrandCooldownTimers[playerId] -= deltaTime;
                if (rebrandCooldownTimers[playerId] <= 0)
                {
                    keysToRemove.Add(playerId);
                }
            }
            foreach (byte key in keysToRemove)
            {
                rebrandCooldownTimers.Remove(key);
            }
        }

        public bool IsBranded(byte playerId)
        {
            return brandedPlayers.Contains(playerId);
        }

        public bool IsOnRebrandCooldown(byte playerId)
        {
            return rebrandCooldownTimers.ContainsKey(playerId) && rebrandCooldownTimers[playerId] > 0;
        }

        public void ClearProximityTimer(byte playerId)
        {
            if (proximityTimers.ContainsKey(playerId))
            {
                proximityTimers.Remove(playerId);
            }
        }

        public void HandleExiledPlayer(byte playerId)
        {
            if (brandedPlayers.Contains(playerId))
            {
                brandedPlayers.Remove(playerId);
                brandEffectTimers.Remove(playerId);
                // Decide if rebrandCooldownTimers should also be cleared or let run its course
                // rebrandCooldownTimers.Remove(playerId);
                // TODO: Add any notification logic if a branded player is exiled
            }
        }

        // Core logic to be called for each potential target player in Brander.Update()
        // Returns true if a brand was freshly applied this tick for notification purposes.
        public bool ProcessPlayerProximity(
            PlayerControl brander,
            PlayerControl targetPlayer,
            float proximityRange,
            float timeToBrand,
            float brandEffectDuration, // Simplified: using one duration for now
            float rebrandCooldown,
            int maxBrandedPlayers,
            float deltaTime) // Pass deltaTime here
        {
            // Basic checks for targetPlayer should be done in Brander.cs before calling this
            // e.g., targetPlayer != null, not self, not dead, not disconnected.

            if (IsOnRebrandCooldown(targetPlayer.PlayerId))
            {
                return false;
            }

            if (brandedPlayers.Count >= maxBrandedPlayers && !IsBranded(targetPlayer.PlayerId))
            {
                return false;
            }

            Vector2 branderPosition = brander.GetTruePosition();
            Vector2 targetPosition = targetPlayer.GetTruePosition();
            float distance = Vector2.Distance(branderPosition, targetPosition);

            if (distance <= proximityRange)
            {
                if (!proximityTimers.ContainsKey(targetPlayer.PlayerId))
                {
                    proximityTimers[targetPlayer.PlayerId] = 0f;
                }
                proximityTimers[targetPlayer.PlayerId] += deltaTime;

                if (proximityTimers[targetPlayer.PlayerId] >= timeToBrand && !IsBranded(targetPlayer.PlayerId))
                {
                    brandedPlayers.Add(targetPlayer.PlayerId);
                    proximityTimers.Remove(targetPlayer.PlayerId);

                    brandEffectTimers[targetPlayer.PlayerId] = brandEffectDuration;
                    rebrandCooldownTimers[targetPlayer.PlayerId] = rebrandCooldown;

                    // TODO: Determine if target is Impostor for specific effects (logic to be added in Brander or passed via callback)
                    // TODO: Actual notification logic will be in Brander.cs, this method just signals.
                    return true; // Brand was applied
                }
            }
            else
            {
                ClearProximityTimer(targetPlayer.PlayerId);
            }
            return false; // No brand applied this tick
        }
    }
}
