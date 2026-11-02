using System.Collections.Generic;
using TMPro;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.FemmeFatale;

#nullable enable

public sealed class JoseStatusModel(float maxTimer) : IStatusModel
{
	private readonly float maxSelfKillTimer = maxTimer;
	public float CurrentSelfKillTimer { get; private set; } = maxTimer;

	private bool hasExploded = false;
	private TextMeshPro? timerText;
	private HashSet<uint> completedTasks = new();

	public void Reset()
	{
		this.CurrentSelfKillTimer = this.maxSelfKillTimer;
	}

	public void OnMeetingStart()
	{
		if (this.timerText != null)
		{
			this.timerText.gameObject.SetActive(false);
		}
		Reset();
	}

	public void OnMeetingEnd()
	{
		Reset();
		this.completedTasks.Clear();
	}

	public void OnKill()
	{
		Reset();
	}

	public void Update(PlayerControl rolePlayer, float deltaTime)
	{
		if (!GameProgressSystem.IsTaskPhase || rolePlayer == null || rolePlayer.Data == null)
		{
			if (this.timerText != null)
			{
				this.timerText.gameObject.SetActive(false);
			}
			return;
		}

		// Full task completion resets self-destruct timer
		if (rolePlayer.Data.Tasks != null)
		{
			foreach (var task in rolePlayer.Data.Tasks)
			{
				if (task.Complete && !this.completedTasks.Contains(task.Id))
				{
					this.completedTasks.Add(task.Id);
					Reset();
				}
			}
		}

		bool explode = UpdateTimer(deltaTime);
		if (explode)
		{
			Player.RpcUncheckMurderPlayer(
				rolePlayer.PlayerId,
				rolePlayer.PlayerId,
				byte.MaxValue);
		}

		updateTimerText();
	}

	public bool UpdateTimer(float deltaTime)
	{
		if (!GameProgressSystem.IsTaskPhase || this.hasExploded)
		{
			return false;
		}

		this.CurrentSelfKillTimer -= deltaTime;

		if (this.CurrentSelfKillTimer <= 0.0f)
		{
			this.CurrentSelfKillTimer = 0.0f;
			this.hasExploded = true;
			return true;
		}

		return false;
	}

	public void Cleanup()
	{
		if (this.timerText != null)
		{
			this.timerText.gameObject.SetActive(false);
			Object.Destroy(this.timerText.gameObject);
			this.timerText = null;
		}
	}

	private void updateTimerText()
	{
		if (this.timerText == null &&
			HudManager.Instance != null &&
			HudManager.Instance.KillButton != null &&
			HudManager.Instance.KillButton.cooldownTimerText != null &&
			HudManager.Instance.UseButton != null)
		{
			createTimerText(HudManager.Instance);
		}

		if (this.timerText != null)
		{
			this.timerText.gameObject.SetActive(true);
			this.timerText.text = $"{Mathf.CeilToInt(this.CurrentSelfKillTimer)}";
		}
	}

	private void createTimerText(HudManager hud)
	{
		var killButton = hud.KillButton;

		this.timerText = Object.Instantiate(
			killButton.cooldownTimerText,
			killButton.transform.parent);
		this.timerText.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
		this.timerText.transform.localPosition =
			hud.UseButton.transform.localPosition + new Vector3(-2.0f, -0.125f, 0);
		this.timerText.gameObject.SetActive(true);

		var timerInfoText = Object.Instantiate(
			hud.KillButton.cooldownTimerText,
			this.timerText.transform);
		timerInfoText.enableWordWrapping = false;
		timerInfoText.transform.localScale = Vector3.one * 0.5f;
		timerInfoText.transform.localPosition += new Vector3(-0.05f, 0.6f, 0);
		timerInfoText.gameObject.SetActive(true);

		timerInfoText.text = Tr.GetString("untilSelfKill");
	}
}
