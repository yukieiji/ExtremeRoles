using System.Runtime.CompilerServices;

using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.GhostRoles.Impostor;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.Compat;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType;

namespace ExtremeRoles.Module;

public class VisionComputer
{
	public enum Modifier
	{
		None,
		LastWolfLightOff,
		WispLightOff,
		IgniterLightOff,
	}

	public static VisionComputer Instance => instance;
	private static VisionComputer instance = new VisionComputer();

	public static float CrewmateLightVision => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
		FloatOptionNames.CrewLightMod);

	public static float ImpostorLightVision => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
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
				if (ExtremeSystemTypeManager.Instance.TryGet<WispTorchSystem>(ExtremeSystemType.WispTorch, out var system) &&
					!system!.HasTorch(playerInfo.PlayerId))
				{
					vision = shipStatus.MinLightRadius * CrewmateLightVision;
					return false;
				}
				break;
			case Modifier.IgniterLightOff:
				if (Igniter.TryComputeVison(playerInfo, out float effectVison))
				{
					vision = effectVison;
					return false;
				}
				break;
			default:
				break;
		}
		bool isRequireCustomVision =
			CompatModManager.Instance.TryGetModMap(out var modMap) &&
			modMap!.IsCustomCalculateLightRadius;

		if (!RoleAssignState.Instance.IsRoleSetUpEnd)
		{
			return checkNormalOrCustomCalculateLightRadius(
				modMap, isRequireCustomVision, playerInfo, ref vision);
		}
		var systems = shipStatus.Systems;
		ISystemType electricalSystem = systems.ContainsKey(electrical) ? systems[electrical] : null;
		if (electricalSystem == null)
		{
			return checkNormalOrCustomCalculateLightRadius(
				modMap, isRequireCustomVision, playerInfo, ref vision);
		}

		SwitchSystem switchSystem = electricalSystem.TryCast<SwitchSystem>();
		if (switchSystem == null)
		{
			return checkNormalOrCustomCalculateLightRadius(
				modMap, isRequireCustomVision, playerInfo, ref vision);
		}

		var allRole = ExtremeRoleManager.GameRole;

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
				visionMulti = ImpostorLightVision;
			}
			else
			{
				visionMulti = CrewmateLightVision;
			}

			vision = modMap!.CalculateLightRadius(playerInfo, visionMulti, applayVisionEffects);

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
			vision = baseVision * ImpostorLightVision;
		}
		else
		{
			vision = switchVisionMulti * CrewmateLightVision;
		}
		return false;
	}

	private static bool checkNormalOrCustomCalculateLightRadius(
		IMapMod modMap,
		bool isRequireCustomVision, GameData.PlayerInfo player, ref float result)
	{
		if (!isRequireCustomVision) { return true; }

		result = modMap.CalculateLightRadius(player, false, player.Role.IsImpostor);
		return false;
	}
}
