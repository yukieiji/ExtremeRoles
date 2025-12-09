using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Module
{
    public sealed class PlayerReviver
    {
        private InnerToken? token;
        private TextMeshPro? resurrectText;

        public bool IsReviving => token != null;

        public void Start(float resurrectTime, PlayerControl rolePlayer, Action onReviveCompleted)
        {
            if (resurrectText == null)
            {
                resurrectText = UnityEngine.Object.Instantiate(
                    HudManager.Instance.KillButton.cooldownTimerText,
                    Camera.main.transform, false);
                resurrectText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
                resurrectText.enableWordWrapping = false;
            }

            Action onRevive = () => executeRevive(rolePlayer, onReviveCompleted);
            token = new InnerToken(resurrectTime, resurrectText, onRevive, () => token = null);
        }

        public void Update()
        {
            token?.Update();
        }

        public void Reset()
        {
            token?.Reset();
        }

        private void executeRevive(PlayerControl rolePlayer, Action onReviveCompleted)
        {
            if (rolePlayer == null) return;

            byte playerId = rolePlayer.PlayerId;

            Player.RpcUncheckRevive(playerId);

            if (rolePlayer.Data == null ||
                rolePlayer.Data.IsDead ||
                rolePlayer.Data.Disconnected)
            {
                return;
            }

            List<Vector2> randomPos = new List<Vector2>();
            Map.AddSpawnPoint(randomPos, playerId);

            Player.RpcUncheckSnap(playerId, randomPos[
                RandomGenerator.Instance.Next(randomPos.Count)]);

            HudManager.Instance.Chat.chatBubblePool.ReclaimAll();

            onReviveCompleted?.Invoke();
        }
    }
}
