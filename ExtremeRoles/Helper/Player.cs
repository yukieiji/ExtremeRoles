using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using AmongUs.GameOptions;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Roles.Solo.Host;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

#nullable enable

namespace ExtremeRoles.Helper;

public static class Player
{
	public static float DefaultKillCoolTime => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);

	private static Il2CppReferenceArray<Collider2D> hitBuffer = new Il2CppReferenceArray<Collider2D>(20);
	private static ContactFilter2D playerHitFilter = new ContactFilter2D()
	{
		layerMask = Constants.PlayersOnlyMask,
		useLayerMask = true,
		useTriggers = false,
	};

	public static Dictionary<byte, PoolablePlayer> CreatePlayerIcon(
		Transform? parent = null, Vector3? scale = null)
	{
		if (parent == null)
		{
			parent = HudManager.Instance.transform;
		}
		Vector3 newScale = scale.HasValue ? scale.Value : Vector3.one;

		Dictionary<byte, PoolablePlayer> playerIcon = new Dictionary<byte, PoolablePlayer>();

		foreach (PlayerControl player in PlayerCache.AllPlayerControl)
		{
			PoolablePlayer poolPlayer = Object.Instantiate(
				Module.Prefab.PlayerPrefab, parent);

			poolPlayer.gameObject.SetActive(true);

			poolPlayer.cosmetics.SetName(player.Data.DefaultOutfit.PlayerName);
			poolPlayer.cosmetics.nameText.transform.localPosition = new Vector3(
				poolPlayer.cosmetics.nameText.transform.localPosition.x,
				poolPlayer.cosmetics.nameText.transform.localPosition.y - 1.0f,
				poolPlayer.cosmetics.nameText.transform.localPosition.z);

			poolPlayer.name = $"poolable_{player.PlayerId}";

			poolPlayer.SetFlipX(true);
			poolPlayer.UpdateFromPlayerData(
				player.Data, PlayerOutfitType.Default,
				PlayerMaterial.MaskType.None, true);

			poolPlayer.gameObject.SetActive(false);
			poolPlayer.transform.localScale = newScale;
			playerIcon.Add(player.PlayerId, poolPlayer);
		}

		return playerIcon;

	}

	public static PlayerControl? GetPlayerControlById(byte id)
    {
        foreach (PlayerControl player in PlayerCache.AllPlayerControl)
        {
            if (player.PlayerId == id) { return player; }
        }
        return null;
    }

	public static bool TryGetPlayerControl(byte id, [NotNullWhen(true)] out PlayerControl? result)
	{
		result = PlayerCache.AllPlayerControl.FirstOrDefault(x => x != null && x.PlayerId == id);
		return result != null;
	}

	public static bool TryGetPlayerInfo(byte id, [NotNullWhen(true)] out NetworkedPlayerInfo? player)
	{
		if (GameData.Instance == null)
		{
			player = null;
			return false;
		}

		player = GameData.Instance.GetPlayerById(id);

		return player != null;
	}

    public static Console? GetClosestConsole(PlayerControl player, float radius)
    {
        Console? closestConsole = null;
        float closestConsoleDist = 9999;
        Vector2 pos = player.GetTruePosition();

        foreach (Collider2D collider in Physics2D.OverlapCircleAll(
            pos, radius, Constants.Usables))
        {
            if (!collider.TryGetComponent<Console>(out var checkConsole))
            {
                continue;
            }

            Vector3 targetPos = collider.transform.position;

            float checkDist = Vector2.Distance(pos, targetPos);

            if (checkDist < closestConsoleDist)
            {
                closestConsole = checkConsole;
                closestConsoleDist = checkDist;
            }
        }
        return closestConsole;
    }

    public static PlayerControl? GetClosestPlayerInKillRange()
    {
        var playersInAbilityRangeSorted =
            PlayerControl.LocalPlayer.Data.Role.GetPlayersInAbilityRangeSorted(
                RoleBehaviour.GetTempPlayerList());
        if (playersInAbilityRangeSorted.Count <= 0)
        {
            return null;
        }
        return playersInAbilityRangeSorted[0];
    }

    public static PlayerControl? GetClosestPlayerInKillRange(PlayerControl player)
    {
        var playersInAbilityRangeSorted =
            player.Data.Role.GetPlayersInAbilityRangeSorted(
                RoleBehaviour.GetTempPlayerList());
        if (playersInAbilityRangeSorted.Count <= 0)
        {
            return null;
        }
        return playersInAbilityRangeSorted[0];
    }

	public static bool TryGetClosestPlayerInRange(
		SingleRoleBase role,
		float range,
		[NotNullWhen(true)] out PlayerControl? targetPlayer)
	{
		targetPlayer = GetClosestPlayerInRange(PlayerControl.LocalPlayer, role, range);
		return targetPlayer != null;
	}

	public static bool TryGetClosestPlayerInRange(
		PlayerControl sourcePlayer,
		SingleRoleBase role,
		float range,
		[NotNullWhen(true)] out PlayerControl? targetPlayer)
	{
		targetPlayer = GetClosestPlayerInRange(sourcePlayer, role, range);
		return targetPlayer != null;
	}

	public static PlayerControl? GetClosestPlayerInRange(
        PlayerControl sourcePlayer,
        SingleRoleBase role,
        float range)
    {

        List<PlayerControl> allPlayer = GetAllPlayerInRange(
            sourcePlayer, role, range);

        if (allPlayer.Count <= 0)
		{
			return null;
		}

        PlayerControl result = allPlayer[0];

		PlayerOutLine.SetOutline(result, role.GetNameColor());

        return result;
    }

    public static bool IsPlayerInRangeAndDrawOutLine(
        PlayerControl sourcePlayer,
        PlayerControl targetPlayer,
        SingleRoleBase role,
        float range)
    {
        bool result = isPlayerInRange(sourcePlayer, targetPlayer, role, range);

        if (result)
        {
			PlayerOutLine.SetOutline(targetPlayer, role.GetNameColor());
        }
        return result;
    }

    public static bool TryGetTaskType(
        PlayerControl player, TaskTypes taskType, [NotNullWhen(true)] out NormalPlayerTask? task)
    {
        task = null;

        SingleRoleBase role = ExtremeRoleManager.GameRole[player.PlayerId];

        if (!role.HasTask())
        {
            return false;
        }

        PlayerTask playerTask = player.myTasks.Find(
            (Il2CppSystem.Predicate<PlayerTask>)(
                (PlayerTask task) =>
                    task && task.TaskType == taskType));

        if (!playerTask) { return false; }
        task = playerTask.TryCast<NormalPlayerTask>();

        return task != null && !task.IsComplete;
    }

    public static List<PlayerControl> GetAllPlayerInRange(
        PlayerControl sourcePlayer,
        SingleRoleBase role, float range)
    {

        List<PlayerControl> result = new List<PlayerControl>();

        if (!ShipStatus.Instance)
        {
            return result;
        }

        Vector2 truePosition = sourcePlayer.GetTruePosition();

        foreach (NetworkedPlayerInfo playerInfo in
            GameData.Instance.AllPlayers.GetFastEnumerator())
        {
            if (IsValidPlayer(role, sourcePlayer, playerInfo))
            {
                PlayerControl target = playerInfo.Object;

                Vector2 vector = target.GetTruePosition() - truePosition;
                float magnitude = vector.magnitude;
                if (magnitude <= range &&
                    !PhysicsHelpers.AnyNonTriggersBetween(
                        truePosition, vector.normalized,
                        magnitude, Constants.ShipAndObjectsMask))
                {
                    result.Add(target);
                }
            }
        }

        result.Sort(delegate (PlayerControl a, PlayerControl b)
        {
            float magnitude2 = (a.GetTruePosition() - truePosition).magnitude;
            float magnitude3 = (b.GetTruePosition() - truePosition).magnitude;
            if (magnitude2 > magnitude3)
            {
                return 1;
            }
            if (magnitude2 < magnitude3)
            {
                return -1;
            }
            return 0;
        });

        return result;
    }

    public static float GetPlayerTaskGage(PlayerControl player)
    {
        return GetPlayerTaskGage(player.Data);
    }

    public static float GetPlayerTaskGage(NetworkedPlayerInfo player)
    {
        int taskNum = 0;
        int compNum = 0;

        foreach (var task in player.Tasks.GetFastEnumerator())
        {
            ++taskNum;

            if (task.Complete)
            {
                ++compNum;
            }
        }

        return (float)compNum / (float)taskNum;

    }

    public static NetworkedPlayerInfo? GetDeadBodyInfo(float range)
    {
        Vector2 playerPos = PlayerControl.LocalPlayer.GetTruePosition();

        foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(
            playerPos, range,
            Constants.PlayersOnlyMask))
        {
            if (!collider2D.CompareTag("DeadBody")) { continue; }

            DeadBody component = collider2D.GetComponent<DeadBody>();

            if (component && !component.Reported)
            {
                Vector2 truePosition = component.TruePosition;
                if ((Vector2.Distance(truePosition, playerPos) <= range) &&
                    (PlayerControl.LocalPlayer.CanMove) &&
                    (!PhysicsHelpers.AnythingBetween(
                        playerPos, truePosition, Constants.ShipAndObjectsMask, false)))
                {
                    return GameData.Instance.GetPlayerById(component.ParentId);
                }
            }
        }
        return null;
    }

	public static void RpcUncheckSnap(byte targetPlayerId, Vector2 pos, bool isTeleportXion=false)
    {
		if (isTeleportXion &&
			Xion.PlayerId == targetPlayerId &&
			PlayerControl.LocalPlayer.PlayerId == targetPlayerId)
		{
			Xion.RpcTeleportTo(pos);
			return;
		}

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.UncheckedSnapTo))
        {
            caller.WriteByte(targetPlayerId);
            caller.WriteFloat(pos.x);
            caller.WriteFloat(pos.y);
        }

        RPCOperator.UncheckedSnapTo(targetPlayerId, pos);
    }

    public static void RpcUncheckMurderPlayer(
        byte killerPlayerId, byte targetPlayerId, byte useAnimation)
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.UncheckedMurderPlayer))
        {
            caller.WriteByte(killerPlayerId);
            caller.WriteByte(targetPlayerId);
            caller.WriteByte(useAnimation);
        }

        RPCOperator.UncheckedMurderPlayer(
            killerPlayerId, targetPlayerId, useAnimation);
    }

	public static void RpcUncheckExiled(byte targetPlayerId)
	{
		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.UncheckedExiledPlayer))
		{
			caller.WriteByte(targetPlayerId);
		}

		RPCOperator.UncheckedExiledPlayer(targetPlayerId);
	}

	public static void RpcUncheckRevive(byte targetPlayerId)
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.UncheckedRevive))
        {
            caller.WriteByte(targetPlayerId);
        }
        RPCOperator.UncheckedRevive(targetPlayerId);
    }

    public static void RpcCleanDeadBody(byte targetPlayerId)
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.CleanDeadBody))
        {
            caller.WriteByte(targetPlayerId);
        }
        RPCOperator.CleanDeadBody(targetPlayerId);
    }

	public static bool TryGetPlayerRoom(PlayerControl player, [NotNullWhen(true)] out SystemTypes? roomeId)
	{
		roomeId = null;

		if (player == null)
		{
			return false;
		}

		Collider2D collider = player.GetComponent<Collider2D>();
		return TryGetPlayerColiderRoom(collider, out roomeId);
	}

	public static bool TryGetPlayerColiderRoom(Collider2D playerCollider, [NotNullWhen(true)] out SystemTypes? roomeId)
	{
		roomeId = null;

		if (playerCollider == null)
		{
			return false;
		}

		foreach (PlainShipRoom room in ShipStatus.Instance.AllRooms)
		{
			if (room != null && room.roomArea)
			{
				int hitCount = room.roomArea.OverlapCollider(playerHitFilter, hitBuffer);
				if (isHit(playerCollider, hitBuffer, hitCount))
				{
					roomeId = room.RoomId;
					return true;
				}
			}
		}
		return false;
	}

	private static bool isPlayerInRange(
        PlayerControl sourcePlayer,
        PlayerControl targetPlayer,
        SingleRoleBase role, float range)
    {
        if (!ShipStatus.Instance)
        {
            return false;
        }

        Vector2 truePosition = sourcePlayer.GetTruePosition();

        if (!IsValidPlayer(role, sourcePlayer, targetPlayer.Data))
        {
            return false;
        }

        Vector2 vector = targetPlayer.GetTruePosition() - truePosition;
        float magnitude = vector.magnitude;

        return
            magnitude <= range &&
            !PhysicsHelpers.AnyNonTriggersBetween(
                truePosition, vector.normalized,
                magnitude, Constants.ShipAndObjectsMask);

    }

	private static bool isHit(
		Collider2D playerCollinder,
		Collider2D[] buffer,
		int hitCount)
	{
		for (int i = 0; i < hitCount; i++)
		{
			if (buffer[i] == playerCollinder)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsValidPlayer(
        SingleRoleBase role,
        PlayerControl sourcePlayer,
        NetworkedPlayerInfo targetPlayer)
    {
		if (targetPlayer == null)
		{
			return false;
		}
		byte targetPlayerId = targetPlayer.PlayerId;
		byte sourcePlayerId = sourcePlayer.PlayerId;

        return (
			targetPlayerId != sourcePlayerId &&
			targetPlayer.IsAlive() &&
            !targetPlayer.Object.inVent &&
			!targetPlayer.Object.inMovingPlat &&
			!targetPlayer.Object.onLadder &&
			ExtremeRoleManager.TryGetRole(targetPlayerId, out var targetRole) &&
            !role.IsSameTeam(targetRole) &&
			(targetRole.AbilityClass is not IInvincible invincible || invincible.IsValidAbilitySource(sourcePlayerId))
        );
    }
}
