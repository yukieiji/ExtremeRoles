using System.Collections.Generic;
using System.Linq;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;
using UnityEngine;

namespace ExtremeRoles.Roles.Solo.Crewmate.Delusioner
{
    public class DelusionerAbilityHandler : IAbility, IKilledFrom
    {
        private DelusionerStatusModel status;

        public DelusionerAbilityHandler(DelusionerStatusModel status)
        {
            this.status = status;
        }

        public bool TryKilledFrom(PlayerControl rolePlayer, PlayerControl fromPlayer)
        {
            byte rolePlayerId = rolePlayer.PlayerId;
            if (status.system is null ||
                !status.system.TryGetCounter(rolePlayerId, out int countNum))
            {
                return true;
            }

            List<PlayerControl> allPlayer = Player.GetAllPlayerInRange(
                rolePlayer, status.range);

            int num = allPlayer.Count;
            if (allPlayer.Count == 0)
            {
                return true;
            }

            int reduceNum = Mathf.Clamp(allPlayer.Count, 0, countNum);
            var targets = allPlayer.OrderBy(
                x => RandomGenerator.Instance.Next())
                .Take(reduceNum)
                .Select(x => x.PlayerId)
                .ToHashSet();
            foreach (byte target in targets)
            {
                UseAbilityTo(rolePlayer, target, false, targets);
            }

            status.system.ReduceCounter(rolePlayerId, reduceNum);

            var newTaget = Player.GetClosestPlayerInKillRange(rolePlayer);

            return newTaget != null && newTaget.PlayerId == rolePlayerId;
        }

        public bool UseAbilityTo(
            in PlayerControl rolePlayer,
            in byte teloportTarget,
            in bool includeRolePlayer,
            in IReadOnlySet<byte> ignores)
        {
            List<Vector2> randomPos = new List<Vector2>(
                PlayerControl.AllPlayerControls.Count);
            var allPlayer = GameData.Instance.AllPlayers;

            if (includeRolePlayer)
            {
                randomPos.Add(rolePlayer.transform.position);
            }

            if (status.includeSpawnPoint)
            {
                Map.AddSpawnPoint(randomPos, teloportTarget);
            }

            foreach (var player in allPlayer.GetFastEnumerator())
            {
                if (player == null ||
                    player.Disconnected ||
                    player.PlayerId == rolePlayer.PlayerId ||
                    player.PlayerId == teloportTarget ||
                    player.IsDead ||
                    player.Object == null ||
                    player.Object.onLadder || // はしご中？
                    player.Object.inVent || // ベント入ってる？
                    player.Object.inMovingPlat || // なんか乗ってる状態
                    ignores.Contains(player.PlayerId))
                {
                    continue;
                }

                Vector3 targetPos = player.Object.transform.position;

                if (ExtremeSpawnSelectorMinigame.IsCloseWaitPos(targetPos))
                {
                    continue;
                }

                randomPos.Add(targetPos);
            }

            if (randomPos.Count == 0)
            {
                return false;
            }

            Player.RpcUncheckSnap(teloportTarget, randomPos[
                RandomGenerator.Instance.Next(randomPos.Count)]);

            if (status.Button != null &&
                status.deflectDamagePenaltyMod < 1.0f)
            {
                status.curCoolTime = status.curCoolTime * status.deflectDamagePenaltyMod;
                status.Button.Behavior.SetCoolTime(status.curCoolTime);
            }
            return true;
        }
    }
}
