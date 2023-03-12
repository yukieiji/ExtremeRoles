using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;


namespace ExtremeRoles.Helper;

public static class Player
{
    private static PlayerControl prevTarget;

    public static void ResetTarget()
    {
        resetPlayerOutLine();
        prevTarget = null;
    }

    public static PlayerControl GetPlayerControlById(byte id)
    {
        foreach (PlayerControl player in CachedPlayerControl.AllPlayerControls)
        {
            if (player.PlayerId == id) { return player; }
        }
        return null;
    }

    public static Console GetClosestConsole(PlayerControl player, float radius)
    {
        Console closestConsole = null;
        float closestConsoleDist = 9999;
        Vector2 pos = player.GetTruePosition();

        foreach (Collider2D collider in Physics2D.OverlapCircleAll(
            pos, radius, Constants.Usables))
        {
            Console checkConsole = collider.GetComponent<Console>();
            if (checkConsole is null) { continue; }

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

    public static PlayerControl GetClosestPlayerInKillRange()
    {
        var playersInAbilityRangeSorted = 
            CachedPlayerControl.LocalPlayer.Data.Role.GetPlayersInAbilityRangeSorted(
                RoleBehaviour.GetTempPlayerList());
        if (playersInAbilityRangeSorted.Count <= 0)
        {
            return null;
        }
        return playersInAbilityRangeSorted[0];
    }

    public static PlayerControl GetClosestPlayerInKillRange(PlayerControl player)
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

    public static PlayerControl GetClosestPlayerInRange(
        PlayerControl sourcePlayer,
        SingleRoleBase role,
        float range)
    {

        List<PlayerControl> allPlayer = GetAllPlayerInRange(
            sourcePlayer, role, range);

        resetPlayerOutLine();

        if (allPlayer.Count <= 0) { return null; }

        PlayerControl result = allPlayer[0];

        SetPlayerOutLine(result, role.GetNameColor());

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
            SetPlayerOutLine(targetPlayer, role.GetNameColor());
        }
        else
        {
            resetPlayerOutLine();
        }
        return result;
    }

    public static bool TryGetTaskType(
        PlayerControl player, TaskTypes taskType, out NormalPlayerTask task)
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

        return task && !task.IsComplete;
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

        foreach (GameData.PlayerInfo playerInfo in 
            GameData.Instance.AllPlayers.GetFastEnumerator())
        {
            if (isValidPlayer(role, sourcePlayer, playerInfo))
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

    public static float GetPlayerTaskGage(GameData.PlayerInfo player)
    {
        int taskNum = 0;
        int compNum = 0;

        foreach (GameData.TaskInfo task in player.Tasks.GetFastEnumerator())
        {

            ++taskNum;

            if (task.Complete)
            {
                ++compNum;
            }
        }

        return (float)compNum / (float)taskNum;

    }

    public static GameData.PlayerInfo GetDeadBodyInfo(float range)
    {

        Vector2 playerPos = CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();

        foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(
            playerPos, range,
            Constants.PlayersOnlyMask))
        {
            if (collider2D.tag == "DeadBody")
            {
                DeadBody component = collider2D.GetComponent<DeadBody>();

                if (component && !component.Reported)
                {
                    Vector2 truePosition = component.TruePosition;
                    if ((Vector2.Distance(truePosition, playerPos) <= range) &&
                        (CachedPlayerControl.LocalPlayer.PlayerControl.CanMove) &&
                        (!PhysicsHelpers.AnythingBetween(
                            playerPos, truePosition, Constants.ShipAndObjectsMask, false)))
                    {
                        return GameData.Instance.GetPlayerById(component.ParentId);
                    }
                }
            }
        }
        return null;
    }

    public static void RpcUncheckSnap(byte targetPlayerId, Vector2 pos)
    {
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

    public static void SetPlayerOutLine(PlayerControl target, Color color)
    {
        
        if (target == null || target.cosmetics.currentBodySprite.BodySprite == null) { return; }

        target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
        target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
        prevTarget = target;
    }

    public static Dictionary<byte, PoolablePlayer> CreatePlayerIcon(
        Transform parent = null, Vector3? scale = null)
    {
        if (parent == null)
        {
            parent = FastDestroyableSingleton<HudManager>.Instance.transform;
        }
        Vector3 newScale = scale.HasValue ? scale.Value : Vector3.one;

        Dictionary<byte, PoolablePlayer> playerIcon = new Dictionary<byte, PoolablePlayer>();

        foreach (PlayerControl player in CachedPlayerControl.AllPlayerControls)
        {
            PoolablePlayer poolPlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(
                Module.Prefab.PlayerPrefab, parent);

            poolPlayer.gameObject.SetActive(true);
            poolPlayer.UpdateFromPlayerData(
                player.Data, PlayerOutfitType.Default,
                PlayerMaterial.MaskType.None, true);
            poolPlayer.cosmetics.SetName(player.Data.DefaultOutfit.PlayerName);
            poolPlayer.cosmetics.nameText.transform.localPosition = new Vector3(
                poolPlayer.cosmetics.nameText.transform.localPosition.x,
                poolPlayer.cosmetics.nameText.transform.localPosition.y - 1.0f,
                poolPlayer.cosmetics.nameText.transform.localPosition.z);
            poolPlayer.name = $"poolable_{player.PlayerId}";
            poolPlayer.SetFlipX(true);
            poolPlayer.gameObject.SetActive(false);
            poolPlayer.transform.localScale = newScale;
            playerIcon.Add(player.PlayerId, poolPlayer);
        }

        return playerIcon;

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

        if (!isValidPlayer(role, sourcePlayer, targetPlayer.Data))
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

    private static void resetPlayerOutLine()
    {
        if (prevTarget != null &&
            prevTarget.cosmetics.currentBodySprite.BodySprite != null)
        {
            prevTarget.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 0f);
        }
    }

    private static bool isValidPlayer(
        SingleRoleBase role,
        PlayerControl sourcePlayer,
        GameData.PlayerInfo targetPlayer)
    {
        return (
            targetPlayer != null &&
            !targetPlayer.Disconnected &&
            targetPlayer.PlayerId != sourcePlayer.PlayerId &&
            !targetPlayer.IsDead &&
            targetPlayer.Object &&
            !targetPlayer.Object.inVent &&
            !role.IsSameTeam(Roles.ExtremeRoleManager.GameRole[targetPlayer.PlayerId])
        );
    }
}
