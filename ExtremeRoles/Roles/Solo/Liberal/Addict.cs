using System;
using UnityEngine;
using TMPro;
using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class Addict :
	SingleRoleBase,
	IRoleUpdate,
	IRoleResetMeeting
{
	public enum AddictOption
	{
		SelfKillTimerTime,
		MovementTimeToRecover,
	}

	private DoveCommonAbilityHandler? handler;
	private AddictStatusModel? statusModel;
	private TextMeshPro? timerText;

	public override IStatusModel? Status => statusModel;

	public Addict() : base(
		RoleArgs.BuildLiberalDove(ExtremeRoleId.Addict))
	{
	}

	public void Update(PlayerControl rolePlayer)
	{
		try
		{
			this.handler?.Update(rolePlayer);
		}
		catch (Exception ex)
		{
			Logging.Debug($"Addict DoveCommonAbilityHandler error: {ex.Message}");
		}

		if (this.statusModel is null)
		{
			return;
		}

		if (!GameProgressSystem.IsTaskPhase || rolePlayer.IsInValid())
		{
			if (this.timerText != null)
			{
				this.timerText.gameObject.SetActive(false);
			}
			return;
		}

		var curPos = rolePlayer.GetTruePosition();

		if (isCloseTo(this.statusModel.PrevPlayerPos, new Vector2(100.0f, 100.0f), 0.01f))
		{
			this.statusModel.PrevPlayerPos = curPos;
		}

		bool isMoving = rolePlayer.CanMove &&
			Minigame.Instance == null &&
			!rolePlayer.inVent &&
			isNotCloseTo(this.statusModel.PrevPlayerPos, curPos);

		this.statusModel.PrevPlayerPos = curPos;

		if (isMoving)
		{
			this.statusModel.CurrentMovementTime += Time.deltaTime;
			if (this.statusModel.CurrentMovementTime >= this.statusModel.MovementTimeToRecover)
			{
				this.statusModel.CurrentSelfKillTimer = Math.Min(this.statusModel.MaxSelfKillTimer, this.statusModel.CurrentSelfKillTimer + Time.deltaTime);
			}
		}
		else
		{
			this.statusModel.CurrentMovementTime = 0.0f;
			this.statusModel.CurrentSelfKillTimer -= Time.deltaTime;

			if (this.statusModel.CurrentSelfKillTimer <= 0.0f)
			{
				this.statusModel.CurrentSelfKillTimer = 0.0f;
				if (!this.statusModel.HasExploded)
				{
					this.statusModel.HasExploded = true;
					var localPlayer = PlayerControl.LocalPlayer;
					if (localPlayer != null && rolePlayer.PlayerId == localPlayer.PlayerId)
					{
						try
						{
							Player.RpcUncheckMurderPlayer(
								rolePlayer.PlayerId,
								rolePlayer.PlayerId,
								byte.MaxValue);
						}
						catch (Exception ex)
						{
							Logging.Debug($"Addict RpcUncheckMurderPlayer error: {ex.Message}");
						}
					}
				}
			}
		}

		var currentLocalPlayer = PlayerControl.LocalPlayer;
		if (currentLocalPlayer != null && rolePlayer.PlayerId == currentLocalPlayer.PlayerId)
		{
			if (this.timerText == null &&
				HudManager.Instance != null &&
				HudManager.Instance.KillButton != null &&
				HudManager.Instance.KillButton.cooldownTimerText != null &&
				HudManager.Instance.UseButton != null)
			{
				createTimerText();
			}

			if (this.timerText != null)
			{
				this.timerText.gameObject.SetActive(true);
				this.timerText.text = Tr.GetString("addictSelfKill", Mathf.CeilToInt(this.statusModel.CurrentSelfKillTimer));
			}
		}
	}

	public void ResetOnMeetingStart()
	{
		if (this.timerText != null)
		{
			this.timerText.gameObject.SetActive(false);
		}
		this.statusModel?.Reset();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		this.statusModel?.Reset();
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
		this.handler?.ClearTask(rolePlayer);
	}

	public override void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		this.handler?.ClearTask(rolePlayer);
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateFloatOption(
			AddictOption.SelfKillTimerTime,
			15.0f, 5.0f, 120.0f, 1.0f,
			format: OptionUnit.Second);

		factory.CreateFloatOption(
			AddictOption.MovementTimeToRecover,
			5.0f, 1.0f, 15.0f, 0.5f,
			format: OptionUnit.Second);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;

		try
		{
			if (ExtremeRolesPlugin.Instance?.Provider != null)
			{
				var liberalOption = ExtremeRolesPlugin.Instance.Provider.GetService<LiberalDefaultOptionLoader>();
				if (liberalOption != null)
				{
					LiberalSettingOverrider.OverrideDefault(this, liberalOption);
					this.handler = new DoveCommonAbilityHandler(liberalOption);
				}
			}
		}
		catch (Exception ex)
		{
			Logging.Debug($"Addict LiberalOption error: {ex.Message}");
		}

		float maxTimer = loader.GetValue<AddictOption, float>(AddictOption.SelfKillTimerTime);
		float recoveryTime = loader.GetValue<AddictOption, float>(AddictOption.MovementTimeToRecover);

		this.statusModel = new AddictStatusModel(maxTimer, recoveryTime);
	}

	private void createTimerText()
	{
		if (HudManager.Instance == null ||
			HudManager.Instance.KillButton == null ||
			HudManager.Instance.KillButton.cooldownTimerText == null ||
			HudManager.Instance.UseButton == null) { return; }

		var hudManager = HudManager.Instance;
		var killButton = hudManager.KillButton;

		this.timerText = UnityEngine.Object.Instantiate(
			killButton.cooldownTimerText,
			killButton.transform.parent);
		this.timerText.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
		this.timerText.transform.localPosition =
			hudManager.UseButton.transform.localPosition + new Vector3(-2.0f, -0.125f, 0);
		this.timerText.gameObject.SetActive(true);
	}

	private static bool isCloseTo(Vector2 a, Vector2 b, float sqrEps = 0.001f)
	{
		float dx = a.x - b.x;
		float dy = a.y - b.y;
		return (dx * dx + dy * dy) <= sqrEps;
	}

	private static bool isNotCloseTo(Vector2 a, Vector2 b, float sqrEps = 0.001f)
	{
		return !isCloseTo(a, b, sqrEps);
	}
}
