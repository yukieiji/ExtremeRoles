using System.Collections.Generic;

using UnityEngine;

using HarmonyLib;

using ExtremeRoles.Performance;


namespace ExtremeRoles.Patches.MapOverlay
{
    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
    public static class MapCountOverlayUpdatePatch
    {
        public static Dictionary<SystemTypes, List<Color>> PlayerColor = new Dictionary<SystemTypes, List<Color>>();

        public static bool Prefix(MapCountOverlay __instance)
        {
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return true; }

            var admin = Roles.ExtremeRoleManager.GetSafeCastedLocalPlayerRole<
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

			foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
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
								else if (component?.MyRend?.material != null)
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
						Helper.Logging.Debug($"Couldn't find counter for: {counterArea.RoomType}");
					}
				}
				else
				{
					counterArea.UpdateCount(0);
				}
			}
			return false;
		}
    }
}
