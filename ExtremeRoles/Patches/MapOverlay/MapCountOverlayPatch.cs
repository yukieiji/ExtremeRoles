using System.Collections.Generic;

using UnityEngine;

using HarmonyLib;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;


namespace ExtremeRoles.Patches.MapOverlay
{
    // Dont Copy Other MODS!!
    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Awake))]
    public static class MapCountOverlayAwakePatch
    {
        public static void Postfix(MapCountOverlay __instance)
        {
            var filter = __instance.filter;
            filter.useTriggers = true;
            __instance.filter = filter;
        }
    }

    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
    public static class MapCountOverlayUpdatePatch
    {
        public static Dictionary<SystemTypes, List<Color>> PlayerColor = new Dictionary<SystemTypes, List<Color>>();

        private static float adminTimer = 0.0f;
        private static bool enableAdminLimit = false;
        private static bool isRemoveAdmin = false;
        private static TMPro.TextMeshPro timerText;

        private static readonly HashSet<ExtremeRoleId> adminUseRole = new HashSet<ExtremeRoleId>()
        {
            ExtremeRoleId.Supervisor,
            ExtremeRoleId.Traitor,
            ExtremeRoleId.Doll
        };

        public static bool Prefix(MapCountOverlay __instance)
        {
            bool isHudOverrideTaskActive = PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(
                CachedPlayerControl.LocalPlayer);
            if (ExtremeRoleManager.GameRole.Count == 0)
            {
                fixedDefaultCountOverlayUpdate(__instance, isHudOverrideTaskActive);
                return false;
            }

            var admin = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<
                Roles.Solo.Crewmate.Supervisor>();

			PlayerColor.Clear();

			if (admin == null || !admin.Boosted || !admin.IsAbilityActive)
            {
                fixedDefaultCountOverlayUpdate(__instance, isHudOverrideTaskActive);
                return false;
            }

			__instance.timer += Time.deltaTime;
			if (__instance.timer < 0.1f)
			{
				return false;
			}
			__instance.timer = 0f;

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

			for (int i = 0; i < __instance.CountAreas.Length; i++)
			{
				CounterArea counterArea = __instance.CountAreas[i];
				List<Color> roomPlayerColor = new List<Color>();
				PlayerColor.Add(counterArea.RoomType, roomPlayerColor);

				if (!isHudOverrideTaskActive)
				{
					if (CachedShipStatus.FastRoom.TryGetValue(
                        counterArea.RoomType, out PlainShipRoom plainShipRoom)
                        && plainShipRoom.roomArea)
					{
						int hitNum = plainShipRoom.roomArea.OverlapCollider(
                            __instance.filter, __instance.buffer);
						int showNum = hitNum;

						Color addColor = Palette.EnabledColor;

						for (int j = 0; j < hitNum; j++)
						{
							Collider2D collider2D = __instance.buffer[j];
							if (!collider2D.CompareTag("DeadBody") || !__instance.includeDeadBodies)
							{
								PlayerControl component = collider2D.GetComponent<PlayerControl>();
								if (!component || 
                                    component.Data == null || 
                                    component.Data.Disconnected || 
                                    component.Data.IsDead || 
                                    (!__instance.showLivePlayerPosition && component.AmOwner) ||
                                    (!collider2D.isTrigger && !component.AmOwner))
								{
                                    showNum--;
								}
								else if (component?.cosmetics.currentBodySprite.BodySprite?.material != null)
                                {
									addColor = Palette.PlayerColors[component.Data.DefaultOutfit.ColorId];
								}
							}
							else
							{
								DeadBody component = collider2D.GetComponent<DeadBody>();
								if (component)
								{
									GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(
                                        component.ParentId);
									if (playerInfo != null)
									{
										addColor = Palette.PlayerColors[
                                            playerInfo.Object.CurrentOutfit.ColorId];
									}
								}
							}

							roomPlayerColor.Add(addColor);

						}
						counterArea.UpdateCount(showNum);
					}
					else
					{
						Logging.Debug($"Couldn't find counter for: {counterArea.RoomType}");
					}
				}
				else
				{
					counterArea.UpdateCount(0);
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
                disableVital();
                MapBehaviour.Instance.Close();
            }
        }

        public static void Initialize()
        {
            Object.Destroy(timerText);

            var adminOpt = ExtremeGameManager.Instance.ShipOption.Admin;
            if (adminOpt == null) { return; }

            adminTimer = adminOpt.AdminLimitTime;
            isRemoveAdmin = adminOpt.DisableAdmin;
            enableAdminLimit = adminOpt.EnableAdminLimit;

            Logging.Debug("---- AdminCondition ----");
            Logging.Debug($"IsRemoveAdmin:{isRemoveAdmin}");
            Logging.Debug($"EnableAdminLimit:{enableAdminLimit}");
            Logging.Debug($"AdminTime:{adminTimer}");
        }

        public static bool IsAbilityUse()
        {
            SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
            MultiAssignRoleBase multiAssignRole = role as MultiAssignRoleBase;

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

        // Dont Copy Other MODS!!
        private static void fixedDefaultCountOverlayUpdate(
            MapCountOverlay instance, bool isHudOverrideTaskActive)
        {
            instance.timer += Time.deltaTime;
            if (instance.timer < 0.1f)
            {
                return;
            }

            instance.timer = 0f;

            if (!instance.isSab && isHudOverrideTaskActive)
            {
                instance.isSab = true;
                instance.BackgroundColor.SetColor(Palette.DisabledGrey);
                instance.SabotageText.gameObject.SetActive(true);
                return;
            }
            if (instance.isSab && !isHudOverrideTaskActive)
            {
                instance.isSab = false;
                instance.BackgroundColor.SetColor(Color.green);
                instance.SabotageText.gameObject.SetActive(false);
            }
            for (int i = 0; i < instance.CountAreas.Length; i++)
            {
                CounterArea counterArea = instance.CountAreas[i];
                if (!isHudOverrideTaskActive)
                {
                    if (CachedShipStatus.FastRoom.TryGetValue(
                            counterArea.RoomType, out PlainShipRoom plainShipRoom) &&
                        plainShipRoom.roomArea)
                    {
                        int hitNum = plainShipRoom.roomArea.OverlapCollider(
                            instance.filter, instance.buffer);
                        int showCount = hitNum;
                        for (int j = 0; j < hitNum; j++)
                        {
                            Collider2D collider2D = instance.buffer[j];

                            if (!collider2D.CompareTag("DeadBody") || !instance.includeDeadBodies)
                            {
                                PlayerControl component = collider2D.GetComponent<PlayerControl>();
                                if (!component ||
                                    component.Data == null ||
                                    component.Data.Disconnected ||
                                    component.Data.IsDead ||
                                    (!instance.showLivePlayerPosition && component.AmOwner) ||
                                    (!collider2D.isTrigger && !component.AmOwner))
                                {
                                    showCount--;
                                }
                            }
                        }
                        counterArea.UpdateCount(showCount);
                    }
                    else
                    {
                        Debug.LogWarning($"Couldn't find counter for:{counterArea.RoomType}");
                    }
                }
                else
                {
                    counterArea.UpdateCount(0);
                }
            }
        }

        private static void disableVital()
        {
            HashSet<string> vitalObj = new HashSet<string>();
            if (ExtremeRolesPlugin.Compat.IsModMap)
            {
                vitalObj = ExtremeRolesPlugin.Compat.ModMap.GetSystemObjectName(
                    Compat.Interface.SystemConsoleType.Admin);
            }
            else
            {
                switch (GameOptionsManager.Instance.CurrentGameOptions.GetByte(
                    ByteOptionNames.MapId))
                {
                    case 0:
                        vitalObj.Add(GameSystem.SkeldAdmin);
                        break;
                    case 1:
                        vitalObj.Add(GameSystem.MiraHqAdmin);
                        break;
                    case 2:
                        vitalObj.Add(GameSystem.PolusAdmin1);
                        vitalObj.Add(GameSystem.PolusAdmin2);
                        break;
                    case 4:
                        vitalObj.Add(GameSystem.AirShipArchiveAdmin);
                        vitalObj.Add(GameSystem.AirShipCockpitAdmin);
                        break;
                    default:
                        break;
                }
            }

            foreach (string objectName in vitalObj)
            {
                GameSystem.DisableMapModule(objectName);
            }
        }
    }
}
