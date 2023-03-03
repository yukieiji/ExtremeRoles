using System.Runtime.CompilerServices;

using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Roles.Solo.Impostor;


namespace ExtremeRoles.Module
{
    public class VisionComputer
    {
        public enum Modifier
        {
            None,
            LastWolfLightOff,
            WispLightOff,
        }

        public static VisionComputer Instance => instance;
        private static VisionComputer instance = new VisionComputer();

        private static float crewLightVision => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
            FloatOptionNames.CrewLightMod);

        private static float impLightVision => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
            FloatOptionNames.ImpostorLightMod);

        private const SystemTypes electrical = SystemTypes.Electrical;

        public Modifier CurrentModifier => modifier;
        private Modifier modifier = Modifier.None;

        public void SetModifier(Modifier newVision)
        {
            this.modifier = newVision;
        }
        public void ResetModifier()
        {
            this.modifier = Modifier.None;
        }
        public bool IsModifierResetted() => this.modifier == Modifier.None;

        public bool IsVanillaVisionAndGetVision(
            ShipStatus shipStatus, GameData.PlayerInfo playerInfo, out float vision)
        {
            vision = shipStatus.MaxLightRadius;

            switch (this.modifier)
            {
                case Modifier.LastWolfLightOff:
                    if (ExtremeRoleManager.GetSafeCastedLocalPlayerRole<LastWolf>() == null)
                    {
                        vision = LastWolf.LightOffVision;
                        return false;
                    }
                    break;
                case Modifier.WispLightOff:
                    if (!Wisp.HasTorch(playerInfo.PlayerId))
                    {
                        vision = shipStatus.MinLightRadius * crewLightVision;
                        return false;
                    }
                    break;
                default:
                    break;
            }
            bool isRequireCustomVision = requireCustomCustomCalculateLightRadius();

            if (!RoleAssignState.Instance.IsRoleSetUpEnd)
            {
                return checkNormalOrCustomCalculateLightRadius(isRequireCustomVision, playerInfo, ref vision);
            }
            var systems = shipStatus.Systems;
            ISystemType systemType = systems.ContainsKey(electrical) ? systems[electrical] : null;
            if (systemType == null)
            {
                return checkNormalOrCustomCalculateLightRadius(isRequireCustomVision, playerInfo, ref vision);
            }

            SwitchSystem switchSystem = systemType.TryCast<SwitchSystem>();
            if (switchSystem == null)
            {
                return checkNormalOrCustomCalculateLightRadius(isRequireCustomVision, playerInfo, ref vision);
            }

            var allRole = ExtremeRoleManager.GameRole;

            if (allRole.Count == 0)
            {
                if (isRequireCustomVision)
                {
                    vision = ExtremeRolesPlugin.Compat.ModMap.CalculateLightRadius(
                        playerInfo, false, playerInfo.Role.IsImpostor);
                    return false;
                }
                return true;
            }

            SingleRoleBase role = allRole[playerInfo.PlayerId];

            if (isRequireCustomVision)
            {
                float visionMulti;
                bool applayVisionEffects = !role.IsImpostor();

                if (role.TryGetVisionMod(out visionMulti, out bool isApplyEnvironmentVision))
                {
                    applayVisionEffects = isApplyEnvironmentVision;
                }
                else if (playerInfo.Role.IsImpostor)
                {
                    visionMulti = impLightVision;
                }
                else
                {
                    visionMulti = crewLightVision;
                }

                vision = ExtremeRolesPlugin.Compat.ModMap.CalculateLightRadius(
                    playerInfo, visionMulti, applayVisionEffects);

                return false;
            }

            float num = (float)switchSystem.Value / 255f;
            float switchVisionMulti = Mathf.Lerp(
                shipStatus.MinLightRadius,
                shipStatus.MaxLightRadius, num);

            float baseVision = shipStatus.MaxLightRadius;

            if (playerInfo == null || playerInfo.IsDead) // IsDead
            {
                vision = baseVision;
            }
            else if (role.TryGetVisionMod(out float visionMulti, out bool isApplyEnvironmentVision))
            {
                if (isApplyEnvironmentVision)
                {
                    baseVision = switchVisionMulti;
                }
                vision = baseVision * visionMulti;
            }
            else if (playerInfo.Role.IsImpostor)
            {
                vision = baseVision * impLightVision;
            }
            else
            {
                vision = switchVisionMulti * crewLightVision;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool requireCustomCustomCalculateLightRadius() =>
            ExtremeRolesPlugin.Compat.IsModMap &&
            ExtremeRolesPlugin.Compat.ModMap.IsCustomCalculateLightRadius;

        private static bool checkNormalOrCustomCalculateLightRadius(
            bool isRequireCustomVision, GameData.PlayerInfo player, ref float result)
        {
            if (isRequireCustomVision)
            {
                result = ExtremeRolesPlugin.Compat.ModMap.CalculateLightRadius(
                    player, false, player.Role.IsImpostor);
                return false;
            }
            return true;
        }
    }
}
