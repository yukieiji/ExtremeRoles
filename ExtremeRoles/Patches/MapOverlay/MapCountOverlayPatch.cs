using System.Collections.Generic;

using UnityEngine;

using HarmonyLib;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;


namespace ExtremeRoles.Patches.MapOverlay
{
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
            if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

            var admin = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<
                Roles.Solo.Crewmate.Supervisor>();

			PlayerColor.Clear();

			if (admin == null || !admin.Boosted || !admin.IsAbilityActive) { return true; }

			__instance.timer += Time.deltaTime;
			if (__instance.timer < 0.1f)
			{
				return false;
			}
			__instance.timer = 0f;

			bool commsActive = false;

			foreach (PlayerTask task in 
				CachedPlayerControl.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
            {
				if (task.TaskType == TaskTypes.FixComms)
				{
					commsActive = true;
				}
			}

			if (!__instance.isSab && commsActive)
			{
				__instance.isSab = true;
				__instance.BackgroundColor.SetColor(Palette.DisabledGrey);
				__instance.SabotageText.gameObject.SetActive(true);
				return false;
			}

			if (__instance.isSab && !commsActive)
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

				if (!commsActive)
				{

					PlainShipRoom plainShipRoom = CachedShipStatus.Instance.FastRooms[counterArea.RoomType];
					if (plainShipRoom != null && plainShipRoom.roomArea)
					{
						int num = plainShipRoom.roomArea.OverlapCollider(__instance.filter, __instance.buffer);
						int num2 = num;

						Color addColor = Palette.EnabledColor;

						for (int j = 0; j < num; j++)
						{
							Collider2D collider2D = __instance.buffer[j];
							if (!(collider2D.tag == "DeadBody"))
							{
								PlayerControl component = collider2D.GetComponent<PlayerControl>();
								if (!component || component.Data == null || component.Data.Disconnected || component.Data.IsDead)
								{
									num2--;
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
									GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
									if (playerInfo != null)
									{
										addColor = Palette.PlayerColors[playerInfo.Object.CurrentOutfit.ColorId];
									}
								}
							}

							roomPlayerColor.Add(addColor);

						}
						counterArea.UpdateCount(num2);
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
            adminTimer = OptionHolder.Ship.AdminLimitTime;
            isRemoveAdmin = OptionHolder.Ship.IsRemoveAdmin;
            enableAdminLimit = OptionHolder.Ship.EnableAdminLimit;

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
                switch (PlayerControl.GameOptions.MapId)
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
