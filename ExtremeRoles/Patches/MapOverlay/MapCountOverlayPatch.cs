using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using UnityEngine;


#nullable enable

namespace ExtremeRoles.Patches.MapOverlay;

public class MapCountOverlayUpdatePatchBody(IModLogger logger, IGameRuntime runtime)
{
	private readonly IModLogger _logger = logger;
	private readonly IGameRuntime _runtime = runtime;


	public IReadOnlyDictionary<SystemTypes, IReadOnlyList<int>> PlayerColors => _playerColors;
	private readonly Dictionary<SystemTypes, IReadOnlyList<int>> _playerColors = [];


	private float _adminTimer = 0.0f;
	private TMPro.TextMeshPro? _timerText;

	private readonly IReadOnlySet<ExtremeRoleId> adminUseRole = new HashSet<ExtremeRoleId>()
	{
		ExtremeRoleId.Supervisor,
		ExtremeRoleId.Traitor,
		ExtremeRoleId.Doll
	};

	public static MapCountOverlayUpdatePatchBody Instance
	{
		get
		{
			if (_instance is null)
			{
				_instance = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<MapCountOverlayUpdatePatchBody>();
			}
			return _instance;
		}
	}
	private static MapCountOverlayUpdatePatchBody? _instance;

	public bool IsAbilityUse()
		=> IRoleAbility.IsLocalPlayerAbilityUse(adminUseRole);

	public void Initialize()
	{
		if (_timerText != null)
		{
			Object.Destroy(_timerText);
		}
		
		if (!_runtime.TryGetGameContext(out var ctx))
		{
			return;
		}

		var adminOpt = ctx.GlobalOption.Admin;

		_adminTimer = adminOpt.LimitTime;

		_logger.LogTrace("---- AdminCondition ----");
		_logger.LogTrace($"IsRemoveAdmin:{adminOpt.Disable}");
		_logger.LogTrace($"EnableAdminLimit:{adminOpt.EnableLimit}");
		_logger.LogTrace($"AdminTime:{_adminTimer}");
	}

	public bool Prefix(MapCountOverlay __instance)
	{
		if (!_runtime.TryGetGameContext(out var ctx) || 
			ctx.Roles.All.Count == 0) // <= ここは後でRoleAssignStateに置き換える
		{
			return true;
		}
		Override(__instance, ctx);
		return false;
	}

	private void Override(MapCountOverlay __instance, IGameContext ctx)
	{
		var supervisor = ctx.Roles.GetSafeCastedLocalPlayerRole<Roles.Solo.Crewmate.Supervisor>();
		bool isSupervisorEnhance = supervisor is not null && supervisor.Boosted && supervisor.IsAbilityActive;
		__instance.timer += Time.deltaTime;
		if (__instance.timer < 0.1f)
		{
			return;
		}

		__instance.timer = 0f;
		_playerColors.Clear();

		bool isHudOverrideTaskActive = PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(
			PlayerControl.LocalPlayer);
		if (!__instance.isSab && isHudOverrideTaskActive)
		{
			ActivateSabotageMapOvelay(__instance, true, Palette.DisabledGrey);
			return;
		}

		if (__instance.isSab && !isHudOverrideTaskActive)
		{
			ActivateSabotageMapOvelay(__instance, false, Color.green);
		}

		bool containFake = AdminDummySystem.TryGet(out var system);

		foreach (var counterArea in __instance.CountAreas)
		{
			if (counterArea.DetectiveExclusiveLocation)
			{
				continue;
			}

			if (isHudOverrideTaskActive)
			{
				counterArea.UpdateCount(0);
				continue;
			}

			if (containFake &&
				system!.Mode is AdminDummySystem.DummyMode.Override)
			{
				AddFakePlayerCount(__instance, counterArea, system!, isSupervisorEnhance);
				continue;
			}
			OverrideNomalCountOverlay(__instance, counterArea, system, isSupervisorEnhance);
		}
	}

