using System.Collections.Generic;

using UnityEngine;

using HarmonyLib;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.GameMode;
using ExtremeRoles.Compat;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;

#nullable enable

namespace ExtremeRoles.Patches.MapOverlay;

[HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
public static class MapCountOverlayUpdatePatch
{
    public static Dictionary<SystemTypes, List<int>> PlayerColor =
        new Dictionary<SystemTypes, List<int>>();

    private static float adminTimer = 0.0f;
    private static bool enableAdminLimit = false;
    private static bool isRemoveAdmin = false;
    private static TMPro.TextMeshPro? timerText;

    private static readonly HashSet<ExtremeRoleId> adminUseRole = new HashSet<ExtremeRoleId>()
    {
        ExtremeRoleId.Supervisor,
        ExtremeRoleId.Traitor,
        ExtremeRoleId.Doll
    };

    public static bool Prefix(MapCountOverlay __instance)
    {
        if (ExtremeRoleManager.GameRole.Count == 0)
        {
            return true;
        }

        var supervisor = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<
            Roles.Solo.Crewmate.Supervisor>();

		bool isSupervisorEnhance = supervisor is not null && supervisor.Boosted && supervisor.IsAbilityActive;

        __instance.timer += Time.deltaTime;
		if (__instance.timer < 0.1f)
		{
			return false;
		}
		__instance.timer = 0f;

		PlayerColor.Clear();

		bool isHudOverrideTaskActive = PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(
            CachedPlayerControl.LocalPlayer);

        if (!__instance.isSab && isHudOverrideTaskActive)
		{
			__instance.isSab = true;
			__instance.BackgroundColor.SetColor(Palette.DisabledGrey);
			__instance.SabotageText.gameObject.SetActive(true);
			return false;
		}

		if (__instance.isSab && !isHudOverrideTaskActive)
		{
			__instance.isSab = false;
			__instance.BackgroundColor.SetColor(Color.green);
			__instance.SabotageText.gameObject.SetActive(false);
		}

		bool inFaker = ExtremeSystemTypeManager.Instance.TryGet<FakerDummySystem>(ExtremeSystemType.FakerDummy, out var system);

		for (int i = 0; i < __instance.CountAreas.Length; i++)
		{
			CounterArea counterArea = __instance.CountAreas[i];

			if (isHudOverrideTaskActive)
            {
                counterArea.UpdateCount(0);
                continue;
            }

            if (CachedShipStatus.FastRoom.TryGetValue(
                    counterArea.RoomType,
                    out PlainShipRoom? plainShipRoom) &&
				plainShipRoom != null &&
                plainShipRoom.roomArea != null)
            {
                int hitNum = plainShipRoom.roomArea.OverlapCollider(
                    __instance.filter, __instance.buffer);
                int showNum = 0;

				HashSet<byte> alreadyShowPlayerIds = new HashSet<byte>(hitNum);
				List<int> addColor = new List<int>(hitNum);

                for (int j = 0; j < hitNum; j++)
                {
                    Collider2D collider2D = __instance.buffer[j];
                    if (collider2D.CompareTag("DeadBody") && __instance.includeDeadBodies)
                    {
                        DeadBody component = collider2D.GetComponent<DeadBody>();
                        if (component != null &&
							alreadyShowPlayerIds.Add(component.ParentId))
                        {
							showNum++;

							GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(
                                component.ParentId);
                            if (playerInfo != null)
                            {
								addColor.Add(playerInfo.DefaultOutfit.ColorId);
                            }
                        }
                    }
                    else if (!collider2D.isTrigger)
                    {
                        PlayerControl component = collider2D.GetComponent<PlayerControl>();

                        if (component &&
                            component.Data != null &&
                            !component.Data.Disconnected &&
                            !component.Data.IsDead &&
                            (__instance.showLivePlayerPosition || !component.AmOwner) &&
                            alreadyShowPlayerIds.Add(component.PlayerId))
                        {
                            showNum++;
							addColor.Add(component.Data.DefaultOutfit.ColorId);
                        }
                    }
                }
				if (inFaker &&
					system!.TryGetDummyColors(counterArea.RoomType, out var dummyColor) &&
					dummyColor.Count != 0)
				{
					addColor.AddRange(dummyColor);
					showNum += dummyColor.Count;
				}

				if (isSupervisorEnhance)
				{
					PlayerColor.Add(counterArea.RoomType, addColor);
				}
				counterArea.UpdateCount(showNum);
            }
            else
            {
                Logging.Debug($"Couldn't find counter for: {counterArea.RoomType}");
            }
        }
		return false;
	}

    public static void Postfix(MapCountOverlay __instance)
    {

        if (ExtremeRoleManager.GameRole.Count == 0) { return; }

        if (isRemoveAdmin || // アドミン無効化してる
            !enableAdminLimit) //アドミン制限あるか
        {
            return;
        }

        if (IsAbilityUse())
        {
            return;
        }

        if (timerText == null)
        {
            timerText = Object.Instantiate(
                FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
                __instance.transform);
            timerText.transform.localPosition = new Vector3(3.4f, 2.7f, -9.0f);
            timerText.name = "vitalTimer";
        }

        if (adminTimer > 0.0f)
        {
            adminTimer -= Time.deltaTime;
        }

        timerText.text = $"{Mathf.CeilToInt(adminTimer)}";
        timerText.gameObject.SetActive(true);

        if (adminTimer <= 0.0f)
        {
			GameSystem.DisableAdmin();
            MapBehaviour.Instance.Close();
        }
    }

    public static void Initialize()
    {
        Object.Destroy(timerText);
    }
    public static void LoadOptionValue()
    {
        var adminOpt = ExtremeGameModeManager.Instance.ShipOption.Admin;
        if (adminOpt == null) { return; }

        adminTimer = adminOpt.AdminLimitTime;
        isRemoveAdmin = adminOpt.Disable;
        enableAdminLimit = adminOpt.EnableAdminLimit;

        Logging.Debug("---- AdminCondition ----");
        Logging.Debug($"IsRemoveAdmin:{isRemoveAdmin}");
        Logging.Debug($"EnableAdminLimit:{enableAdminLimit}");
        Logging.Debug($"AdminTime:{adminTimer}");
    }

    public static bool IsAbilityUse()
    {
        SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
        MultiAssignRoleBase? multiAssignRole = role as MultiAssignRoleBase;

        if (adminUseRole.Contains(role.Id))
        {
            if (((IRoleAbility)role).Button.IsAbilityActive())
            {
                return true;
            }
        }
        if (multiAssignRole?.AnotherRole != null)
        {
            if (adminUseRole.Contains(
                multiAssignRole.AnotherRole.Id))
            {
                if (((IRoleAbility)multiAssignRole.AnotherRole).Button.IsAbilityActive())
                {
                    return true;
                }
            }
        }
        return false;
    }
}
