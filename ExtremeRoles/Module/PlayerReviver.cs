using System;
using System.Collections.Generic;
using UnityEngine;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Module;

#nullable enable

public sealed class PlayerReviver
{
    private readonly Action onRevive;
    private float resurrectTimer;
    private readonly float resurrectTime;

    private TMPro.TextMeshPro? resurrectText;

    public bool IsReviving { get; private set; }

    public PlayerReviver(float resurrectTime, Action onRevive)
    {
        this.resurrectTime = resurrectTime;
        this.resurrectTimer = resurrectTime;
        this.onRevive = onRevive;
        this.IsReviving = false;
    }

    public void Start()
    {
        IsReviving = true;
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (!IsReviving || !rolePlayer.Data.IsDead)
        {
            return;
        }

        if (resurrectText == null)
        {
            resurrectText = UnityEngine.Object.Instantiate(
                HudManager.Instance.KillButton.cooldownTimerText,
                Camera.main.transform, false);
            resurrectText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
            resurrectText.enableWordWrapping = false;
        }

        resurrectText.gameObject.SetActive(true);
        resurrectTimer -= Time.deltaTime;
        resurrectText.text = Tr.GetString(
            "resurrectText",
            Mathf.CeilToInt(resurrectTimer));

        if (resurrectTimer <= 0.0f)
        {
            IsReviving = false;
            Revive(rolePlayer);
        }
    }

    public void Revive(PlayerControl rolePlayer)
    {
        if (rolePlayer == null) { return; }

        byte playerId = rolePlayer.PlayerId;

        Player.RpcUncheckRevive(playerId);

        if (rolePlayer.Data == null ||
            rolePlayer.Data.IsDead ||
            rolePlayer.Data.Disconnected) { return; }

        List<Vector2> randomPos = new List<Vector2>();
        Map.AddSpawnPoint(randomPos, playerId);

        Player.RpcUncheckSnap(playerId, randomPos[
            RandomGenerator.Instance.Next(randomPos.Count)]);

        onRevive.Invoke();

        HudManager.Instance.Chat.chatBubblePool.ReclaimAll();
        if (resurrectText != null)
        {
            resurrectText.gameObject.SetActive(false);
        }

        resurrectTimer = resurrectTime;
    }

    public void Stop()
    {
        IsReviving = false;
        if (resurrectText != null)
        {
            resurrectText.gameObject.SetActive(false);
        }
    }
}