	public void Postfix(MapCountOverlay __instance)
    {

		if (!_runtime.TryGetGameContext(out var ctx) ||
			ctx.Roles.All.Count == 0) // <= ここは後でRoleAssignStateに置き換える
		{
			return;
		}

		var adminOpt = ctx.GlobalOption.Admin;

		if (adminOpt.Disable || // アドミン無効化してる
            !adminOpt.EnableLimit || //アドミン制限あるか
			IsAbilityUse())
        {
            return;
        }

        if (_timerText == null)
        {
            _timerText = Object.Instantiate(
                HudManager.Instance.KillButton.cooldownTimerText,
                __instance.transform);
            _timerText.transform.localPosition = new Vector3(3.4f, 2.7f, -9.0f);
            _timerText.name = "vitalTimer";
        }

        if (_adminTimer > 0.0f)
        {
            _adminTimer -= Time.deltaTime;
        }

        _timerText.text = $"{Mathf.CeilToInt(_adminTimer)}";
        _timerText.gameObject.SetActive(true);

        if (_adminTimer <= 0.0f)
        {
			Map.DisableAdmin();
            MapBehaviour.Instance.Close();
        }
    }

	private void OverrideNomalCountOverlay(
		MapCountOverlay __instance, CounterArea counterArea, AdminDummySystem? system, bool isSupervisorEnhance)
	{
		if (!(ShipStatusCache.KeyedRoom.TryGetValue(
					counterArea.RoomType,
					out PlainShipRoom? plainShipRoom) &&
				plainShipRoom != null &&
				plainShipRoom.roomArea != null
			))
		{
			_logger.LogTrace($"Couldn't find counter for: {counterArea.RoomType}");
			return;
		}

		int hitNum = plainShipRoom.roomArea.OverlapCollider(__instance.filter, __instance.buffer);
		int showNum = 0;

		HashSet<byte> alreadyShowPlayerIds = new HashSet<byte>(hitNum);
		List<int> addColor = new List<int>(hitNum);

		for (int j = 0; j < hitNum; j++)
		{
			Collider2D collider2D = __instance.buffer[j];
			if (collider2D.CompareTag("DeadBody") && __instance.includeDeadBodies)
			{
				if (collider2D.TryGetComponent<DeadBody>(out var deadBody) &&
					alreadyShowPlayerIds.Add(deadBody.ParentId))
				{
					showNum++;

					NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(deadBody.ParentId);
					if (playerInfo != null)
					{
						addColor.Add(playerInfo.DefaultOutfit.ColorId);
					}
				}
			}
			else if (!collider2D.isTrigger)
			{
				if (collider2D.TryGetComponent<PlayerControl>(out var playerControl) &&
					playerControl.Data != null &&
					!playerControl.Data.Disconnected &&
					!playerControl.Data.IsDead &&
					(__instance.showLivePlayerPosition || !playerControl.AmOwner) &&
					alreadyShowPlayerIds.Add(playerControl.PlayerId))
				{
					showNum++;
					addColor.Add(playerControl.Data.DefaultOutfit.ColorId);
				}
			}
		}
		if (system is not null &&
			system.TryGet(counterArea.RoomType, out var dummyColor) &&
			dummyColor.Count != 0)
		{
			addColor.AddRange(dummyColor);
			showNum += dummyColor.Count;
		}

		if (isSupervisorEnhance)
		{
			_playerColors.Add(counterArea.RoomType, addColor);
		}
		counterArea.UpdateCount(showNum);
	}

	private void AddFakePlayerCount(MapCountOverlay __instance, CounterArea counterArea, AdminDummySystem system, bool isSupervisorEnhance)
	{
		if (system.TryGet(counterArea.RoomType, out var overrideDummyColor))
		{
			if (isSupervisorEnhance)
			{
				_playerColors.Add(counterArea.RoomType, overrideDummyColor);
			}
			counterArea.UpdateCount(overrideDummyColor.Count);
		}
		else
		{
			counterArea.UpdateCount(0);
		}
	}

	private static void ActivateSabotageMapOvelay(MapCountOverlay __instance, bool isActive, Color color)
	{
		__instance.isSab = isActive;
		__instance.BackgroundColor.SetColor(color);
		__instance.SabotageText.gameObject.SetActive(isActive);
	}
}



[HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
public static class MapCountOverlayUpdatePatch
{
	public static IReadOnlyDictionary<SystemTypes, IReadOnlyList<int>> PlayerColor => MapCountOverlayUpdatePatchBody.Instance.PlayerColors;

	public static bool Prefix(MapCountOverlay __instance)
		=> MapCountOverlayUpdatePatchBody.Instance.Prefix(__instance);


	public static void Postfix(MapCountOverlay __instance)
		=> MapCountOverlayUpdatePatchBody.Instance.Postfix(__instance);

	public static bool IsAbilityUse()
		=> MapCountOverlayUpdatePatchBody.Instance.IsAbilityUse();

	public static void LoadOptionValue()
		=> MapCountOverlayUpdatePatchBody.Instance.Initialize();
}
