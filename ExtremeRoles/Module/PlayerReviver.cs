using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

namespace ExtremeRoles.Module;

#nullable enable

public sealed class PlayerReviver(float resurrectTime, Action<PlayerControl>? onReviveCompleted = null)
{
	private ReviveToken? token;
	private TextMeshPro? resurrectText;
	private readonly float resurrectTime = resurrectTime;
	private readonly Action<PlayerControl> onReviveCompleted = onReviveCompleted != null ? onReviveCompleted : (_) => { };

    public bool IsReviving => token != null;

    public void Start(PlayerControl rolePlayer)
    {
		hideChatWhenMeeting();

		if (this.resurrectText == null)
        {
			this.resurrectText = UnityEngine.Object.Instantiate(
                HudManager.Instance.KillButton.cooldownTimerText,
                Camera.main.transform, false);
			this.resurrectText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
			this.resurrectText.enableWordWrapping = false;
        }

        this.token = new ReviveToken(
			this.resurrectTime,
			this.resurrectText,
			rolePlayer,
			this.onReviveCompleted,
			() => this.token = null);
    }

    public void Update()
    {
		this.token?.Update();
    }

    public void Reset()
    {
		this.token?.Reset();
    }

	private static void hideChatWhenMeeting()
	{
		// 特殊会議以外はチャットは消しておく
		if (!OnemanMeetingSystemManager.IsActive)
		{
			HudManager.Instance.Chat.gameObject.SetActive(false);
		}
	}

    private sealed class ReviveToken(float resurrectTime, TextMeshPro resurrectText, PlayerControl rolePlayer, Action<PlayerControl> onReviveCompleted, Action onDispose)
	{
        private float resurrectTimer = resurrectTime;
		private readonly float maxTime = resurrectTime;
        private readonly TextMeshPro resurrectText = resurrectText;
        private readonly PlayerControl rolePlayer = rolePlayer;
        private readonly Action<PlayerControl> onReviveCompleted = onReviveCompleted;
		private readonly Action onDispose = onDispose;

        public void Update()
        {
			hideChatWhenMeeting();

			if (this.resurrectTimer > 0.0f)
			{
				this.resurrectText.gameObject.SetActive(true);
				this.resurrectTimer -= Time.deltaTime;
				this.resurrectText.text = string.Format(
					Tr.GetString("resurrectText"),
					Mathf.CeilToInt(this.resurrectTimer));
			}

            if (this.resurrectTimer <= 0.0f)
            {
                executeRevive();
				this.resurrectText.gameObject.SetActive(false);
				this.onDispose.Invoke();
            }
        }

        public void Reset()
        {
			hideChatWhenMeeting();
			this.resurrectTimer = this.maxTime;

			if (resurrectText != null)
            {
                resurrectText.gameObject.SetActive(false);
            }
        }

        private void executeRevive()
        {
			if (rolePlayer == null)
			{
				return;
			}

            byte playerId = rolePlayer.PlayerId;

            Player.RpcUncheckRevive(playerId);

            if (this.rolePlayer.Data == null ||
				this.rolePlayer.Data.IsDead ||
				this.rolePlayer.Data.Disconnected)
            {
                return;
            }

			List<Vector2> randomPos = [];
            Map.AddSpawnPoint(randomPos, playerId);

            Player.RpcUncheckSnap(playerId, randomPos[
                RandomGenerator.Instance.Next(randomPos.Count)]);

            HudManager.Instance.Chat.chatBubblePool.ReclaimAll();
			this.onReviveCompleted.Invoke(rolePlayer);
        }
    }
}
