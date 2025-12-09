using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Module
{
    public sealed class PlayerReviver
    {
        private ReviveToken? token;
        private TextMeshPro? resurrectText;
        private readonly float resurrectTime;

        public bool IsReviving => token != null;

        public PlayerReviver(float resurrectTime)
        {
            this.resurrectTime = resurrectTime;
        }

        public void Start(PlayerControl rolePlayer, Action onReviveCompleted)
        {
            if (resurrectText == null)
            {
                resurrectText = UnityEngine.Object.Instantiate(
                    HudManager.Instance.KillButton.cooldownTimerText,
                    Camera.main.transform, false);
                resurrectText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
                resurrectText.enableWordWrapping = false;
            }

            token = new ReviveToken(resurrectTime, resurrectText, rolePlayer, onReviveCompleted, () => token = null);
        }

        public void Update()
        {
            token?.Update();
        }

        public void Reset()
        {
            token?.Reset();
        }

        private sealed class ReviveToken
        {
            private float resurrectTimer;
            private readonly TextMeshPro resurrectText;
            private readonly PlayerControl rolePlayer;
            private readonly Action onReviveCompleted;
            private readonly Action onDispose;

            public ReviveToken(float resurrectTime, TextMeshPro resurrectText, PlayerControl rolePlayer, Action onReviveCompleted, Action onDispose)
            {
                this.resurrectTimer = resurrectTime;
                this.resurrectText = resurrectText;
                this.rolePlayer = rolePlayer;
                this.onReviveCompleted = onReviveCompleted;
                this.onDispose = onDispose;
            }

            public void Update()
            {
                if (resurrectTimer <= 0.0f) return;

                resurrectText.gameObject.SetActive(true);
                resurrectTimer -= Time.deltaTime;
                resurrectText.text = string.Format(
                    Tr.GetString("resurrectText"),
                    Mathf.CeilToInt(resurrectTimer));

                if (resurrectTimer <= 0.0f)
                {
                    executeRevive();
                    onDispose?.Invoke();
                }
            }

            public void Reset()
            {
                if (resurrectText != null)
                {
                    resurrectText.gameObject.SetActive(false);
                }
            }

            private void executeRevive()
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
}
